Option Explicit On
Option Strict On
Option Infer On

Imports ConsoleGUI
Imports Xunit

' The debugger as the user meets it: F8, F10, F7, F5, F9, the watches. The program runs for real in a process of its own.
Public Class DebugUiTests

  Private Const Program As String =
    "Module Demo" & vbCrLf &                        ' 0
    "  Dim total As Integer" & vbCrLf &             ' 1
    "  Sub Main()" & vbCrLf &                       ' 2
    "    Dim x As Integer = 1" & vbCrLf &           ' 3
    "    x = x + 1" & vbCrLf &                      ' 4
    "    Add(x)" & vbCrLf &                         ' 5
    "    x = x + 10" & vbCrLf &                     ' 6
    "    Console.WriteLine(total)" & vbCrLf &       ' 7
    "  End Sub" & vbCrLf &                          ' 8
    "  Sub Add(n As Integer)" & vbCrLf &            ' 9
    "    total = total + n" & vbCrLf &              ' 10
    "  End Sub" & vbCrLf &                          ' 11
    "End Module"

  Private NotInheritable Class Session
    Implements IDisposable

    Public ReadOnly Console As New HeadlessConsole(100, 30)
    Public ReadOnly Ui As MyScreen

    Public Sub New(source As String)
      Me.Ui = New MyScreen(Me.Console)
      Me.Ui.WaitAfterProgram = Sub() Exit Sub
      Dim ui2 = Me.Ui
      Me.Ui.Post(Sub()
                   App.NewUntitledWorkspace()
                   ui2.ViewDocument("Untitled", "Untitled.vb", source, title:="Untitled")
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

    ''' <summary>The program runs on its own thread and process: keeps handling what it reports until the condition holds.</summary>
    Public Sub WaitUntil(condition As Func(Of Boolean), what As String)
      Dim limit = Date.UtcNow.AddSeconds(40)
      Do
        Me.Run()
        If condition() Then Return
        If Date.UtcNow > limit Then Throw New TimeoutException("Gave up waiting for " & what & Environment.NewLine & Me.Console.GetText())
        Threading.Thread.Sleep(50)
      Loop
    End Sub

    Public Sub WaitForPause()
      Me.WaitUntil(Function() Me.Ui.IsPaused, "the program to stop")
    End Sub

    Public Sub WaitForEnd()
      Me.WaitUntil(Function() Not Me.Ui.HasDebugSession, "the program to end")
    End Sub

    Public Function Text() As String
      Return Me.Console.GetText()
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
      Me.Ui.EndSession()
    End Sub

  End Class

  Private Shared Function IsMarked(s As Session, row As Integer) As Boolean
    Return (s.Ui.Editor.Lines(row).Marks And LineMarks.CurrentStatement) <> 0
  End Function

  ' Roslyn re-indents the sample, so a line is looked up by what is on it
  Private Shared Function RowOf(s As Session, text As String) As Integer
    Return s.Ui.Editor.Lines.Select(Function(l) l.Text).ToList().FindIndex(Function(l) l.Contains(text))
  End Function

  <Fact>
  Public Sub F8StepsThroughTheProgramAStatementAtATime()
    Using s As New Session(Program)

      s.Press(VirtualKey.VK_F8)
      s.WaitForPause()

      Dim first = RowOf(s, "Dim x As Integer")
      Assert.Equal(first, s.Ui.EditorView.CursorRow)
      Assert.True(IsMarked(s, first))
      Assert.Contains("F5=Continue", s.Text())

      s.Press(VirtualKey.VK_F8)
      s.WaitUntil(Function() s.Ui.IsPaused AndAlso s.Ui.EditorView.CursorRow = first + 1, "the second statement")
      Assert.False(IsMarked(s, first))
      Assert.True(IsMarked(s, first + 1))

      s.Press(VirtualKey.VK_F5)                          ' let it finish
      s.WaitForEnd()
      Assert.False(s.Ui.IsPaused)
      Assert.False(IsMarked(s, first + 1))
    End Using
  End Sub

  <Fact>
  Public Sub TheTextCannotBeChangedWhileTheProgramIsStopped()
    Using s As New Session(Program)

      s.Press(VirtualKey.VK_F8)
      s.WaitForPause()

      Dim before = s.Ui.Editor.Text
      s.Type("zz")
      Assert.Equal(before, s.Ui.Editor.Text)

      s.Press(VirtualKey.VK_F5)
      s.WaitForEnd()

      s.Type("zz")
      Assert.NotEqual(before, s.Ui.Editor.Text)
    End Using
  End Sub

  <Fact>
  Public Sub F10StepsOverAProcedureCall()
    Using s As New Session(Program)

      Dim call_ = RowOf(s, "Add(x)")
      s.Ui.EditorView.MoveCursor(call_, 0)
      s.Press(VirtualKey.VK_F7)                          ' run to the cursor
      s.WaitForPause()
      Assert.Equal(call_, s.Ui.EditorView.CursorRow)

      s.Press(VirtualKey.VK_F10)
      s.WaitUntil(Function() s.Ui.IsPaused AndAlso s.Ui.EditorView.CursorRow <> call_, "the next statement")
      Assert.Equal(call_ + 1, s.Ui.EditorView.CursorRow)

      s.Press(VirtualKey.VK_F5)
      s.WaitForEnd()
    End Using
  End Sub

  <Fact>
  Public Sub F8StepsIntoAProcedure()
    Using s As New Session(Program)

      Dim call_ = RowOf(s, "Add(x)")
      s.Ui.EditorView.MoveCursor(call_, 0)
      s.Press(VirtualKey.VK_F7)
      s.WaitForPause()

      s.Press(VirtualKey.VK_F8)
      Dim inside = RowOf(s, "total = total + n")
      s.WaitUntil(Function() s.Ui.IsPaused AndAlso s.Ui.EditorView.CursorRow = inside, "the statement inside Add")

      s.Press(VirtualKey.VK_F5)
      s.WaitForEnd()
    End Using
  End Sub

  <Fact>
  Public Sub F9TogglesABreakpointAndF5RunsToIt()
    Using s As New Session(Program)

      Dim target = RowOf(s, "x = x + 10")
      s.Ui.EditorView.MoveCursor(target, 0)
      s.Press(VirtualKey.VK_F9)
      Assert.True((s.Ui.Editor.Lines(target).Marks And LineMarks.Breakpoint) <> 0)

      s.Ui.EditorView.MoveCursor(0, 0)
      s.Press(VirtualKey.VK_F5)                          ' there is a breakpoint: the debugger is used
      s.WaitForPause()
      Assert.Equal(target, s.Ui.EditorView.CursorRow)

      s.Press(VirtualKey.VK_F5)
      s.WaitForEnd()

      ' F9 again takes it off
      s.Ui.EditorView.MoveCursor(target, 0)
      s.Press(VirtualKey.VK_F9)
      Assert.True((s.Ui.Editor.Lines(target).Marks And LineMarks.Breakpoint) = 0)
    End Using
  End Sub

  <Fact>
  Public Sub BreakpointsStayWithTheirLinesWhenTextIsAddedAbove()
    Using s As New Session(Program)

      Dim target = RowOf(s, "x = x + 10")
      s.Ui.EditorView.MoveCursor(target, 0)
      s.Press(VirtualKey.VK_F9)

      ' a blank line above (Ctrl+N at the start of the file)
      s.Ui.EditorView.MoveCursor(0, 0)
      s.Console.QueueKey(VirtualKey.VK_N, state:=ControlKeyState.LEFT_CTRL_PRESSED)
      s.Run()

      Assert.True((s.Ui.Editor.Lines(RowOf(s, "x = x + 10")).Marks And LineMarks.Breakpoint) <> 0)
    End Using
  End Sub

  <Fact>
  Public Sub ClearAllBreakpointsTakesThemAllOff()
    Using s As New Session(Program)

      s.Ui.EditorView.MoveCursor(RowOf(s, "x = x + 10"), 0)
      s.Press(VirtualKey.VK_F9)
      s.Menu("d", "c")                                   ' Debug > Clear All Breakpoints

      Assert.True(s.Ui.Editor.Lines.All(Function(l) (l.Marks And LineMarks.Breakpoint) = 0))
    End Using
  End Sub

  <Fact>
  Public Sub WatchExpressionsShowTheirValuesWhileTheProgramIsStopped()
    Using s As New Session(Program)

      s.Ui.EditorView.MoveCursor(RowOf(s, "x = x + 10"), 0)
      s.Press(VirtualKey.VK_F7)
      s.WaitForPause()

      s.Menu("d", "a")                                   ' Debug > Add Watch
      s.Type("x * 2")
      s.Press(VirtualKey.VK_RETURN)
      s.Type("")

      Assert.Contains("x * 2: 4", s.Text())
      Assert.Contains(" Watch ", s.Text())

      s.Press(VirtualKey.VK_F5)
      s.WaitForEnd()
      Assert.DoesNotContain("x * 2: 4", s.Text())        ' the pane shows the diagnostics again
    End Using
  End Sub

  <Fact>
  Public Sub InstantWatchShowsTheValueUnderTheCursor()
    Using s As New Session(Program)

      Dim row = RowOf(s, "x = x + 10")
      s.Ui.EditorView.MoveCursor(row, 0)
      s.Press(VirtualKey.VK_F7)
      s.WaitForPause()

      s.Press(VirtualKey.VK_F9, ControlKeyState.SHIFT_PRESSED)

      Assert.Contains("Instant Watch", s.Text())
      Assert.Contains("Expression:  x", s.Text())
      Assert.Contains("Value:       2", s.Text())

      s.Press(VirtualKey.VK_ESCAPE)
      s.Press(VirtualKey.VK_F5)
      s.WaitForEnd()
    End Using
  End Sub

  <Fact>
  Public Sub AWatchpointStopsTheProgramWhenItBecomesTrue()
    Using s As New Session(Program)

      s.Menu("d", "w")                                   ' Debug > Watchpoint
      s.Type("total > 0")
      s.Press(VirtualKey.VK_RETURN)

      s.Press(VirtualKey.VK_F5)
      s.WaitForPause()

      ' total was changed by the statement in Add, so the program stops at the statement after it
      Assert.Contains("Watchpoint: total > 0", s.Text())
      Assert.Equal(RowOf(s, "x = x + 10"), s.Ui.EditorView.CursorRow)

      s.Press(VirtualKey.VK_F5)
      s.WaitUntil(Function() Not s.Ui.HasDebugSession OrElse s.Ui.IsPaused, "the next stop or the end")
      ' every statement now tests true: it keeps stopping until the end
      Do While s.Ui.IsPaused
        s.Press(VirtualKey.VK_F5)
        s.WaitUntil(Function() Not s.Ui.HasDebugSession OrElse s.Ui.IsPaused, "the next stop or the end")
      Loop
    End Using
  End Sub

  <Fact>
  Public Sub DeleteWatchAndDeleteAllWatchRemoveExpressions()
    Using s As New Session(Program)

      s.Menu("d", "a")
      s.Type("x")
      s.Press(VirtualKey.VK_RETURN)
      s.Menu("d", "a")
      s.Type("total")
      s.Press(VirtualKey.VK_RETURN)

      s.Menu("d", "d")                                   ' Debug > Delete Watch
      Assert.Contains("Choose the watch expression to delete", s.Text())
      s.Press(VirtualKey.VK_RETURN)

      s.Menu("d", "a")                                   ' Debug > Delete All Watch
      Assert.Contains(" Add Watch ", s.Text())
      s.Press(VirtualKey.VK_ESCAPE)
    End Using
  End Sub

  <Fact>
  Public Sub AProgramWithAnErrorIsNotStarted()
    Using s As New Session("Module Demo" & vbCrLf & "  Sub Main()" & vbCrLf & "    Dim x As Integer = " & vbCrLf & "  End Sub" & vbCrLf & "End Module")

      s.Press(VirtualKey.VK_F8)

      Assert.Contains("Syntax error", s.Text())
      Assert.False(s.Ui.HasDebugSession)
    End Using
  End Sub

  <Fact>
  Public Sub RestartStartsOverStoppedAtTheFirstStatement()
    Using s As New Session(Program)

      s.Press(VirtualKey.VK_F8)
      s.WaitForPause()
      s.Press(VirtualKey.VK_F8)
      s.WaitUntil(Function() s.Ui.IsPaused AndAlso s.Ui.EditorView.CursorRow > RowOf(s, "Dim x As Integer"), "the second statement")

      s.Menu("r", "r")                                   ' Run > Restart
      s.WaitUntil(Function() s.Ui.IsPaused AndAlso s.Ui.EditorView.CursorRow = RowOf(s, "Dim x As Integer"), "the first statement again")

      s.Press(VirtualKey.VK_F5)
      s.WaitForEnd()
    End Using
  End Sub

  <Fact>
  Public Sub TheCallsMenuListsTheRunningProceduresAndShowsTheOneChosen()
    Using s As New Session(Program)

      Dim inside = RowOf(s, "total = total + n")
      s.Ui.EditorView.MoveCursor(inside, 0)
      s.Press(VirtualKey.VK_F7)
      s.WaitForPause()
      Assert.Equal(inside, s.Ui.EditorView.CursorRow)

      s.Console.QueueKey(VirtualKey.VK_MENU)
      s.Console.QueueText("c")                            ' the Calls menu
      s.Run()
      Assert.Contains("1 Add", s.Text())
      Assert.Contains("2 Main", s.Text())

      s.Type("2")                                          ' Main: where it called Add
      Assert.Equal(RowOf(s, "Add(x)"), s.Ui.EditorView.CursorRow)

      s.Press(VirtualKey.VK_F5)
      s.WaitForEnd()
    End Using
  End Sub

  <Fact>
  Public Sub ViewNextStatementReturnsToTheStatementTheProgramStoppedAt()
    Using s As New Session(Program)

      s.Press(VirtualKey.VK_F8)
      s.WaitForPause()
      Dim stopped = s.Ui.EditorView.CursorRow

      s.Ui.EditorView.MoveCursor(0, 0)
      s.Menu("v", "n")                                     ' View > Next Statement
      Assert.Equal(stopped, s.Ui.EditorView.CursorRow)

      s.Press(VirtualKey.VK_F5)
      s.WaitForEnd()
    End Using
  End Sub

  <Fact>
  Public Sub BreakOnErrorsStopsAtAnErrorTheProgramDoesNotHandle()
    Using s As New Session("Module Demo" & vbCrLf & "  Sub Main()" & vbCrLf & "    Dim x As Integer = 3" & vbCrLf & "    Throw New InvalidOperationException(""boom"")" & vbCrLf & "  End Sub" & vbCrLf & "End Module")

      s.Menu("d", "e")                                     ' Debug > Break on Errors
      Assert.Contains("Break on Errors is on", s.Text())

      s.Press(VirtualKey.VK_F8)
      s.WaitForPause()
      s.Press(VirtualKey.VK_F5)
      s.WaitUntil(Function() s.Ui.IsPaused AndAlso s.Text().Contains("boom"), "the error")

      Assert.Contains("boom", s.Text())
      Assert.Equal(RowOf(s, "Throw New"), s.Ui.EditorView.CursorRow)

      s.Press(VirtualKey.VK_RETURN)                        ' the message box
      s.Press(VirtualKey.VK_F5)
      s.WaitForEnd()
    End Using
  End Sub

  Private Shared Sub WithNoDotnet(action As Action)
    QuickVBWorkspace.DotnetHostOverride = IO.Path.Combine(IO.Path.GetTempPath(), "quickvb-no-such-dotnet-" & Guid.NewGuid().ToString("N"))
    Try
      action()
    Finally
      QuickVBWorkspace.DotnetHostOverride = Nothing
    End Try
  End Sub

  <Fact>
  Public Sub ARunWithoutDotnetSaysSoInsteadOfCrashing()
    Using s As New Session("Module Demo" & vbCrLf & "  Sub Main()" & vbCrLf & "  End Sub" & vbCrLf & "End Module")

      WithNoDotnet(Sub() s.Press(VirtualKey.VK_F5, ControlKeyState.SHIFT_PRESSED))

      Assert.Contains("Unable to start 'dotnet'", s.Text())
      Assert.False(s.Ui.HasDebugSession)
    End Using
  End Sub

  <Fact>
  Public Sub DebuggingWithoutDotnetSaysSoInsteadOfCrashing()
    Using s As New Session("Module Demo" & vbCrLf & "  Sub Main()" & vbCrLf & "  End Sub" & vbCrLf & "End Module")

      WithNoDotnet(Sub() s.Press(VirtualKey.VK_F8))

      Assert.Contains("Unable to start 'dotnet'", s.Text())
      Assert.False(s.Ui.HasDebugSession)
    End Using
  End Sub

End Class
