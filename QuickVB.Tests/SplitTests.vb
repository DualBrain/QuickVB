Option Explicit On
Option Strict On
Option Infer On

Imports System.IO
Imports ConsoleGUI
Imports Xunit

' View > Split: two editor windows, each with its own file (or the same one).
Public Class SplitTests
  Implements IDisposable

  Private ReadOnly _root As String = Path.Combine(Path.GetTempPath(), "QuickVB.Split." & Guid.NewGuid().ToString("N"))

  Private NotInheritable Class Session
    Implements IDisposable

    Public ReadOnly Console As New HeadlessConsole(100, 30)
    Public ReadOnly Ui As MyScreen

    Public Sub New(mainPath As String)
      Me.Ui = New MyScreen(Me.Console)
      Dim ui2 = Me.Ui
      Me.Ui.Post(Sub()
                   ui2.OpenProgram(mainPath)
                   ui2.EnableRoslyn()
                 End Sub)
      Me.Run()
    End Sub

    Public Sub Run()
      Screen.NavigateTo(Me.Ui)
    End Sub

    Public Sub Do_(action As Action)
      Me.Ui.Post(action)
      Me.Run()
    End Sub

    Public Sub Press(key As VirtualKey, Optional state As ControlKeyState = 0)
      Me.Console.QueueKey(key, If(key = VirtualKey.VK_RETURN, ChrW(13), ChrW(0)), state)
      Me.Run()
    End Sub

    Public Sub Type(text As String)
      Me.Console.QueueText(text)
      Me.Run()
    End Sub

    Public Sub Menu(ParamArray letters() As String)
      Me.Console.QueueKey(VirtualKey.VK_MENU)
      For Each letter In letters
        Me.Console.QueueText(letter)
      Next
      Me.Run()
    End Sub

    Public Function Screen_() As String
      Return Me.Console.GetText()
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
      Me.Ui.EndSession()
    End Sub
  End Class

  Private ReadOnly _mainPath As String
  Private ReadOnly _helperPath As String

  Public Sub New()
    Directory.CreateDirectory(Me._root)
    Me._mainPath = Path.Combine(Me._root, "Main.vb")
    Me._helperPath = Path.Combine(Me._root, "Helper.vb")
    File.WriteAllText(Me._mainPath, "Module MainModule" & vbCrLf & "  Sub Main()" & vbCrLf & "    Greeter.Hello()" & vbCrLf & "  End Sub" & vbCrLf & "End Module")
    File.WriteAllText(Me._helperPath, "Module Greeter" & vbCrLf & "  Sub Hello()" & vbCrLf & "    Console.WriteLine(""hi"")" & vbCrLf & "  End Sub" & vbCrLf & "End Module")
  End Sub

  Public Sub Dispose() Implements IDisposable.Dispose
    Try
      Directory.Delete(Me._root, True)
    Catch ex As IOException
    End Try
  End Sub

  ' Main.vb and Helper.vb are loaded; Main.vb is in the window
  Private Function Started() As Session
    Dim s As New Session(Me._mainPath)
    s.Do_(Sub() s.Ui.LoadFile(Me._helperPath))
    s.Do_(Sub() s.Ui.ShowSwitchToDialogForTests(0))
    Return s
  End Function

  Private Shared Function Occurrences(text As String, what As String) As Integer
    Return text.Split({what}, StringSplitOptions.None).Length - 1
  End Function

  ' View > Switch to... and choose the second file of the list
  Private Shared Sub SwitchToHelper(s As Session)
    s.Menu("v", "w")
    s.Press(VirtualKey.VK_DOWN)
    s.Press(VirtualKey.VK_RETURN)
  End Sub

  <Fact>
  Public Sub SplitOpensASecondWindowOnTheSameFile()
    Dim s = Me.Started()

    s.Menu("v", "p")                                         ' View > Split

    Assert.True(s.Ui.IsSplit)
    Assert.Equal("Main.vb", s.Ui.SplitFileName)
    Assert.Equal(2, Occurrences(s.Screen_(), " Main.vb "))   ' the title of each window
    Assert.Equal(s.Ui.UpperView.Buffer.Text, s.Ui.SplitEditor.Text)
    Assert.True(s.Ui.SplitView.Height > 2)
    Assert.True(s.Ui.UpperView.Height > 2)
  End Sub

  <Fact>
  Public Sub TheLowerWindowCanShowAnotherFile()
    Dim s = Me.Started()
    s.Menu("v", "p")

    s.Press(VirtualKey.VK_F6)                                ' to the lower window
    Assert.Same(s.Ui.SplitView, s.Ui.ActiveControl)

    SwitchToHelper(s)

    Assert.Equal("Helper.vb", s.Ui.SplitFileName)
    Assert.Contains("Module Greeter", s.Ui.SplitEditor.Text)
    Assert.Contains("Module MainModule", s.Ui.UpperView.Buffer.Text)
    Assert.Contains(" Helper.vb ", s.Screen_())
    Assert.Contains(" Main.vb ", s.Screen_())
  End Sub

  <Fact>
  Public Sub WhatIsTypedInOneWindowAppearsInTheOtherWhenTheyShowTheSameFile()
    Dim s = Me.Started()
    s.Menu("v", "p")

    s.Type("xyz")

    Assert.StartsWith("xyz", s.Ui.UpperView.Buffer.Text.TrimStart())
    Assert.Equal(s.Ui.UpperView.Buffer.Text, s.Ui.SplitEditor.Text)

    s.Press(VirtualKey.VK_F6)
    s.Press(VirtualKey.VK_BACK)
    Assert.Equal(s.Ui.UpperView.Buffer.Text, s.Ui.SplitEditor.Text)
  End Sub

  <Fact>
  Public Sub TypingInTheLowerWindowDoesNotTouchADifferentFile()
    Dim s = Me.Started()
    s.Menu("v", "p")
    s.Press(VirtualKey.VK_F6)
    SwitchToHelper(s)

    Dim mainText = s.Ui.UpperView.Buffer.Text
    s.Type("q")

    Assert.StartsWith("q", s.Ui.SplitEditor.Text.TrimStart())
    Assert.Equal(mainText, s.Ui.UpperView.Buffer.Text)
  End Sub

  <Fact>
  Public Sub ClosingTheSplitKeepsTheFileOfTheWindowWithTheFocus()
    Dim s = Me.Started()
    s.Menu("v", "p")
    s.Press(VirtualKey.VK_F6)
    SwitchToHelper(s)                                        ' Helper.vb below, and the focus there

    s.Menu("v", "p")                                         ' close

    Assert.False(s.Ui.IsSplit)
    Assert.Equal("Helper.vb", s.Ui.ActiveFileName)
    Assert.Contains("Module Greeter", s.Ui.UpperView.Buffer.Text)
    Assert.Same(s.Ui.UpperView, s.Ui.ActiveControl)
    Assert.Equal(1, Occurrences(s.Screen_(), " Helper.vb "))
  End Sub

  <Fact>
  Public Sub ClickingAWindowGivesItTheFocus()
    Dim s = Me.Started()
    s.Menu("v", "p")
    Assert.Same(s.Ui.UpperView, s.Ui.ActiveControl)

    Dim row = s.Ui.SplitView.ScreenTop + 1
    s.Console.QueueMouse(s.Ui.SplitView.ScreenLeft + 3, row, ButtonState.FROM_LEFT_1ST_BUTTON_PRESSED)
    s.Console.QueueMouse(s.Ui.SplitView.ScreenLeft + 3, row, 0)
    s.Run()

    Assert.Same(s.Ui.SplitView, s.Ui.ActiveControl)
    Assert.Equal(1, s.Ui.SplitView.CursorRow)
    Assert.Equal(3, s.Ui.SplitView.CursorColumn)
  End Sub

  <Fact>
  Public Sub F6CyclesThroughBothEditorsAndTheOutputWindow()
    Dim s = Me.Started()
    s.Menu("v", "p")

    s.Press(VirtualKey.VK_F6)
    Assert.Same(s.Ui.SplitView, s.Ui.ActiveControl)
    s.Press(VirtualKey.VK_F6)
    Assert.NotSame(s.Ui.SplitView, s.Ui.ActiveControl)
    Assert.NotSame(s.Ui.UpperView, s.Ui.ActiveControl)
    s.Press(VirtualKey.VK_F6)
    Assert.Same(s.Ui.UpperView, s.Ui.ActiveControl)
  End Sub

  <Fact>
  Public Sub BreakpointsShowInBothWindowsOfTheSameFile()
    Dim s = Me.Started()
    s.Menu("v", "p")

    s.Ui.UpperView.MoveCursor(2, 0)
    s.Press(VirtualKey.VK_F9)

    Assert.True((s.Ui.UpperView.Buffer.Lines(2).Marks And LineMarks.Breakpoint) <> 0)
    Assert.True((s.Ui.SplitEditor.Lines(2).Marks And LineMarks.Breakpoint) <> 0)
  End Sub

  <Fact>
  Public Sub AltPlusAndMinusMoveTheBorderBetweenTheWindows()
    Dim s = Me.Started()
    s.Menu("v", "p")
    Dim upper = s.Ui.UpperView.Height
    Dim lower = s.Ui.SplitView.Height

    s.Press(VirtualKey.VK_OEM_PLUS, ControlKeyState.LEFT_ALT_PRESSED)   ' the upper window is active: it grows

    Assert.Equal(upper + 1, s.Ui.UpperView.Height)
    Assert.Equal(lower - 1, s.Ui.SplitView.Height)
  End Sub

  <Fact>
  Public Sub ChangesMadeInTheLowerWindowAreSavedWithTheProgram()
    Dim s = Me.Started()
    s.Menu("v", "p")
    s.Press(VirtualKey.VK_F6)
    SwitchToHelper(s)
    s.Type("x")

    s.Do_(Sub() s.Ui.SaveAll())

    Assert.Contains("x", File.ReadAllText(Me._helperPath))
  End Sub

  <Fact>
  Public Sub AFileTakenOutOfTheProgramLeavesTheLowerWindow()
    Dim s = Me.Started()
    s.Menu("v", "p")
    s.Press(VirtualKey.VK_F6)
    SwitchToHelper(s)                                        ' Helper.vb below
    s.Press(VirtualKey.VK_F6)
    s.Press(VirtualKey.VK_F6)                                ' back to the upper window

    s.Menu("f", "u")                                         ' File > Unload File
    s.Press(VirtualKey.VK_DOWN)                              ' Helper.vb
    s.Press(VirtualKey.VK_RETURN)

    Assert.Equal({"Main.vb"}, s.Ui.FileNames.ToArray())
    Assert.Equal("Main.vb", s.Ui.SplitFileName)
  End Sub

End Class
