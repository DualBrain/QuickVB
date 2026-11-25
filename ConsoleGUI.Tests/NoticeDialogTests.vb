Option Strict On
Option Infer On

Imports ConsoleGUI
Imports Xunit

Public Class NoticeDialogTests

  Private Const TopLeft As Char = ChrW(&H250C)
  Private Const TopRight As Char = ChrW(&H2510)
  Private Const BottomLeft As Char = ChrW(&H2514)
  Private Const BottomRight As Char = ChrW(&H2518)
  Private Const Horizontal As Char = ChrW(&H2500)
  Private Const Vertical As Char = ChrW(&H2502)
  Private Const SeparatorLeft As Char = ChrW(&H251C)
  Private Const SeparatorRight As Char = ChrW(&H2524)

  Private Class NoticeScreen
    Inherits Screen

    Public ReadOnly Property Dialog As New NoticeDialog With {.EnterText = "Press Enter to see the Survival Guide",
                                                              .EscapeText = "Press ESC to clear this dialog box"}
    Public Property EnterCount As Integer
    Public Property EscapeCount As Integer

    Public Sub New(driver As IConsoleDriver)
      MyBase.New(driver)
      Me.Dialog.Lines = New List(Of String) From {"Welcome to QuickVB for Windows", "", "Copyright (C) Somebody, 1964-2026.", "All rights reserved."}
      ' shown by the first queued event, so keys queued afterwards reach it
      Me.Post(Sub() Me.Dialog.Show(Me, Sub() Me.EnterCount += 1, Sub() Me.EscapeCount += 1))
    End Sub

    Public Sub Run()
      NavigateTo(Me)
    End Sub

  End Class

  Private Shared Function Find(console As HeadlessConsole, text As String) As (Column As Integer, Row As Integer)
    For row = 0 To console.Height - 1
      Dim column = console.RowText(row).IndexOf(text, StringComparison.Ordinal)
      If column >= 0 Then Return (column, row)
    Next
    Throw New InvalidOperationException("'" & text & "' is not on the screen:" & Environment.NewLine & console.GetText())
  End Function

  <Fact>
  Public Sub DrawsACentredSingleLineBoxWithTheTwoActions()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New NoticeScreen(console)

    screen.Run()

    ' Four lines of text: 4 + 7 rows, 58 columns, centred.
    Dim d = screen.Dialog
    Assert.Equal(11, d.Height)
    Assert.Equal(58, d.Width)
    Assert.Equal((80 - 58) \ 2, d.Left)

    Assert.Equal(TopLeft, console.CharAt(d.Left, d.Top))
    Assert.Equal(TopRight, console.CharAt(d.Right, d.Top))
    Assert.Equal(BottomLeft, console.CharAt(d.Left, d.Bottom))
    Assert.Equal(BottomRight, console.CharAt(d.Right, d.Bottom))
    Assert.Equal(Horizontal, console.CharAt(d.Left + 5, d.Top))
    Assert.Equal(Vertical, console.CharAt(d.Left, d.Top + 3))

    Dim text = console.GetText()
    Assert.Contains("Welcome to QuickVB for Windows", text)
    Assert.Contains("Copyright (C) Somebody, 1964-2026.", text)
    Assert.Contains("< Press Enter to see the Survival Guide >", text)
    Assert.Contains("< Press ESC to clear this dialog box >", text)

    ' The separator (single line) meets the frame between the two actions.
    Dim separatorRow = Find(console, "< Press ESC").Row - 1
    Assert.Equal(SeparatorLeft, console.CharAt(d.Left, separatorRow))
    Assert.Equal(SeparatorRight, console.CharAt(d.Right, separatorRow))

    ' Text is centred; the angle brackets are bright white on gray, the words black.
    Dim welcome = Find(console, "Welcome to QuickVB for Windows")
    Assert.Equal(d.Left + (d.Width - "Welcome to QuickVB for Windows".Length) \ 2, welcome.Column)
    Dim action = Find(console, "< Press Enter")
    Assert.Equal(ConsoleColor.White, console.ForegroundAt(action.Column, action.Row))
    Assert.Equal(ConsoleColor.Black, console.ForegroundAt(action.Column + 2, action.Row))
    Assert.Equal(ConsoleColor.Gray, console.BackgroundAt(action.Column + 2, action.Row))
  End Sub

  <Fact>
  Public Sub OffersTheStandardDialogStatusHint()
    Dim dialog As New NoticeDialog()
    Assert.Equal("F1=Help  Enter=Execute  Esc=Cancel  Tab=Next Field  Arrow=Next Item", dialog.StatusHint)
    Assert.Equal(Screen.DialogStatusHint, dialog.StatusHint)
  End Sub

  <Fact>
  Public Sub EnterChoosesTheFirstActionAndClosesTheDialog()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New NoticeScreen(console)

    console.QueueKey(VirtualKey.VK_RETURN, ChrW(13))
    screen.Run()

    Assert.Equal(1, screen.EnterCount)
    Assert.Equal(0, screen.EscapeCount)
    Assert.False(screen.Dialog.IsOpen)
    Assert.Null(screen.PopupControl)
    Assert.True(console.CursorVisible)
    Assert.DoesNotContain("Welcome to QuickVB", console.GetText())
  End Sub

  <Fact>
  Public Sub EscapeChoosesTheSecondActionAndClosesTheDialog()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New NoticeScreen(console)

    console.QueueKey(VirtualKey.VK_ESCAPE)
    screen.Run()

    Assert.Equal(0, screen.EnterCount)
    Assert.Equal(1, screen.EscapeCount)
    Assert.False(screen.Dialog.IsOpen)
  End Sub

  <Fact>
  Public Sub OtherKeysAreSwallowedWhileOpen()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New NoticeScreen(console)
    Dim leaked As Boolean = False
    AddHandler screen.BeforeKeyDown, Sub(sender, e) leaked = True

    console.QueueText("abc")
    screen.Run()

    Assert.False(leaked)
    Assert.True(screen.Dialog.IsOpen)
    Assert.Equal(0, screen.EnterCount + screen.EscapeCount)
  End Sub

  <Fact>
  Public Sub ClickingAnActionChoosesIt()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New NoticeScreen(console)

    screen.Run()
    Dim escape = Find(console, "< Press ESC")
    console.QueueClick(escape.Column + 4, escape.Row)
    screen.Run()

    Assert.Equal(1, screen.EscapeCount)
    Assert.False(screen.Dialog.IsOpen)
  End Sub

  <Fact>
  Public Sub ADialogTooLargeForTheWindowIsHiddenButStillClosesWithEsc()
    Dim console As New HeadlessConsole(20, 6)
    Dim screen As New NoticeScreen(console)

    console.QueueKey(VirtualKey.VK_ESCAPE)
    screen.Run()   ' must not throw

    Assert.Equal(1, screen.EscapeCount)
  End Sub

End Class
