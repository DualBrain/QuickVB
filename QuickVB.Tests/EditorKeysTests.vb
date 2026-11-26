Option Explicit On
Option Strict On
Option Infer On

Imports ConsoleGUI
Imports Xunit

' The mouse and the editing keys in the whole QuickVB screen (the editor itself is tested in ConsoleGUI.Tests).
Public Class EditorKeysTests

  Private NotInheritable Class Session
    Public ReadOnly Console As New HeadlessConsole(80, 25)
    Public ReadOnly Ui As MyScreen

    Public Sub New(text As String)
      Me.Ui = New MyScreen(Me.Console)
      Dim ui2 = Me.Ui
      Me.Ui.Post(Sub()
                   App.NewUntitledWorkspace()
                   ui2.ViewDocument("Untitled", "Untitled.vb", text, title:="Untitled")
                   ui2.EnableRoslyn()
                 End Sub)
      Me.Run()
    End Sub

    Public Sub Run()
      Screen.NavigateTo(Me.Ui)
    End Sub

    Public Sub Menu(ParamArray letters() As String)
      Me.Console.QueueKey(VirtualKey.VK_MENU)
      For Each letter In letters
        Me.Console.QueueText(letter)
      Next
      Me.Run()
    End Sub

    Public Sub Do_(action As Action)
      Me.Ui.Post(action)
      Me.Run()
    End Sub

    Public Sub Press(key As VirtualKey, Optional state As ControlKeyState = 0, Optional character As Char = ChrW(0))
      Me.Console.QueueKey(key, character, state)
      Me.Run()
    End Sub

  End Class

  Private Const Sample As String = "Module Untitled" & vbCrLf & "  Sub Main()" & vbCrLf & "    Dim apple = 1" & vbCrLf & "    apple = 2" & vbCrLf & "  End Sub" & vbCrLf & "End Module"

  ' selects the first "apple" (Roslyn re-indents the sample, so the column is looked up) and returns where the next one is
  Private Shared Function SelectFirstApple(s As Session) As Integer
    Dim lines = s.Ui.EditorView.Buffer.Lines
    Dim first = lines(2).Text.IndexOf("apple", StringComparison.Ordinal)
    s.Ui.EditorView.SelectTextRange(2, first, 2, first + 5)
    Assert.Equal("apple", s.Ui.EditorView.SelectedText)
    Return lines(3).Text.IndexOf("apple", StringComparison.Ordinal)
  End Function

  <Fact>
  Public Sub ControlBackslashSearchesForTheSelectedText()
    Dim s As New Session(Sample)
    Dim nextColumn = SelectFirstApple(s)

    s.Press(VirtualKey.VK_OEM_5, ControlKeyState.LEFT_CTRL_PRESSED)

    Assert.Equal(3, s.Ui.EditorView.CursorRow) ' the next "apple" is on the following line
    Assert.Equal(nextColumn, s.Ui.EditorView.CursorColumn)
  End Sub

  <Fact>
  Public Sub TerminalsSendControlBackslashAsAControlCharacter()
    Dim s As New Session(Sample)
    SelectFirstApple(s)

    s.Press(CType(0, VirtualKey), ControlKeyState.LEFT_CTRL_PRESSED, ChrW(28))

    Assert.Equal(3, s.Ui.EditorView.CursorRow)
  End Sub

  <Fact>
  Public Sub TheRightMouseButtonOpensHelpAndShiftRightAnInstantWatch()
    Dim s As New Session(Sample)

    s.Console.QueueMouse(10, 4, ButtonState.RIGHTMOST_BUTTON_PRESSED)
    s.Run()
    Assert.True(s.Ui.Help.Visible)

    s.Press(VirtualKey.VK_ESCAPE)
    Assert.False(s.Ui.Help.Visible)

    s.Console.QueueMouse(10, 4, ButtonState.RIGHTMOST_BUTTON_PRESSED, 0, ControlKeyState.SHIFT_PRESSED)
    s.Run()
    Assert.Contains("Invalid expression for Instant Watch", s.Console.GetText())
  End Sub

  <Fact>
  Public Sub ShiftF6MovesBackwardsThroughTheWindows()
    Dim s As New Session(Sample)

    s.Press(VirtualKey.VK_F6)
    Dim forward = s.Ui.ActiveControl
    s.Press(VirtualKey.VK_F6)
    s.Press(VirtualKey.VK_F6, ControlKeyState.SHIFT_PRESSED)

    Assert.Same(forward, s.Ui.ActiveControl)
  End Sub

  <Fact>
  Public Sub ClickingInTheEditorMovesTheCursor()
    Dim s As New Session(Sample)

    ' the first text row is on screen row 2 (below the menu bar and the frame), inside the frame at column 1
    s.Console.QueueMouse(6, 4, ButtonState.FROM_LEFT_1ST_BUTTON_PRESSED)
    s.Console.QueueMouse(6, 4, 0)
    s.Run()

    Assert.Equal(2, s.Ui.EditorView.CursorRow)
    Assert.Equal(5, s.Ui.EditorView.CursorColumn)
  End Sub

  <Fact>
  Public Sub DraggingWithTheMouseSelectsText()
    Dim s As New Session(Sample)

    s.Console.QueueMouse(1, 2, ButtonState.FROM_LEFT_1ST_BUTTON_PRESSED)
    s.Console.QueueMouse(7, 2, ButtonState.FROM_LEFT_1ST_BUTTON_PRESSED, MouseEventFlags.MOUSE_MOVED)
    s.Console.QueueMouse(7, 2, 0)
    s.Run()

    Assert.Equal("Module", s.Ui.EditorView.SelectedText)
  End Sub

  Private Const TwoProcedures As String = "Module M" & vbCrLf & "  Sub First()" & vbCrLf & "  End Sub" & vbCrLf & "  Sub Second()" & vbCrLf & "  End Sub" & vbCrLf & "End Module"

  <Fact>
  Public Sub ShiftF2AndCtrlF2MoveBetweenProcedures()
    Dim s As New Session(TwoProcedures)
    Dim first = s.Ui.EditorView.Buffer.Lines.ToList().FindIndex(Function(l) l.Text.Contains("First"))
    Dim second = s.Ui.EditorView.Buffer.Lines.ToList().FindIndex(Function(l) l.Text.Contains("Second"))

    s.Press(VirtualKey.VK_F2, ControlKeyState.SHIFT_PRESSED)
    Assert.Equal(first, s.Ui.EditorView.CursorRow)
    s.Press(VirtualKey.VK_F2, ControlKeyState.SHIFT_PRESSED)
    Assert.Equal(second, s.Ui.EditorView.CursorRow)
    s.Press(VirtualKey.VK_F2, ControlKeyState.SHIFT_PRESSED) ' wraps
    Assert.Equal(first, s.Ui.EditorView.CursorRow)
    s.Press(VirtualKey.VK_F2, ControlKeyState.LEFT_CTRL_PRESSED)
    Assert.Equal(second, s.Ui.EditorView.CursorRow)
  End Sub

  <Fact>
  Public Sub UnimplementedHotkeysAndMenuCommandsSayNotImplemented()
    Dim s As New Session(Sample)

    s.Press(VirtualKey.VK_F8, ControlKeyState.SHIFT_PRESSED)   ' Shift+F8: trace history backward
    Assert.Contains("Not implemented.", s.Console.GetText())
    s.Press(VirtualKey.VK_RETURN, 0, ChrW(13))
    Assert.DoesNotContain("Not implemented.", s.Console.GetText())

    s.Console.QueueKey(VirtualKey.VK_MENU)
    s.Console.QueueText("dt") ' Debug > Trace On
    s.Run()
    Assert.Contains("Not implemented.", s.Console.GetText())
  End Sub

  <Fact>
  Public Sub AltPlusMakesTheActiveWindowLargerAndAltMinusSmaller()
    Dim s As New Session(Sample)
    Dim before = s.Ui.EditorView.Height

    s.Press(VirtualKey.VK_OEM_PLUS, ControlKeyState.LEFT_ALT_PRESSED)
    Assert.Equal(before + 1, s.Ui.EditorView.Height)

    s.Press(VirtualKey.VK_OEM_MINUS, ControlKeyState.LEFT_ALT_PRESSED)
    s.Press(VirtualKey.VK_OEM_MINUS, ControlKeyState.LEFT_ALT_PRESSED)
    Assert.Equal(before - 1, s.Ui.EditorView.Height)
  End Sub

  <Fact>
  Public Sub DisplaySettingsAreKeptInTheSettingsFile()
    Dim path = IO.Path.Combine(IO.Path.GetTempPath(), "quickvb-settings-" & Guid.NewGuid().ToString("N") & ".ini")
    Try
      IO.File.WriteAllLines(path, {"normal=White,DarkGreen", "scrollbars=False", "tabstops=2"})

      Dim s As New Session(Sample)
      s.Ui.SettingsPath = path
      s.Ui.LoadSettings()

      Assert.Equal(ConsoleColor.DarkGreen, s.Ui.EditorView.BackgroundColor)
      Assert.Equal(2, s.Ui.EditorView.TabStops)
      Assert.False(s.Ui.EditorView.ScrollBars)
    Finally
      IO.File.Delete(path)
    End Try
  End Sub

  <Fact>
  Public Sub UndoWorksFromTheKeyboardAndTheEditMenu()
    Dim s As New Session(Sample)
    Dim original = s.Ui.EditorView.Buffer.Text

    s.Console.QueueText("xyz")
    s.Run()
    Assert.NotEqual(original, s.Ui.EditorView.Buffer.Text)

    s.Press(VirtualKey.VK_BACK, ControlKeyState.LEFT_ALT_PRESSED, ChrW(8))
    Assert.Equal(original, s.Ui.EditorView.Buffer.Text)

    s.Console.QueueText("q")
    s.Run()
    Assert.NotEqual(original, s.Ui.EditorView.Buffer.Text)

    s.Console.QueueKey(VirtualKey.VK_MENU)
    s.Console.QueueText("eu") ' Edit > Undo
    s.Run()
    Assert.Equal(original, s.Ui.EditorView.Buffer.Text)
  End Sub

  <Fact>
  Public Sub MergeInsertsAFilesTextAtTheCursor()
    Dim s As New Session(Sample)
    Dim path = IO.Path.Combine(IO.Path.GetTempPath(), "quickvb-merge-" & Guid.NewGuid().ToString("N") & ".bas")
    Try
      IO.File.WriteAllText(path, "' merged in")
      s.Ui.EditorView.MoveCursor(0, 0)
      s.Do_(Sub() s.Ui.MergeFile(path))
      Assert.StartsWith("' merged in", s.Ui.EditorView.Buffer.Text)
      s.Press(VirtualKey.VK_BACK, ControlKeyState.LEFT_ALT_PRESSED, ChrW(8))   ' and it can be undone
      Assert.DoesNotContain("merged in", s.Ui.EditorView.Buffer.Text)
    Finally
      IO.File.Delete(path)
    End Try
  End Sub

  <Fact>
  Public Sub DosShellRunsAShellAndComesBack()
    Dim s As New Session(Sample)
    Dim before = s.Ui.EditorView.Buffer.Text

    If OperatingSystem.IsWindows() Then
      s.Ui.ShellFileName = "cmd.exe"
      s.Ui.ShellArguments = "/c exit"
    Else
      s.Ui.ShellFileName = "/bin/sh"
      s.Ui.ShellArguments = "-c true"
    End If

    s.Do_(Sub() s.Ui.RunShell())

    Assert.Equal(before, s.Ui.EditorView.Buffer.Text)
    Assert.Contains("Module", s.Console.GetText())
  End Sub

  <Fact>
  Public Sub TheRightMouseButtonCanRunToTheCursorInstead()
    Dim s As New Session(Sample)

    s.Console.QueueKey(VirtualKey.VK_MENU)
    s.Console.QueueText("om")                                ' Options > Right Mouse
    s.Run()
    Assert.Contains("Right Mouse", s.Console.GetText())
    s.Press(VirtualKey.VK_DOWN)                              ' Run to the cursor
    s.Press(VirtualKey.VK_RETURN)

    s.Console.QueueMouse(10, 3, ButtonState.RIGHTMOST_BUTTON_PRESSED)
    s.Run()

    Assert.False(s.Ui.Help.Visible)                          ' no help this time
    s.Ui.EndSession()
  End Sub

  <Fact>
  Public Sub TheHelpButtonOfADialogExplainsItAndComesBack()
    Dim s As New Session(Sample)

    s.Menu("s", "f")                                         ' Search > Find
    Assert.Contains(" Find ", s.Console.GetText())
    s.Press(VirtualKey.VK_TAB)
    s.Press(VirtualKey.VK_TAB)
    s.Press(VirtualKey.VK_TAB)
    s.Press(VirtualKey.VK_TAB)
    s.Console.QueueText("h")                                 ' < Help >
    s.Run()

    Assert.Contains("Type the text to look for", s.Console.GetText())

    s.Press(VirtualKey.VK_RETURN)                            ' OK: back to the Find box
    Assert.Contains("Find What:", s.Console.GetText())
    s.Press(VirtualKey.VK_ESCAPE)
    Assert.Null(s.Ui.PopupControl)
  End Sub

  <Fact>
  Public Sub FullMenusOffShowsOnlyTheEverydayCommands()
    Dim s As New Session(Sample)

    s.Menu("d")
    Assert.Contains("Watchpoint", s.Console.GetText())
    s.Press(VirtualKey.VK_ESCAPE)
    Assert.Contains("Calls", s.Console.GetText())

    s.Menu("o", "f")                                         ' Options > Full Menus
    s.Menu("d")
    Assert.DoesNotContain("Watchpoint", s.Console.GetText())
    Assert.Contains("Toggle Breakpoint", s.Console.GetText())
    s.Press(VirtualKey.VK_ESCAPE)
    Assert.DoesNotContain("Calls", s.Console.GetText().Split(vbLf)(0))

    s.Menu("o", "f")                                         ' and back
    s.Menu("d")
    Assert.Contains("Watchpoint", s.Console.GetText())
  End Sub

  <Fact>
  Public Sub SyntaxCheckingCanBeTurnedOff()
    Dim s As New Session("Module Demo" & vbCrLf & "  Sub Main()" & vbCrLf & "    Dim x As Integer = " & vbCrLf & "  End Sub" & vbCrLf & "End Module")

    s.Menu("o")
    Assert.Contains("Syntax Checking    " & ChrW(&H221A), s.Console.GetText())
    s.Press(VirtualKey.VK_ESCAPE)

    s.Menu("o", "s")                                         ' Options > Syntax Checking
    s.Menu("o")
    Assert.Contains("Syntax Checking", s.Console.GetText())
    Assert.DoesNotContain("Syntax Checking    " & ChrW(&H221A), s.Console.GetText())
  End Sub

  <Fact>
  Public Sub AnEmptyProgramIsNotReportedAsAnError()
    Dim s As New Session("")

    Assert.DoesNotContain("BC30420", s.Console.GetText())
    Assert.DoesNotContain("Diagnostics", s.Console.GetText())
  End Sub

  Private Shared Sub WaitFor(s As Session, condition As Func(Of Boolean))
    Dim limit = Date.UtcNow.AddSeconds(30)
    Do
      s.Run()
      If condition() Then Return
      If Date.UtcNow > limit Then Throw New TimeoutException(s.Console.GetText())
      Threading.Thread.Sleep(50)
    Loop
  End Sub

  <Fact>
  Public Sub UsingAQuickBasicStatementImportsItForTheProgram()
    Dim source = "Module M" & vbLf & "  Sub Main()" & vbLf & "    CLS()" & vbLf & "  End Sub" & vbLf & "End Module"
    Dim s As New Session(source)

    WaitFor(s, Function() GlobalImports().Contains("QB.Video"))
    WaitFor(s, Function() s.Console.GetText().Contains("The program imports QB.Video"))

    ' no line is added to the source
    Assert.DoesNotContain("Imports", s.Ui.EditorView.Buffer.Text)

    ' and it builds as it is
    Dim errors = App.TheWorkspace.BuildProgram().Where(Function(d) d.Severity = Microsoft.CodeAnalysis.DiagnosticSeverity.Error).ToList()
    Assert.True(errors.Count = 0, String.Join(vbLf, errors))
  End Sub

  Private Shared Function GlobalImports() As List(Of String)
    Dim options = CType(App.TheWorkspace.CurrentSolution.Projects.First().CompilationOptions, Microsoft.CodeAnalysis.VisualBasic.VisualBasicCompilationOptions)
    Return options.GlobalImports.Select(Function(g) g.Clause.ToString().Trim()).ToList()
  End Function

  <Fact>
  Public Sub AProgramThatDrawsGetsGraphicsInPlaceOfVideo()
    Dim s As New Session("Imports QB.Video" & vbLf & vbLf & "Module M" & vbLf & "  Sub Main()" & vbLf & "    SCREEN(9)" & vbLf & "    PRINT(""hi"")" & vbLf & "  End Sub" & vbLf & "End Module")

    WaitFor(s, Function() s.Ui.EditorView.Buffer.Text.Contains("Imports QB.Graphics"))

    Dim lines = s.Ui.EditorView.Buffer.Lines.Select(Function(l) l.Text).ToList()
    Assert.Equal("Imports QB.Graphics", lines(0))
    Assert.DoesNotContain("Imports QB.Video", s.Ui.EditorView.Buffer.Text)
  End Sub

  <Fact>
  Public Sub ALowerCaseProgramIsWrittenWithTheCasingOfQuickBasic()
    Dim s As New Session("module program" & vbLf & "sub Main" & vbLf & "print(""hi"")" & vbLf & "end sub" & vbLf & "end module")

    WaitFor(s, Function() s.Ui.EditorView.Buffer.Text.Contains("PRINT(") AndAlso GlobalImports().Contains("QB.Video"))

    Dim text = s.Ui.EditorView.Buffer.Text
    Assert.Contains("Module program", text)
    Assert.Contains("Sub Main", text)
    Assert.Contains("End Sub", text)
    Assert.Contains("End Module", text)
  End Sub

  <Fact>
  Public Sub EnterAfterABlockHeaderWritesItsEnd()
    Dim s As New Session("")

    s.Console.QueueText("module program")
    s.Press(VirtualKey.VK_RETURN, character:=Chr(13))

    Dim lines = s.Ui.EditorView.Buffer.Lines.Select(Function(l) l.Text).ToList()
    Assert.Contains("Module program", lines(0), StringComparison.OrdinalIgnoreCase)
    Assert.Equal("", lines(1).Trim())                ' the body is a blank line ...
    Assert.Equal("End Module", lines(2))
    Assert.Equal(1, s.Ui.EditorView.CursorRow)       ' ... with the cursor on it, one tab stop in
    Assert.Equal(4, s.Ui.EditorView.CursorColumn)

    s.Console.QueueText("x")
    s.Run()
    Assert.Equal("    x", s.Ui.EditorView.Buffer.Lines(1).Text)
  End Sub

  <Fact>
  Public Sub TheMissingEndIsNotReportedOnceEnterHasWrittenIt()
    Dim s As New Session("")

    s.Console.QueueText("module program")
    s.Press(VirtualKey.VK_RETURN, character:=Chr(13))

    ' the compile that follows must see the End Module (a stale "must end with a matching End Module" stays until the next edit)
    Dim limit = Date.UtcNow.AddSeconds(4)
    Do While Date.UtcNow < limit
      s.Run()
      Threading.Thread.Sleep(50)
    Loop

    Assert.Contains("End Module", s.Ui.EditorView.Buffer.Text)
    Assert.DoesNotContain("BC30625", s.Console.GetText())
  End Sub

  <Fact>
  Public Sub DeletingEverythingClearsTheDiagnostics()
    Dim s As New Session("Module M" & vbLf & "  Sub S()" & vbLf & "    For x = 1 To 3" & vbLf & "  End Sub" & vbLf & "End Module")

    ' the program has an error (the For has no Next) and the diagnostics say so
    s.Do_(Sub() s.Ui.EditorView.MoveCursor(4, 0))
    s.Do_(Sub() s.Ui.EditorView.MoveCursor(0, 0))
    WaitFor(s, Function() s.Console.GetText().Contains("BC30084"))

    ' select everything from the bottom up and delete it: nothing is left to complain about
    s.Do_(Sub() s.Ui.EditorView.MoveCursor(4, 0))
    s.Press(VirtualKey.VK_END)
    s.Press(VirtualKey.VK_HOME, ControlKeyState.LEFT_CTRL_PRESSED Or ControlKeyState.SHIFT_PRESSED)
    s.Press(VirtualKey.VK_DELETE)

    WaitFor(s, Function() Not s.Console.GetText().Contains("BC30084"))
    Assert.Equal("", s.Ui.EditorView.Buffer.Text)
  End Sub

  <Fact>
  Public Sub NewProgramStartsWithHelloWorld()
    Dim s As New Session("")

    s.Do_(Sub() s.Ui.NewProgram())

    WaitFor(s, Function() s.Ui.EditorView.Buffer.Text.Contains("Hello World!"))
    Assert.Contains("Module Program", s.Ui.EditorView.Buffer.Text)
  End Sub

  <Fact>
  Public Sub TheStartingProgramIsNotAskedToBeSaved()
    Dim s As New Session("")

    s.Do_(Sub() s.Ui.NewProgram())
    WaitFor(s, Function() GlobalImports().Contains("QB.Video"))   ' the program imports what it uses
    WaitFor(s, Function() True)

    s.Do_(Sub() s.Ui.NewProgram())
    Assert.DoesNotContain("not saved", s.Console.GetText())
  End Sub

  <Fact>
  Public Sub EnterInTheMiddleOfAHeaderDoesNotWriteAnEnd()
    Dim s As New Session("")

    s.Console.QueueText("module program")
    s.Press(VirtualKey.VK_LEFT)
    s.Press(VirtualKey.VK_RETURN, character:=Chr(13))

    Assert.DoesNotContain("End Module", s.Ui.EditorView.Buffer.Text)
  End Sub

  <Fact>
  Public Sub LeavingAQuickBasicLineWritesItsParentheses()
    Dim s As New Session("Imports QB.Video" & vbLf & "Module M" & vbLf & "  Sub S()" & vbLf & "    PRINT ""hi""" & vbLf & "  End Sub" & vbLf & "End Module")

    s.Do_(Sub() s.Ui.EditorView.MoveCursor(3, s.Ui.EditorView.Buffer.Lines(3).Text.Length))
    s.Console.QueueText(" ")                           ' an edit makes the line one to commit when it is left
    s.Run()
    s.Press(VirtualKey.VK_DOWN)                        ' leave the line

    WaitFor(s, Function() s.Ui.EditorView.Buffer.Lines(3).Text.Contains("PRINT(""hi"")"))
  End Sub

  <Fact>
  Public Sub AProgramThatDrawsImportsGraphicsForTheProgramInPlaceOfVideo()
    Dim s As New Session("Module M" & vbLf & "  Sub Main()" & vbLf & "    CLS()" & vbLf & "    SCREEN(9)" & vbLf & "  End Sub" & vbLf & "End Module")

    WaitFor(s, Function() GlobalImports().Contains("QB.Graphics"))

    Assert.DoesNotContain("QB.Video", GlobalImports())
    Assert.DoesNotContain("Imports QB", s.Ui.EditorView.Buffer.Text)
  End Sub

  <Fact>
  Public Sub AProgramThatUsesNothingOfQuickBasicImportsNothing()
    Dim s As New Session("Module M" & vbLf & "  Sub Main()" & vbLf & "  End Sub" & vbLf & "End Module")
    s.Run()
    Threading.Thread.Sleep(500)
    s.Run()

    Assert.DoesNotContain("QB.Video", GlobalImports())
    Assert.DoesNotContain("Imports QB", s.Ui.EditorView.Buffer.Text)
  End Sub

End Class
