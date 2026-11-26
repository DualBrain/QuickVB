Option Strict On
Option Infer On

Imports ConsoleGUI
Imports Xunit

Public Class HelpWindowTests

  Private Const LeftArrow As Char = ChrW(&H25C4)
  Private Const RightArrow As Char = ChrW(&H25BA)

  Private Shared Function Topics() As Dictionary(Of String, HelpTopic)

    Dim many As New List(Of String) From {"First line of Alpha", "<hr>", "See [[Gamma topic|c]] and {{bold words}} here."}
    For i = 4 To 30
      many.Add("Alpha line " & i)
    Next
    many.Add("<hr:Titled>")
    many.Add("The very last line")

    Dim result As New Dictionary(Of String, HelpTopic)()
    result("a") = New HelpTopic("a", "Alpha", {New HelpNavItem("Alpha", "a", True), New HelpNavItem("Beta", "b"), New HelpNavItem("Index", "i")}, many)
    result("b") = New HelpTopic("b", "Beta", {New HelpNavItem("Alpha", "a"), New HelpNavItem("Beta", "b", True)}, {"Beta body", "[[Back to alpha|a]]"})
    result("c") = New HelpTopic("c", "Gamma", {New HelpNavItem("Gamma", "c", True)}, {"Gamma body"})
    result("u") = New HelpTopic("u", "Online", {New HelpNavItem("Online", "u", True)}, {"Read [[the docs|https://example.com/page]] now."})
    result("i") = New HelpTopic("i", "Index", {New HelpNavItem("Index", "i", True)}, {"Index body"}, ruleTitle:="Index Rule")
    Return result

  End Function

  Private Class HelpScreen
    Inherits Screen

    Public ReadOnly Property Help As New HelpWindow()
    Public Property Closed As Integer

    Public Sub New(driver As IConsoleDriver, Optional height As Integer = 14, Optional openOn As String = "a")
      MyBase.New(driver)
      Dim all = Topics()
      Me.Help.Topics = Function(id) If(all.ContainsKey(id), all(id), Nothing)
      Me.Help.TopicOrder = {"a", "b", "c", "i"}
      Me.Help.OnClose = Sub() Me.Closed += 1
      Me.Help.Left = 0
      Me.Help.Top = 1
      Me.Help.Width = driver.GetBufferDimensions().Width
      Me.Help.Height = height
      Me.Help.Visible = True
      Me.Help.Open(openOn)
      Me.Controls.Add(Me.Help)
      Me.ActiveControl = Me.Help
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

  Private Shared Sub Key(console As HeadlessConsole, key As VirtualKey, Optional state As ControlKeyState = 0, Optional ch As Char = ChrW(0))
    console.QueueKey(key, ch, state)
  End Sub

  ' ---- drawing

  <Fact>
  Public Sub AnAddressIsOpenedInTheBrowserAndTheTopicStays()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New HelpScreen(console, openOn:="u")
    Dim opened As New List(Of String)()
    screen.Help.OnOpenUrl = Sub(url) opened.Add(url)

    Key(console, VirtualKey.VK_RETURN)
    screen.Run()

    Assert.Equal({"https://example.com/page"}, opened)
    Assert.Equal("u", screen.Help.CurrentTopic.Id)
  End Sub

  <Fact>
  Public Sub DrawsTheTitleLinkBarRuleAndText()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New HelpScreen(console)

    screen.Run()

    Dim text = console.GetText()
    Assert.Contains(" HELP: Alpha ", text)
    Assert.Contains(LeftArrow & "Alpha" & RightArrow, text)
    Assert.Contains(LeftArrow & "Beta" & RightArrow, text)
    Assert.Contains(LeftArrow & "Index" & RightArrow, text)
    Assert.Contains("First line of Alpha", text)

    ' title in reverse video on the top border, the rule under the link bar, the scroll bars
    Dim title = Find(console, " HELP: Alpha ")
    Assert.Equal(1, title.Row)
    Assert.Equal(ConsoleColor.Black, console.ForegroundAt(title.Column + 2, title.Row))
    Assert.Equal(ConsoleColor.Gray, console.BackgroundAt(title.Column + 2, title.Row))
    Assert.Equal(ChrW(&H251C), console.CharAt(0, 3))         ' rule under the link bar
    Assert.Equal(ChrW(&H2191), console.CharAt(79, 2))        ' vertical scroll bar arrows
    Assert.Equal(ChrW(&H2193), console.CharAt(79, 13))
    Assert.Equal(ChrW(&H2190), console.CharAt(0, 14))        ' horizontal scroll bar along the bottom
    Assert.Equal(ChrW(&H2192), console.CharAt(79, 14))
    Assert.Equal(ConsoleColor.Black, console.BackgroundAt(10, 6))   ' black window
  End Sub

  <Fact>
  Public Sub TheCurrentTopicIsWhiteAndOtherLinksHaveGreenArrows()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New HelpScreen(console)

    screen.Run()

    Dim current = Find(console, LeftArrow & "Alpha" & RightArrow)
    Assert.Equal(ConsoleColor.White, console.ForegroundAt(current.Column, current.Row))
    Assert.Equal(ConsoleColor.White, console.ForegroundAt(current.Column + 2, current.Row))

    Dim beta = Find(console, LeftArrow & "Beta" & RightArrow)
    Assert.Equal(ConsoleColor.Green, console.ForegroundAt(beta.Column, beta.Row))
    Assert.Equal(ConsoleColor.Green, console.ForegroundAt(beta.Column + 5, beta.Row))
  End Sub

  <Fact>
  Public Sub RulesAndTitledRulesAreDrawnAcrossTheWindow()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New HelpScreen(console)

    screen.Run()
    ' content row 1 is "<hr>": first content row is Top + 3 = 4, so the rule is on row 5
    Assert.Equal(ChrW(&H251C), console.CharAt(0, 5))
    Assert.Equal(ChrW(&H2500), console.CharAt(40, 5))

    Dim index As New HelpScreen(New HeadlessConsole(80, 25), openOn:="i")
    Dim c2 = DirectCast(index.Help, HelpWindow)
    Assert.Equal("Index Rule", c2.CurrentTopic.RuleTitle)
  End Sub

  <Fact>
  Public Sub ARuleTitleIsDrawnInTheRuleUnderTheLinkBar()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New HelpScreen(console, openOn:="i")

    screen.Run()

    Assert.Equal(ChrW(&H251C), console.CharAt(0, 3))
    Assert.Equal("Index Rule", console.RowText(3).Substring((80 - "Index Rule".Length) \ 2, "Index Rule".Length))
  End Sub

  <Fact>
  Public Sub InlineLinksHaveArrowsAndEmphasisIsBright()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New HelpScreen(console)

    screen.Run()

    Dim line = Find(console, "See " & LeftArrow & "Gamma topic" & RightArrow & " and bold words here.")
    Dim arrow = line.Column + 4
    Assert.Equal(ConsoleColor.Green, console.ForegroundAt(arrow, line.Row))
    Dim bold = Find(console, "bold words")
    Assert.Equal(ConsoleColor.White, console.ForegroundAt(bold.Column, bold.Row))
    Dim plain = Find(console, "here.")
    Assert.Equal(ConsoleColor.Gray, console.ForegroundAt(plain.Column, plain.Row))
  End Sub

  <Fact>
  Public Sub ContentRowsCountsTheLines()
    Dim screen As New HelpScreen(New HeadlessConsole(80, 25))
    Assert.Equal(32, screen.Help.ContentRows)           ' 3 + 27 + 2
    Assert.Equal(10, screen.Help.VisibleRows)           ' 14 rows: border, links, rule, 10 text, scroll bar
  End Sub

  ' ---- links and history

  <Fact>
  Public Sub TheFirstLinkHasTheFocusAndTabMovesThroughAllOfThemWrapping()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New HelpScreen(console)

    screen.Run()
    Dim seen As New List(Of String) From {screen.Help.FocusedLinkText}
    For i = 1 To 4
      Key(console, VirtualKey.VK_TAB)
      screen.Run()
      seen.Add(screen.Help.FocusedLinkText)
    Next
    Key(console, VirtualKey.VK_TAB, ControlKeyState.SHIFT_PRESSED)
    screen.Run()
    seen.Add(screen.Help.FocusedLinkText)

    ' link bar (Beta, Index), then the inline link, then round again
    Assert.Equal({"Beta", "Index", "Gamma topic", "Beta", "Index", "Beta"}, seen.ToArray())
  End Sub

  <Fact>
  Public Sub TheFocusedLinkIsShownInReverseVideo()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New HelpScreen(console)

    screen.Run()

    Dim beta = Find(console, LeftArrow & "Beta" & RightArrow)
    Assert.Equal(ConsoleColor.Gray, console.BackgroundAt(beta.Column + 2, beta.Row))     ' reverse: gray behind
    Assert.Equal(ConsoleColor.Black, console.ForegroundAt(beta.Column + 2, beta.Row))
    Dim index = Find(console, LeftArrow & "Index" & RightArrow)
    Assert.Equal(ConsoleColor.Black, console.BackgroundAt(index.Column + 2, index.Row))
  End Sub

  <Fact>
  Public Sub EnterFollowsALinkAndAltF1GoesBack()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New HelpScreen(console)

    Key(console, VirtualKey.VK_RETURN)               ' Beta
    screen.Run()

    Assert.Equal("b", screen.Help.CurrentTopic.Id)
    Assert.Contains(" HELP: Beta ", console.GetText())
    Assert.Contains("Beta body", console.GetText())
    Assert.Equal(1, screen.Help.BackDepth)

    Key(console, VirtualKey.VK_F1, ControlKeyState.LEFT_ALT_PRESSED)
    screen.Run()

    Assert.Equal("a", screen.Help.CurrentTopic.Id)
    Assert.Equal(0, screen.Help.BackDepth)
    Assert.Contains("First line of Alpha", console.GetText())
  End Sub

  <Fact>
  Public Sub BackRemembersTwentyTopics()
    Dim screen As New HelpScreen(New HeadlessConsole(80, 25))

    For i = 1 To 30
      screen.Help.Follow(If(i Mod 2 = 0, "a", "b"))
    Next

    Assert.Equal(20, screen.Help.BackDepth)
    Dim count = 0
    Do While screen.Help.GoBack()
      count += 1
    Loop
    Assert.Equal(20, count)
  End Sub

  <Fact>
  Public Sub CtrlF1StepsThroughTheTopicsInOrder()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New HelpScreen(console)

    Key(console, VirtualKey.VK_F1, ControlKeyState.LEFT_CTRL_PRESSED)
    screen.Run()
    Assert.Equal("b", screen.Help.CurrentTopic.Id)

    Key(console, VirtualKey.VK_F1, ControlKeyState.LEFT_CTRL_PRESSED)
    Key(console, VirtualKey.VK_F1, ControlKeyState.LEFT_CTRL_PRESSED)
    Key(console, VirtualKey.VK_F1, ControlKeyState.LEFT_CTRL_PRESSED)          ' c, i, then wraps to a
    screen.Run()
    Assert.Equal("a", screen.Help.CurrentTopic.Id)

    Key(console, VirtualKey.VK_F1, ControlKeyState.LEFT_CTRL_PRESSED Or ControlKeyState.SHIFT_PRESSED)    ' previous wraps to the last
    screen.Run()
    Assert.Equal("i", screen.Help.CurrentTopic.Id)
  End Sub

  <Fact>
  Public Sub ALetterJumpsToTheNextLinkStartingWithItAndShiftToThePrevious()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New HelpScreen(console)

    console.QueueText("g")                           ' the inline link "Gamma topic"
    screen.Run()
    Assert.Equal("Gamma topic", screen.Help.FocusedLinkText)

    console.QueueText("i")
    screen.Run()
    Assert.Equal("Index", screen.Help.FocusedLinkText)

    console.QueueText("B")                           ' Shift+B: previous link starting with B
    screen.Run()
    Assert.Equal("Beta", screen.Help.FocusedLinkText)
  End Sub

  <Fact>
  Public Sub ClickingALinkFollowsIt()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New HelpScreen(console)

    screen.Run()
    Dim index = Find(console, LeftArrow & "Index" & RightArrow)
    console.QueueClick(index.Column + 3, index.Row)
    screen.Run()

    Assert.Equal("i", screen.Help.CurrentTopic.Id)

  End Sub

  <Fact>
  Public Sub ClickingAnInlineLinkFollowsItToo()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New HelpScreen(console)

    screen.Run()
    Dim gamma = Find(console, LeftArrow & "Gamma topic" & RightArrow)
    console.QueueClick(gamma.Column + 4, gamma.Row)
    screen.Run()

    Assert.True(screen.Help.CurrentTopic.Id = "c", console.GetText())
    Assert.Contains("Gamma body", console.GetText())
  End Sub

  ' ---- scrolling

  <Fact>
  Public Sub TheArrowAndPageKeysScrollTheTextAndClamp()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New HelpScreen(console)

    screen.Run()
    Assert.Equal(0, screen.Help.TopLine)
    Assert.DoesNotContain("The very last line", console.GetText())

    Key(console, VirtualKey.VK_UP)                    ' already at the top
    Key(console, VirtualKey.VK_DOWN)
    Key(console, VirtualKey.VK_DOWN)
    screen.Run()
    Assert.Equal(2, screen.Help.TopLine)

    Key(console, VirtualKey.VK_NEXT)                  ' a page of 10
    screen.Run()
    Assert.Equal(12, screen.Help.TopLine)

    Key(console, VirtualKey.VK_END)
    screen.Run()
    Assert.Equal(32 - 10, screen.Help.TopLine)
    Assert.Contains("The very last line", console.GetText())
    Assert.DoesNotContain("First line of Alpha", console.GetText())

    Key(console, VirtualKey.VK_NEXT)                  ' clamped
    screen.Run()
    Assert.Equal(22, screen.Help.TopLine)

    Key(console, VirtualKey.VK_HOME)
    screen.Run()
    Assert.Equal(0, screen.Help.TopLine)
  End Sub

  <Fact>
  Public Sub TabbingToALinkOffScreenScrollsToIt()
    Dim many As New List(Of String)()
    For i = 1 To 40
      many.Add("filler " & i)
    Next
    many.Add("the link [[Gamma|c]] is far down")

    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New HelpScreen(console)
    Dim all = Topics()
    all("a") = New HelpTopic("a", "Alpha", {New HelpNavItem("Alpha", "a", True)}, many)
    screen.Help.Topics = Function(id) If(all.ContainsKey(id), all(id), Nothing)
    screen.Help.Open("a")

    screen.Run()
    Assert.DoesNotContain("is far down", console.GetText())

    Key(console, VirtualKey.VK_TAB)                  ' (the only link is the inline one; it has the focus already) -> wraps to itself
    screen.Run()
    Assert.Contains("is far down", console.GetText())
    Assert.Equal("Gamma", screen.Help.FocusedLinkText)
  End Sub

  <Fact>
  Public Sub WideTextScrollsSideways()
    Dim all = Topics()
    all("a") = New HelpTopic("a", "Wide", {New HelpNavItem("Wide", "a", True)}, {"start " & New String("x"c, 100) & " THE END"})

    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New HelpScreen(console)
    screen.Help.Topics = Function(id) If(all.ContainsKey(id), all(id), Nothing)
    screen.Help.Open("a")

    screen.Run()
    Assert.StartsWith("start", console.RowText(4).Substring(1))
    Assert.DoesNotContain("THE END", console.GetText())

    For i = 1 To 60
      Key(console, VirtualKey.VK_RIGHT)
    Next
    screen.Run()
    Assert.Contains("THE END", console.GetText())
    Assert.DoesNotContain("start", console.GetText())
  End Sub

  <Fact>
  Public Sub ClickingTheScrollBarArrowsScrolls()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New HelpScreen(console)

    screen.Run()
    console.QueueClick(79, 13)                       ' down arrow (Bottom - 1)
    console.QueueClick(79, 13)
    screen.Run()
    Assert.Equal(2, screen.Help.TopLine)

    console.QueueClick(79, 2)                        ' up arrow
    screen.Run()
    Assert.Equal(1, screen.Help.TopLine)
  End Sub

  ' ---- other keys

  <Fact>
  Public Sub EscapeClosesAndTheStatusHintIsTheHelpKeys()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New HelpScreen(console)

    Assert.Equal("<Shift+F1=Help> <F6=Window> <Esc=Cancel> <Ctrl+F1=Next> <Alt+F1=Back>", screen.Help.StatusHint)

    Key(console, VirtualKey.VK_ESCAPE)
    screen.Run()

    Assert.Equal(1, screen.Closed)
  End Sub

  <Fact>
  Public Sub KeysTheWindowDoesNotUseAreLeftForTheScreen()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New HelpScreen(console)
    Dim seen As New List(Of VirtualKey)()
    AddHandler screen.AfterKeyDown, Sub(sender, e) seen.Add(e.VirtualKey)

    Key(console, VirtualKey.VK_F6)
    Key(console, VirtualKey.VK_F5)
    screen.Run()

    Assert.Contains(VirtualKey.VK_F6, seen)
    Assert.Contains(VirtualKey.VK_F5, seen)
  End Sub

  <Fact>
  Public Sub CtrlF10TogglesMaximized()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New HelpScreen(console)

    Key(console, VirtualKey.VK_F10, ControlKeyState.LEFT_CTRL_PRESSED)
    screen.Run()
    Assert.True(screen.Help.Maximized)

    Key(console, VirtualKey.VK_F10, ControlKeyState.LEFT_CTRL_PRESSED)
    screen.Run()
    Assert.False(screen.Help.Maximized)
  End Sub

  <Fact>
  Public Sub OpeningAnUnknownTopicChangesNothing()
    Dim screen As New HelpScreen(New HeadlessConsole(80, 25))

    Assert.False(screen.Help.Follow("nope"))
    Assert.Equal("a", screen.Help.CurrentTopic.Id)
    Assert.Equal(0, screen.Help.BackDepth)
  End Sub

  <Fact>
  Public Sub ATinyWindowDrawsNothingAndDoesNotThrow()
    Dim console As New HeadlessConsole(30, 8)
    Dim screen As New HelpScreen(console, height:=5)

    screen.Run()

    Assert.DoesNotContain("HELP", console.GetText())
  End Sub

End Class
