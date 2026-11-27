Option Strict On
Option Infer On

Imports System.IO
Imports ConsoleGUI
Imports QuickVB
Imports Xunit

' End-to-end: the whole QuickVB screen running on a HeadlessConsole.
' Screen.GetScreen is static, so these tests must not run in parallel with each other (they live in one class).
Public Class OpenProgramTests
  Implements IDisposable

  Private Const TopLeftCorner As Char = ChrW(&H250C)
  Private Const TopRightCorner As Char = ChrW(&H2510)

  Private ReadOnly _root As String
  Private ReadOnly _programPath As String

  Public Sub New()
    Me._root = Path.Combine(Path.GetTempPath(), "QuickVB.Tests." & Guid.NewGuid().ToString("N"))
    Directory.CreateDirectory(Me._root)
    Me._programPath = Path.Combine(Me._root, "sample.bas")
    File.WriteAllText(Me._programPath, "PRINT ""from sample.bas""" & vbCrLf)
  End Sub

  Public Sub Dispose() Implements IDisposable.Dispose
    Try
      Directory.Delete(Me._root, True)
    Catch ex As IOException
    End Try
  End Sub

  ''' <summary>A MyScreen whose first queued event creates the Untitled workspace and document and enables Roslyn, like App.Main does.</summary>
  Private Shared Function StartScreen(console As HeadlessConsole) As MyScreen
    Dim ui As New MyScreen(console)
    ui.Post(Sub()
              App.NewUntitledWorkspace()
              ui.ViewDocument("Untitled", "Untitled.vb", "Module Untitled" & vbCrLf & "End Module")
              ui.EnableRoslyn()
            End Sub)
    Return ui
  End Function

  ' With Roslyn on, a finished background compile retitles the pane from "Output" to "Diagnostics" at some point.
  Private Shared Function IsOutputPaneTitle(row As String) As Boolean
    Return row.Contains("Output") OrElse row.Contains("Diagnostics")
  End Function

  <Fact>
  Public Sub KeysBeforeTheFirstDocumentExistsAreHarmless()
    Dim console As New HeadlessConsole(80, 25)
    Dim ui As New MyScreen(console)    ' no workspace or document yet, as at the very start of the app

    console.QueueText("abc")
    console.QueueKey(VirtualKey.VK_MENU)
    Screen.NavigateTo(ui)

    Assert.Contains("File", console.RowText(0))
  End Sub

  Private Shared Function PlatformName() As String
    If OperatingSystem.IsWindows() Then Return "Windows"
    If OperatingSystem.IsLinux() Then Return "Linux"
    If OperatingSystem.IsMacOS() Then Return "macOS"
    Return "your platform"
  End Function

  ' Like StartScreen, but the welcome box is shown too, as at a real start-up.
  Private Shared Function StartScreenWithWelcome(console As HeadlessConsole) As MyScreen
    Dim ui As New MyScreen(console)
    ui.Post(Sub()
              App.NewUntitledWorkspace()
              ui.ViewDocument("Untitled", "Untitled.vb", "Module Untitled" & vbCrLf & "End Module", title:="Untitled")
              ui.EnableRoslyn()
              ui.ShowWelcome()
            End Sub)
    Return ui
  End Function

  <Fact>
  Public Sub StartsWithTheWelcomeBoxForThisPlatform()
    Dim console As New HeadlessConsole(80, 25)
    Dim ui = StartScreenWithWelcome(console)

    Screen.NavigateTo(ui)

    Dim text = console.GetText()
    Assert.Contains("Welcome to QuickVB for " & PlatformName(), text)
    Assert.Contains("Copyright (C) Dartmouth Didn't, 1964-2026.", text)
    Assert.Contains("All rights reserved.", text)
    Assert.Contains("< Press Enter to see the Survival Guide >", text)
    Assert.Contains("< Press ESC to clear this dialog box >", text)
    Assert.NotNull(ui.PopupControl)
    ' the editor window behind it is called "Untitled", not "Untitled.vb"
    Assert.Contains("Untitled", console.RowText(1))
    Assert.DoesNotContain("Untitled.vb", text)
    ' and the status bar shows the dialog's hint
    Assert.Contains("Enter=Execute", console.RowText(24))
    Assert.Contains("Arrow=Next Item", console.RowText(24))
  End Sub

  <Fact>
  Public Sub EscapeClearsTheWelcomeBoxAndRestoresTheStatusBar()
    Dim console As New HeadlessConsole(80, 25)
    Dim ui = StartScreenWithWelcome(console)

    console.QueueKey(VirtualKey.VK_ESCAPE)
    Screen.NavigateTo(ui)

    Assert.Null(ui.PopupControl)
    Assert.DoesNotContain("Welcome to QuickVB", console.GetText())
    Assert.Contains("<F5=Run>", console.RowText(24))
    Assert.DoesNotContain("Enter=Execute", console.RowText(24))
    Assert.Contains("Module Untitled", console.GetText())
  End Sub

  <Fact>
  Public Sub EnterClearsTheWelcomeBoxAndOpensTheSurvivalGuide()
    Dim console As New HeadlessConsole(80, 25)
    Dim ui = StartScreenWithWelcome(console)

    console.QueueKey(VirtualKey.VK_RETURN, ChrW(13))
    Screen.NavigateTo(ui)

    Assert.Null(ui.PopupControl)
    Assert.DoesNotContain("Welcome to QuickVB", console.GetText())
    Assert.Contains("QuickVB Survival Guide", console.GetText())
    Assert.DoesNotContain("not available", console.GetText())
  End Sub

  <Fact>
  Public Sub TypingWhileTheWelcomeBoxIsOpenDoesNotEditTheProgram()
    Dim console As New HeadlessConsole(80, 25)
    Dim ui = StartScreenWithWelcome(console)

    console.QueueText("zzz")
    console.QueueKey(VirtualKey.VK_ESCAPE)
    Screen.NavigateTo(ui)

    Assert.DoesNotContain("zzz", console.GetText())
    Assert.Contains("Module Untitled", console.GetText())
  End Sub

  <Fact>
  Public Sub TheOpenDialogShowsTheSameStatusHint()
    Dim console As New HeadlessConsole(80, 25)
    Dim ui = StartScreen(console)

    console.QueueKey(VirtualKey.VK_MENU)
    console.QueueText("f")
    console.QueueText("o")
    Screen.NavigateTo(ui)
    Assert.Contains("Tab=Next Field", console.RowText(24))

    console.QueueKey(VirtualKey.VK_ESCAPE)
    Screen.NavigateTo(ui)
    Assert.Contains("<F5=Run>", console.RowText(24))
  End Sub

  <Fact>
  Public Sub StartsWithMenuBarEditorAndStatusLine()
    Dim console As New HeadlessConsole(80, 25)
    Dim ui = StartScreen(console)

    Screen.NavigateTo(ui)

    Dim text = console.GetText()
    Assert.Contains("File", console.RowText(0))
    Assert.Contains("Help", console.RowText(0))
    Assert.Contains("Untitled.vb", text)
    Assert.Contains("Module Untitled", text)
    Assert.Contains("<F5=Run>", text)
  End Sub

  <Fact>
  Public Sub ChromeFollowsTheWindowSize()
    Dim console As New HeadlessConsole(120, 40)
    Dim ui = StartScreen(console)

    Screen.NavigateTo(ui)

    ' Menu bar spans the width with Help anchored at the right edge.
    Assert.EndsWith("Help", console.RowText(0).TrimEnd())
    Assert.True(console.RowText(0).TrimEnd().Length >= 115)
    ' The editor frame spans the width; the status line is on the last row.
    Assert.Equal(TopLeftCorner, console.CharAt(0, 1))
    Assert.Equal(TopRightCorner, console.CharAt(119, 1))
    Assert.Contains("<F5=Run>", console.RowText(39))
    Assert.Contains("00001:001", console.RowText(39))
    Assert.True(IsOutputPaneTitle(console.RowText(36)), console.RowText(36))

    ' Make the window smaller, then back to the original size: everything is laid out again each time.
    console.QueueResize(100, 30)
    Screen.NavigateTo(ui)
    Assert.EndsWith("Help", console.RowText(0).TrimEnd())
    Assert.Equal(100, console.RowText(0).Length)
    Assert.True(console.RowText(0).TrimEnd().Length >= 95)
    Assert.Contains("<F5=Run>", console.RowText(29))
    Assert.True(IsOutputPaneTitle(console.RowText(26)), console.RowText(26))

    console.QueueResize(80, 25)
    Screen.NavigateTo(ui)
    Assert.Contains("<F5=Run>", console.RowText(24))
    Assert.True(IsOutputPaneTitle(console.RowText(21)), console.RowText(21))
    Assert.True(console.GetText().Contains("Module Untitled"), console.GetText())
  End Sub

  ' "Module" is the first word of the editor text (row 2, column 1, inside the frame).
  Private Shared Function KeywordColor(console As HeadlessConsole) As ConsoleColor
    Assert.Equal("Module", console.RowText(2).Substring(1, 6))
    Return console.ForegroundAt(1, 2)
  End Function

  <Fact>
  Public Sub RoslynIsOnByDefaultSoKeywordsAreColoured()
    Dim console As New HeadlessConsole(80, 25)
    Dim ui = StartScreen(console)

    Screen.NavigateTo(ui)

    Assert.Equal(ConsoleColor.Cyan, KeywordColor(console))
  End Sub

  <Fact>
  Public Sub OptionsMenuTogglesRoslynOffAndOn()
    Dim console As New HeadlessConsole(80, 25)
    Dim ui = StartScreen(console)

    console.QueueKey(VirtualKey.VK_MENU)
    console.QueueText("o")   ' Options
    Screen.NavigateTo(ui)
    Assert.Contains("Disable Roslyn", console.GetText())

    console.QueueText("r")   ' Disable Roslyn
    Screen.NavigateTo(ui)
    Assert.Equal(ConsoleColor.Gray, KeywordColor(console))

    console.QueueKey(VirtualKey.VK_MENU)
    console.QueueText("o")
    Screen.NavigateTo(ui)
    Assert.Contains("Enable Roslyn", console.GetText())

    console.QueueText("r")   ' Enable Roslyn
    Screen.NavigateTo(ui)
    Assert.Equal(ConsoleColor.Cyan, KeywordColor(console))
  End Sub

  <Fact>
  Public Sub FileOpenShowsTheDialogAndLoadsTheChosenProgram()
    Dim console As New HeadlessConsole(80, 25)
    Dim ui = StartScreen(console)

    console.QueueKey(VirtualKey.VK_MENU)   ' activate the menu bar
    console.QueueText("f")                 ' File
    console.QueueText("o")                 ' Open Program...
    Screen.NavigateTo(ui)

    Assert.Contains("File Name:", console.GetText())
    Assert.Contains("File Name:", console.GetText())
    Assert.NotNull(ui.PopupControl)

    ' Type the full path and press Enter.
    console.QueueText(Me._programPath)
    console.QueueKey(VirtualKey.VK_RETURN, ChrW(13))
    Screen.NavigateTo(ui)

    Dim text = console.GetText()
    Assert.Null(ui.PopupControl)
    Assert.DoesNotContain("File Name:", text)
    Assert.Contains("sample.bas", text)
    Assert.Contains("from sample.bas", text)
  End Sub

  <Fact>
  Public Sub CancellingTheOpenDialogKeepsTheCurrentProgram()
    Dim console As New HeadlessConsole(80, 25)
    Dim ui = StartScreen(console)

    console.QueueKey(VirtualKey.VK_MENU)
    console.QueueText("f")
    console.QueueText("o")
    Screen.NavigateTo(ui)
    Assert.NotNull(ui.PopupControl)

    console.QueueKey(VirtualKey.VK_ESCAPE)
    Screen.NavigateTo(ui)

    Dim text = console.GetText()
    Assert.Null(ui.PopupControl)
    Assert.DoesNotContain("File Name:", text)
    Assert.Contains("Untitled.vb", text)
    Assert.Contains("Module Untitled", text)
  End Sub

End Class
