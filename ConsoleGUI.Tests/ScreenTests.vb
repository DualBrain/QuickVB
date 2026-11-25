Option Strict On
Option Infer On

Imports ConsoleGUI
Imports Xunit

' Screen.GetScreen is static, so these tests must not run in parallel with each other (they live in one class).
Public Class ScreenTests

  Private Class TestScreen
    Inherits Screen

    Public ReadOnly Property Title As New Label With {.Left = 2, .Top = 1, .Width = 30, .Text = "Hello, ConsoleGUI"}
    Public ReadOnly Property Status As New Label With {.Left = 0, .Top = 24, .Width = 80, .Text = "Ready",
                                                       .ForegroundColor = ConsoleColor.Black, .BackgroundColor = ConsoleColor.Gray}

    Public Sub New(driver As IConsoleDriver)
      MyBase.New(driver)
      Me.Controls.Add(Me.Title)
      Me.Controls.Add(Me.Status)
    End Sub

    Public Sub Run()
      NavigateTo(Me)
    End Sub

  End Class

  <Fact>
  Public Sub RendersControlsIntoTheConsole()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New TestScreen(console)

    screen.Run()

    Assert.StartsWith("  Hello, ConsoleGUI", console.RowText(1))
    Assert.StartsWith("Ready", console.RowText(24))
    Assert.Equal(ConsoleColor.Black, console.ForegroundAt(0, 24))
    Assert.Equal(ConsoleColor.Gray, console.BackgroundAt(0, 24))
  End Sub

  <Fact>
  Public Sub OnlyChangedSpansAreWrittenAfterTheFirstFrame()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New TestScreen(console)

    screen.Post(Sub()
                  screen.Status.Text = "Stop!"
                  screen.Invalidate()
                End Sub)
    screen.Run()

    Assert.True(console.WriteCount >= 2)
    Assert.Equal(1, console.LastSpanCount)
    Assert.StartsWith("Stop!", console.RowText(24))
    Assert.StartsWith("  Hello, ConsoleGUI", console.RowText(1))
  End Sub

  <Fact>
  Public Sub ResizeEventResizesTheScreenAndRedraws()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New TestScreen(console)
    Dim resizedTo As Dimensions = Nothing
    AddHandler screen.Resize, Sub(sender, e) resizedTo = e.Dimensions

    console.QueueResize(100, 30)
    screen.Run()

    Assert.Equal(100, resizedTo.Width)
    Assert.Equal(30, resizedTo.Height)
    Assert.Equal(100, screen.Width)
    Assert.StartsWith("  Hello, ConsoleGUI", console.RowText(1))
  End Sub

  <Fact>
  Public Sub TextRunningPastTheEdgeIsClippedNotAnError()
    Dim console As New HeadlessConsole(20, 5)
    Dim ui As New TestScreen(console)
    ui.Title.Left = 15   ' "Hello, ConsoleGUI" does not fit
    ui.Title.Width = 17
    ui.Status.Top = 4
    ui.Status.Width = 20

    ui.Run()

    Assert.Equal("Hello", console.RowText(1).Substring(15))
  End Sub

  <Fact>
  Public Sub KeyEventsReachTheScreen()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New TestScreen(console)
    Dim typed As New System.Text.StringBuilder()
    AddHandler screen.BeforeKeyDown, Sub(sender, e) typed.Append(e.Character)

    console.QueueText("Hi 4")
    screen.Run()

    Assert.Equal("Hi 4", typed.ToString())
  End Sub

  <Fact>
  Public Sub MouseClickIsHitTestedToTheControlUnderIt()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New TestScreen(console)
    Dim clicked As Control = Nothing
    AddHandler screen.MouseDown, Sub(sender, e) clicked = screen.HitTest(e.X, e.Y)

    console.QueueClick(5, 1)
    screen.Run()

    Assert.Same(screen.Title, clicked)
  End Sub

  <Fact>
  Public Sub DropDownMenuRunsTheChosenItem()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New TestScreen(console)
    Dim ran As String = Nothing

    Dim fileMenu As New Menu With {.DropDownMenu = True}
    fileMenu.SetMenuItems(New MenuItem() {New MenuItem(Name:="&New", Action:=Sub() ran = "new"),
                                          New MenuItem(Name:="&Open...", Action:=Sub() ran = "open"),
                                          New MenuItem(Name:="E&xit", Action:=Sub() ran = "exit")})

    Dim mainMenu As New Menu With {.Left = 0, .Top = 0, .Width = 80, .Height = 1}
    mainMenu.SetMenuItems(New MenuItem() {New MenuItem(Name:="&File", DropDownMenu:=fileMenu)})
    screen.Controls.Add(mainMenu)

    ' Activate the menu bar like MyScreen does for Alt/F10; F opens the File menu; Down moves to Open...; Enter runs it.
    screen.Post(Sub() mainMenu.Activate(screen, screen.ActiveControl))
    console.QueueText("f")
    screen.Run()

    Assert.Contains("Open...", console.GetText())
    Assert.Null(ran)

    console.QueueKey(VirtualKey.VK_DOWN)
    console.QueueKey(VirtualKey.VK_RETURN, ChrW(13))
    screen.Run()

    Assert.Equal("open", ran)
  End Sub

End Class
