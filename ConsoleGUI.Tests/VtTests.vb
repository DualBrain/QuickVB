Option Strict On
Option Infer On

Imports System.IO
Imports System.Text
Imports ConsoleGUI
Imports Xunit

Public Class VtTests

  Private Shared ReadOnly Esc As String = ChrW(27)

  ' ---- VtRenderer

  Private Shared Sub Fill(chars() As Char, fg() As ConsoleColor, bg() As ConsoleColor, text As String, foreground As ConsoleColor, background As ConsoleColor)
    For i = 0 To chars.Length - 1
      chars(i) = If(i < text.Length, text(i), " "c)
      fg(i) = foreground
      bg(i) = background
    Next
  End Sub

  Private Shared Function Render(renderer As VtRenderer, text As String, foreground As ConsoleColor, background As ConsoleColor, width As Integer, height As Integer, spans As List(Of RowSpan)) As String
    Dim chars(width * height - 1) As Char
    Dim fg(width * height - 1) As ConsoleColor
    Dim bg(width * height - 1) As ConsoleColor
    Fill(chars, fg, bg, text, foreground, background)
    Return renderer.Render(chars, fg, bg, width, height, spans)
  End Function

  <Fact>
  Public Sub ASingleCellIsExactlyCursorMoveColoursCharacterReset()
    ' ESC in the buffer must never reach the terminal as a control character.
    Dim renderer As New VtRenderer(VtColorMode.Ansi16, synchronizedOutput:=False)

    Dim output = Render(renderer, ChrW(27), ConsoleColor.Gray, ConsoleColor.Black, 4, 2, New List(Of RowSpan) From {New RowSpan(0, 0, 1)})

    Assert.Equal(Esc & "[1;1H" & Esc & "[37;40m" & " " & Esc & "[0m", output)
  End Sub

  <Theory>
  <InlineData(ConsoleColor.Black, 30, 40)>
  <InlineData(ConsoleColor.DarkBlue, 34, 44)>
  <InlineData(ConsoleColor.DarkGreen, 32, 42)>
  <InlineData(ConsoleColor.DarkCyan, 36, 46)>
  <InlineData(ConsoleColor.DarkRed, 31, 41)>
  <InlineData(ConsoleColor.DarkYellow, 33, 43)>
  <InlineData(ConsoleColor.Gray, 37, 47)>
  <InlineData(ConsoleColor.DarkGray, 90, 100)>
  <InlineData(ConsoleColor.Blue, 94, 104)>
  <InlineData(ConsoleColor.Cyan, 96, 106)>
  <InlineData(ConsoleColor.Red, 91, 101)>
  <InlineData(ConsoleColor.Yellow, 93, 103)>
  <InlineData(ConsoleColor.White, 97, 107)>
  Public Sub Ansi16UsesTheTerminalsColourNumbers(color As ConsoleColor, expectedForeground As Integer, expectedBackground As Integer)
    Dim renderer As New VtRenderer(VtColorMode.Ansi16, synchronizedOutput:=False)

    Dim asForeground = Render(renderer, "x", color, ConsoleColor.Black, 2, 1, New List(Of RowSpan) From {New RowSpan(0, 0, 1)})
    Dim asBackground = Render(renderer, "x", ConsoleColor.Black, color, 2, 1, New List(Of RowSpan) From {New RowSpan(0, 0, 1)})

    Assert.Contains(Esc & "[" & expectedForeground & ";40m", asForeground)
    Assert.Contains(Esc & "[30;" & expectedBackground & "m", asBackground)
  End Sub

  <Fact>
  Public Sub TrueColorUsesThePalette()
    Dim renderer As New VtRenderer(VtColorMode.TrueColor, synchronizedOutput:=False)

    Dim output = Render(renderer, "x", ConsoleColor.Cyan, ConsoleColor.DarkBlue, 2, 1, New List(Of RowSpan) From {New RowSpan(0, 0, 1)})

    ' VGA: bright cyan 55FFFF on blue 0000AA.
    Assert.Contains(Esc & "[38;2;85;255;255;48;2;0;0;170m", output)
  End Sub

  <Fact>
  Public Sub ACustomPaletteIsUsed()
    Dim colors(15) As Integer
    colors(CInt(ConsoleColor.Red)) = &H102030
    Dim renderer As New VtRenderer(VtColorMode.TrueColor, New VtPalette(colors), synchronizedOutput:=False)

    Dim output = Render(renderer, "x", ConsoleColor.Red, ConsoleColor.Black, 2, 1, New List(Of RowSpan) From {New RowSpan(0, 0, 1)})

    Assert.Contains("38;2;16;32;48", output)
  End Sub

  <Fact>
  Public Sub ColoursAreOnlyWrittenWhereTheyChange()
    Dim renderer As New VtRenderer(VtColorMode.Ansi16, synchronizedOutput:=False)
    Dim chars = "abcd".ToCharArray()
    Dim fg = {ConsoleColor.Cyan, ConsoleColor.Cyan, ConsoleColor.Red, ConsoleColor.Red}
    Dim bg = {ConsoleColor.DarkBlue, ConsoleColor.DarkBlue, ConsoleColor.DarkBlue, ConsoleColor.DarkBlue}

    Dim output = renderer.Render(chars, fg, bg, 4, 1, New List(Of RowSpan) From {New RowSpan(0, 0, 4)})

    Assert.Equal(Esc & "[1;1H" & Esc & "[96;44mab" & Esc & "[91;44mcd" & Esc & "[0m", output)
  End Sub

  <Fact>
  Public Sub EachSpanGetsItsOwnCursorMove()
    Dim renderer As New VtRenderer(VtColorMode.Ansi16, synchronizedOutput:=False)

    Dim output = Render(renderer, "abcdefgh", ConsoleColor.Gray, ConsoleColor.Black, 4, 2,
                        New List(Of RowSpan) From {New RowSpan(0, 1, 3), New RowSpan(1, 2, 4)})

    Assert.Equal(Esc & "[1;2H" & Esc & "[37;40mbc" & Esc & "[2;3H" & "gh" & Esc & "[0m", output)
  End Sub

  <Fact>
  Public Sub NothingMeansEveryRowIsRedrawn()
    Dim renderer As New VtRenderer(VtColorMode.Ansi16, synchronizedOutput:=False)

    Dim output = Render(renderer, "", ConsoleColor.Gray, ConsoleColor.Black, 3, 4, Nothing)

    For row = 1 To 4
      Assert.Contains(Esc & "[" & row & ";1H", output)
    Next
  End Sub

  <Fact>
  Public Sub FramesAreWrappedInSynchronizedOutputMarkers()
    Dim renderer As New VtRenderer(VtColorMode.Ansi16)

    Dim output = Render(renderer, "x", ConsoleColor.Gray, ConsoleColor.Black, 2, 1, Nothing)

    Assert.StartsWith(Esc & "[?2026h", output)
    Assert.EndsWith(Esc & "[?2026l", output)
  End Sub

  <Fact>
  Public Sub UnicodeBoxCharactersPassThrough()
    Dim renderer As New VtRenderer(VtColorMode.Ansi16, synchronizedOutput:=False)

    Dim output = Render(renderer, ChrW(&H250C) & ChrW(&H2500) & ChrW(&H2510), ConsoleColor.Gray, ConsoleColor.Black, 3, 1, Nothing)

    Assert.Contains(ChrW(&H250C) & ChrW(&H2500) & ChrW(&H2510), output)
  End Sub

  <Fact>
  Public Sub ColorModeFollowsTheEnvironment()
    Dim old = Environment.GetEnvironmentVariable("QUICKVB_COLORS")
    Try
      Environment.SetEnvironmentVariable("QUICKVB_COLORS", "16")
      Assert.Equal(VtColorMode.Ansi16, VtRenderer.DetectColorMode())
      Environment.SetEnvironmentVariable("QUICKVB_COLORS", "truecolor")
      Assert.Equal(VtColorMode.TrueColor, VtRenderer.DetectColorMode())
    Finally
      Environment.SetEnvironmentVariable("QUICKVB_COLORS", old)
    End Try
  End Sub

  ' ---- VtConsole

  Private Class VtScreen
    Inherits Screen

    Public ReadOnly Property Title As New Label With {.Left = 2, .Top = 1, .Width = 30, .Text = "Hello, ConsoleGUI"}
    Public ReadOnly Property Status As New Label With {.Left = 0, .Top = 24, .Width = 80, .Text = "Ready"}

    Public Sub New(driver As IConsoleDriver)
      MyBase.New(driver)
      Me.Controls.Add(Me.Title)
      Me.Controls.Add(Me.Status)
    End Sub

    Public Sub Run()
      NavigateTo(Me)
    End Sub

  End Class

  Private Shared Function Text(stream As MemoryStream) As String
    Return Encoding.UTF8.GetString(stream.ToArray())
  End Function

  Private Shared Function Count(haystack As String, needle As String) As Integer
    Dim n = 0
    Dim i = haystack.IndexOf(needle, StringComparison.Ordinal)
    Do While i >= 0
      n += 1
      i = haystack.IndexOf(needle, i + needle.Length, StringComparison.Ordinal)
    Loop
    Return n
  End Function

  <Fact>
  Public Sub ScreenDrawsThroughTheVtDriverEnterAlternateScreenAndRestoreOnDispose()
    Dim stream As New MemoryStream()
    Dim input As New HeadlessConsole(80, 25)
    Dim driver As New VtConsole(input, stream, New VtRenderer(VtColorMode.TrueColor), Function() New Dimensions(80, 25))
    Dim ui As New VtScreen(driver)

    ui.Run()
    driver.Dispose()

    Dim output = Text(stream)
    Assert.StartsWith(VtRenderer.EnterScreen, output)
    Assert.Contains("Hello, ConsoleGUI", output)
    Assert.Contains(Esc & "[1;1H", output)           ' the first frame draws every row, top ...
    Assert.Contains(Esc & "[25;1H", output)          ' ... to bottom
    Assert.Contains("48;2;0;0;0", output)            ' true colour from the VGA palette
    Assert.EndsWith(VtRenderer.LeaveScreen, output)
  End Sub

  <Fact>
  Public Sub LaterFramesOnlyWriteWhatChanged()
    Dim stream As New MemoryStream()
    Dim input As New HeadlessConsole(80, 25)
    Dim driver As New VtConsole(input, stream, New VtRenderer(VtColorMode.Ansi16), Function() New Dimensions(80, 25))
    Dim ui As New VtScreen(driver)

    ui.Post(Sub()
              ui.Status.Text = "Stop!"
              ui.Invalidate()
            End Sub)
    ui.Run()

    Dim output = Text(stream)
    Assert.Equal(1, Count(output, "Hello, ConsoleGUI"))   ' drawn once, never repeated
    Assert.Equal(1, Count(output, "Stop!"))
    Assert.Contains(Esc & "[25;1H", output)               ' the changed span starts at the status line
  End Sub

  <Fact>
  Public Sub BufferSizeEventsReportTheVisibleWindowSize()
    Dim stream As New MemoryStream()
    Dim input As New HeadlessConsole(80, 25)
    Dim window As New Dimensions(100, 30)
    Dim driver As New VtConsole(input, stream, New VtRenderer(VtColorMode.Ansi16), Function() window)
    Dim ui As New VtScreen(driver)
    Dim resized As Dimensions = Nothing
    AddHandler ui.Resize, Sub(sender, e) resized = e.Dimensions

    input.QueueEvent(New BufferSizeEvent(9001, 9001))   ' what the console input reports: the (huge) buffer
    ui.Run()

    Assert.Equal(100, resized.Width)
    Assert.Equal(30, resized.Height)
    Assert.Equal(100, ui.Width)
  End Sub

  <Fact>
  Public Sub CursorVisibilityIsSentAsEscapeSequences()
    Dim stream As New MemoryStream()
    Dim input As New HeadlessConsole(80, 25)
    Dim driver As New VtConsole(input, stream, New VtRenderer(VtColorMode.Ansi16), Function() New Dimensions(80, 25))

    driver.SetCursorVisible(False)
    driver.SetCursorVisible(True)
    driver.SetCursorPosition(4, 2)

    Assert.Equal(VtRenderer.HideCursor & VtRenderer.ShowCursor & Esc & "[3;5H", Text(stream))
  End Sub

End Class
