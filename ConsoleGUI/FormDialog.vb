' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Namespace ConsoleGUI

  ''' <summary>How wide a <see cref="FormDialog"/> is on an 80 column window; larger windows scale it up.</summary>
  Public Enum FormDialogSize
    ''' <summary>As wide as its content needs.</summary>
    Content
    ''' <summary>About half the window (39 columns on 80): small input boxes.</summary>
    Half
    ''' <summary>Window width minus 20 (60 columns on 80): Find and Change.</summary>
    Medium
    ''' <summary>Window width minus 7 (73 columns on 80): lists.</summary>
    Wide
    ''' <summary>Window width minus 12 (68 columns on 80): several labelled input boxes.</summary>
    Large
  End Enum

  ''' <summary>
  ''' A QBasic style dialog built from a few parts: text, single-line input boxes, check boxes, a list box, a group of radio buttons, and a row of
  ''' buttons (OK / Cancel / Help, Yes / No / Cancel / Help ...) spread evenly along the bottom under a separator.
  ''' Tab / Shift+Tab move between the inputs, lists, check boxes, radio groups and buttons; Enter presses the default button (or the focused one);
  ''' Esc presses the Cancel button; Left / Right move between buttons; Space toggles a check box or presses a button; a button's hot key presses it
  ''' when no input has the focus. Build it with AddText / AddInput / AddChecks / AddList / AddRadios / AddButton and call <see cref="Show"/>.
  ''' Button actions run after the dialog has closed, so they can open another dialog.
  ''' </summary>
  Public Class FormDialog
    Inherits Control

    ' ---- parts

    Public MustInherit Class Part
      Friend MustOverride ReadOnly Property Rows As Integer
      Friend MustOverride ReadOnly Property MinWidth As Integer
      Friend Overridable ReadOnly Property Focusable As Boolean
        Get
          Return False
        End Get
      End Property
    End Class

    Public NotInheritable Class TextPart
      Inherits Part

      Public Property Text As String
      Public ReadOnly Property Centered As Boolean

      Public Sub New(text As String, centered As Boolean)
        Me.Text = text
        Me.Centered = centered
      End Sub

      Friend Overrides ReadOnly Property Rows As Integer
        Get
          Return 1
        End Get
      End Property

      Friend Overrides ReadOnly Property MinWidth As Integer
        Get
          Return Me.Text.Length + 3
        End Get
      End Property

    End Class

    ''' <summary>A labelled single-line text box (three rows with its frame). The text starts out selected: typing replaces it.</summary>
    Public NotInheritable Class InputPart
      Inherits Part

      Public ReadOnly Property Label As String
      Public ReadOnly Property SubLabel As String
      Public Property Text As String
      Public Property TextSelected As Boolean

      Friend Left, Right, Row As Integer

      Public Sub New(label As String, text As String, Optional subLabel As String = Nothing)
        Me.Label = label
        Me.SubLabel = subLabel
        Me.Text = text
        Me.TextSelected = text.Length > 0
      End Sub

      Friend Overrides ReadOnly Property Rows As Integer
        Get
          Return 3
        End Get
      End Property

      Friend Overrides ReadOnly Property MinWidth As Integer
        Get
          Return Me.Label.Length + 12
        End Get
      End Property

      Friend Overrides ReadOnly Property Focusable As Boolean
        Get
          Return True
        End Get
      End Property

    End Class

    ''' <summary>One "[ ] label" check box.</summary>
    Public NotInheritable Class CheckItem

      Public ReadOnly Property Label As String
      Public Property Checked As Boolean

      Friend Left, Row As Integer

      Public Sub New(label As String, checked As Boolean)
        Me.Label = label
        Me.Checked = checked
      End Sub

    End Class

    ''' <summary>Check boxes side by side on one row.</summary>
    Public NotInheritable Class CheckRowPart
      Inherits Part

      Public ReadOnly Property Items As IReadOnlyList(Of CheckItem)

      Public Sub New(items As IEnumerable(Of CheckItem))
        Me.Items = items.ToList()
      End Sub

      Friend Overrides ReadOnly Property Rows As Integer
        Get
          Return 1
        End Get
      End Property

      Friend Overrides ReadOnly Property MinWidth As Integer
        Get
          Return Me.Items.Sum(Function(i) i.Label.Length + 8) + 4
        End Get
      End Property

    End Class

    ''' <summary>A framed list of strings with a horizontal scroll bar along its bottom edge; Up / Down / Page keys change the selection.</summary>
    Public NotInheritable Class ListPart
      Inherits Part

      Public ReadOnly Property Items As List(Of String)
      Public Property SelectedIndex As Integer

      ''' <summary>The rows of the framed list at 80 x 25, including its frame.</summary>
      Public Property BaseRows As Integer = 11

      ''' <summary>When True the list gets the extra rows of a window taller than 25.</summary>
      Public Property GrowsWithWindow As Boolean = True

      ''' <summary>Called after the selection changes.</summary>
      Public Property SelectionChanged As Action

      Friend ExtraRows As Integer
      Friend Left, Right, Top As Integer
      Friend TopIndex As Integer

      Public Sub New(items As IEnumerable(Of String))
        Me.Items = items.ToList()
      End Sub

      Friend Overrides ReadOnly Property Rows As Integer
        Get
          Return Me.BaseRows + Me.ExtraRows
        End Get
      End Property

      Friend Overrides ReadOnly Property MinWidth As Integer
        Get
          Return 20
        End Get
      End Property

      Friend Overrides ReadOnly Property Focusable As Boolean
        Get
          Return True
        End Get
      End Property

      Public ReadOnly Property SelectedText As String
        Get
          Return If(Me.SelectedIndex >= 0 AndAlso Me.SelectedIndex < Me.Items.Count, Me.Items(Me.SelectedIndex), Nothing)
        End Get
      End Property

      ''' <summary>How many items fit inside the frame.</summary>
      Friend ReadOnly Property Visible As Integer
        Get
          Return Math.Max(1, Me.Rows - 2)
        End Get
      End Property

      Public Sub Select_(index As Integer)
        If Me.Items.Count = 0 Then Return
        Dim target = Math.Max(0, Math.Min(Me.Items.Count - 1, index))
        If target = Me.SelectedIndex Then Return
        Me.SelectedIndex = target
        Me.KeepVisible()
        If Me.SelectionChanged IsNot Nothing Then Me.SelectionChanged.Invoke()
      End Sub

      Friend Sub KeepVisible()
        If Me.SelectedIndex < Me.TopIndex Then Me.TopIndex = Me.SelectedIndex
        If Me.SelectedIndex >= Me.TopIndex + Me.Visible Then Me.TopIndex = Me.SelectedIndex - Me.Visible + 1
        Me.TopIndex = Math.Max(0, Math.Min(Me.TopIndex, Math.Max(0, Me.Items.Count - Me.Visible)))
      End Sub

    End Class

    ''' <summary>A group of "( )" options, one of which is selected; Up / Down change the selection.</summary>
    Public NotInheritable Class RadioPart
      Inherits Part

      Public ReadOnly Property Options As String()
      Public Property SelectedIndex As Integer

      Friend Left, FirstRow As Integer

      Public Sub New(options As String(), selectedIndex As Integer)
        Me.Options = options
        Me.SelectedIndex = selectedIndex
      End Sub

      Friend Overrides ReadOnly Property Rows As Integer
        Get
          Return Me.Options.Length
        End Get
      End Property

      Friend Overrides ReadOnly Property MinWidth As Integer
        Get
          Return Me.Options.Max(Function(o) o.Length) + 16
        End Get
      End Property

      Friend Overrides ReadOnly Property Focusable As Boolean
        Get
          Return True
        End Get
      End Property

      Public ReadOnly Property SelectedText As String
        Get
          Return Me.Options(Me.SelectedIndex)
        End Get
      End Property

    End Class

    ''' <summary>A check box on the left and, on the right, a heading with a group of radio buttons under it (Make EXE File).</summary>
    Public NotInheritable Class OptionsPart
      Inherits Part

      Public ReadOnly Property Check As CheckItem
      Public ReadOnly Property Heading As String
      Public ReadOnly Property Radios As RadioPart

      Public Sub New(check As CheckItem, heading As String, radios As RadioPart)
        Me.Check = check
        Me.Heading = heading
        Me.Radios = radios
      End Sub

      Friend Overrides ReadOnly Property Rows As Integer
        Get
          Return 1 + Me.Radios.Options.Length
        End Get
      End Property

      Friend Overrides ReadOnly Property MinWidth As Integer
        Get
          Return Me.Check.Label.Length + Me.Radios.Options.Max(Function(o) o.Length) + 24
        End Get
      End Property

    End Class

    Private NotInheritable Class DialogButton

      Public Text As String
      Public Hotkey As Char
      Public AfterClose As Action
      Public Validate As Func(Of Boolean)
      Public Closes As Boolean = True
      Public IsCancel As Boolean
      Public IsDefault As Boolean
      Public Left As Integer

    End Class

    ' ---- state

    Private ReadOnly _parts As New List(Of Part)()
    Private ReadOnly _buttons As New List(Of DialogButton)()
    Private _focusIndex As Integer     ' index into the focus ring (focusable parts and check boxes, then the buttons)
    Private _message As String = ""

    Private _screen As Screen
    Private _previousPopup As Control
    Private _buttonRow As Integer

    Public Sub New()
      Me.ForegroundColor = ConsoleColor.Black
      Me.BackgroundColor = ConsoleColor.Gray
      Me.TopPadding = 1
      Me.BottomPadding = 1
    End Sub

    Public Property Title As String

    ''' <summary>Blank rows above the content (an input box sits right under the title, so it uses 0).</summary>
    Public Property TopPadding As Integer

    ''' <summary>Blank rows below the content; the last one shows <see cref="Message"/>.</summary>
    Public Property BottomPadding As Integer

    Public Property Size As FormDialogSize

    ''' <summary>Columns from the frame to labels and left-aligned text (QBasic uses 2, the Set Paths dialog 3).</summary>
    Public Property LabelIndent As Integer = 2

    ''' <summary>The dialog is never narrower than this many columns (message boxes are wider than their text).</summary>
    Public Property MinimumWidth As Integer

    Public ReadOnly Property IsOpen As Boolean
      Get
        Return Me._screen IsNot Nothing
      End Get
    End Property

    ''' <summary>Text shown in the last padding row (for example a validation error); cleared by the next key.</summary>
    Public Property Message As String
      Get
        Return Me._message
      End Get
      Set(value As String)
        Me._message = If(value, "")
      End Set
    End Property

    Public Overrides ReadOnly Property StatusHint As String
      Get
        Return Screen.DialogStatusHint
      End Get
    End Property

    ''' <summary>The part with the focus: "Input", "List", "Radios", "Check:label" or the focused button's text.</summary>
    Public ReadOnly Property FocusedPart As String
      Get
        Dim item = Me.FocusedItem()
        If TypeOf item Is InputPart Then Return "Input"
        If TypeOf item Is ListPart Then Return "List"
        If TypeOf item Is RadioPart Then Return "Radios"
        If TypeOf item Is CheckItem Then Return "Check:" & DirectCast(item, CheckItem).Label
        If TypeOf item Is DialogButton Then Return DirectCast(item, DialogButton).Text
        Return ""
      End Get
    End Property

    ' ---- building

    Public Sub Clear()
      Me._parts.Clear()
      Me._buttons.Clear()
      Me._message = ""
      Me._focusIndex = 0
    End Sub

    Public Function AddText(text As String, Optional centered As Boolean = True) As TextPart
      Dim part As New TextPart(text, centered)
      Me._parts.Add(part)
      Return part
    End Function

    Public Function AddInput(label As String, Optional text As String = "", Optional subLabel As String = Nothing) As InputPart
      Dim part As New InputPart(label, text, subLabel)
      Me._parts.Add(part)
      Return part
    End Function

    ''' <summary>Check boxes side by side on one row.</summary>
    Public Function AddChecks(ParamArray labels As String()) As CheckRowPart
      Dim part As New CheckRowPart(labels.Select(Function(l) New CheckItem(l, False)))
      Me._parts.Add(part)
      Return part
    End Function

    ''' <summary>A check box with a group of radio buttons beside it (see <see cref="OptionsPart"/>).</summary>
    Public Function AddOptions(checkLabel As String, heading As String, selectedIndex As Integer, options As String()) As OptionsPart
      Dim part As New OptionsPart(New CheckItem(checkLabel, False), heading, New RadioPart(options, selectedIndex))
      Me._parts.Add(part)
      Return part
    End Function

    Public Function AddList(items As IEnumerable(Of String)) As ListPart
      Dim part As New ListPart(items)
      Me._parts.Add(part)
      Return part
    End Function

    Public Function AddRadios(selectedIndex As Integer, options As String()) As RadioPart
      Dim part As New RadioPart(options, selectedIndex)
      Me._parts.Add(part)
      Return part
    End Function

    ''' <param name="text">The label as drawn, for example "&lt; OK &gt;" or "&lt;Cancel&gt;".</param>
    ''' <param name="hotkey">Pressing this key (when no input has the focus) presses the button; use ControlChars.NullChar for none.</param>
    ''' <param name="afterClose">Runs after the dialog has closed (so it can open another one).</param>
    ''' <param name="validate">Runs before closing; returning False keeps the dialog open (set <see cref="Message"/> to say why).</param>
    ''' <param name="closes">False for a button, like Help, that leaves the dialog open (its action then runs at once).</param>
    Public Sub AddButton(text As String, Optional hotkey As Char = ControlChars.NullChar, Optional afterClose As Action = Nothing,
                         Optional validate As Func(Of Boolean) = Nothing, Optional closes As Boolean = True,
                         Optional isCancel As Boolean = False, Optional isDefault As Boolean = False)
      Me._buttons.Add(New DialogButton With {.Text = text, .Hotkey = Char.ToUpperInvariant(hotkey), .AfterClose = afterClose, .Validate = validate,
                                             .Closes = closes, .IsCancel = isCancel, .IsDefault = isDefault})
    End Sub

    Public Sub Show(screen As Screen)

      Me._screen = screen
      Me._message = ""

      ' Start on the first input / list / check box / radio group, otherwise on the default (or first) button.
      Dim ring = Me.FocusRing()
      Dim firstPart = ring.FindIndex(Function(f) TypeOf f IsNot DialogButton)
      If firstPart >= 0 Then
        Me._focusIndex = firstPart
      Else
        Dim defaultIndex = ring.FindIndex(Function(f) TypeOf f Is DialogButton AndAlso DirectCast(f, DialogButton).IsDefault)
        Me._focusIndex = Math.Max(0, defaultIndex)
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

    ' ---- behaviour

    Private Function FocusRing() As List(Of Object)
      Dim ring As New List(Of Object)()
      For Each part In Me._parts
        Dim checks = TryCast(part, CheckRowPart)
        Dim opts = TryCast(part, OptionsPart)
        If checks IsNot Nothing Then
          ring.AddRange(checks.Items)
        ElseIf opts IsNot Nothing Then
          ring.Add(opts.Check)
          ring.Add(opts.Radios)
        ElseIf part.Focusable Then
          ring.Add(part)
        End If
      Next
      ring.AddRange(Me._buttons)
      Return ring
    End Function

    Private Function FocusedItem() As Object
      Dim ring = Me.FocusRing()
      Return If(Me._focusIndex >= 0 AndAlso Me._focusIndex < ring.Count, ring(Me._focusIndex), Nothing)
    End Function

    Private Function DefaultButton() As DialogButton
      Return If(Me._buttons.FirstOrDefault(Function(b) b.IsDefault), Me._buttons.FirstOrDefault())
    End Function

    ''' <summary>The button drawn with bright brackets: the focused one, or the default one while something else has the focus.</summary>
    Private Function HighlightedButton() As DialogButton
      Dim item = TryCast(Me.FocusedItem(), DialogButton)
      Return If(item, Me.DefaultButton())
    End Function

    Private Sub Press(button As DialogButton)

      If button Is Nothing OrElse Me._screen Is Nothing Then Return

      If Not button.Closes Then
        If button.AfterClose IsNot Nothing Then button.AfterClose()
        Return
      End If

      If button.Validate IsNot Nothing AndAlso Not button.Validate() Then Return

      Dim after = button.AfterClose
      Me.Close()
      If after IsNot Nothing Then after()

    End Sub

    Private Sub MoveFocus(forward As Boolean)
      Dim count = Me.FocusRing().Count
      If count = 0 Then Return
      Me._focusIndex = (Me._focusIndex + If(forward, 1, count - 1)) Mod count
    End Sub

    Private Shared Function IsPrintable(e As KeyEvent) As Boolean
      Dim modified = (e.ControlKeyState And (ControlKeyState.LEFT_CTRL_PRESSED Or ControlKeyState.RIGHT_CTRL_PRESSED Or ControlKeyState.LEFT_ALT_PRESSED Or ControlKeyState.RIGHT_ALT_PRESSED)) <> 0
      Return Not modified AndAlso e.Character >= " "c AndAlso e.Character <> ChrW(127)
    End Function

    Public Overrides Sub OnBeforeKeyDown(sender As Object, e As KeyEvent)

      ' Modal: nothing below the dialog sees keys while it is open.
      e.Handled = True

      If Me._screen Is Nothing Then Return

      Me._message = ""

      Dim key = e.VirtualKey
      Dim item = Me.FocusedItem()
      Dim button = TryCast(item, DialogButton)

      If key = VirtualKey.VK_ESCAPE Then
        Dim cancel = Me._buttons.FirstOrDefault(Function(b) b.IsCancel)
        If cancel IsNot Nothing Then Me.Press(cancel) Else Me.Close()
        Return
      End If

      If key = VirtualKey.VK_TAB Then
        Me.MoveFocus((e.ControlKeyState And ControlKeyState.SHIFT_PRESSED) = 0)
      ElseIf key = VirtualKey.VK_RETURN Then
        Me.Press(If(button, Me.DefaultButton()))
      ElseIf button IsNot Nothing Then
        Me.HandleButtonKey(e, button)
      ElseIf TypeOf item IsNot InputPart AndAlso IsPrintable(e) AndAlso Me.HotKeyButton(e.Character) IsNot Nothing Then
        Me.Press(Me.HotKeyButton(e.Character))
      ElseIf TypeOf item Is InputPart Then
        HandleInputKey(e, DirectCast(item, InputPart))
      ElseIf TypeOf item Is CheckItem Then
        If key = VirtualKey.VK_SPACE Then
          Dim check = DirectCast(item, CheckItem)
          check.Checked = Not check.Checked
        End If
      ElseIf TypeOf item Is ListPart Then
        HandleListKey(key, DirectCast(item, ListPart))
      ElseIf TypeOf item Is RadioPart Then
        Dim radios = DirectCast(item, RadioPart)
        If key = VirtualKey.VK_UP OrElse key = VirtualKey.VK_LEFT Then
          radios.SelectedIndex = Math.Max(0, radios.SelectedIndex - 1)
        ElseIf key = VirtualKey.VK_DOWN OrElse key = VirtualKey.VK_RIGHT Then
          radios.SelectedIndex = Math.Min(radios.Options.Length - 1, radios.SelectedIndex + 1)
        ElseIf key = VirtualKey.VK_HOME Then
          radios.SelectedIndex = 0
        ElseIf key = VirtualKey.VK_END Then
          radios.SelectedIndex = radios.Options.Length - 1
        End If
      End If

      If Me._screen IsNot Nothing Then Me._screen.Invalidate()

    End Sub

    Private Function HotKeyButton(ch As Char) As DialogButton
      Return Me._buttons.FirstOrDefault(Function(b) b.Hotkey <> ControlChars.NullChar AndAlso b.Hotkey = Char.ToUpperInvariant(ch))
    End Function

    Private Sub HandleButtonKey(e As KeyEvent, button As DialogButton)

      Dim key = e.VirtualKey

      If key = VirtualKey.VK_SPACE Then
        Me.Press(button)
      ElseIf key = VirtualKey.VK_LEFT OrElse key = VirtualKey.VK_UP Then
        ' arrows stay among the buttons: no wrapping round to the inputs
        If Me._focusIndex > Me.FocusRing().Count - Me._buttons.Count Then Me._focusIndex -= 1
      ElseIf key = VirtualKey.VK_RIGHT OrElse key = VirtualKey.VK_DOWN Then
        If Me._focusIndex < Me.FocusRing().Count - 1 Then Me._focusIndex += 1
      ElseIf IsPrintable(e) Then
        Dim hot = Me._buttons.FirstOrDefault(Function(b) b.Hotkey <> ControlChars.NullChar AndAlso b.Hotkey = Char.ToUpperInvariant(e.Character))
        If hot IsNot Nothing Then Me.Press(hot)
      End If

    End Sub

    Private Shared Sub HandleInputKey(e As KeyEvent, input As InputPart)
      If e.VirtualKey = VirtualKey.VK_BACK Then
        If input.TextSelected Then
          input.Text = ""
          input.TextSelected = False
        ElseIf input.Text.Length > 0 Then
          input.Text = input.Text.Substring(0, input.Text.Length - 1)
        End If
      ElseIf IsPrintable(e) Then
        If input.TextSelected Then
          input.Text = ""
          input.TextSelected = False
        End If
        input.Text &= e.Character
      End If
    End Sub

    Private Shared Sub HandleListKey(key As VirtualKey, list As ListPart)
      Select Case key
        Case VirtualKey.VK_UP : list.Select_(list.SelectedIndex - 1)
        Case VirtualKey.VK_DOWN : list.Select_(list.SelectedIndex + 1)
        Case VirtualKey.VK_PRIOR : list.Select_(list.SelectedIndex - list.Visible)
        Case VirtualKey.VK_NEXT : list.Select_(list.SelectedIndex + list.Visible)
        Case VirtualKey.VK_HOME : list.Select_(0)
        Case VirtualKey.VK_END : list.Select_(list.Items.Count - 1)
      End Select
    End Sub

    Public Overrides Sub OnKeyUp(sender As Object, e As KeyEvent)
      e.Handled = True
    End Sub

    Public Overrides Sub OnMouseDown(sender As Object, e As MouseEvent)

      If Me._screen Is Nothing Then Return

      Dim x = CInt(e.X)
      Dim y = CInt(e.Y)

      If y = Me._buttonRow Then

        For Each button In Me._buttons
          If x >= button.Left AndAlso x < button.Left + button.Text.Length Then
            Me._focusIndex = Me.FocusRing().IndexOf(button)
            Me.Press(button)
            Exit For
          End If
        Next

      Else

        For Each part In Me._parts

          Dim input = TryCast(part, InputPart)
          Dim radios = TryCast(part, RadioPart)
          Dim checks = TryCast(part, CheckRowPart)
          Dim list = TryCast(part, ListPart)
          Dim opts = TryCast(part, OptionsPart)

          If opts IsNot Nothing Then

            If y = opts.Check.Row AndAlso x >= opts.Check.Left AndAlso x < opts.Check.Left + opts.Check.Label.Length + 4 Then
              Me._focusIndex = Me.FocusRing().IndexOf(opts.Check)
              opts.Check.Checked = Not opts.Check.Checked
            ElseIf y >= opts.Radios.FirstRow AndAlso y < opts.Radios.FirstRow + opts.Radios.Options.Length AndAlso x >= opts.Radios.Left Then
              Me._focusIndex = Me.FocusRing().IndexOf(opts.Radios)
              opts.Radios.SelectedIndex = y - opts.Radios.FirstRow
            End If

          ElseIf input IsNot Nothing AndAlso y >= input.Row - 1 AndAlso y <= input.Row + 1 AndAlso x >= input.Left AndAlso x <= input.Right Then

            Me._focusIndex = Me.FocusRing().IndexOf(input)

          ElseIf radios IsNot Nothing AndAlso y >= radios.FirstRow AndAlso y < radios.FirstRow + radios.Options.Length AndAlso x >= radios.Left Then

            Me._focusIndex = Me.FocusRing().IndexOf(radios)
            radios.SelectedIndex = y - radios.FirstRow

          ElseIf checks IsNot Nothing Then

            For Each check In checks.Items
              If y = check.Row AndAlso x >= check.Left AndAlso x < check.Left + check.Label.Length + 4 Then
                Me._focusIndex = Me.FocusRing().IndexOf(check)
                check.Checked = Not check.Checked
              End If
            Next

          ElseIf list IsNot Nothing AndAlso x > list.Left AndAlso x < list.Right AndAlso y > list.Top AndAlso y < list.Top + list.Rows - 1 Then

            Me._focusIndex = Me.FocusRing().IndexOf(list)
            list.Select_(list.TopIndex + (y - list.Top - 1))

          End If

        Next

      End If

      If Me._screen IsNot Nothing Then Me._screen.Invalidate()

    End Sub

    ' ---- drawing

    Private Shared Function Fit(text As String, width As Integer) As String
      If width <= 0 Then Return ""
      Return If(text.Length > width, text.Substring(0, width), text.PadRight(width))
    End Function

    Private Function ButtonsMinWidth() As Integer
      If Me._buttons.Count = 0 Then Return 0
      ' packed side by side with one space between, and two columns of margin
      Return Me._buttons.Sum(Function(b) b.Text.Length) + (Me._buttons.Count - 1) + 4
    End Function

    Private Sub DrawBracketedLabel(screen As Screen, left As Integer, row As Integer, text As String, bright As Boolean)
      screen.DrawString(left, row, text, Me.ForegroundColor, Me.BackgroundColor)
      If bright Then
        ' the focused / default button has bright white angle brackets
        screen.DrawString(left, row, text.Substring(0, 1), ConsoleColor.White, Me.BackgroundColor)
        screen.DrawString(left + text.Length - 1, row, text.Substring(text.Length - 1), ConsoleColor.White, Me.BackgroundColor)
      End If
    End Sub

    Private Sub DrawList(screen As Screen, list As ListPart, row As Integer, hasFocus As Boolean)

      Dim fg = Me.ForegroundColor
      Dim bg = Me.BackgroundColor

      list.Left = Me.Left + 2
      list.Right = Me.Right - 2
      list.Top = row

      list.KeepVisible()

      screen.DrawBox(list.Left, row, list.Right, row + list.Rows - 1, fg, bg)

      Dim barWidth = Math.Min(23, list.Right - list.Left - 1)

      For r = 0 To list.Visible - 1
        Dim index = list.TopIndex + r
        If index >= list.Items.Count Then Exit For
        Dim selected = index = list.SelectedIndex
        Dim text = Fit(" " & list.Items(index), barWidth)
        screen.DrawString(list.Left + 1, row + 1 + r, text, If(selected AndAlso hasFocus, ConsoleColor.White, If(selected, ConsoleColor.Gray, fg)), If(selected, ConsoleColor.Black, bg))
      Next

      ' the horizontal scroll bar replaces the bottom border
      Dim bottom = row + list.Rows - 1
      screen.DrawLeftArrow(list.Left + 1, bottom, fg, bg)
      screen.DrawRightArrow(list.Right - 1, bottom, fg, bg)
      For x = list.Left + 2 To list.Right - 2
        screen.DrawLightShade(x, bottom, fg, bg)
      Next
      screen.DrawBlock(list.Left + 2, bottom, fg, bg)

    End Sub

    Public Overrides Sub Render(screen As Screen)

      If Not Me.Visible Then Return

      ' lists take the extra rows of a window taller than 25
      For Each list In Me._parts.OfType(Of ListPart)()
        list.ExtraRows = If(list.GrowsWithWindow, Math.Max(0, screen.Height - 25), 0)
      Next

      Dim contentRows = Me._parts.Sum(Function(p) p.Rows)
      Dim minWidth = Math.Max(Me.ButtonsMinWidth(), If(Me._parts.Count = 0, 0, Me._parts.Max(Function(p) p.MinWidth)))

      minWidth = Math.Max(minWidth, Me.MinimumWidth)

      Select Case Me.Size
        Case FormDialogSize.Half : Me.Width = Math.Max(minWidth, screen.Width \ 2 - 1)
        Case FormDialogSize.Medium : Me.Width = Math.Max(minWidth, screen.Width - 20)
        Case FormDialogSize.Wide : Me.Width = Math.Max(minWidth, screen.Width - 7)
        Case FormDialogSize.Large : Me.Width = Math.Max(minWidth, screen.Width - 12)
        Case Else : Me.Width = minWidth
      End Select

      ' Long text is clipped rather than making the dialog wider than the window (but never narrower than its buttons).
      If Me.Width > screen.Width - 4 Then Me.Width = Math.Max(Me.ButtonsMinWidth(), screen.Width - 4)

      Me.Height = contentRows + Me.TopPadding + Me.BottomPadding + 4
      Me.Left = (screen.Width - Me.Width) \ 2
      Me.Top = (screen.Height - Me.Height) \ 2

      ' Too big for the window (the keyboard still works): draw nothing.
      If Me.Left < 0 OrElse Me.Top < 0 OrElse Me.Right + 2 >= screen.Width OrElse Me.Bottom + 1 >= screen.Height Then Return

      Dim fg = Me.ForegroundColor
      Dim bg = Me.BackgroundColor

      screen.DrawShadow(Me.Left + 2, Me.Top + 1, Me.Right + 2, Me.Bottom + 1)
      screen.DrawBoxFill(Me.Left, Me.Top, Me.Right, Me.Bottom, fg, bg)
      screen.DrawBox(Me.Left, Me.Top, Me.Right, Me.Bottom, fg, bg)

      If Not String.IsNullOrEmpty(Me.Title) Then
        Dim title = " " & Me.Title & " "
        screen.DrawString(Me.Left + (Me.Width - title.Length) \ 2, Me.Top, title, fg, bg)
      End If

      Dim focused = Me.FocusedItem()
      Dim row = Me.Top + 1 + Me.TopPadding

      For Each part In Me._parts

        Dim text = TryCast(part, TextPart)
        Dim input = TryCast(part, InputPart)
        Dim radios = TryCast(part, RadioPart)
        Dim checks = TryCast(part, CheckRowPart)
        Dim list = TryCast(part, ListPart)
        Dim opts = TryCast(part, OptionsPart)

        If text IsNot Nothing Then

          Dim shown = If(text.Text.Length > Me.Width - 2, text.Text.Substring(0, Me.Width - 2), text.Text)
          Dim x = If(text.Centered, Me.Left + (Me.Width - shown.Length) \ 2, Me.Left + Me.LabelIndent)
          screen.DrawString(x, row, shown, fg, bg)

        ElseIf input IsNot Nothing Then

          input.Row = row + 1
          input.Left = Me.Left + Me.LabelIndent + Me.WidestLabel() + 1
          input.Right = Me.Right - 2
          screen.DrawString(Me.Left + Me.LabelIndent, input.Row, input.Label, fg, bg)
          If input.SubLabel IsNot Nothing Then screen.DrawString(Me.Left + Me.LabelIndent + 1, input.Row + 1, input.SubLabel, fg, bg)
          screen.DrawBox(input.Left, row, input.Right, row + 2, fg, bg)

          Dim width = input.Right - input.Left - 1
          Dim hasFocus = ReferenceEquals(focused, input)
          Dim shown = input.Text
          If shown.Length >= width Then shown = shown.Substring(shown.Length - width + 1)
          If input.TextSelected AndAlso shown.Length > 0 Then
            screen.DrawString(input.Left + 1, input.Row, shown, ConsoleColor.White, ConsoleColor.Black)
            If hasFocus Then screen.DrawString(input.Left + 1 + shown.Length, input.Row, "_", fg, bg)
          Else
            screen.DrawString(input.Left + 1, input.Row, shown & If(hasFocus, "_", ""), fg, bg)
          End If

        ElseIf checks IsNot Nothing Then

          ' the first box 5 columns in, the second about 63% across (QBasic's positions at 60 columns)
          For i = 0 To checks.Items.Count - 1
            Dim check = checks.Items(i)
            check.Row = row
            check.Left = If(i = 0, Me.Left + 5, Me.Left + CInt(Me.Width * 0.63) + (i - 1) * 20)
            Dim hasFocus = ReferenceEquals(focused, check)
            screen.DrawString(check.Left, row, "[" & If(check.Checked, "X", " ") & "] " & check.Label, fg, bg)
            If hasFocus Then screen.DrawString(check.Left + 1, row, If(check.Checked, "X", " "), ConsoleColor.White, ConsoleColor.Black)
          Next

        ElseIf opts IsNot Nothing Then

          opts.Check.Row = row
          opts.Check.Left = Me.Left + 5
          screen.DrawString(opts.Check.Left, row, "[" & If(opts.Check.Checked, "X", " ") & "] " & opts.Check.Label, fg, bg)
          If ReferenceEquals(focused, opts.Check) Then screen.DrawString(opts.Check.Left + 1, row, If(opts.Check.Checked, "X", " "), ConsoleColor.White, ConsoleColor.Black)

          Dim rightX = Me.Left + CInt(Me.Width * 0.52)
          screen.DrawString(rightX, row, opts.Heading, fg, bg)
          opts.Radios.Left = rightX
          opts.Radios.FirstRow = row + 1
          Dim radiosFocused = ReferenceEquals(focused, opts.Radios)
          For i = 0 To opts.Radios.Options.Length - 1
            screen.DrawString(rightX, row + 1 + i, "( ) " & opts.Radios.Options(i), fg, bg)
            If i = opts.Radios.SelectedIndex Then
              screen.DrawString(rightX + 1, row + 1 + i, ChrW(&H2022).ToString(), If(radiosFocused, ConsoleColor.White, fg), If(radiosFocused, ConsoleColor.Black, bg))
            End If
          Next

        ElseIf list IsNot Nothing Then

          Me.DrawList(screen, list, row, ReferenceEquals(focused, list))

        ElseIf radios IsNot Nothing Then

          Dim blockWidth = radios.Options.Max(Function(o) o.Length) + 4
          radios.Left = Me.Left + (Me.Width - blockWidth) \ 2
          radios.FirstRow = row
          Dim hasFocus = ReferenceEquals(focused, radios)

          For i = 0 To radios.Options.Length - 1
            screen.DrawString(radios.Left, row + i, "( ) " & radios.Options(i), fg, bg)
            If i = radios.SelectedIndex Then
              screen.DrawString(radios.Left + 1, row + i, ChrW(&H2022).ToString(), If(hasFocus, ConsoleColor.White, fg), If(hasFocus, ConsoleColor.Black, bg))
            End If
          Next

        End If

        row += part.Rows

      Next

      ' the message (validation errors ...) sits in the last padding row
      If Me._message.Length > 0 AndAlso Me.BottomPadding > 0 Then
        screen.DrawString(Me.Left + 2, Me.Bottom - 3, Fit(Me._message, Me.Width - 4), ConsoleColor.DarkRed, bg)
      End If

      Dim separatorRow = Me.Bottom - 2
      screen.DrawLeftTee(Me.Left, separatorRow, fg, bg)
      screen.DrawHorizontalLine(Me.Left + 1, Me.Right - 1, separatorRow, fg, bg)
      screen.DrawRightTee(Me.Right, separatorRow, fg, bg)

      ' buttons: equal slots across the inside of the frame, each label centred in its slot; when the labels are too long for that
      ' (Find and Verify / Change All / Cancel / Help) they are packed side by side with one space between, centred as a group
      Me._buttonRow = Me.Bottom - 1
      Dim highlighted = Me.HighlightedButton()
      Dim count = Math.Max(1, Me._buttons.Count)
      Dim slot = (Me.Width - 2) / count
      Dim pack = Me._buttons.Any(Function(b) b.Text.Length + 1 > slot)

      If pack Then
        Dim total = Me._buttons.Sum(Function(b) b.Text.Length) + (count - 1)
        Dim x = Me.Left + (Me.Width - total) \ 2
        For Each button In Me._buttons
          button.Left = x
          x += button.Text.Length + 1
        Next
      Else
        For i = 0 To Me._buttons.Count - 1
          Me._buttons(i).Left = Me.Left + 1 + CInt(Math.Floor(slot * i + (slot - Me._buttons(i).Text.Length) / 2))
        Next
      End If

      For Each button In Me._buttons
        Me.DrawBracketedLabel(screen, button.Left, Me._buttonRow, button.Text, ReferenceEquals(button, highlighted))
      Next

    End Sub

    ''' <summary>All input boxes in a dialog start at the same column, after the widest label.</summary>
    Private Function WidestLabel() As Integer
      Dim inputs = Me._parts.OfType(Of InputPart)().ToList()
      Return If(inputs.Count = 0, 0, inputs.Max(Function(i) Math.Max(i.Label.Length, If(i.SubLabel Is Nothing, 0, i.SubLabel.Length + 1))))
    End Function

  End Class

End Namespace
