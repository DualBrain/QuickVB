Option Strict On
Option Infer On

Imports System.Text.RegularExpressions
Imports ConsoleGUI
Imports QuickVB
Imports Xunit

Public Class HelpContentTests

  Private Shared ReadOnly LinkPattern As New Regex("\[\[[^|\]]+\|(?<target>[^\]]+)\]\]")
  Private Shared ReadOnly TextLinkPattern As New Regex("\[\[(?<text>[^|\]]+)\|[^\]]+\]\]")

  <Fact>
  Public Sub EveryTopicInTheOrderExistsAndEveryTopicIsInTheOrder()
    For Each id In HelpContent.Order
      Assert.NotNull(HelpContent.Find(id))
    Next
    Assert.Equal(HelpContent.Order.Length, HelpContent.Topics.Count)
    Assert.Equal(HelpContent.Order.Length, HelpContent.Order.Distinct().Count())
    For Each topic In HelpContent.Topics
      Assert.Contains(topic.Id, HelpContent.Order)
    Next
  End Sub

  <Fact>
  Public Sub EveryLinkLeadsToATopic()
    For Each topic In HelpContent.Topics
      For Each item In topic.Nav
        Assert.True(HelpContent.Find(item.Target) IsNot Nothing, topic.Id & " nav link " & item.Text)
      Next
      For Each line In topic.Lines
        For Each m As Match In LinkPattern.Matches(line)
          Assert.True(HelpContent.Find(m.Groups("target").Value) IsNot Nothing, topic.Id & " inline link " & m.Value)
        Next
      Next
    Next
  End Sub

  <Fact>
  Public Sub EachTopicNamesItselfOnceInItsLinkBarAndHasATitle()
    For Each topic In HelpContent.Topics
      Assert.False(String.IsNullOrWhiteSpace(topic.Title), topic.Id)
      Assert.Equal(1, topic.Nav.Where(Function(n) n.IsCurrent).Count())
      Assert.Equal(topic.Id, topic.Nav.Single(Function(n) n.IsCurrent).Target)
    Next
  End Sub

  <Fact>
  Public Sub ContentsAndIndexReachEveryTopic()
    Dim contents = String.Join(vbLf, HelpContent.Find(HelpContent.ContentsId).Lines)
    Dim index = String.Join(vbLf, HelpContent.Find(HelpContent.IndexId).Lines)

    For Each topic In HelpContent.Topics
      If topic.Id = HelpContent.ContentsId OrElse topic.Id = HelpContent.IndexId Then Continue For
      Assert.Contains("|" & topic.Id & "]]", index)
    Next

    ' the contents lists the top level topics; the editing keys pages are reached through "Editing Keys"
    For Each id In {HelpContent.ShortcutId, HelpContent.EditingId, HelpContent.ViewKeysId, HelpContent.SearchKeysId, HelpContent.DebuggingId,
                    HelpContent.HelpKeysId, HelpContent.CommandLineId, HelpContent.HelpOnHelpId, HelpContent.SupportId}
      Assert.Contains("|" & id & "]]", contents)
    Next
    Dim editing = String.Join(vbLf, HelpContent.Find(HelpContent.EditingId).Lines)
    For Each id In {HelpContent.CursorId, HelpContent.ScrollingId, HelpContent.SelectingId, HelpContent.InsertCopyId, HelpContent.DeleteId}
      Assert.Contains("|" & id & "]]", editing)
    Next
  End Sub

  <Fact>
  Public Sub NoLineIsWiderThanTheWindow()
    ' help is drawn in an 80 column window with a border and a scroll bar: 77 columns of text
    For Each topic In HelpContent.Topics
      For Each line In topic.Lines
        Dim shown = TextLinkPattern.Replace(line, Function(m) New String("x"c, m.Groups("text").Length + 2))
        shown = shown.Replace("{{", "").Replace("}}", "")
        Assert.True(shown.Length <= 77, topic.Id & ": " & line & " (" & shown.Length & ")")
      Next
    Next
  End Sub

  <Theory>
  <InlineData("cursor", "Character left", "CTRL+S")>
  <InlineData("cursor", "Beginning of module/procedure", "CTRL+Q+R")>
  <InlineData("cursor", "Set place marker number 'n' (where n = 0-3)", "CTRL+K+n")>
  <InlineData("scrolling", "Page down", "CTRL+C")>
  <InlineData("selecting", "To end of module/procedure", "SHIFT+CTRL+END")>
  <InlineData("insertcopy", "Insert blank line above", "HOME+CTRL+N")>
  <InlineData("delete", "Delete leading spaces from selected lines", "SHIFT+TAB")>
  <InlineData("viewkeys", "Toggle between multiple windows and full screen active window", "CTRL+F10")>
  <InlineData("searchkeys", "Repeat the last find", "F3")>
  <InlineData("debugkeys", "Trace execution history forward", "SHIFT+F10")>
  <InlineData("helpkeys", "Look at help topic stored previously in the Help file", "SHIFT+CTRL+F1")>
  <InlineData("shortcut", "Selected text (no save)", "DEL")>
  <InlineData("shortcut", "Instant Watch", "SHIFT+F9")>
  Public Sub TheKeyReferenceHasTheKeysFromQuickBasicsHelp(topicId As String, label As String, keys As String)
    Dim line = HelpContent.Find(topicId).Lines.FirstOrDefault(Function(l) l.Contains(label))
    Assert.NotNull(line)
    Assert.Contains(keys, line.Substring(line.IndexOf(label) + label.Length))
  End Sub

  <Fact>
  Public Sub TheCommandLinePageDocumentsTheOptionsQuickVBHas()
    Dim text = String.Join(vbLf, HelpContent.Find(HelpContent.CommandLineId).Lines)
    Assert.Contains(CommandLineOptions.Usage.Replace("Usage: ", ""), text)
    For Each name In {"program", "--run", "--colors", "--classic", "--help", "--version", "QUICKVB_COLORS", "QUICKVB_RENDERER"}
      Assert.Contains(name, text)
    Next
  End Sub

End Class

Public Class CommandLineOptionsTests

  Private Shared Function Run(ParamArray args() As String) As (Result As CommandLineOptions.Result, Output As String, Errors As String)
    Dim output As New IO.StringWriter()
    Dim errors As New IO.StringWriter()
    Dim result = CommandLineOptions.Parse(args, output, errors)
    Return (result, output.ToString(), errors.ToString())
  End Function

  <Fact>
  Public Sub NoArgumentsMeansPlainStart()
    Dim r = Run()

    Assert.Equal(0, r.Result.ExitCode)
    Assert.NotNull(r.Result.Options)
    Assert.Null(r.Result.Options.ProgramName)
    Assert.False(r.Result.Options.RunProgram)
    Assert.Equal("", r.Result.Options.CommandText)
  End Sub

  <Fact>
  Public Sub ABareArgumentIsTheProgram()
    Dim o = Run("gorilla.bas").Result.Options

    Assert.Equal("gorilla.bas", o.ProgramName)
    Assert.False(o.RunProgram)
  End Sub

  <Theory>
  <InlineData("--run")>
  <InlineData("-r")>
  Public Sub RunAsksForTheProgramToBeRunFirst(flag As String)
    Dim before = Run(flag, "prog.vb").Result.Options
    Dim after = Run("prog.vb", flag).Result.Options

    For Each o In {before, after}
      Assert.True(o.RunProgram)
      Assert.Equal("prog.vb", o.ProgramName)
    Next
  End Sub

  <Fact>
  Public Sub RunWithoutAProgramIsAnError()
    Dim r = Run("--run")

    Assert.Null(r.Result.Options)
    Assert.Equal(1, r.Result.ExitCode)
    Assert.Contains("--run needs a program to run", r.Errors)
  End Sub

  <Fact>
  Public Sub WhatFollowsTheProgramIsTheTextForCommand()
    Dim o = Run("prog.vb", "one", "two", "three four").Result.Options

    Assert.Equal("prog.vb", o.ProgramName)
    Assert.Equal("one two three four", o.CommandText)
  End Sub

  <Fact>
  Public Sub ADoubleDashLetsTheCommandTextStartWithADash()
    Dim o = Run("prog.vb", "--", "--flag", "-x").Result.Options

    Assert.Equal("prog.vb", o.ProgramName)
    Assert.Equal("--flag -x", o.CommandText)
  End Sub

  <Fact>
  Public Sub ColoursAreChosenWithTheColorsOption()
    Dim sixteen = Run("--colors", "16").Result.Options
    Dim truecolor = Run("--colors", "truecolor").Result.Options

    Assert.True(sixteen.SixteenColors)
    Assert.False(sixteen.TrueColors)
    Assert.True(truecolor.TrueColors)
    Assert.False(truecolor.SixteenColors)
  End Sub

  <Fact>
  Public Sub AnotherColourModeIsRefused()
    Dim r = Run("--colors", "purple")

    Assert.Null(r.Result.Options)
    Assert.Equal(1, r.Result.ExitCode)
    Assert.Contains("purple", r.Errors)
  End Sub

  <Fact>
  Public Sub ClassicSelectsTheClassicWindowsRenderer()
    Assert.True(Run("--classic").Result.Options.ClassicRenderer)
    Assert.False(Run().Result.Options.ClassicRenderer)
  End Sub

  <Fact>
  Public Sub AnUnknownOptionIsRefusedAndNamed()
    Dim r = Run("--nohi", "a.vb")

    Assert.Null(r.Result.Options)
    Assert.Equal(1, r.Result.ExitCode)
    Assert.Contains("--nohi", r.Errors)
  End Sub

  <Theory>
  <InlineData("--help")>
  <InlineData("-h")>
  <InlineData("-?")>
  Public Sub HelpListsTheOptions(flag As String)
    Dim r = Run(flag)

    Assert.Null(r.Result.Options)
    Assert.Equal(0, r.Result.ExitCode)
    For Each expected In {"--run", "-r", "--colors", "--classic", "--help", "--version", "program", "COMMAND$"}
      Assert.Contains(expected, r.Output)
    Next
  End Sub

  <Fact>
  Public Sub VersionIsTheVersionOfTheProgram()
    Dim r = Run("--version")

    Assert.Null(r.Result.Options)
    Assert.Equal(0, r.Result.ExitCode)
    Assert.StartsWith(GetType(CommandLineOptions).Assembly.GetCustomAttributes(GetType(System.Reflection.AssemblyInformationalVersionAttribute), False).Cast(Of System.Reflection.AssemblyInformationalVersionAttribute)().First().InformationalVersion.Split("+"c)(0), r.Output.Trim())
  End Sub

  <Fact>
  Public Sub ApplyingTheDisplayOptionsSetsTheEnvironmentVariablesTheScreenReads()
    Dim oldColors = Environment.GetEnvironmentVariable("QUICKVB_COLORS")
    Dim oldRenderer = Environment.GetEnvironmentVariable("QUICKVB_RENDERER")
    Try
      Environment.SetEnvironmentVariable("QUICKVB_COLORS", Nothing)
      Environment.SetEnvironmentVariable("QUICKVB_RENDERER", Nothing)

      Run("--colors", "16", "--classic").Result.Options.ApplyEnvironment()

      Assert.Equal("16", Environment.GetEnvironmentVariable("QUICKVB_COLORS"))
      Assert.Equal("win32", Environment.GetEnvironmentVariable("QUICKVB_RENDERER"))
    Finally
      Environment.SetEnvironmentVariable("QUICKVB_COLORS", oldColors)
      Environment.SetEnvironmentVariable("QUICKVB_RENDERER", oldRenderer)
    End Try
  End Sub

End Class

' The help window inside the editor.
Public Class HelpFlowTests

  Private NotInheritable Class Session
    Public ReadOnly Console As New HeadlessConsole(80, 25)
    Public ReadOnly Ui As MyScreen

    Public Sub New()
      Me.Ui = New MyScreen(Me.Console)
      Dim ui2 = Me.Ui
      Me.Ui.Post(Sub()
                   App.NewUntitledWorkspace()
                   ui2.ViewDocument("Untitled", "Untitled.vb", "' Untitled" & vbCrLf & "Module Untitled" & vbCrLf & "  Sub Main()" & vbCrLf & "  End Sub" & vbCrLf & "End Module", title:="Untitled")
                   ui2.EnableRoslyn()
                 End Sub)
      Me.Run()
    End Sub

    Public Sub Run()
      Screen.NavigateTo(Me.Ui)
    End Sub

    Public Sub Press(key As VirtualKey, Optional state As ControlKeyState = 0)
      Me.Console.QueueKey(key, If(key = VirtualKey.VK_RETURN, ChrW(13), ChrW(0)), state)
      Me.Run()
    End Sub

    Public Sub Menu(ParamArray letters() As String)
      Me.Console.QueueKey(VirtualKey.VK_MENU)
      For Each letter In letters
        Me.Console.QueueText(letter)
      Next
      Me.Run()
    End Sub

    Public Function Text() As String
      Return Me.Console.GetText()
    End Function

    Public Function Row(index As Integer) As String
      Return Me.Console.RowText(index)
    End Function

  End Class

  <Fact>
  Public Sub F1OnAKeywordOpensItsTopic()
    Dim s As New Session()
    s.Ui.EditorView.MoveCursor(2, 4)         ' on "Sub" of "  Sub Main()"
    s.Press(VirtualKey.VK_F1)

    Assert.True(s.Ui.Help.Visible)
    Assert.Equal("kw:SUB", s.Ui.Help.CurrentTopic.Id)
    Assert.Contains("SUB Statement", s.Text())
  End Sub

  <Fact>
  Public Sub F1OpensTheContentsAboveTheEditorAndEscapeClosesIt()
    Dim s As New Session()
    Assert.False(s.Ui.Help.Visible)

    s.Press(VirtualKey.VK_F1)

    Assert.True(s.Ui.Help.Visible)
    Assert.Contains(" HELP: Contents ", s.Text())
    Assert.Contains("Shortcut Keys", s.Text())
    Assert.Contains("Untitled", s.Text())                           ' the editor window is still there, under the help
    Assert.Contains("<Shift+F1=Help> <F6=Window> <Esc=Cancel> <Ctrl+F1=Next> <Alt+F1=Back>", s.Row(24))
    Assert.Same(s.Ui.Help, s.Ui.ActiveControl)

    s.Press(VirtualKey.VK_ESCAPE)

    Assert.False(s.Ui.Help.Visible)
    Assert.DoesNotContain("HELP:", s.Text())
    Assert.Contains("Module Untitled", s.Text())
    Assert.Contains("<F5=Run>", s.Row(24))
    Assert.Same(s.Ui.EditorView, s.Ui.ActiveControl)
  End Sub

  <Fact>
  Public Sub TheHelpWindowIsAsTallAsItsTextAndTheEditorMovesDown()
    Dim s As New Session()

    s.Ui.ShowHelp(HelpContent.SearchKeysId)
    s.Run()

    ' two lines of text: the minimum height of 8 rows, so the editor window's title is on row 9
    Assert.Equal(8, s.Ui.Help.Height)
    Assert.Equal(1, s.Ui.Help.Top)
    Assert.Contains("Untitled", s.Row(9))
    Assert.Contains("HELP: Search Keys", s.Row(1))
    ' and the rule under the link bar carries the topic's title
    Assert.Contains("Search Keys", s.Row(3))
  End Sub

  <Fact>
  Public Sub ATallTopicTakesNearlyTheWholeScreen()
    Dim s As New Session()

    s.Ui.ShowHelp(HelpContent.ShortcutId)
    s.Run()

    Assert.Equal(20, s.Ui.Help.Height)                              ' 16 rows of boxes + 4
    Assert.Contains("Inserting", s.Text())
    Assert.Contains("Debugging", s.Text())
    Assert.Contains("Instant Watch", s.Text())
    Assert.Contains("SHIFT+F9", s.Text())
    Assert.Contains("CTRL+Q+Y", s.Text())
  End Sub

  <Fact>
  Public Sub ShiftF1ShowsHelpOnHelpAndF1InsideHelpDoesToo()
    Dim s As New Session()

    s.Press(VirtualKey.VK_F1, ControlKeyState.SHIFT_PRESSED)
    Assert.Contains(" HELP: Help on Help ", s.Text())
    Assert.Contains(ChrW(&H25A0) & " To get help at any time", s.Text())

    s.Press(VirtualKey.VK_ESCAPE)
    s.Press(VirtualKey.VK_F1)
    Assert.Contains(" HELP: Contents ", s.Text())
    s.Press(VirtualKey.VK_F1)                                       ' F1 while looking at help
    Assert.Contains(" HELP: Help on Help ", s.Text())
  End Sub

  <Fact>
  Public Sub TheHelpMenuOpensTheTopics()
    Dim s As New Session()

    s.Menu("h", "i")
    Assert.Contains(" HELP: Index ", s.Text())
    s.Press(VirtualKey.VK_ESCAPE)

    s.Menu("h", "c")
    Assert.Contains(" HELP: Contents ", s.Text())
    s.Press(VirtualKey.VK_ESCAPE)

    s.Menu("h", "h")
    Assert.Contains(" HELP: Help on Help ", s.Text())
    s.Press(VirtualKey.VK_ESCAPE)

    s.Menu("h", "t")
    Assert.Contains(" HELP: Contents ", s.Text())
  End Sub

  <Fact>
  Public Sub LinksCanBeFollowedAndTheWayBackWorks()
    Dim s As New Session()

    s.Press(VirtualKey.VK_F1)
    s.Press(VirtualKey.VK_RETURN)                                   ' the first link of the Contents: Index
    Assert.Contains(" HELP: Index ", s.Text())

    s.Press(VirtualKey.VK_TAB)                                      ' Contents is the only link in the bar ... then the entries
    s.Press(VirtualKey.VK_TAB)
    s.Press(VirtualKey.VK_RETURN)
    Assert.DoesNotContain(" HELP: Index ", s.Text())

    s.Press(VirtualKey.VK_F1, ControlKeyState.LEFT_ALT_PRESSED)    ' Alt+F1: back
    Assert.Contains(" HELP: Index ", s.Text())
  End Sub

  <Fact>
  Public Sub CtrlF1StepsToTheNextTopic()
    Dim s As New Session()

    s.Press(VirtualKey.VK_F1)
    s.Press(VirtualKey.VK_F1, ControlKeyState.LEFT_CTRL_PRESSED)
    Assert.Contains(" HELP: Index ", s.Text())
    s.Press(VirtualKey.VK_F1, ControlKeyState.LEFT_CTRL_PRESSED)
    Assert.Contains(" HELP: Help on Help ", s.Text())
  End Sub

  <Fact>
  Public Sub F6CyclesThroughHelpTheEditorAndTheOutputPane()
    Dim s As New Session()
    s.Press(VirtualKey.VK_F1)
    Assert.Same(s.Ui.Help, s.Ui.ActiveControl)

    s.Press(VirtualKey.VK_F6)
    Assert.Same(s.Ui.EditorView, s.Ui.ActiveControl)
    Assert.True(s.Ui.Help.Visible)                                  ' help stays open while you edit

    s.Press(VirtualKey.VK_F6)
    Assert.IsType(Of TextBufferView)(s.Ui.ActiveControl)
    Assert.NotSame(s.Ui.EditorView, s.Ui.ActiveControl)

    s.Press(VirtualKey.VK_F6)
    Assert.Same(s.Ui.Help, s.Ui.ActiveControl)
    Assert.Contains("<Esc=Cancel>", s.Row(24))
  End Sub

  <Fact>
  Public Sub F6WithoutHelpStillSwitchesBetweenTheEditorAndTheOutput()
    Dim s As New Session()

    s.Press(VirtualKey.VK_F6)
    Assert.NotSame(s.Ui.EditorView, s.Ui.ActiveControl)
    s.Press(VirtualKey.VK_F6)
    Assert.Same(s.Ui.EditorView, s.Ui.ActiveControl)
  End Sub

  <Fact>
  Public Sub TheEditorWorksNormallyAfterHelpIsClosed()
    Dim s As New Session()
    s.Press(VirtualKey.VK_F1)
    s.Press(VirtualKey.VK_ESCAPE)

    s.Ui.Post(Sub() s.Ui.EditorView.MoveCursor(1, 0))
    s.Run()
    s.Console.QueueText("x")
    s.Run()

    Assert.StartsWith("x", s.Ui.Editor.Lines(1).Text)
    Assert.Contains("<F5=Run>", s.Row(24))
  End Sub

  <Fact>
  Public Sub CtrlF10MaximizesTheHelpWindow()
    Dim s As New Session()
    s.Ui.ShowHelp(HelpContent.CommandLineId)
    s.Run()
    Dim normal = s.Ui.Help.Height

    s.Press(VirtualKey.VK_F10, ControlKeyState.LEFT_CTRL_PRESSED)

    Assert.True(s.Ui.Help.Maximized)
    Assert.True(s.Ui.Help.Height > normal)
    Assert.Equal(23, s.Ui.Help.Height)                              ' everything between the menu bar and the status line
    Assert.DoesNotContain("Module Untitled", s.Text())              ' the editor is hidden behind it

    s.Press(VirtualKey.VK_F10, ControlKeyState.LEFT_CTRL_PRESSED)
    Assert.False(s.Ui.Help.Maximized)
    Assert.Contains("Untitled", s.Text())
  End Sub

  <Fact>
  Public Sub HelpFollowsTheWindowSize()
    Dim s As New Session()
    s.Ui.ShowHelp(HelpContent.CommandLineId)
    s.Run()

    s.Console.QueueResize(120, 40)
    s.Run()
    Assert.Equal(120, s.Ui.Help.Width)
    Assert.True(s.Ui.Help.Height > 12)
    Assert.Contains(" HELP: QuickVB Command Line ", s.Text())

    s.Console.QueueResize(80, 25)
    s.Run()
    Assert.Equal(80, s.Ui.Help.Width)
  End Sub

  <Fact>
  Public Sub ClosingHelpGivesTheEditorItsRowsBack()
    Dim s As New Session()
    Dim before = s.Ui.EditorView.Height

    s.Press(VirtualKey.VK_F1)
    Assert.True(s.Ui.EditorView.Height < before)

    s.Press(VirtualKey.VK_ESCAPE)
    Assert.Equal(before, s.Ui.EditorView.Height)
  End Sub

  <Fact>
  Public Sub DialogsStillWorkWhileHelpIsOpen()
    Dim s As New Session()
    s.Press(VirtualKey.VK_F1)

    s.Menu("s", "f")                                                ' Search > Find

    Assert.Contains("Find What:", s.Text())
    s.Press(VirtualKey.VK_ESCAPE)
    Assert.Contains(" HELP: Contents ", s.Text())
    Assert.True(s.Ui.Help.Visible)
  End Sub

End Class
