' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Imports System.IO

Namespace ConsoleGUI

  Public Enum FileDialogMode
    Open
    Save
  End Enum

  ''' <summary>
  ''' A modal file dialog laid out like QBasic's Open dialog: a File Name box, the current path, a Files list (in columns, with a horizontal
  ''' scroll bar), a Dirs/Drives list and the OK, Cancel and Help buttons. On an 80 x 25 screen it is exactly QBasic's size; on a larger
  ''' window it grows with it, showing more rows and columns of files and room for long names.
  ''' Tab / Shift+Tab move between the parts; typing goes to the File Name box; Enter opens the file named there (or the folder selected in
  ''' the Dirs/Drives list); a wildcard such as *.bas changes the filter; Esc cancels.
  ''' Call <see cref="Show"/>; the callbacks run on the UI thread after the dialog has closed.
  ''' </summary>
  Public Class FileDialog
    Inherits Control

    Private Enum Area
      FileName
      Files
      Dirs
      Ok
      Cancel
      Help
    End Enum

    ''' <summary>A selectable list. The Files list runs down its columns then across (<see cref="ColumnMode"/>); the Dirs list is one column.</summary>
    Private NotInheritable Class PickList

      Public Items As New List(Of FileEntry)()
      Public Cursor As Integer
      Public Top As Integer
      Public TopColumn As Integer
      Public Rows As Integer = 8
      Public VisibleColumns As Integer = 1
      Public ColumnMode As Boolean

      ' True: the view scrolls to keep the selection visible. Scrolling with the scroll bar arrows turns it off until the selection moves again.
      Public FollowCursor As Boolean = True

      Public ReadOnly Property Selected As FileEntry
        Get
          Return If(Me.Cursor >= 0 AndAlso Me.Cursor < Me.Items.Count, Me.Items(Me.Cursor), Nothing)
        End Get
      End Property

      Public ReadOnly Property TotalColumns As Integer
        Get
          Return If(Me.Items.Count = 0, 0, (Me.Items.Count + Me.Rows - 1) \ Me.Rows)
        End Get
      End Property

      Public Sub Reset()
        Me.FollowCursor = True
        Me.Cursor = 0
        Me.Top = 0
        Me.TopColumn = 0
      End Sub

      ''' <summary>After the window changed size: keep the selection visible, or just keep the scroll position valid.</summary>
      Public Sub Normalize()
        If Me.FollowCursor Then
          Me.MoveTo(Me.Cursor)
        Else
          Me.TopColumn = Math.Max(0, Math.Min(Me.TopColumn, Math.Max(0, Me.TotalColumns - Me.VisibleColumns)))
        End If
      End Sub

      ''' <summary>Selects an item and scrolls so that it is visible.</summary>
      Public Sub MoveTo(index As Integer)

        If Me.Items.Count = 0 Then
          Me.Reset()
          Return
        End If

        Me.Cursor = Math.Max(0, Math.Min(Me.Items.Count - 1, index))
        Me.FollowCursor = True

        If Me.ColumnMode Then
          Dim column = Me.Cursor \ Me.Rows
          If column < Me.TopColumn Then Me.TopColumn = column
          If column >= Me.TopColumn + Me.VisibleColumns Then Me.TopColumn = column - Me.VisibleColumns + 1
          Me.TopColumn = Math.Max(0, Math.Min(Me.TopColumn, Math.Max(0, Me.TotalColumns - Me.VisibleColumns)))
        Else
          If Me.Cursor < Me.Top Then Me.Top = Me.Cursor
          If Me.Cursor >= Me.Top + Me.Rows Then Me.Top = Me.Cursor - Me.Rows + 1
          Me.Top = Math.Max(0, Math.Min(Me.Top, Math.Max(0, Me.Items.Count - Me.Rows)))
        End If

      End Sub

    End Class

    Private Shared ReadOnly OpenTabOrder As Area() = {Area.FileName, Area.Files, Area.Dirs, Area.Ok, Area.Cancel, Area.Help}
    Private Shared ReadOnly SaveTabOrder As Area() = {Area.FileName, Area.Dirs, Area.Ok, Area.Cancel, Area.Help}

    Private ReadOnly Property TabOrder As Area()
      Get
        Return If(Me.Mode = FileDialogMode.Save, SaveTabOrder, OpenTabOrder)
      End Get
    End Property

    Private Const OkLabel As String = "< OK >"
    Private Const CancelLabel As String = "< Cancel >"
    Private Const HelpLabel As String = "< Help >"

    ' QBasic's size on an 80 x 25 screen is 67 x 20; these are the fixed parts of that.
    Private Const MinWidth As Integer = 40
    Private Const FixedRows As Integer = 12      ' every row except the list rows (Open)
    Private Const SaveFixedRows As Integer = 11  ' Save As has no spare row above the separator
    Private Const MinListRows As Integer = 2

    Private _screen As Screen
    Private _previousPopup As Control
    Private _onAccept As Action(Of String)
    Private _onCancel As Action

    Private _directory As String = ""
    Private ReadOnly _files As New PickList() With {.ColumnMode = True}
    Private ReadOnly _dirs As New PickList()
    Private _focus As Area = Area.FileName
    Private _typed As String = ""
    Private _typedSelected As Boolean
    Private _message As String = ""
    Private _columnPitch As Integer = 14

    ' screen positions of the parts, filled in while drawing (used for mouse clicks)
    Private _filesLeft, _filesRight, _dirsLeft, _dirsRight, _listTop, _filesBottom As Integer
    Private _fieldLeft, _fieldRight, _fieldRow As Integer
    Private _buttonRow, _okLeft, _cancelLeft, _helpLeft As Integer

    Public Sub New()
      Me.ForegroundColor = ConsoleColor.Black
      Me.BackgroundColor = ConsoleColor.Gray
      Me.Title = "Open"
      Me.Pattern = "*.*"
    End Sub

    Public Property Title As String
    Public Property Mode As FileDialogMode

    ''' <summary>Wildcard patterns separated by ';', for example "*.bas;*.vb".</summary>
    ''' <summary>What the Help button does (nothing: it says there is no help).</summary>
    Public Property OnHelp As Action

    Public Property Pattern As String

    ''' <summary>Appended to a typed file name without an extension in Save mode, for example ".bas".</summary>
    Public Property DefaultExtension As String

    ''' <summary>A file name the Save box starts with (selected, so typing replaces it); Nothing for an empty box.</summary>
    Public Property SuggestedName As String

    Public ReadOnly Property CurrentDirectory As String
      Get
        Return Me._directory
      End Get
    End Property

    Public ReadOnly Property IsOpen As Boolean
      Get
        Return Me._screen IsNot Nothing
      End Get
    End Property

    ''' <summary>The names in the Files list.</summary>
    Public ReadOnly Property FileNames As IReadOnlyList(Of String)
      Get
        Return Me._files.Items.Select(Function(e) e.Name).ToList()
      End Get
    End Property

    ''' <summary>The entries of the Dirs/Drives list as shown (".." , folders, then drives such as [-C-]).</summary>
    Public ReadOnly Property DirectoryNames As IReadOnlyList(Of String)
      Get
        Return Me._dirs.Items.Select(Function(e) e.Display).ToList()
      End Get
    End Property

    Public ReadOnly Property SelectedFile As String
      Get
        Dim e = Me._files.Selected
        Return If(e Is Nothing, Nothing, e.Name)
      End Get
    End Property

    Public ReadOnly Property SelectedDirectory As String
      Get
        Dim e = Me._dirs.Selected
        Return If(e Is Nothing, Nothing, e.Display)
      End Get
    End Property

    ''' <summary>The text of the File Name box.</summary>
    Public ReadOnly Property TypedText As String
      Get
        Return Me._typed
      End Get
    End Property

    ''' <summary>The part that has the focus: FileName, Files, Dirs, Ok, Cancel or Help.</summary>
    Public ReadOnly Property FocusedPart As String
      Get
        Return Me._focus.ToString()
      End Get
    End Property

    ''' <summary>How many rows the lists have at the current window size (8 on an 80 x 25 screen).</summary>
    Public ReadOnly Property ListRows As Integer
      Get
        Return Me._files.Rows
      End Get
    End Property

    ''' <summary>How many columns of the Files list are visible at the current window size.</summary>
    Public ReadOnly Property VisibleFileColumns As Integer
      Get
        Return Me._files.VisibleColumns
      End Get
    End Property

    Public Overrides ReadOnly Property StatusHint As String
      Get
        Return Screen.DialogStatusHint
      End Get
    End Property

    Public ReadOnly Property Message As String
      Get
        Return Me._message
      End Get
    End Property

    ''' <summary>Opens the dialog in <paramref name="startDirectory"/> (the current folder when empty or missing).</summary>
    Public Sub Show(screen As Screen, startDirectory As String, onAccept As Action(Of String), Optional onCancel As Action = Nothing)

      Me._screen = screen
      Me._onAccept = onAccept
      Me._onCancel = onCancel
      Me._message = ""
      Me._focus = Area.FileName

      If String.IsNullOrEmpty(startDirectory) OrElse Not Directory.Exists(startDirectory) Then
        startDirectory = Environment.CurrentDirectory
      End If

      Me.Navigate(Path.GetFullPath(startDirectory), Nothing)

      If Me.Mode = FileDialogMode.Save AndAlso Not String.IsNullOrEmpty(Me.SuggestedName) Then
        Me._typed = Me.SuggestedName
        Me._typedSelected = True
      End If

      Me._previousPopup = screen.PopupControl
      screen.Controls.Add(Me)
      screen.PopupControl = Me
      screen.HideCursor()
      screen.Invalidate()

    End Sub

    Private Sub Close()
      Dim screen = Me._screen
      Me._screen = Nothing
      screen.Controls.Remove(Me)
      screen.PopupControl = Me._previousPopup
      screen.ShowCursor()
      screen.Invalidate()
    End Sub

    Private Sub Accept(path As String)
      Dim callback = Me._onAccept
      Me.Close()
      If callback IsNot Nothing Then callback(path)
    End Sub

    Private Sub Cancel()
      Dim callback = Me._onCancel
      Me.Close()
      If callback IsNot Nothing Then callback()
    End Sub

    ' ---- navigation

    Private Sub ShowPatternInFileNameBox()
      If Me.Mode = FileDialogMode.Save Then
        Me._typed = ""
        Me._typedSelected = False
      Else
        Me._typed = Me.Pattern
        Me._typedSelected = True
      End If
    End Sub

    ''' <summary>Shows <paramref name="directory"/> ("" = the drives); <paramref name="select"/> is the folder to select in the Dirs list.</summary>
    Private Function Navigate(directory As String, [select] As String) As Boolean

      Try

        Dim entries = FileListing.Read(directory, Me.Pattern)

        Dim dirs = entries.Where(Function(e) e.Kind <> FileEntryKind.File).ToList()

        ' Drives are always reachable from the Dirs/Drives list.
        If directory.Length > 0 AndAlso OperatingSystem.IsWindows() Then
          dirs.AddRange(FileListing.Read("").Where(Function(e) e.Kind = FileEntryKind.Drive))
        End If

        Me._files.Items = If(Me.Mode = FileDialogMode.Open, entries.Where(Function(e) e.Kind = FileEntryKind.File).ToList(), New List(Of FileEntry)())
        Me._dirs.Items = dirs
        Me._directory = directory
        Me._files.Reset()
        Me._dirs.Reset()
        Me._message = ""
        Me.ShowPatternInFileNameBox()

        If [select] IsNot Nothing Then
          Dim i = dirs.FindIndex(Function(e) String.Equals(e.Path, [select], StringComparison.OrdinalIgnoreCase))
          If i >= 0 Then Me._dirs.MoveTo(i)
        End If

        Return True

      Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
        Me._message = ex.Message
        Return False
      End Try

    End Function

    Private Sub GoUp()
      If String.IsNullOrEmpty(Me._directory) Then Return
      Dim parent = FileListing.ParentOf(Me._directory)
      If String.Equals(parent, Me._directory, StringComparison.OrdinalIgnoreCase) Then Return
      Me.Navigate(parent, Me._directory)
    End Sub

    Private Sub EnterDirectory(entry As FileEntry)
      If entry Is Nothing Then Return
      If entry.Kind = FileEntryKind.Up Then
        Me.GoUp()
      Else
        Me.Navigate(entry.Path, Nothing)
      End If
    End Sub

    ''' <summary>Selecting a file puts its name in the File Name box.</summary>
    Private Sub SelectFile(index As Integer)
      Me._files.MoveTo(index)
      Dim e = Me._files.Selected
      If e IsNot Nothing Then
        Me._typed = e.Name
        Me._typedSelected = True
      End If
    End Sub

    ''' <summary>Typing selects the first file whose name starts with the typed text.</summary>
    Private Sub SyncFilesToTypedText()
      If Me._typed.Length = 0 OrElse Me._typed.IndexOfAny({"*"c, "?"c}) >= 0 Then Return
      Dim i = Me._files.Items.FindIndex(Function(e) e.Name.StartsWith(Me._typed, StringComparison.OrdinalIgnoreCase))
      If i >= 0 Then Me._files.MoveTo(i)
    End Sub

    Private Sub SetFocus(part As Area)
      Me._focus = part
      If part = Area.Files Then
        Dim e = Me._files.Selected
        If e IsNot Nothing Then
          Me._typed = e.Name
          Me._typedSelected = True
        End If
      End If
    End Sub

    Private Sub MoveFocus(forward As Boolean)
      Dim i = Array.IndexOf(TabOrder, Me._focus)
      i = (i + If(forward, 1, TabOrder.Length - 1)) Mod TabOrder.Length
      Me.SetFocus(TabOrder(i))
    End Sub

    ''' <summary>OK / Enter: act on the File Name box.</summary>
    Private Sub Commit()

      If Me._typed.Length = 0 Then
        Me._message = "Enter a file name."
        Return
      End If

      If Me._typed.IndexOfAny({"*"c, "?"c}) >= 0 Then
        Me.Pattern = Me._typed
        Dim dir = Me._directory
        Me.Navigate(dir, Nothing)
        Me._typed = Me.Pattern
        Return
      End If

      Dim candidate As String = Nothing
      Try
        If Path.IsPathRooted(Me._typed) Then
          candidate = Path.GetFullPath(Me._typed)
        ElseIf Me._directory.Length > 0 Then
          candidate = Path.GetFullPath(Path.Combine(Me._directory, Me._typed))
        End If
      Catch ex As Exception When TypeOf ex Is ArgumentException OrElse TypeOf ex Is NotSupportedException OrElse TypeOf ex Is PathTooLongException
        Me._message = "Invalid file name."
        Return
      End Try

      If candidate IsNot Nothing Then

        If Directory.Exists(candidate) Then
          Me.Navigate(candidate, Nothing)
          Return
        End If

        If File.Exists(candidate) Then
          Me.Accept(candidate)
          Return
        End If

      End If

      If Me.Mode = FileDialogMode.Open Then
        ' Not an exact name: use the file the typed text selected in the list.
        Dim selected = Me._files.Selected
        If selected IsNot Nothing AndAlso selected.Name.StartsWith(Me._typed, StringComparison.OrdinalIgnoreCase) Then
          Me.Accept(selected.Path)
        Else
          Me._message = "File not found."
        End If
      ElseIf candidate IsNot Nothing Then
        If Path.GetExtension(candidate).Length = 0 AndAlso Not String.IsNullOrEmpty(Me.DefaultExtension) Then
          candidate &= Me.DefaultExtension
        End If
        Me.Accept(candidate)
      End If

    End Sub

    Private Sub PressButton(part As Area)
      Select Case part
        Case Area.Ok : Me.Commit()
        Case Area.Cancel : Me.Cancel()
        Case Area.Help
          If Me.OnHelp IsNot Nothing Then Me.OnHelp.Invoke() Else Me._message = "Help is not available yet."
      End Select
    End Sub

    ' ---- input

    Private Shared Function IsPrintable(e As KeyEvent) As Boolean
      Dim modified = (e.ControlKeyState And (ControlKeyState.LEFT_CTRL_PRESSED Or ControlKeyState.RIGHT_CTRL_PRESSED Or ControlKeyState.LEFT_ALT_PRESSED Or ControlKeyState.RIGHT_ALT_PRESSED)) <> 0
      Return Not modified AndAlso e.Character >= " "c AndAlso e.Character <> ChrW(127)
    End Function

    Private Sub TypeCharacter(ch As Char)
      Me._focus = Area.FileName
      If Me._typedSelected Then
        Me._typed = ""
        Me._typedSelected = False
      End If
      Me._typed &= ch
      Me.SyncFilesToTypedText()
    End Sub

    ''' <summary>Moves a list's selection. The Files list also moves a whole column with Left / Right.</summary>
    Private Sub Move(list As PickList, key As VirtualKey, isFiles As Boolean)

      Dim target = list.Cursor
      Dim page = If(list.ColumnMode, list.Rows * Math.Max(1, list.VisibleColumns), list.Rows)

      Select Case key
        Case VirtualKey.VK_UP : target -= 1
        Case VirtualKey.VK_DOWN : target += 1
        Case VirtualKey.VK_LEFT : target -= list.Rows
        Case VirtualKey.VK_RIGHT : target += list.Rows
        Case VirtualKey.VK_PRIOR : target -= page
        Case VirtualKey.VK_NEXT : target += page
        Case VirtualKey.VK_HOME : target = 0
        Case VirtualKey.VK_END : target = list.Items.Count - 1
      End Select

      ' Left from the first column / Right from the last stay where they are.
      If key = VirtualKey.VK_LEFT AndAlso target < 0 Then Return

      If isFiles Then
        Me.SelectFile(target)
      Else
        list.MoveTo(target)
      End If

    End Sub

    Private Shared Function IsListKey(key As VirtualKey) As Boolean
      Return key = VirtualKey.VK_UP OrElse key = VirtualKey.VK_DOWN OrElse key = VirtualKey.VK_PRIOR OrElse key = VirtualKey.VK_NEXT OrElse key = VirtualKey.VK_HOME OrElse key = VirtualKey.VK_END
    End Function

    Public Overrides Sub OnBeforeKeyDown(sender As Object, e As KeyEvent)

      ' Modal: nothing below the dialog sees keys while it is open.
      e.Handled = True

      If Me._screen Is Nothing Then Return

      Me._message = ""

      Dim key = e.VirtualKey
      Dim sideways = key = VirtualKey.VK_LEFT OrElse key = VirtualKey.VK_RIGHT

      If key = VirtualKey.VK_ESCAPE Then
        Me.Cancel()
        Return
      End If

      If key = VirtualKey.VK_TAB Then
        Me.MoveFocus((e.ControlKeyState And ControlKeyState.SHIFT_PRESSED) = 0)
      ElseIf key = VirtualKey.VK_RETURN Then
        Select Case Me._focus
          Case Area.Dirs : Me.EnterDirectory(Me._dirs.Selected)
          Case Area.Cancel, Area.Help : Me.PressButton(Me._focus)
          Case Else : Me.Commit()
        End Select
      ElseIf key = VirtualKey.VK_SPACE AndAlso Me._focus >= Area.Ok Then
        Me.PressButton(Me._focus)
      ElseIf IsListKey(key) Then
        Select Case Me._focus
          Case Area.Dirs : Me.Move(Me._dirs, key, False)
          Case Area.Files, Area.FileName : If Me.Mode = FileDialogMode.Open Then Me.Move(Me._files, key, True)
        End Select
      ElseIf sideways AndAlso Me._focus = Area.Files Then
        Me.Move(Me._files, key, True)
      ElseIf sideways AndAlso Me._focus >= Area.Ok Then
        Dim i = Array.IndexOf(TabOrder, Me._focus) + If(key = VirtualKey.VK_RIGHT, 1, -1)
        If i >= Array.IndexOf(TabOrder, Area.Ok) AndAlso i < TabOrder.Length Then Me._focus = TabOrder(i)
      ElseIf key = VirtualKey.VK_BACK Then
        If Me._focus = Area.FileName OrElse Me._focus = Area.Files Then
          Me._focus = Area.FileName
          If Me._typedSelected Then
            Me._typed = ""
            Me._typedSelected = False
          ElseIf Me._typed.Length > 0 Then
            Me._typed = Me._typed.Substring(0, Me._typed.Length - 1)
          End If
          Me.SyncFilesToTypedText()
        Else
          Me.GoUp()
        End If
      ElseIf IsPrintable(e) Then
        Me.TypeCharacter(e.Character)
      End If

      If Me._screen IsNot Nothing Then Me._screen.Invalidate()

    End Sub

    Public Overrides Sub OnKeyUp(sender As Object, e As KeyEvent)
      e.Handled = True
    End Sub

    Public Overrides Sub OnMouseDown(sender As Object, e As MouseEvent)

      If Me._screen Is Nothing Then Return

      Dim x = CInt(e.X)
      Dim y = CInt(e.Y)

      Me._message = ""

      If y = Me._buttonRow Then
        If x >= Me._okLeft AndAlso x < Me._okLeft + OkLabel.Length Then
          Me.SetFocus(Area.Ok)
          Me.PressButton(Area.Ok)
        ElseIf x >= Me._cancelLeft AndAlso x < Me._cancelLeft + CancelLabel.Length Then
          Me.SetFocus(Area.Cancel)
          Me.PressButton(Area.Cancel)
        ElseIf x >= Me._helpLeft AndAlso x < Me._helpLeft + HelpLabel.Length Then
          Me.SetFocus(Area.Help)
          Me.PressButton(Area.Help)
        End If
      ElseIf y >= Me._fieldRow - 1 AndAlso y <= Me._fieldRow + 1 AndAlso x >= Me._fieldLeft AndAlso x <= Me._fieldRight Then
        Me.SetFocus(Area.FileName)
      ElseIf y = Me._filesBottom AndAlso x > Me._filesLeft AndAlso x < Me._filesRight Then
        ' the Files list's horizontal scroll bar: its arrows scroll a column at a time
        Dim files = Me._files
        If x = Me._filesLeft + 1 Then
          files.TopColumn = Math.Max(0, files.TopColumn - 1)
          files.FollowCursor = False
        ElseIf x = Me._filesRight - 1 Then
          files.TopColumn = Math.Max(0, Math.Min(files.TotalColumns - files.VisibleColumns, files.TopColumn + 1))
          files.FollowCursor = False
        End If
      ElseIf y >= Me._listTop AndAlso y < Me._listTop + Me._files.Rows Then
        Dim row = y - Me._listTop
        If x >= Me._filesLeft + 2 AndAlso x < Me._filesRight Then
          Dim column = (x - (Me._filesLeft + 2)) \ Me._columnPitch
          Dim index = (Me._files.TopColumn + column) * Me._files.Rows + row
          If column < Me._files.VisibleColumns AndAlso index < Me._files.Items.Count Then
            Dim already = Me._focus = Area.Files AndAlso index = Me._files.Cursor
            Me._focus = Area.Files
            Me.SelectFile(index)
            If already Then Me.Commit()
          End If
        ElseIf x > Me._dirsLeft AndAlso x < Me._dirsRight Then
          Dim index = Me._dirs.Top + row
          If index < Me._dirs.Items.Count Then
            Dim already = Me._focus = Area.Dirs AndAlso index = Me._dirs.Cursor
            Me._focus = Area.Dirs
            Me._dirs.MoveTo(index)
            If already Then Me.EnterDirectory(Me._dirs.Selected)
          End If
        End If
      End If

      If Me._screen IsNot Nothing Then Me._screen.Invalidate()

    End Sub

    ' ---- drawing

    Private Shared Function Tail(text As String, width As Integer) As String
      If width <= 0 Then Return ""
      If text.Length <= width Then Return text
      If width <= 3 Then Return text.Substring(text.Length - width)
      Return "..." & text.Substring(text.Length - (width - 3))
    End Function

    Private Shared Function Fit(text As String, width As Integer) As String
      If width <= 0 Then Return ""
      Return If(text.Length > width, text.Substring(0, width), text.PadRight(width))
    End Function

    ''' <summary>A name cut to <paramref name="width"/> with an ellipsis when it does not fit.</summary>
    Private Shared Function Shorten(text As String, width As Integer) As String
      If text.Length <= width Then Return text
      If width <= 1 Then Return text.Substring(0, Math.Max(0, width))
      Return text.Substring(0, width - 1) & ChrW(&H2026)
    End Function

    ''' <summary>
    ''' Sizes the dialog from the window. Open is QBasic's 67 x 20 on 80 x 25, Save As its 42 x 18; both grow with larger windows.
    ''' </summary>
    Private Function LayoutInScreen(screen As Screen) As Boolean

      Dim save = Me.Mode = FileDialogMode.Save
      Dim chrome = If(save, SaveFixedRows, FixedRows)

      Me.Width = screen.Width - If(save, 38, 13)
      Me.Height = Math.Min(screen.Height - If(save, 7, 5), 60 + chrome)

      If Me.Width < MinWidth OrElse Me.Height < chrome + MinListRows Then Return False

      Me.Left = (screen.Width - Me.Width) \ 2
      Me.Top = (screen.Height - Me.Height) \ 2

      Dim rows = Me.Height - chrome
      Me._files.Rows = rows
      Me._dirs.Rows = rows

      Return True

    End Function

    Private Sub DrawDirsList(screen As Screen, left As Integer, right As Integer, focused As Boolean)

      Dim fg = Me.ForegroundColor
      Dim bg = Me.BackgroundColor
      Dim list = Me._dirs
      Dim rows = list.Rows

      list.MoveTo(list.Cursor)

      screen.DrawBox(left, Me._listTop - 1, right, Me._listTop + rows, fg, bg)

      Dim textWidth = right - left - 2

      For r = 0 To rows - 1
        Dim index = list.Top + r
        Dim y = Me._listTop + r
        If index < list.Items.Count Then
          Dim selected = index = list.Cursor
          Dim shown = Fit(Shorten(list.Items(index).Display, textWidth), textWidth)
          screen.DrawString(left + 2, y, shown, If(selected, If(focused, ConsoleColor.White, ConsoleColor.Gray), fg), If(selected, ConsoleColor.Black, bg))
        End If
      Next

      ' The vertical scroll bar is the right border: arrows at both ends, a thumb between.
      screen.DrawUpArrow(right, Me._listTop, fg, bg)
      screen.DrawDownArrow(right, Me._listTop + rows - 1, fg, bg)
      For y = Me._listTop + 1 To Me._listTop + rows - 2
        screen.DrawLightShade(right, y, fg, bg)
      Next
      If rows >= 3 Then
        Dim fraction = If(list.Items.Count > 1, CDbl(list.Cursor) / (list.Items.Count - 1), 0.0)
        screen.DrawBlock(right, Utility.GetRangeValue(Me._listTop + 1, Me._listTop + rows - 2, fraction), fg, bg)
      End If

    End Sub

    Private Sub DrawFilesList(screen As Screen, focused As Boolean)

      Dim fg = Me.ForegroundColor
      Dim bg = Me.BackgroundColor
      Dim list = Me._files
      Dim rows = list.Rows
      Dim left = Me._filesLeft
      Dim right = Me._filesRight

      ' Columns are as wide as the longest name plus two spaces (and never wider than the box), as many as fit.
      Dim usable = right - left - 2
      Dim longest = If(list.Items.Count = 0, 8, list.Items.Max(Function(e) e.Name.Length))
      Me._columnPitch = Math.Max(4, Math.Min(longest + 2, usable))
      list.VisibleColumns = Math.Max(1, usable \ Me._columnPitch)
      list.Normalize()

      screen.DrawBox(left, Me._listTop - 1, right, Me._filesBottom, fg, bg)

      For c = 0 To list.VisibleColumns - 1
        For r = 0 To rows - 1
          Dim index = (list.TopColumn + c) * rows + r
          If index >= list.Items.Count Then Exit For
          Dim selected = index = list.Cursor
          Dim name = Shorten(list.Items(index).Name, Me._columnPitch - 1)
          Dim shown = If(selected, name.PadRight(Math.Min(Me._columnPitch - 1, usable)), name)
          screen.DrawString(left + 2 + c * Me._columnPitch, Me._listTop + r, shown,
                            If(selected, If(focused, ConsoleColor.White, ConsoleColor.Gray), fg), If(selected, ConsoleColor.Black, bg))
        Next
      Next

      ' The horizontal scroll bar replaces the bottom border: arrows at both ends, a thumb between.
      screen.DrawLeftArrow(left + 1, Me._filesBottom, fg, bg)
      screen.DrawRightArrow(right - 1, Me._filesBottom, fg, bg)
      For x = left + 2 To right - 2
        screen.DrawLightShade(x, Me._filesBottom, fg, bg)
      Next
      If right - left >= 5 Then
        Dim totalColumns = list.TotalColumns
        Dim fraction = If(totalColumns > 1, CDbl(list.Cursor \ rows) / (totalColumns - 1), 0.0)
        screen.DrawBlock(Utility.GetRangeValue(left + 2, right - 2, fraction), Me._filesBottom, fg, bg)
      End If

    End Sub

    Private Sub DrawButton(screen As Screen, left As Integer, text As String, part As Area)
      Dim focused = Me._focus = part
      If focused Then
        screen.DrawString(left, Me._buttonRow, text, ConsoleColor.White, ConsoleColor.Black)
      Else
        screen.DrawString(left, Me._buttonRow, text, Me.ForegroundColor, Me.BackgroundColor)
        ' QBasic draws the angle brackets in bright white.
        screen.DrawString(left, Me._buttonRow, "<", ConsoleColor.White, Me.BackgroundColor)
        screen.DrawString(left + text.Length - 1, Me._buttonRow, ">", ConsoleColor.White, Me.BackgroundColor)
      End If
    End Sub

    Public Overrides Sub Render(screen As Screen)

      If Not Me.Visible Then Return

      ' Too small to lay out (the keyboard still works): just say so.
      If Not Me.LayoutInScreen(screen) Then
        screen.DrawString(0, 0, Fit(" Window too small", screen.Width), ConsoleColor.Black, ConsoleColor.Gray)
        Return
      End If

      Dim fg = Me.ForegroundColor
      Dim bg = Me.BackgroundColor
      Dim left = Me.Left
      Dim right = Me.Right
      Dim top = Me.Top
      Dim rows = Me._files.Rows

      screen.DrawShadow(left + 2, top + 1, right + 2, Me.Bottom + 1)
      screen.DrawBoxFill(left, top, right, Me.Bottom, fg, bg)
      screen.DrawBox(left, top, right, Me.Bottom, fg, bg)

      Dim title = " " & Me.Title & " "
      screen.DrawString(left + (Me.Width - title.Length) \ 2, top, title, fg, bg)

      ' File Name box
      Const label As String = "File Name:"
      Me._fieldRow = top + 2
      Me._fieldLeft = left + 2 + label.Length + 1
      Me._fieldRight = right - 2
      screen.DrawString(left + 2, Me._fieldRow, label, fg, bg)
      screen.DrawBox(Me._fieldLeft, top + 1, Me._fieldRight, top + 3, fg, bg)
      Dim fieldWidth = Me._fieldRight - Me._fieldLeft - 1
      Dim shown = Me._typed
      If shown.Length >= fieldWidth Then shown = shown.Substring(shown.Length - fieldWidth + 1)
      If Me._typedSelected AndAlso Me._typed.Length > 0 Then
        screen.DrawString(Me._fieldLeft + 1, Me._fieldRow, Fit(shown, fieldWidth), ConsoleColor.White, ConsoleColor.Black)
      Else
        screen.DrawString(Me._fieldLeft + 1, Me._fieldRow, Fit(shown & If(Me._focus = Area.FileName, "_", ""), fieldWidth), fg, bg)
      End If

      ' current path (or the latest message)
      Dim inner = Me.Width - 4
      If Me._message.Length > 0 Then
        screen.DrawString(left + 2, top + 4, Fit(Me._message, inner), ConsoleColor.DarkRed, bg)
      Else
        screen.DrawString(left + 2, top + 4, Fit(Tail(If(Me._directory.Length = 0, "(drives)", Me._directory), inner), inner), fg, bg)
      End If

      Dim innerTotal = Me.Width - 3
      Me._listTop = top + 7

      If Me.Mode = FileDialogMode.Open Then

        ' The Files list takes most of the width, the Dirs/Drives list about a quarter.
        Dim dirsWidth = Math.Max(14, (innerTotal + 3) \ 4)
        Me._filesBottom = Me._listTop + rows
        Me._filesLeft = left + 2
        Me._dirsRight = right - 2
        Me._dirsLeft = Me._dirsRight - dirsWidth + 1
        Me._filesRight = Me._dirsLeft - 3

        screen.DrawString(Me._filesLeft + (Me._filesRight - Me._filesLeft + 1 - 5) \ 2, top + 5, "Files", fg, bg)
        screen.DrawString(Me._dirsLeft + (dirsWidth - 11) \ 2, top + 5, "Dirs/Drives", fg, bg)

        Me.DrawFilesList(screen, Me._focus = Area.Files)

      Else

        ' Save As: only the Dirs/Drives list, centred.
        Dim dirsWidth = Math.Max(14, (innerTotal * 2 + 2) \ 5)
        Me._dirsLeft = left + (Me.Width - dirsWidth) \ 2
        Me._dirsRight = Me._dirsLeft + dirsWidth - 1
        Me._filesLeft = -1
        Me._filesRight = -1
        Me._filesBottom = -1

        screen.DrawString(Me._dirsLeft + (dirsWidth - 11) \ 2, top + 5, "Dirs/Drives", fg, bg)

      End If

      Me.DrawDirsList(screen, Me._dirsLeft, Me._dirsRight, Me._focus = Area.Dirs)

      ' separator and buttons: OK, Cancel and Help spread evenly, Cancel in the middle
      Dim separatorRow = Me.Bottom - 2
      screen.DrawLeftTee(left, separatorRow, fg, bg)
      screen.DrawHorizontalLine(left + 1, right - 1, separatorRow, fg, bg)
      screen.DrawRightTee(right, separatorRow, fg, bg)

      Me._buttonRow = Me.Bottom - 1
      Dim centre = left + Me.Width \ 2
      Dim spread = If(Me.Mode = FileDialogMode.Save, (Me.Width - 4) \ 3, (Me.Width - 10) \ 3)
      Me._cancelLeft = centre - CancelLabel.Length \ 2
      Me._okLeft = centre - spread - OkLabel.Length \ 2
      Me._helpLeft = centre + spread - HelpLabel.Length \ 2

      Me.DrawButton(screen, Me._okLeft, OkLabel, Area.Ok)
      Me.DrawButton(screen, Me._cancelLeft, CancelLabel, Area.Cancel)
      Me.DrawButton(screen, Me._helpLeft, HelpLabel, Area.Help)

    End Sub

  End Class

End Namespace
