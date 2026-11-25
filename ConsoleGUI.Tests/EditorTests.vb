Option Explicit On
Option Strict On
Option Infer On

Imports ConsoleGUI
Imports Xunit

' The editing keys the help pages list (selection, clipboard, WordStar keys) and the mouse in the text editor.
' Screen.GetScreen is static, so these tests share one class and don't run in parallel.
Public Class EditorTests

  Private Const LeftButton As ButtonState = ButtonState.FROM_LEFT_1ST_BUTTON_PRESSED

  Private Class EditorScreen
    Inherits Screen

    Public ReadOnly Property Console As HeadlessConsole
    Public ReadOnly Property View As New TextBufferView

    Public Sub New(console As HeadlessConsole, text As String)
      MyBase.New(console)
      Me.Console = console
      Me.View.Buffer = New TextBuffer With {.Text = text}
      Me.View.Left = 0
      Me.View.Top = 0
      Me.View.Width = 40
      Me.View.Height = 10
      Me.View.ForegroundColor = ConsoleColor.Gray
      Me.View.BackgroundColor = ConsoleColor.DarkBlue
      Me.Controls.Add(Me.View)
      Me.ActiveControl = Me.View
    End Sub

    Public Sub Run()
      NavigateTo(Me)
    End Sub

    Public ReadOnly Property Text As String
      Get
        Return Me.View.Buffer.Text
      End Get
    End Property

  End Class

  Private Shared Function Editor(text As String) As EditorScreen
    TextClipboard.UseSystemClipboard = False
    TextClipboard.SetText("")
    Return New EditorScreen(New HeadlessConsole(80, 25), text)
  End Function

  Private Const Shift As ControlKeyState = ControlKeyState.SHIFT_PRESSED
  Private Const Ctrl As ControlKeyState = ControlKeyState.LEFT_CTRL_PRESSED

  ' ---- selection

  <Fact>
  Public Sub ShiftArrowsSelectAndTypingReplacesTheSelection()
    Dim s = Editor("hello world")

    s.Console.QueueKey(VirtualKey.VK_RIGHT, state:=Shift)
    s.Console.QueueKey(VirtualKey.VK_RIGHT, state:=Shift)
    s.Console.QueueKey(VirtualKey.VK_RIGHT, state:=Shift)
    s.Run()

    Assert.True(s.View.HasTextSelection)
    Assert.Equal("hel", s.View.SelectedText)

    s.Console.QueueText("J")
    s.Run()

    Assert.Equal("Jlo world", s.Text)
    Assert.False(s.View.HasTextSelection)
  End Sub

  <Fact>
  Public Sub MovingWithoutShiftDropsTheSelection()
    Dim s = Editor("hello")

    s.Console.QueueKey(VirtualKey.VK_END, state:=Shift)
    s.Console.QueueKey(VirtualKey.VK_LEFT)
    s.Run()

    Assert.False(s.View.HasTextSelection)
    Assert.Equal(4, s.View.CursorColumn)
  End Sub

  <Fact>
  Public Sub TheSelectionIsDrawnInReverse()
    Dim s = Editor("hello world")

    s.Console.QueueKey(VirtualKey.VK_RIGHT, state:=Shift)
    s.Console.QueueKey(VirtualKey.VK_RIGHT, state:=Shift)
    s.Run()

    Assert.Equal(ConsoleColor.DarkBlue, s.Console.ForegroundAt(0, 0))
    Assert.Equal(ConsoleColor.Gray, s.Console.BackgroundAt(0, 0))
    Assert.Equal(ConsoleColor.Gray, s.Console.ForegroundAt(2, 0))
    Assert.Equal(ConsoleColor.DarkBlue, s.Console.BackgroundAt(2, 0))
  End Sub

  <Fact>
  Public Sub ShiftDownSelectsWholeLinesAndBackspaceDeletesThem()
    Dim s = Editor("one" & vbCrLf & "two" & vbCrLf & "three")

    s.Console.QueueKey(VirtualKey.VK_DOWN, state:=Shift)
    s.Console.QueueKey(VirtualKey.VK_BACK)
    s.Run()

    Assert.Equal("two" & vbCrLf & "three", s.Text)
    Assert.Equal(0, s.View.CursorRow)
    Assert.Equal(0, s.View.CursorColumn)
  End Sub

  <Fact>
  Public Sub ControlArrowMovesByWordsAndShiftControlSelectsThem()
    Dim s = Editor("alpha beta gamma")

    s.Console.QueueKey(VirtualKey.VK_RIGHT, state:=Ctrl)
    s.Run()
    Assert.Equal(6, s.View.CursorColumn)

    s.Console.QueueKey(VirtualKey.VK_RIGHT, state:=Ctrl Or Shift)
    s.Run()
    Assert.Equal("beta ", s.View.SelectedText)

    s.Console.QueueKey(VirtualKey.VK_LEFT, state:=Ctrl)
    s.Run()
    Assert.Equal(6, s.View.CursorColumn)
  End Sub

  <Fact>
  Public Sub ShiftControlEndSelectsToTheEndOfTheText()
    Dim s = Editor("one" & vbCrLf & "two")

    s.Console.QueueKey(VirtualKey.VK_END, state:=Ctrl Or Shift)
    s.Run()

    Assert.Equal("one" & vbCrLf & "two", s.View.SelectedText)
  End Sub

  ' ---- clipboard

  <Fact>
  Public Sub CtrlInsertCopiesAndShiftInsertPastes()
    Dim s = Editor("abc")

    s.Console.QueueKey(VirtualKey.VK_END, state:=Shift)
    s.Console.QueueKey(VirtualKey.VK_INSERT, state:=Ctrl)
    s.Console.QueueKey(VirtualKey.VK_END)
    s.Console.QueueKey(VirtualKey.VK_INSERT, state:=Shift)
    s.Run()

    Assert.Equal("abcabc", s.Text)
    Assert.Equal(6, s.View.CursorColumn)
  End Sub

  <Fact>
  Public Sub ShiftDeleteCutsTheSelection()
    Dim s = Editor("abcdef")

    s.Console.QueueKey(VirtualKey.VK_RIGHT, state:=Shift)
    s.Console.QueueKey(VirtualKey.VK_RIGHT, state:=Shift)
    s.Console.QueueKey(VirtualKey.VK_DELETE, state:=Shift)
    s.Run()

    Assert.Equal("cdef", s.Text)
    Assert.Equal("ab", TextClipboard.GetText())
  End Sub

  <Fact>
  Public Sub PastingSeveralLinesSplitsTheLineAtTheCursor()
    Dim s = Editor("12" & vbCrLf & "end")
    TextClipboard.SetText("a" & vbCrLf & "b" & vbCrLf & "c")

    s.Console.QueueKey(VirtualKey.VK_RIGHT)
    s.Console.QueueKey(VirtualKey.VK_INSERT, state:=Shift)
    s.Run()

    Assert.Equal("1a" & vbCrLf & "b" & vbCrLf & "c2" & vbCrLf & "end", s.Text)
    Assert.Equal(2, s.View.CursorRow)
    Assert.Equal(1, s.View.CursorColumn)
  End Sub

  <Fact>
  Public Sub PasteReplacesTheSelection()
    Dim s = Editor("hello")
    TextClipboard.SetText("J")

    s.Console.QueueKey(VirtualKey.VK_RIGHT, state:=Shift)
    s.Console.QueueKey(VirtualKey.VK_INSERT, state:=Shift)
    s.Run()

    Assert.Equal("Jello", s.Text)
  End Sub

  ' ---- WordStar keys

  <Fact>
  Public Sub WordStarCursorKeys()
    Dim s = Editor("one two" & vbCrLf & "three four" & vbCrLf & "five")

    s.Console.QueueKey(VirtualKey.VK_D, state:=Ctrl)
    s.Console.QueueKey(VirtualKey.VK_D, state:=Ctrl)
    s.Console.QueueKey(VirtualKey.VK_X, state:=Ctrl)
    s.Run()
    Assert.Equal(1, s.View.CursorRow)
    Assert.Equal(2, s.View.CursorColumn)

    s.Console.QueueKey(VirtualKey.VK_S, state:=Ctrl)
    s.Console.QueueKey(VirtualKey.VK_E, state:=Ctrl)
    s.Run()
    Assert.Equal(0, s.View.CursorRow)
    Assert.Equal(1, s.View.CursorColumn)

    s.Console.QueueKey(VirtualKey.VK_F, state:=Ctrl)
    s.Run()
    Assert.Equal(4, s.View.CursorColumn) ' the start of "two"

    s.Console.QueueKey(VirtualKey.VK_A, state:=Ctrl)
    s.Run()
    Assert.Equal(0, s.View.CursorColumn)
  End Sub

  <Fact>
  Public Sub CtrlJGoesToTheBeginningOfTheNextLine()
    Dim s = Editor("abc" & vbCrLf & "def")

    s.Console.QueueKey(VirtualKey.VK_RIGHT)
    s.Console.QueueKey(VirtualKey.VK_J, state:=Ctrl)
    s.Run()

    Assert.Equal(1, s.View.CursorRow)
    Assert.Equal(0, s.View.CursorColumn)
  End Sub

  <Fact>
  Public Sub QuickCursorChordsMoveWithinTheLine()
    Dim s = Editor("  hello")

    s.Console.QueueKey(VirtualKey.VK_Q, state:=Ctrl)
    s.Console.QueueKey(VirtualKey.VK_D)
    s.Run()
    Assert.Equal(7, s.View.CursorColumn)

    s.Console.QueueKey(VirtualKey.VK_Q, state:=Ctrl)
    s.Console.QueueKey(VirtualKey.VK_S, state:=Ctrl) ' the second key may keep Ctrl down
    s.Run()
    Assert.Equal(0, s.View.CursorColumn)
  End Sub

  <Fact>
  Public Sub QuickCursorChordsMoveToTheStartAndEndOfTheText()
    Dim s = Editor("one" & vbCrLf & "two" & vbCrLf & "three")

    s.Console.QueueKey(VirtualKey.VK_Q, state:=Ctrl)
    s.Console.QueueKey(VirtualKey.VK_C)
    s.Run()
    Assert.Equal(2, s.View.CursorRow)
    Assert.Equal(5, s.View.CursorColumn)

    s.Console.QueueKey(VirtualKey.VK_Q, state:=Ctrl)
    s.Console.QueueKey(VirtualKey.VK_R)
    s.Run()
    Assert.Equal(0, s.View.CursorRow)
    Assert.Equal(0, s.View.CursorColumn)
  End Sub

  <Fact>
  Public Sub PlaceMarkersAreSetAndFound()
    Dim s = Editor("one" & vbCrLf & "two" & vbCrLf & "three")

    s.Console.QueueKey(VirtualKey.VK_DOWN)
    s.Console.QueueKey(VirtualKey.VK_RIGHT)
    s.Console.QueueKey(VirtualKey.VK_K, state:=Ctrl)
    s.Console.QueueKey(VirtualKey.VK_2)
    s.Console.QueueKey(VirtualKey.VK_HOME, state:=Ctrl)
    s.Console.QueueKey(VirtualKey.VK_Q, state:=Ctrl)
    s.Console.QueueKey(VirtualKey.VK_2)
    s.Run()

    Assert.Equal(1, s.View.CursorRow)
    Assert.Equal(1, s.View.CursorColumn)
    Assert.Equal("one" & vbCrLf & "two" & vbCrLf & "three", s.Text)
  End Sub

  <Fact>
  Public Sub CtrlYCutsTheLineAndCtrlQYTheRestOfIt()
    Dim s = Editor("one" & vbCrLf & "two three" & vbCrLf & "four")

    s.Console.QueueKey(VirtualKey.VK_DOWN)
    s.Console.QueueKey(VirtualKey.VK_Y, state:=Ctrl)
    s.Run()

    Assert.Equal("one" & vbCrLf & "four", s.Text)
    Assert.Equal("two three" & vbCrLf, TextClipboard.GetText())

    s.Console.QueueKey(VirtualKey.VK_RIGHT)
    s.Console.QueueKey(VirtualKey.VK_RIGHT)
    s.Console.QueueKey(VirtualKey.VK_Q, state:=Ctrl)
    s.Console.QueueKey(VirtualKey.VK_Y)
    s.Run()

    Assert.Equal("one" & vbCrLf & "fo", s.Text)
    Assert.Equal("ur", TextClipboard.GetText())
  End Sub

  <Fact>
  Public Sub CtrlGCtrlHAndCtrlTDelete()
    Dim s = Editor("alpha beta")

    s.Console.QueueKey(VirtualKey.VK_G, state:=Ctrl) ' the character at the cursor
    s.Run()
    Assert.Equal("lpha beta", s.Text)

    s.Console.QueueKey(VirtualKey.VK_T, state:=Ctrl) ' the rest of the word and the space
    s.Run()
    Assert.Equal("beta", s.Text)

    s.Console.QueueKey(VirtualKey.VK_RIGHT)
    s.Console.QueueKey(VirtualKey.VK_H, state:=Ctrl)
    s.Run()
    Assert.Equal("eta", s.Text)
  End Sub

  <Fact>
  Public Sub CtrlNInsertsALineKeepingTheCursor()
    Dim s = Editor("abcd")

    s.Console.QueueKey(VirtualKey.VK_RIGHT)
    s.Console.QueueKey(VirtualKey.VK_RIGHT)
    s.Console.QueueKey(VirtualKey.VK_N, state:=Ctrl)
    s.Run()

    Assert.Equal("ab" & vbCrLf & "cd", s.Text)
    Assert.Equal(0, s.View.CursorRow)
    Assert.Equal(2, s.View.CursorColumn)
  End Sub

  <Fact>
  Public Sub UnknownControlLettersAreNotTyped()
    Dim s = Editor("abc")

    s.Console.QueueKey(VirtualKey.VK_B, ChrW(2), Ctrl)
    s.Console.QueueKey(VirtualKey.VK_U, ChrW(21), Ctrl)
    s.Run()

    Assert.Equal("abc", s.Text)
  End Sub

  <Fact>
  Public Sub InsertTogglesOvertype()
    Dim s = Editor("abcd")

    s.Console.QueueKey(VirtualKey.VK_INSERT)
    s.Console.QueueText("xy")
    s.Run()
    Assert.Equal("xycd", s.Text)
    Assert.False(s.View.InsertMode)

    s.Console.QueueKey(VirtualKey.VK_V, state:=Ctrl)
    s.Console.QueueText("z")
    s.Run()
    Assert.Equal("xyzcd", s.Text)
  End Sub

  ' ---- scrolling

  Private Shared Function ManyLines(count As Integer) As String
    Return String.Join(vbCrLf, Enumerable.Range(0, count).Select(Function(i) "line " & i))
  End Function

  <Fact>
  Public Sub CtrlWAndCtrlZScrollTheTextWithoutLosingTheCursor()
    Dim s = Editor(ManyLines(50))

    s.Console.QueueKey(VirtualKey.VK_Z, state:=Ctrl)
    s.Console.QueueKey(VirtualKey.VK_Z, state:=Ctrl)
    s.Run()

    Assert.Equal(2, s.View.WindowTop)
    Assert.Equal(2, s.View.CursorRow) ' the cursor can't stay above the window

    s.Console.QueueKey(VirtualKey.VK_W, state:=Ctrl)
    s.Run()
    Assert.Equal(1, s.View.WindowTop)
  End Sub

  <Fact>
  Public Sub CtrlArrowsAlsoScroll()
    Dim s = Editor(ManyLines(50))

    s.Console.QueueKey(VirtualKey.VK_DOWN, state:=Ctrl)
    s.Run()

    Assert.Equal(1, s.View.WindowTop)
  End Sub

  ' ---- indenting

  <Fact>
  Public Sub TabIndentsTheSelectedLinesAndShiftTabUnindentsThem()
    Dim s = Editor("a" & vbCrLf & "b" & vbCrLf & "c")

    s.Console.QueueKey(VirtualKey.VK_DOWN, state:=Shift)
    s.Console.QueueKey(VirtualKey.VK_DOWN, state:=Shift)
    s.Console.QueueKey(VirtualKey.VK_TAB, ChrW(9))
    s.Run()

    Assert.Equal("    a" & vbCrLf & "    b" & vbCrLf & "c", s.Text)

    s.Console.QueueKey(VirtualKey.VK_TAB, ChrW(9), Shift)
    s.Run()

    Assert.Equal("a" & vbCrLf & "b" & vbCrLf & "c", s.Text)
  End Sub

  <Fact>
  Public Sub ShiftTabUnindentsTheCursorLine()
    Dim s = Editor("      x")

    s.Console.QueueKey(VirtualKey.VK_END)
    s.Console.QueueKey(VirtualKey.VK_TAB, ChrW(9), Shift)
    s.Run()

    Assert.Equal("  x", s.Text)
  End Sub

  <Fact>
  Public Sub TabStopsOfTwoIndentByTwo()
    Dim s = Editor("a" & vbCrLf & "b")
    s.View.TabStops = 2

    s.Console.QueueKey(VirtualKey.VK_DOWN, state:=Shift)
    s.Console.QueueKey(VirtualKey.VK_DOWN, state:=Shift)
    s.Console.QueueKey(VirtualKey.VK_TAB, ChrW(9))
    s.Run()

    Assert.Equal("  a" & vbCrLf & "  b", s.Text)
  End Sub

  <Fact>
  Public Sub BackspaceInTheIndentationGoesBackToThePreviousTabStop()
    Dim s = Editor("      x")
    s.View.TabStops = 2
    s.View.MoveCursor(0, 3)

    s.Console.QueueKey(VirtualKey.VK_BACK, ChrW(8))
    s.Run()

    Assert.Equal("     x", s.Text)   ' column 3 to column 2: one space
  End Sub

  ' ---- mouse

  <Fact>
  Public Sub ClickingPlacesTheCursorWhereTheTextIs()
    Dim s = Editor(ManyLines(50))

    s.Console.QueueKey(VirtualKey.VK_NEXT) ' scrolled down a page
    s.Console.QueueMouse(3, 2, LeftButton)
    s.Console.QueueMouse(3, 2, 0)
    s.Run()

    Assert.Equal(s.View.WindowTop + 2, s.View.CursorRow)
    Assert.Equal(3, s.View.CursorColumn)
  End Sub

  <Fact>
  Public Sub ClickingBeyondTheEndOfALinePutsTheCursorAtItsEnd()
    Dim s = Editor("abc")

    s.Console.QueueMouse(30, 0, LeftButton)
    s.Run()

    Assert.Equal(3, s.View.CursorColumn)
  End Sub

  <Fact>
  Public Sub DraggingSelectsText()
    Dim s = Editor("hello world" & vbCrLf & "second line")

    s.Console.QueueMouse(2, 0, LeftButton)
    s.Console.QueueMouse(5, 1, LeftButton, MouseEventFlags.MOUSE_MOVED)
    s.Console.QueueMouse(5, 1, 0)
    s.Run()

    Assert.Equal("llo world" & vbCrLf & "secon", s.View.SelectedText)
  End Sub

  <Fact>
  Public Sub AMovementWithoutAButtonDoesNotSelect()
    Dim s = Editor("hello world")

    s.Console.QueueMouse(2, 0, 0, MouseEventFlags.MOUSE_MOVED)
    s.Run()

    Assert.False(s.View.HasTextSelection)
  End Sub

  <Fact>
  Public Sub DoubleClickSelectsTheWord()
    Dim s = Editor("hello world")

    s.Console.QueueMouse(7, 0, LeftButton, MouseEventFlags.DOUBLE_CLICK)
    s.Run()

    Assert.Equal("world", s.View.SelectedText)
  End Sub

  <Fact>
  Public Sub ShiftClickExtendsTheSelection()
    Dim s = Editor("hello world")

    s.Console.QueueMouse(2, 0, LeftButton)
    s.Console.QueueMouse(2, 0, 0)
    s.Console.QueueMouse(8, 0, LeftButton, 0, Shift)
    s.Run()

    Assert.Equal("llo wo", s.View.SelectedText)
  End Sub

  <Fact>
  Public Sub TheWheelScrollsThreeLinesAtATime()
    Dim s = Editor(ManyLines(50))

    s.Console.QueueWheel(5, 5, -120)
    s.Run()
    Assert.Equal(3, s.View.WindowTop)

    s.Console.QueueWheel(5, 5, 120)
    s.Run()
    Assert.Equal(0, s.View.WindowTop)
  End Sub

  <Fact>
  Public Sub TheScrollBarArrowsScrollOneLine()
    Dim s = Editor(ManyLines(50))

    s.Console.QueueMouse(40, 9, LeftButton) ' the down arrow
    s.Console.QueueMouse(40, 9, 0)
    s.Run()
    Assert.Equal(1, s.View.WindowTop)

    s.Console.QueueMouse(40, 0, LeftButton) ' the up arrow
    s.Console.QueueMouse(40, 0, 0)
    s.Run()
    Assert.Equal(0, s.View.WindowTop)
  End Sub

  <Fact>
  Public Sub ClickingTheScrollBarTrackPages()
    Dim s = Editor(ManyLines(100))

    s.Console.QueueMouse(40, 8, LeftButton) ' below the thumb
    s.Console.QueueMouse(40, 8, 0)
    s.Run()

    Assert.Equal(10, s.View.WindowTop)
  End Sub

  <Fact>
  Public Sub DraggingTheScrollBarThumbMovesThroughTheText()
    Dim s = Editor(ManyLines(100))

    s.Console.QueueMouse(40, 1, LeftButton) ' the thumb is at the top
    s.Console.QueueMouse(40, 4, LeftButton, MouseEventFlags.MOUSE_MOVED)
    s.Console.QueueMouse(40, 4, 0)
    s.Run()

    Assert.True(s.View.CursorRow > 20 AndAlso s.View.CursorRow < 80, "cursor row " & s.View.CursorRow)
  End Sub

  <Fact>
  Public Sub ARightClickDoesNotMoveTheCursor()
    Dim s = Editor("hello")

    s.Console.QueueMouse(3, 0, ButtonState.RIGHTMOST_BUTTON_PRESSED)
    s.Run()

    Assert.Equal(0, s.View.CursorColumn)
  End Sub

  ' ---- undo

  Private Shared Sub Undo(s As EditorScreen)
    s.Console.QueueKey(VirtualKey.VK_BACK, ChrW(8), ControlKeyState.LEFT_ALT_PRESSED)
    s.Run()
  End Sub

  <Fact>
  Public Sub AltBackspaceUndoesTypingAWordAtATime()
    Dim s = Editor("")

    s.Console.QueueText("one two")
    s.Run()
    Assert.Equal("one two", s.Text)

    Undo(s)
    Assert.Equal("one ", s.Text)
    Assert.Equal(4, s.View.CursorColumn)

    Undo(s)
    Assert.Equal("", s.Text)
    Assert.False(s.View.CanUndo)
  End Sub

  <Fact>
  Public Sub UndoRestoresACutAPasteAndALineDeletion()
    Dim s = Editor("one" & vbCrLf & "two" & vbCrLf & "three")

    s.Console.QueueKey(VirtualKey.VK_Y, state:=Ctrl)
    s.Run()
    Assert.Equal("two" & vbCrLf & "three", s.Text)
    Undo(s)
    Assert.Equal("one" & vbCrLf & "two" & vbCrLf & "three", s.Text)
    Assert.Equal(0, s.View.CursorRow)

    s.Console.QueueKey(VirtualKey.VK_END, state:=Shift Or Ctrl)
    s.Console.QueueKey(VirtualKey.VK_DELETE, state:=Shift)
    s.Run()
    Assert.Equal("", s.Text)
    Undo(s)
    Assert.Equal("one" & vbCrLf & "two" & vbCrLf & "three", s.Text)
  End Sub

  <Fact>
  Public Sub MovingTheCursorBetweenTypingMakesTwoSteps()
    Dim s = Editor("")

    s.Console.QueueText("ab")
    s.Console.QueueKey(VirtualKey.VK_HOME)
    s.Console.QueueText("cd")
    s.Run()
    Assert.Equal("cdab", s.Text)

    Undo(s)
    Assert.Equal("ab", s.Text)
    Undo(s)
    Assert.Equal("", s.Text)
  End Sub

  <Fact>
  Public Sub UndoWithNothingToUndoDoesNothing()
    Dim s = Editor("abc")
    Undo(s)
    Assert.Equal("abc", s.Text)
  End Sub

End Class
