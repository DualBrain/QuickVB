' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

'Imports System.Text
'Imports System.Threading.Tasks

Namespace ConsoleGUI

  Public Class RowChangedEventArgs
    Inherits EventArgs

    Public Property OldRow() As Integer
    Public Property NewRow() As Integer

    Public Sub New(oldRow As Integer, newRow As Integer)
      Me.OldRow = oldRow
      Me.NewRow = newRow
    End Sub

  End Class

  Public Class SelectionChangedEventArgs
    Inherits EventArgs

    Public Property OldSelection() As ViewSpan
    Public Property NewSelection() As ViewSpan

    Public Sub New(oldSelection As ViewSpan, newSelection As ViewSpan)
      Me.OldSelection = oldSelection
      Me.NewSelection = newSelection
    End Sub

  End Class

  Public Class TextBufferView
    Inherits Control

    Private _buffer As TextBuffer

    Public Property Buffer() As TextBuffer
      Get
        Return Me._buffer
      End Get
      Set(value As TextBuffer)
        If Me._buffer IsNot Nothing Then
          RemoveHandler Me._buffer.BufferChanged, AddressOf Me._buffer_BufferChanged
        End If
        Me._buffer = value
        AddHandler Me._buffer.BufferChanged, AddressOf Me._buffer_BufferChanged
      End Set
    End Property

    Private Sub _buffer_BufferChanged(sender As Object, e As EventArgs)
      Me._version += 1
      Dim screen = ConsoleGUI.Screen.GetScreen()
      If screen IsNot Nothing Then
        screen.Invalidate()
      End If
    End Sub

    Public Property WindowLeft() As Integer
    Public Property WindowTop() As Integer

    Public ReadOnly Property WindowRight() As Integer
      Get
        Return Me.WindowLeft + Me.Width - 1
      End Get
    End Property

    Public ReadOnly Property WindowBottom() As Integer
      Get
        Return Me.WindowTop + Me.Height - 1
      End Get
    End Property

    Public Property CursorRow() As Integer
      Get
        Return Me.Selection.StartRow
      End Get
      Private Set(value As Integer)
        Me.Selection = New ViewSpan(value, Me.CursorColumn)
      End Set
    End Property

    Public Property CursorColumn() As Integer
      Get
        Return Me.Selection.StartColumn
      End Get
      Private Set(value As Integer)
        Me.Selection = New ViewSpan(Me.CursorRow, value)
      End Set
    End Property

    Private _selection As ViewSpan

    Public Property Selection() As ViewSpan
      Get
        Return Me._selection
      End Get
      Private Set(value As ViewSpan)
        Me._selection = value
        Me.NotifyCursorMoved(Screen.GetScreen())
      End Set
    End Property

    Public Sub MoveCursor(row As Integer, col As Integer)
      Me.CursorRow = row
      Me.CursorColumn = col
      Me.NotifyCursorMoved(Screen.GetScreen())
    End Sub

    Public Event RowChanged As EventHandler(Of RowChangedEventArgs)
    Public Event SelectionChanged As EventHandler(Of SelectionChangedEventArgs)

    Public Property ScrollBars() As Boolean = True

    ''' <summary>When True the text can be looked at, moved around in and copied but not changed.</summary>
    Public Property IsReadOnly() As Boolean

    Public Property BreakpointForeground() As ConsoleColor = ConsoleColor.Gray
    Public Property BreakpointBackground() As ConsoleColor = ConsoleColor.DarkRed
    Public Property StatementForeground() As ConsoleColor = ConsoleColor.White
    Public Property StatementBackground() As ConsoleColor = ConsoleColor.DarkBlue

    ' The distance between tab stops (Options > Display).
    Public Property TabStops() As Integer = 4
    Public Property ScrollBarsVisible() As Boolean = True

    Public Overrides Sub Render(screen As Screen)

      Dim cr As Integer = Me.ScreenTop
      Dim wr As Integer = Me.WindowTop

      Do While cr <= Me.ScreenBottom

        Dim line = Me.Buffer.GetLineOrDefault(wr)

        Dim windowLineWidth = Math.Min(line.Text.Length - Me.WindowLeft, Me.Width)

        Dim screenLine = line.Text.SubstringPadRight(Me.WindowLeft, windowLineWidth, Me.Width)

        screen.DrawString(Me.ScreenLeft, cr, screenLine, Me.ForegroundColor, Me.BackgroundColor)

        For Each span In line.ColorSpans

          If span.IntersectsWith(Me.WindowLeft, Me.WindowRight) Then

            Dim l = Me.Left - Me.WindowLeft + span.Start
            Dim r = Me.Left - Me.WindowLeft + span.End

            If l < Me.ScreenLeft Then
              l = Me.ScreenLeft
            End If
            If r > Me.ScreenRight Then
              r = Me.ScreenRight
            End If

            screen.RecolorLine(l, cr, r - l + 1, span.ForegroundColor, span.BackgroundColor)

          End If

        Next

        Me.RenderMarks(screen, line, cr)
        Me.RenderSelection(screen, wr, cr, line.Text.Length)

        cr += 1
        wr += 1

      Loop

      Me.RenderScrollBars(screen)

    End Sub

    ' A line with a breakpoint, or the statement the program stopped at, is drawn in the colours chosen for it (the statement wins).
    Private Sub RenderMarks(screen As Screen, line As TextBufferLine, screenRow As Integer)

      If line.Marks = 0 Then Return

      Dim fg, bg As ConsoleColor

      If (line.Marks And LineMarks.CurrentStatement) <> 0 Then
        fg = Me.StatementForeground : bg = Me.StatementBackground
      Else
        fg = Me.BreakpointForeground : bg = Me.BreakpointBackground
      End If

      screen.RecolorLine(Me.ScreenLeft, screenRow, Me.Width, fg, bg)

    End Sub

    ' The selected text is shown in reverse.
    Private Sub RenderSelection(screen As Screen, textRow As Integer, screenRow As Integer, lineLength As Integer)

      If Not Me.HasTextSelection Then Return

      Dim startRow, startColumn, endRow, endColumn As Integer
      Me.GetSelectedRange(startRow, startColumn, endRow, endColumn)

      If textRow < startRow OrElse textRow > endRow Then Return

      Dim from = If(textRow = startRow, startColumn, 0)
      Dim [to] = If(textRow = endRow, endColumn, lineLength + 1) ' the line break is one selected cell

      from = Math.Max(from, Me.WindowLeft)
      [to] = Math.Min([to] - 1, Me.WindowRight)

      If [to] >= from Then
        screen.RecolorLine(Me.ScreenLeft + from - Me.WindowLeft, screenRow, [to] - from + 1, Me.BackgroundColor, Me.ForegroundColor)
      End If

    End Sub

    Private Sub RenderScrollBars(screen As Screen)

      If Me._ScrollBars AndAlso Me.ScrollBarsVisible Then

        Dim fg, bg As ConsoleColor

        fg = ConsoleColor.Black
        bg = ConsoleColor.Gray

        screen.DrawLeftArrow(Me.ScreenLeft, Me.ScreenBottom + 1, fg, bg)

        For x = Me.ScreenLeft + 1 To Me.ScreenRight - 1
          screen.DrawLightShade(x, Me.ScreenBottom + 1, fg, bg)
        Next

        screen.DrawRightArrow(Me.ScreenRight, Me.ScreenBottom + 1, fg, bg)

        Dim blockColumn = Utility.GetRangeValue(Me.ScreenLeft + 1, Me.ScreenRight - 1, (CDbl(Me.WindowLeft) / 255))

        screen.DrawBlock(blockColumn, Me.ScreenBottom + 1, fg, bg)

        screen.DrawUpArrow(Me.ScreenRight + 1, Me.ScreenTop, fg, bg)

        For y = Me.ScreenTop + 1 To Me.ScreenBottom - 1
          screen.DrawLightShade(Me.ScreenRight + 1, y, fg, bg)
        Next

        screen.DrawDownArrow(Me.ScreenRight + 1, Me.ScreenBottom, fg, bg)

        Dim blockRow = Utility.GetRangeValue(Me.ScreenTop + 1, Me.ScreenBottom - 1, (CDbl(Me.CursorRow) / Math.Max(1, Me.Buffer.Lines.Count - 1)))

        screen.DrawBlock(Me.ScreenRight + 1, blockRow, fg, bg)

      End If

    End Sub

    ' ---- undo

    Private NotInheritable Class UndoStep
      Public Text As String
      Public Row, Column As Integer
      Public Typing As Boolean
    End Class

    Private Const MaxUndoSteps As Integer = 500

    Private ReadOnly _undoSteps As New List(Of UndoStep)()
    Private _version As Integer
    Private _snapshotVersion As Integer
    Private _snapshotText As String
    Private _editRow, _editColumn As Integer
    Private _editIsTyping As Boolean
    Private _typingOpen As Boolean
    Private _inEdit As Boolean

    ''' <summary>True when there is an edit to take back.</summary>
    Public ReadOnly Property CanUndo() As Boolean
      Get
        Return Me._undoSteps.Count > 0
      End Get
    End Property

    ''' <summary>Forgets what can be undone (a different program was loaded).</summary>
    Public Sub ClearUndo()
      Me._undoSteps.Clear()
      Me._snapshotText = Nothing
      Me._typingOpen = False
    End Sub

    Private Sub BeginEdit()

      Me._inEdit = True
      Me._editRow = Me.CursorRow
      Me._editColumn = Me.CursorColumn
      Me._editIsTyping = False

      ' changes made since the last edit (loading a file, Roslyn formatting) are not undoable steps of their own
      If Me._snapshotText Is Nothing OrElse Me._version <> Me._snapshotVersion Then
        Me._snapshotText = Me.Buffer.Text
        Me._snapshotVersion = Me._version
        Me._typingOpen = False
      End If

    End Sub

    Private Sub EndEdit(typedCharacter As Char)

      Me._inEdit = False

      If Me._version = Me._snapshotVersion Then
        ' nothing changed: moving the cursor ends a run of typing
        Me._typingOpen = False
        Return
      End If

      Dim typing = Me._editIsTyping

      If Not (typing AndAlso Me._typingOpen) Then
        Me._undoSteps.Add(New UndoStep With {.Text = Me._snapshotText, .Row = Me._editRow, .Column = Me._editColumn, .Typing = typing})
        If Me._undoSteps.Count > MaxUndoSteps Then Me._undoSteps.RemoveAt(0)
      End If

      ' a run of typed characters is one step, a word at a time
      Me._typingOpen = typing AndAlso typedCharacter <> " "c

      Me._snapshotText = Me.Buffer.Text
      Me._snapshotVersion = Me._version

    End Sub

    ''' <summary>Runs an edit (for example a menu command) so that it can be undone as one step.</summary>
    Public Sub Edit(action As Action)
      If Me.IsReadOnly Then Return
      Me.BeginEdit()
      Try
        action()
      Finally
        Me.EndEdit(ChrW(0))
      End Try
      Screen.GetScreen()?.Invalidate()
    End Sub

    ''' <summary>Takes back the last edit (Alt+Backspace).</summary>
    Public Sub Undo()
      If Me.IsReadOnly Then Return


      If Me._undoSteps.Count = 0 Then Return

      Dim last = Me._undoSteps(Me._undoSteps.Count - 1)
      Me._undoSteps.RemoveAt(Me._undoSteps.Count - 1)

      Me.ClearTextSelection()
      Me.Buffer.Text = last.Text

      Me._snapshotText = last.Text
      Me._snapshotVersion = Me._version
      Me._typingOpen = False

      Me.MoveCursor(last.Row, last.Column)
      Screen.GetScreen()?.Invalidate()

    End Sub

    ' ---- keyboard

    ' The first key of a two key command (Ctrl+K or Ctrl+Q, as WordStar does it) waits here for the second.
    Private _chord As Char

    ' Where the markers of Ctrl+K n / Ctrl+Q n are (row, column); -1 when not set.
    Private ReadOnly _markers(3) As ViewSpan
    Private ReadOnly _markerIsSet(3) As Boolean

    ''' <summary>False when typing overwrites the character under the cursor (the INS key and Ctrl+V switch it).</summary>
    Public Property InsertMode() As Boolean = True

    Private Shared Function IsModifierKey(key As VirtualKey) As Boolean
      Select Case key
        Case VirtualKey.VK_SHIFT, VirtualKey.VK_CONTROL, VirtualKey.VK_MENU, VirtualKey.VK_LSHIFT, VirtualKey.VK_RSHIFT, VirtualKey.VK_LCONTROL,
             VirtualKey.VK_RCONTROL, VirtualKey.VK_LMENU, VirtualKey.VK_RMENU, VirtualKey.VK_CAPITAL, VirtualKey.VK_NUMLOCK, VirtualKey.VK_SCROLL
          Return True
        Case Else
          Return False
      End Select
    End Function

    Public Overrides Sub OnBeforeKeyDown(sender As Object, e As KeyEvent)

      Dim screen = CType(sender, Screen)

      ' A modifier on its own does nothing (and doesn't cancel a Ctrl+K / Ctrl+Q waiting for its second key).
      If IsModifierKey(e.VirtualKey) Then Return

      Dim shift = (e.ControlKeyState And ControlKeyState.SHIFT_PRESSED) <> 0
      Dim ctrl = e.ControlKeyState.IsControlPressed()
      Dim alt = (e.ControlKeyState And (ControlKeyState.LEFT_ALT_PRESSED Or ControlKeyState.RIGHT_ALT_PRESSED)) <> 0

      Dim pendingChord = Me._chord
      Me._chord = ChrW(0)

      ' Alt+Backspace takes back the last edit
      If alt AndAlso Not ctrl AndAlso e.VirtualKey = VirtualKey.VK_BACK Then
        Me.Undo()
        e.Handled = True
        Return
      End If

      Me.BeginEdit()
      Me._windowUpdatesSuspended = True

      Try

        If pendingChord <> ChrW(0) AndAlso Me.HandleChord(pendingChord, e) Then
          e.Handled = True
        ElseIf ctrl AndAlso Not alt AndAlso Me.HandleControlKey(e) Then
          e.Handled = True
        ElseIf Me.HandleClipboardKey(e, shift, ctrl) Then
          e.Handled = True
        ElseIf Not Me.HandleNavigationKey(e.VirtualKey, shift, ctrl) Then
          Me.HandleEditKey(e, screen, shift, ctrl, alt)
        End If

      Finally
        Me._windowUpdatesSuspended = False
        Me.EndEdit(If(e.Character >= " "c, e.Character, ChrW(0)))
      End Try

      Me.NotifyCursorMoved(screen)

      ' the selection is drawn with the text
      screen.Invalidate()

    End Sub

    ''' <summary>Moves the cursor with <paramref name="move"/>; with <paramref name="extend"/> the text it passes over is selected, otherwise a selection is dropped.</summary>
    Private Sub Navigate(extend As Boolean, move As Action)

      If extend Then
        Me.BeginTextSelection()
      Else
        Me.ClearTextSelection()
      End If

      move()

    End Sub

    ''' <summary>The cursor, selection and scrolling keys. False when <paramref name="key"/> isn't one of them.</summary>
    Private Function HandleNavigationKey(key As VirtualKey, shift As Boolean, ctrl As Boolean) As Boolean

      Dim lineText = Me.Buffer.GetLineOrDefault(Me.CursorRow).Text

      Select Case key

        Case VirtualKey.VK_LEFT
          If ctrl Then
            Me.Navigate(shift, AddressOf Me.WordLeft)
          Else
            Me.Navigate(shift, AddressOf Me.CharacterLeft)
          End If

        Case VirtualKey.VK_RIGHT
          If ctrl Then
            Me.Navigate(shift, AddressOf Me.WordRight)
          Else
            Me.Navigate(shift, Sub() Me.CursorColumn += 1)
          End If

        Case VirtualKey.VK_UP
          If ctrl AndAlso Not shift Then
            Me.ScrollWindow(-1)
          Else
            Me.Navigate(shift, Sub() Me.CursorRow -= 1)
          End If

        Case VirtualKey.VK_DOWN
          If ctrl AndAlso Not shift Then
            Me.ScrollWindow(1)
          Else
            Me.Navigate(shift, Sub() Me.CursorRow += 1)
          End If

        Case VirtualKey.VK_HOME
          If ctrl Then
            Me.Navigate(shift, Sub() Me.MoveCursor(0, 0))
          Else
            Me.Navigate(shift, Sub()
                                 Dim preSpace = lineText.Length - lineText.TrimStart().Length
                                 Me.CursorColumn = If(Me.CursorColumn <> preSpace, preSpace, 0)
                               End Sub)
          End If

        Case VirtualKey.VK_END
          If ctrl Then
            Me.Navigate(shift, AddressOf Me.MoveToEndOfText)
          Else
            Me.Navigate(shift, Sub() Me.CursorColumn = lineText.Length)
          End If

        Case VirtualKey.VK_PRIOR
          If ctrl Then
            Me.ScrollWindowSideways(-Me.Width)
          Else
            Me.Navigate(shift, Sub() Me.CursorRow -= Me.Height)
          End If

        Case VirtualKey.VK_NEXT
          If ctrl Then
            Me.ScrollWindowSideways(Me.Width)
          Else
            Me.Navigate(shift, Sub() Me.CursorRow += Me.Height)
          End If

        Case VirtualKey.VK_RETURN
          If Not ctrl Then Return False
          Me.Navigate(False, Sub() Me.MoveCursor(Me.CursorRow + 1, 0)) ' Ctrl+Enter: the beginning of the next line

        Case Else
          Return False

      End Select

      Return True

    End Function

    ''' <summary>Ctrl+letter: the WordStar keys.</summary>
    Private Function HandleControlKey(e As KeyEvent) As Boolean

      Select Case e.VirtualKey

        Case VirtualKey.VK_K : Me._chord = "K"c
        Case VirtualKey.VK_Q : Me._chord = "Q"c

        Case VirtualKey.VK_S : Me.HandleNavigationKey(VirtualKey.VK_LEFT, False, False)
        Case VirtualKey.VK_D : Me.HandleNavigationKey(VirtualKey.VK_RIGHT, False, False)
        Case VirtualKey.VK_A : Me.HandleNavigationKey(VirtualKey.VK_LEFT, False, True)
        Case VirtualKey.VK_F : Me.HandleNavigationKey(VirtualKey.VK_RIGHT, False, True)
        Case VirtualKey.VK_E : Me.HandleNavigationKey(VirtualKey.VK_UP, False, False)
        Case VirtualKey.VK_X : Me.HandleNavigationKey(VirtualKey.VK_DOWN, False, False)
        Case VirtualKey.VK_R : Me.HandleNavigationKey(VirtualKey.VK_PRIOR, False, False)
        Case VirtualKey.VK_C : Me.HandleNavigationKey(VirtualKey.VK_NEXT, False, False)
        Case VirtualKey.VK_W : Me.ScrollWindow(-1)
        Case VirtualKey.VK_Z : Me.ScrollWindow(1)
        Case VirtualKey.VK_J : Me.HandleNavigationKey(VirtualKey.VK_RETURN, False, True)

        Case VirtualKey.VK_H : Me.DeleteBackwards()
        Case VirtualKey.VK_G : Me.DeleteForwards()
        Case VirtualKey.VK_T : Me.DeleteWordRight()
        Case VirtualKey.VK_Y : Me.CutLine()
        Case VirtualKey.VK_N : Me.InsertLineBreakKeepingCursor()
        Case VirtualKey.VK_V : Me.InsertMode = Not Me.InsertMode

        Case Else
          ' Ctrl plus a letter that has no meaning here: it must not be typed into the text
          Return e.VirtualKey >= VirtualKey.VK_A AndAlso e.VirtualKey <= VirtualKey.VK_Z

      End Select

      Return True

    End Function

    ''' <summary>The second key of Ctrl+K (set a place marker) or Ctrl+Q (go to a marker, the quick cursor moves).</summary>
    Private Function HandleChord(first As Char, e As KeyEvent) As Boolean

      Dim key = e.VirtualKey

      If key >= VirtualKey.VK_0 AndAlso key <= VirtualKey.VK_3 AndAlso (e.ControlKeyState And ControlKeyState.SHIFT_PRESSED) = 0 Then

        Dim index = CInt(key) - CInt(VirtualKey.VK_0)

        If first = "K"c Then
          Me._markers(index) = New ViewSpan(Me.CursorRow, Me.CursorColumn)
          Me._markerIsSet(index) = True
        ElseIf Me._markerIsSet(index) Then
          Me.Navigate(False, Sub() Me.MoveCursor(Me._markers(index).StartRow, Me._markers(index).StartColumn))
        End If

        Return True

      End If

      If first <> "Q"c Then Return False

      Select Case key
        Case VirtualKey.VK_S : Me.Navigate(False, Sub() Me.CursorColumn = 0)
        Case VirtualKey.VK_D : Me.Navigate(False, Sub() Me.CursorColumn = Me.Buffer.GetLineOrDefault(Me.CursorRow).Text.Length)
        Case VirtualKey.VK_E : Me.Navigate(False, Sub() Me.CursorRow = Me.WindowTop)
        Case VirtualKey.VK_X : Me.Navigate(False, Sub() Me.CursorRow = Me.WindowBottom)
        Case VirtualKey.VK_R : Me.Navigate(False, Sub() Me.MoveCursor(0, 0))
        Case VirtualKey.VK_C : Me.Navigate(False, AddressOf Me.MoveToEndOfText)
        Case VirtualKey.VK_Y : Me.CutToEndOfLine()
        Case Else : Return False
      End Select

      Return True

    End Function

    ''' <summary>INS, Ctrl+Ins (copy), Shift+Del (cut) and Shift+Ins (paste).</summary>
    Private Function HandleClipboardKey(e As KeyEvent, shift As Boolean, ctrl As Boolean) As Boolean

      Select Case e.VirtualKey

        Case VirtualKey.VK_INSERT
          If ctrl Then
            Me.CopySelectedText()
          ElseIf shift Then
            Me.PasteFromClipboard()
          Else
            Me.InsertMode = Not Me.InsertMode
          End If
          Return True

        Case VirtualKey.VK_DELETE
          If shift Then
            Me.CutSelectedText()
            Return True
          End If

      End Select

      Return False

    End Function

    ''' <summary>Typing, deleting and indenting.</summary>
    Private Sub HandleEditKey(e As KeyEvent, screen As Screen, shift As Boolean, ctrl As Boolean, alt As Boolean)

      Select Case e.VirtualKey

        Case VirtualKey.VK_ESCAPE
          Me.ClearTextSelection()

        Case VirtualKey.VK_TAB
          If shift Then
            Me.UnindentLines()
          ElseIf Me.HasTextSelection Then
            Me.IndentLines()
          Else
            Me.CharacterEdit(e, screen)
          End If

        Case VirtualKey.VK_BACK, VirtualKey.VK_DELETE, VirtualKey.VK_RETURN
          Me.CharacterEdit(e, screen)

        Case Else
          ' only printable characters are typed (a control key that isn't a command would arrive as a control code)
          If e.Character >= " "c AndAlso Not ctrl Then
            Me._editIsTyping = Not Me.HasTextSelection
            Me.CharacterEdit(e, screen)
          End If

      End Select

    End Sub

    Private Sub CharacterLeft()
      If Me.CursorColumn > 0 Then
        Me.CursorColumn -= 1
      ElseIf Me.CursorRow > 0 Then
        Me.MoveCursor(Me.CursorRow - 1, Me.Buffer.GetLineOrDefault(Me.CursorRow - 1).Text.Length)
      End If
    End Sub

    Private Sub MoveToEndOfText()
      Dim last = Math.Max(0, Me.Buffer.Lines.Count - 1)
      Me.MoveCursor(last, Me.Buffer.GetLineOrDefault(last).Text.Length)
    End Sub

    Private Shared Function IsWordCharacter(c As Char) As Boolean
      Return Char.IsLetterOrDigit(c) OrElse c = "_"c
    End Function

    Private Sub WordLeft()

      Dim row = Me.CursorRow
      Dim col = Me.CursorColumn
      Dim text = Me.Buffer.GetLineOrDefault(row).Text

      If col > text.Length Then col = text.Length

      If col = 0 Then
        If row > 0 Then Me.MoveCursor(row - 1, Me.Buffer.GetLineOrDefault(row - 1).Text.Length)
        Return
      End If

      Do While col > 0 AndAlso Not IsWordCharacter(text(col - 1))
        col -= 1
      Loop
      Do While col > 0 AndAlso IsWordCharacter(text(col - 1))
        col -= 1
      Loop

      Me.CursorColumn = col

    End Sub

    Private Sub WordRight()

      Dim row = Me.CursorRow
      Dim col = Me.CursorColumn
      Dim text = Me.Buffer.GetLineOrDefault(row).Text

      If col >= text.Length Then
        If row < Me.Buffer.Lines.Count - 1 Then Me.MoveCursor(row + 1, 0)
        Return
      End If

      Do While col < text.Length AndAlso IsWordCharacter(text(col))
        col += 1
      Loop
      Do While col < text.Length AndAlso Not IsWordCharacter(text(col))
        col += 1
      Loop

      Me.CursorColumn = col

    End Sub

    ''' <summary>Scrolls the text by <paramref name="rows"/> lines; the cursor stays on screen.</summary>
    Public Sub ScrollWindow(rows As Integer)

      If Me.Height < 1 Then Return

      Me.WindowTop = Math.Max(0, Math.Min(Math.Max(0, Me.Buffer.Lines.Count - 1), Me.WindowTop + rows))

      If Me.CursorRow < Me.WindowTop Then
        Me.CursorRow = Me.WindowTop
      ElseIf Me.CursorRow > Me.WindowBottom Then
        Me.CursorRow = Me.WindowBottom
      End If

      Screen.GetScreen()?.Invalidate()

    End Sub

    Private Sub ScrollWindowSideways(columns As Integer)

      If Me.Width < 1 Then Return

      Me.WindowLeft = Math.Max(0, Me.WindowLeft + columns)

      If Me.CursorColumn < Me.WindowLeft Then
        Me.CursorColumn = Me.WindowLeft
      ElseIf Me.CursorColumn > Me.WindowRight Then
        Me.CursorColumn = Me.WindowRight
      End If

      Screen.GetScreen()?.Invalidate()

    End Sub

    Public Overrides Sub OnMouseWheel(sender As Object, e As MouseEvent)
      Me.ScrollWindow(-Math.Sign(e.WheelDelta) * 3)
    End Sub

    ' ---- text selection

    Private _hasAnchor As Boolean
    Private _anchorRow, _anchorColumn As Integer

    ''' <summary>True while some text is selected (the other end of the selection is the cursor).</summary>
    Public ReadOnly Property HasTextSelection() As Boolean
      Get
        Return Me._hasAnchor AndAlso (Me._anchorRow <> Me.CursorRow OrElse Me._anchorColumn <> Me.CursorColumn)
      End Get
    End Property

    Private Sub BeginTextSelection()
      If Not Me._hasAnchor Then
        Me._hasAnchor = True
        Me._anchorRow = Me.CursorRow
        Me._anchorColumn = Me.CursorColumn
      End If
    End Sub

    Public Sub ClearTextSelection()
      Me._hasAnchor = False
    End Sub

    ''' <summary>Selects from (<paramref name="startRow"/>, <paramref name="startColumn"/>) up to the cursor, which is moved to the end.</summary>
    Public Sub SelectTextRange(startRow As Integer, startColumn As Integer, endRow As Integer, endColumn As Integer)
      Me._hasAnchor = True
      Me._anchorRow = startRow
      Me._anchorColumn = startColumn
      Me.MoveCursor(endRow, endColumn)
    End Sub

    ''' <summary>The selected text in order, whichever way it was selected.</summary>
    Public ReadOnly Property TextSelection() As ViewSpan
      Get
        Dim startRow, startColumn, endRow, endColumn As Integer
        Me.GetSelectedRange(startRow, startColumn, endRow, endColumn)
        Return New ViewSpan(startRow, startColumn, endRow, endColumn)
      End Get
    End Property

    Private Sub GetSelectedRange(ByRef startRow As Integer, ByRef startColumn As Integer, ByRef endRow As Integer, ByRef endColumn As Integer)

      If Not Me._hasAnchor Then
        startRow = Me.CursorRow : startColumn = Me.CursorColumn
        endRow = startRow : endColumn = startColumn
        Return
      End If

      If Me._anchorRow < Me.CursorRow OrElse (Me._anchorRow = Me.CursorRow AndAlso Me._anchorColumn <= Me.CursorColumn) Then
        startRow = Me._anchorRow : startColumn = Me._anchorColumn
        endRow = Me.CursorRow : endColumn = Me.CursorColumn
      Else
        startRow = Me.CursorRow : startColumn = Me.CursorColumn
        endRow = Me._anchorRow : endColumn = Me._anchorColumn
      End If

    End Sub

    Public ReadOnly Property SelectedText() As String
      Get

        If Not Me.HasTextSelection Then Return ""

        Dim startRow, startColumn, endRow, endColumn As Integer
        Me.GetSelectedRange(startRow, startColumn, endRow, endColumn)

        Dim builder As New System.Text.StringBuilder()

        For row = startRow To endRow

          Dim text = Me.Buffer.GetLineOrDefault(row).Text
          Dim from = If(row = startRow, Math.Min(startColumn, text.Length), 0)
          Dim [to] = If(row = endRow, Math.Min(endColumn, text.Length), text.Length)

          If row > startRow Then builder.Append(vbCrLf)
          If [to] > from Then builder.Append(text, from, [to] - from)

        Next

        Return builder.ToString()

      End Get
    End Property

    ''' <summary>Removes the selected text; the cursor ends up where it was.</summary>
    Public Sub DeleteSelectedText()
      If Me.IsReadOnly Then Return


      If Not Me.HasTextSelection Then Return

      Dim startRow, startColumn, endRow, endColumn As Integer
      Me.GetSelectedRange(startRow, startColumn, endRow, endColumn)

      Me.ClearTextSelection()
      Me.DeleteRange(startRow, startColumn, endRow, endColumn)

    End Sub

    Private Sub DeleteRange(startRow As Integer, startColumn As Integer, endRow As Integer, endColumn As Integer)

      Dim count = Me.Buffer.Lines.Count

      If startRow >= count Then
        Me.MoveCursor(startRow, startColumn)
        Return
      End If

      If endRow >= count Then
        endRow = count - 1
        endColumn = Me.Buffer.Lines(endRow).Text.Length
      End If

      startColumn = Math.Min(startColumn, Me.Buffer.Lines(startRow).Text.Length)
      endColumn = Math.Min(endColumn, Me.Buffer.Lines(endRow).Text.Length)

      If startRow = endRow Then

        If endColumn > startColumn Then Me.Buffer.RemoveText(startRow, startColumn, endColumn - startColumn)

      Else

        Dim tail = Me.Buffer.Lines(endRow).Text.Substring(endColumn)

        Me.Buffer.TruncateText(startRow, startColumn)

        For row = endRow To startRow + 1 Step -1
          Me.Buffer.RemoveLine(row)
        Next

        If tail.Length > 0 Then Me.Buffer.AppendText(startRow, tail)

      End If

      Me.MoveCursor(startRow, startColumn)

    End Sub

    ' ---- the clipboard

    Public Sub CopySelectedText()
      If Me.HasTextSelection Then TextClipboard.SetText(Me.SelectedText)
    End Sub

    Public Sub CutSelectedText()
      If Me.IsReadOnly Then Return

      If Me.HasTextSelection Then
        TextClipboard.SetText(Me.SelectedText)
        Me.DeleteSelectedText()
      End If
    End Sub

    Public Sub PasteFromClipboard()
      If Me.IsReadOnly Then Return

      Me.InsertTextAtCursor(TextClipboard.GetText())
    End Sub

    ''' <summary>Types <paramref name="text"/> (any number of lines) at the cursor, replacing the selection if there is one.</summary>
    Public Sub InsertTextAtCursor(text As String)
      If Me.IsReadOnly Then Return


      If String.IsNullOrEmpty(text) Then Return

      If Me.HasTextSelection Then Me.DeleteSelectedText()
      Me.ClearTextSelection()

      Dim lines = text.Split({vbCrLf, vbCr, vbLf}, StringSplitOptions.None)
      Dim row = Me.CursorRow

      Do While Me.Buffer.Lines.Count <= row
        Me.Buffer.InsertLine(Me.Buffer.Lines.Count, "")
      Loop

      Dim col = Math.Min(Me.CursorColumn, Me.Buffer.Lines(row).Text.Length)

      If lines.Length = 1 Then
        Me.Buffer.InsertText(row, col, lines(0))
        Me.MoveCursor(row, col + lines(0).Length)
        Return
      End If

      Dim tail = Me.Buffer.TruncateText(row, col)

      Me.Buffer.AppendText(row, lines(0))

      For i = 1 To lines.Length - 1
        Me.Buffer.InsertLine(row + i, lines(i))
      Next

      Dim lastRow = row + lines.Length - 1
      Dim lastColumn = lines(lines.Length - 1).Length

      If tail.Length > 0 Then Me.Buffer.AppendText(lastRow, tail)

      Me.MoveCursor(lastRow, lastColumn)

    End Sub

    ' ---- line and word editing

    Private Sub DeleteBackwards()
      Me.CharacterEdit(New KeyEvent(True, ChrW(8), 1, VirtualKey.VK_BACK, 0), Screen.GetScreen())
    End Sub

    Private Sub DeleteForwards()
      Me.CharacterEdit(New KeyEvent(True, ChrW(0), 1, VirtualKey.VK_DELETE, 0), Screen.GetScreen())
    End Sub

    Private Sub DeleteWordRight()
      If Me.IsReadOnly Then Return


      If Me.HasTextSelection Then
        Me.DeleteSelectedText()
        Return
      End If

      Dim row = Me.CursorRow
      Dim col = Me.CursorColumn
      Dim text = Me.Buffer.GetLineOrDefault(row).Text

      If col >= text.Length Then
        Me.DeleteForwards()
        Return
      End If

      Dim [end] = col

      Do While [end] < text.Length AndAlso IsWordCharacter(text([end]))
        [end] += 1
      Loop
      Do While [end] < text.Length AndAlso Not IsWordCharacter(text([end]))
        [end] += 1
      Loop

      Me.Buffer.RemoveText(row, col, [end] - col)

    End Sub

    ''' <summary>Ctrl+Y: the current line goes to the clipboard and is removed.</summary>
    Private Sub CutLine()
      If Me.IsReadOnly Then Return


      Dim row = Me.CursorRow

      Me.ClearTextSelection()

      If row >= Me.Buffer.Lines.Count Then Return

      TextClipboard.SetText(Me.Buffer.Lines(row).Text & vbCrLf)

      If Me.Buffer.Lines.Count = 1 Then
        Me.Buffer.RemoveText(0, 0, Me.Buffer.Lines(0).Text.Length)
      Else
        Me.Buffer.RemoveLine(row)
      End If

    End Sub

    ''' <summary>Ctrl+Q Y: the rest of the line goes to the clipboard and is removed.</summary>
    Private Sub CutToEndOfLine()
      If Me.IsReadOnly Then Return


      Dim row = Me.CursorRow
      Dim col = Me.CursorColumn

      Me.ClearTextSelection()

      If row >= Me.Buffer.Lines.Count OrElse col >= Me.Buffer.Lines(row).Text.Length Then Return

      TextClipboard.SetText(Me.Buffer.TruncateText(row, col))

    End Sub

    ''' <summary>Ctrl+N: breaks the line at the cursor, which stays where it is (so at the start of the line this inserts a blank line above).</summary>
    Private Sub InsertLineBreakKeepingCursor()
      If Me.IsReadOnly Then Return


      Dim row = Me.CursorRow
      Dim col = Me.CursorColumn

      Me.ClearTextSelection()

      If row >= Me.Buffer.Lines.Count Then Return

      Dim tail = Me.Buffer.TruncateText(row, Math.Min(col, Me.Buffer.Lines(row).Text.Length))
      Me.Buffer.InsertLine(row + 1, tail)

    End Sub

    ''' <summary>The first and last line a block operation covers: the selected lines, or the cursor line.</summary>
    Private Sub GetBlockRows(ByRef firstRow As Integer, ByRef lastRow As Integer)

      If Me.HasTextSelection Then
        Dim startColumn, endColumn As Integer
        Me.GetSelectedRange(firstRow, startColumn, lastRow, endColumn)
        ' a selection that stops at the start of a line doesn't include that line
        If endColumn = 0 AndAlso lastRow > firstRow Then lastRow -= 1
      Else
        firstRow = Me.CursorRow
        lastRow = Me.CursorRow
      End If

      lastRow = Math.Min(lastRow, Me.Buffer.Lines.Count - 1)

    End Sub

    Private Sub IndentLines()
      If Me.IsReadOnly Then Return


      Dim firstRow, lastRow As Integer
      Me.GetBlockRows(firstRow, lastRow)

      For row = firstRow To lastRow
        If Me.Buffer.Lines(row).Text.Length > 0 Then Me.Buffer.InsertText(row, 0, New String(" "c, Me.TabStops))
      Next

      Me.SelectTextRange(firstRow, 0, lastRow, Me.Buffer.Lines(lastRow).Text.Length)

    End Sub

    ''' <summary>Shift+Tab: removes up to one tab stop of leading spaces from the selected lines (or the cursor line).</summary>
    Private Sub UnindentLines()
      If Me.IsReadOnly Then Return


      Dim hadSelection = Me.HasTextSelection
      Dim firstRow, lastRow As Integer
      Me.GetBlockRows(firstRow, lastRow)

      If firstRow >= Me.Buffer.Lines.Count Then Return

      Dim removedFromCursorLine = 0

      For row = firstRow To lastRow

        Dim text = Me.Buffer.Lines(row).Text
        Dim spaces = 0

        Do While spaces < Math.Min(Me.TabStops, text.Length) AndAlso text(spaces) = " "c
          spaces += 1
        Loop

        If spaces > 0 Then Me.Buffer.RemoveText(row, 0, spaces)
        If row = Me.CursorRow Then removedFromCursorLine = spaces

      Next

      If hadSelection Then
        Me.SelectTextRange(firstRow, 0, lastRow, Me.Buffer.Lines(lastRow).Text.Length)
      Else
        Me.CursorColumn = Math.Max(0, Me.CursorColumn - removedFromCursorLine)
      End If

    End Sub

    ' ---- mouse

    Private Enum DragTarget
      None
      Text
      VerticalThumb
      HorizontalThumb
    End Enum

    Private _dragTarget As DragTarget

    Private ReadOnly Property ShowsScrollBars() As Boolean
      Get
        Return Me.ScrollBars AndAlso Me.ScrollBarsVisible
      End Get
    End Property

    ' The scroll bars are drawn just outside the text area, which a click on them has to reach.
    Public Overrides Function HitTest(x As Integer, y As Integer) As Control

      If Not Me.Visible Then Return Nothing

      Dim extra = If(Me.ShowsScrollBars, 1, 0)

      If x >= Me.ScreenLeft AndAlso x <= Me.ScreenRight + extra AndAlso y >= Me.ScreenTop AndAlso y <= Me.ScreenBottom + extra Then
        Return Me
      End If

      Return Nothing

    End Function

    Private Function VerticalThumbRow() As Integer
      Return CInt(Utility.GetRangeValue(Me.ScreenTop + 1, Me.ScreenBottom - 1, (CDbl(Me.CursorRow) / Math.Max(1, Me.Buffer.Lines.Count - 1))))
    End Function

    Private Function HorizontalThumbColumn() As Integer
      Return CInt(Utility.GetRangeValue(Me.ScreenLeft + 1, Me.ScreenRight - 1, (CDbl(Me.WindowLeft) / 255)))
    End Function

    Private Sub ClampToText(x As Integer, y As Integer, ByRef row As Integer, ByRef column As Integer)

      row = Math.Max(0, Math.Min(Math.Max(0, Me.Buffer.Lines.Count - 1), y - Me.ScreenTop + Me.WindowTop))
      column = Math.Max(0, x - Me.ScreenLeft + Me.WindowLeft)
      column = Math.Min(column, Me.Buffer.GetLineOrDefault(row).Text.Length)

    End Sub

    ''' <summary>Puts the cursor where the screen position (<paramref name="x"/>, <paramref name="y"/>) is in the text.</summary>
    Public Sub PlaceCursorAt(x As Integer, y As Integer)
      Dim row, column As Integer
      Me.ClampToText(x, y, row, column)
      Me.ClearTextSelection()
      Me.MoveCursor(row, column)
    End Sub

    Public Overrides Sub OnMouseDown(sender As Object, e As MouseEvent)

      If (e.ButtonState And ButtonState.FROM_LEFT_1ST_BUTTON_PRESSED) = 0 Then Return

      Me._dragTarget = DragTarget.None

      Dim x = CInt(e.X)
      Dim y = CInt(e.Y)

      If Me.ShowsScrollBars Then

        If x = Me.ScreenRight + 1 AndAlso y >= Me.ScreenTop AndAlso y <= Me.ScreenBottom Then
          Me.ClickVerticalScrollBar(y)
          Return
        End If

        If y = Me.ScreenBottom + 1 AndAlso x >= Me.ScreenLeft AndAlso x <= Me.ScreenRight Then
          Me.ClickHorizontalScrollBar(x)
          Return
        End If

      End If

      Dim row, column As Integer
      Me.ClampToText(x, y, row, column)

      If (e.ControlKeyState And ControlKeyState.SHIFT_PRESSED) <> 0 Then
        Me.BeginTextSelection()
        Me.MoveCursor(row, column)
      ElseIf (e.MouseEventFlags And MouseEventFlags.DOUBLE_CLICK) <> 0 Then
        Me.SelectWordAt(row, column)
      Else
        Me.MoveCursor(row, column)
        Me._hasAnchor = True
        Me._anchorRow = row
        Me._anchorColumn = column
      End If

      Me._dragTarget = DragTarget.Text

      Screen.GetScreen()?.Invalidate()

    End Sub

    Private Sub SelectWordAt(row As Integer, column As Integer)

      Dim text = Me.Buffer.GetLineOrDefault(row).Text

      If column >= text.Length OrElse Not IsWordCharacter(text(column)) Then
        Me.MoveCursor(row, column)
        Return
      End If

      Dim first = column
      Dim last = column

      Do While first > 0 AndAlso IsWordCharacter(text(first - 1))
        first -= 1
      Loop
      Do While last < text.Length AndAlso IsWordCharacter(text(last))
        last += 1
      Loop

      Me.SelectTextRange(row, first, row, last)

    End Sub

    Private Sub ClickVerticalScrollBar(y As Integer)

      If y = Me.ScreenTop Then
        Me.ScrollWindow(-1)
      ElseIf y = Me.ScreenBottom Then
        Me.ScrollWindow(1)
      Else

        Dim thumb = Me.VerticalThumbRow()

        If y < thumb Then
          Me.ScrollWindow(-Me.Height)
        ElseIf y > thumb Then
          Me.ScrollWindow(Me.Height)
        Else
          Me._dragTarget = DragTarget.VerticalThumb
        End If

      End If

      Screen.GetScreen()?.Invalidate()

    End Sub

    Private Sub ClickHorizontalScrollBar(x As Integer)

      If x = Me.ScreenLeft Then
        Me.ScrollWindowSideways(-1)
      ElseIf x = Me.ScreenRight Then
        Me.ScrollWindowSideways(1)
      Else

        Dim thumb = Me.HorizontalThumbColumn()

        If x < thumb Then
          Me.ScrollWindowSideways(-Me.Width)
        ElseIf x > thumb Then
          Me.ScrollWindowSideways(Me.Width)
        Else
          Me._dragTarget = DragTarget.HorizontalThumb
        End If

      End If

      Screen.GetScreen()?.Invalidate()

    End Sub

    Public Overrides Sub OnMouseDrag(sender As Object, e As MouseEvent)

      Select Case Me._dragTarget

        Case DragTarget.Text
          Dim row, column As Integer
          Me.ClampToText(CInt(e.X), CInt(e.Y), row, column)
          Me.MoveCursor(row, column)

        Case DragTarget.VerticalThumb
          Dim span = Math.Max(1, Me.Height - 3)
          Dim fraction = Math.Max(0.0, Math.Min(1.0, (CInt(e.Y) - (Me.ScreenTop + 1)) / span))
          Me.MoveCursor(CInt(Math.Round(fraction * Math.Max(0, Me.Buffer.Lines.Count - 1))), Me.CursorColumn)

        Case DragTarget.HorizontalThumb
          Dim span = Math.Max(1, Me.Width - 3)
          Dim fraction = Math.Max(0.0, Math.Min(1.0, (CInt(e.X) - (Me.ScreenLeft + 1)) / span))
          Dim column = CInt(Math.Round(fraction * 255))
          Me.WindowLeft = column
          Me.CursorColumn = Math.Max(Me.WindowLeft, Math.Min(Me.WindowRight, Me.CursorColumn))

        Case Else
          Return

      End Select

      Screen.GetScreen()?.Invalidate()

    End Sub

    Public Overrides Sub OnMouseUp(sender As Object, e As MouseEvent)
      Me._dragTarget = DragTarget.None
    End Sub

    Private lastNotifySpan As New ViewSpan(-1, -1)

    Private Sub NotifyCursorMoved(screen As Screen)

      Me.UpdateWindow(screen)

      If Me.CursorRow <> Me.lastNotifySpan.StartRow Then
        RaiseEvent RowChanged(Me, New RowChangedEventArgs(Me.lastNotifySpan.StartRow, Me.CursorRow))
      End If

      If Me.Selection <> Me.lastNotifySpan Then
        RaiseEvent SelectionChanged(Me, New SelectionChangedEventArgs(Me.lastNotifySpan, Me.Selection))
      End If

      Me.lastNotifySpan = Me.Selection

    End Sub

    Private Sub CharacterEdit(ch As Char, screen As Screen)

      If Me.CursorRow < 0 OrElse Me.CursorRow > Me.Buffer.Lines.Count OrElse Me.CursorColumn < 0 Then
        Return
      End If

      If ch = ControlChars.NullChar Then
        Return
      End If

      Dim lineText = Me.Buffer.GetLineOrDefault(Me.CursorRow).Text
      Dim preSpace = lineText.Length - lineText.TrimStart().Length

      Select Case ch

        Case CChar(vbTab) 'CChar(ConsoleKey.Tab)

          Dim newColumn = (Me.CursorColumn + Me.TabStops) - (Me.CursorColumn Mod Me.TabStops)

          If preSpace < lineText.Length Then
            Me.Buffer.InsertText(Me.CursorRow, Me.CursorColumn, New String(" "c, newColumn - Me.CursorColumn))
          End If

          Me.CursorColumn = newColumn

        Case CChar(vbBack) ' Chr(9) 'CChar(ConsoleKey.Backspace)

          If Me.CursorColumn > 0 Then

            Dim count As Integer

            If Me.CursorColumn <= preSpace OrElse lineText.Trim().Length = 0 Then
              count = ((Me.CursorColumn - 1) Mod Me.TabStops) + 1
            Else
              count = 1
            End If

            Me.Buffer.RemoveText(Me.CursorRow, Me.CursorColumn - count, count)
            Me.CursorColumn -= count

          Else
            If Me.CursorRow > 0 Then
              Me.MoveCursor(Me.CursorRow - 1, Me.Buffer.Lines(Me.CursorRow - 1).Text.Length)
              If Me.CursorRow < Me.Buffer.Lines.Count - 1 Then
                Me.Buffer.AppendText(Me.CursorRow, Me.Buffer.Lines(Me.CursorRow + 1).Text)
                Me.Buffer.RemoveLine(Me.CursorRow + 1)
              End If
            End If
          End If

        Case Chr(13) 'CChar(ConsoleKey.Enter)

          If Me.CursorRow >= Me.Buffer.Lines.Count Then
            Me.Buffer.InsertLine(Me.CursorRow, "")
          End If

          Dim padding = Me.Buffer.Lines(Me.CursorRow).Text.Length - Me.Buffer.Lines(Me.CursorRow).Text.TrimStart(" "c).Length
          Dim truncatedText = Me.Buffer.TruncateText(Me.CursorRow, Me.CursorColumn)
          Me.Buffer.InsertLine(Me.CursorRow + 1, New String(" "c, padding) & truncatedText)
          Me.MoveCursor(Me.CursorRow + 1, padding)

        Case Else

          ' overtype: the character under the cursor is replaced
          If Not Me.InsertMode AndAlso Me.CursorColumn < lineText.Length Then
            Me.Buffer.RemoveText(Me.CursorRow, Me.CursorColumn, 1)
          End If

          Me.Buffer.InsertText(Me.CursorRow, Me.CursorColumn, ch.ToString())
          Me.CursorColumn += 1

      End Select

    End Sub

    Public Sub CharacterEdit(e As KeyEvent, screen As Screen)

      If Me.IsReadOnly Then Return

      Me._windowUpdatesSuspended = True

      Try

        ' deleting removes the selection; anything typed replaces it
        If Me.HasTextSelection Then
          Dim removeOnly = e.VirtualKey = VirtualKey.VK_BACK OrElse e.VirtualKey = VirtualKey.VK_DELETE
          Me.DeleteSelectedText()
          If removeOnly Then Return
        End If

        Me.ClearTextSelection()

        If Me.CursorRow < 0 OrElse Me.CursorRow > Me.Buffer.Lines.Count OrElse Me.CursorColumn < 0 Then
          Return
        End If

        Select Case e.VirtualKey
          Case VirtualKey.VK_DELETE
            If Me.CursorColumn < Me.Buffer.GetLineOrDefault(Me.CursorRow).Text.Length Then
              Me.Buffer.RemoveText(Me.CursorRow, Me.CursorColumn, 1)
            Else
              If Me.CursorRow < Me.Buffer.Lines.Count - 1 Then
                Me.Buffer.AppendText(Me.CursorRow, Me.Buffer.Lines(Me.CursorRow + 1).Text)
                Me.Buffer.RemoveLine(Me.CursorRow + 1)
              End If
            End If
          Case Else
            Me.CharacterEdit(e.Character, screen)
        End Select

      Finally
        Me._windowUpdatesSuspended = False
        Me.UpdateWindow(screen)
      End Try

    End Sub

    Private _windowUpdatesSuspended As Boolean = False

    Private Sub UpdateWindow(screen As Screen)

      If Me._windowUpdatesSuspended Then
        Return
      End If

      ' A view with no room (a collapsed pane) has nothing to scroll; trying would ask for a redraw on every redraw.
      If Me.Height < 1 OrElse Me.Width < 1 Then
        Return
      End If

      Me._windowUpdatesSuspended = True

      Try

        Dim scrolled = False

        Dim windowBottom = Me.WindowTop + Me.Height - 1
        Dim windowRight = Me.WindowLeft + Me.Width - 1

        If Me.CursorRow < 0 Then
          Me.CursorRow = 0
        End If

        If Me.CursorRow > Me.Buffer.Lines.Count Then
          Me.CursorRow = Me.Buffer.Lines.Count
        End If

        If Me.CursorColumn < 0 Then
          Me.CursorColumn = 0
        End If

        If Me.CursorRow < Me.WindowTop Then
          Me.WindowTop = Me.CursorRow
          scrolled = True
        ElseIf Me.CursorRow > Me.WindowBottom Then
          Me.WindowTop = Me.CursorRow - Me.Height + 1
          scrolled = True
        End If

        If Me.CursorColumn < Me.WindowLeft Then
          Me.WindowLeft = Me.CursorColumn
          scrolled = True
        ElseIf Me.CursorColumn > Me.WindowRight Then
          Me.WindowLeft = Me.CursorColumn - Me.Width + 1
          scrolled = True
        End If

        If scrolled Then
          screen.Invalidate()
        End If

      Finally
        Me._windowUpdatesSuspended = False
      End Try

    End Sub

    Public Overrides Sub PlaceCursor(screen As Screen)
      Me.UpdateWindow(screen)
      screen.SetCursorPosition(Me.ScreenLeft + (Me.CursorColumn - Me.WindowLeft), Me.ScreenTop + (Me.CursorRow - Me.WindowTop))
    End Sub
  End Class

  Public Structure ViewSpan

    Public Sub New(row As Integer, col As Integer)
      'Me.New()
      Me.StartRow = row
      Me.StartColumn = col
      Me.EndRow = row
      Me.EndColumn = col
    End Sub

    Public Sub New(startRow As Integer, startCol As Integer, endRow As Integer, endCol As Integer)
      'Me.New()
      Me.StartRow = startRow
      Me.StartColumn = startCol
      Me.EndRow = endRow
      Me.EndColumn = endCol
    End Sub

    Public Property StartRow() As Integer
    Public Property StartColumn() As Integer
    Public Property EndRow() As Integer
    Public Property EndColumn() As Integer

    Public Overrides Function Equals(other As Object) As Boolean
      Return TypeOf other Is ViewSpan AndAlso
             Me = CType(other, ViewSpan)
    End Function

    Public Shared Operator =(x As ViewSpan, y As ViewSpan) As Boolean
      Return x.StartRow = y.StartRow AndAlso
             x.StartColumn = y.StartColumn AndAlso
             x.EndRow = y.EndRow AndAlso
             x.EndColumn = y.EndColumn
    End Operator

    Public Shared Operator <>(x As ViewSpan, y As ViewSpan) As Boolean
      Return Not (x = y)
    End Operator

    Public Overrides Function GetHashCode() As Integer
      Return Me.StartRow Xor
             Me.StartColumn Xor
             Me.EndRow Xor
             Me.EndColumn
    End Function

  End Structure

End Namespace