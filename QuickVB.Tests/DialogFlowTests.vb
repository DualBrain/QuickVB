Option Strict On
Option Infer On

Imports System.IO
Imports ConsoleGUI
Imports QuickVB
Imports Xunit

' The QBasic style dialogs, driven end to end through the menus and keys on a HeadlessConsole.
Public Class DialogFlowTests
  Implements IDisposable

  Private ReadOnly _root As String

  Public Sub New()
    Me._root = Path.Combine(Path.GetTempPath(), "QuickVB.DialogTests." & Guid.NewGuid().ToString("N"))
    Directory.CreateDirectory(Me._root)
  End Sub

  Public Sub Dispose() Implements IDisposable.Dispose
    Try
      Directory.Delete(Me._root, True)
    Catch ex As IOException
    End Try
  End Sub

  ' ---- helpers

  Private Const Program As String = "Module Untitled" & vbCrLf &
                                    "  Sub Main()" & vbCrLf &
                                    "    Dim total As Integer = 1" & vbCrLf &
                                    "    PRINT(total)" & vbCrLf &
                                    "  End Sub" & vbCrLf &
                                    "End Module"

  Private NotInheritable Class Session
    Public ReadOnly Console As New HeadlessConsole(80, 25)
    Public ReadOnly Ui As MyScreen
    Public ReadOnly Exited As New List(Of String)()

    ''' <summary>The program as the editor holds it after Roslyn has formatted it.</summary>
    Public Initial As String

    Public Sub New(Optional text As String = Program)
      Me.Ui = New MyScreen(Me.Console)
      Me.Ui.ExitAction = Sub() Me.Exited.Add("exit")
      Dim ui2 = Me.Ui
      Me.Ui.Post(Sub()
                   App.NewUntitledWorkspace()
                   ui2.ViewDocument("Untitled", "Untitled.vb", text, title:="Untitled")
                   ui2.EnableRoslyn()
                 End Sub)
      Me.Run()
      Me.Initial = Me.Ui.Editor.Text
    End Sub

    Public Sub Run()
      Screen.NavigateTo(Me.Ui)
    End Sub

    ''' <summary>Opens a menu with Alt and the accelerator letters, and lets the posted action show its dialog.</summary>
    Public Sub Menu(ParamArray letters() As String)
      Me.Console.QueueKey(VirtualKey.VK_MENU)
      For Each letter In letters
        Me.Console.QueueText(letter)
      Next
      Me.Run()
    End Sub

    Public Sub Type(text As String)
      Me.Console.QueueText(text)
      Me.Run()
    End Sub

    Public Sub Press(key As VirtualKey)
      Me.Console.QueueKey(key, If(key = VirtualKey.VK_RETURN, ChrW(13), ChrW(0)))
      Me.Run()
    End Sub

    Public Function LineOf(text As String) As Integer
      Return Me.Ui.Editor.Lines.Select(Function(l) l.Text).ToList().FindIndex(Function(l) l.Contains(text))
    End Function

    Public Function Screen_() As String
      Return Me.Console.GetText()
    End Function

    Public Function StatusLine() As String
      Return Me.Console.RowText(Me.Console.Height - 1)
    End Function

    ''' <summary>Changes the program as if the user had typed (without the completion popup typing would bring up).</summary>
    Public Sub Edit(text As String)
      Dim ui2 = Me.Ui
      Me.Ui.Post(Sub() ui2.Editor.Text = text)
      Me.Run()
    End Sub

  End Class

  Private Function NewFile(name As String, content As String) As String
    Dim path = System.IO.Path.Combine(Me._root, name)
    File.WriteAllText(path, content)
    Return path
  End Function

  ' ---- exit

  <Fact>
  Public Sub ExitLeavesAtOnceWhenTheProgramIsUnchanged()
    Dim s As New Session()

    s.Menu("f", "x")

    Assert.Equal({"exit"}, s.Exited.ToArray())
    Assert.DoesNotContain("not saved", s.Screen_())
  End Sub

  <Fact>
  Public Sub ExitAsksWhatToDoWithAnUnsavedProgram()
    Dim s As New Session()
    s.Edit(s.Initial & vbCrLf & "' changed")

    s.Menu("f", "x")

    Assert.Contains("Loaded file is not saved. Save it now?", s.Screen_())
    Assert.Contains("< Yes >", s.Screen_())
    Assert.Contains("< No >", s.Screen_())
    Assert.Contains("<Cancel>", s.Screen_())
    Assert.Contains("< Help >", s.Screen_())
    Assert.Contains("Tab=Next Field", s.StatusLine())
    Assert.Empty(s.Exited)

    s.Press(VirtualKey.VK_ESCAPE)       ' Cancel
    Assert.Empty(s.Exited)
    Assert.DoesNotContain("not saved", s.Screen_())
  End Sub

  <Fact>
  Public Sub NoLeavesWithoutSaving()
    Dim s As New Session()
    s.Edit(s.Initial & vbCrLf & "' changed")

    s.Menu("f", "x")
    s.Type("n")

    Assert.Equal({"exit"}, s.Exited.ToArray())
  End Sub

  <Fact>
  Public Sub YesSavesFirstAskingForANameWhenTheProgramHasNone()
    Dim s As New Session()
    s.Edit(s.Initial & vbCrLf & "' changed")

    s.Menu("f", "x")
    s.Type("y")
    Assert.Contains("File Name:", s.Screen_())
    Assert.Contains(" Save ", s.Screen_())
    Assert.Empty(s.Exited)

    Dim target = Path.Combine(Me._root, "prog")
    s.Type(target)
    s.Press(VirtualKey.VK_RETURN)

    Assert.True(File.Exists(target & ".vb"))
    Assert.Equal(s.Initial & vbCrLf & "' changed", File.ReadAllText(target & ".vb"))
    Assert.Equal({"exit"}, s.Exited.ToArray())
  End Sub

  <Fact>
  Public Sub CancellingTheSaveDialogStopsTheExit()
    Dim s As New Session()
    s.Edit(s.Initial & vbCrLf & "' changed")

    s.Menu("f", "x")
    s.Type("y")
    s.Press(VirtualKey.VK_ESCAPE)

    Assert.Empty(s.Exited)
    Assert.Null(s.Ui.PopupControl)
  End Sub

  ' ---- save

  <Fact>
  Public Sub SaveAsWritesTheProgramAndRenamesTheWindow()
    Dim s As New Session()

    s.Menu("f", "a")
    Assert.Contains(" Save As ", s.Screen_())
    Assert.Contains("Dirs/Drives", s.Screen_())
    Assert.DoesNotContain("Files", s.Screen_().Replace("Dirs/Drives", ""))     ' no Files list in Save As

    Dim target = Path.Combine(Me._root, "gorilla.vb")
    s.Type(target)
    s.Press(VirtualKey.VK_RETURN)

    Assert.Equal(s.Initial, File.ReadAllText(target))
    Assert.Equal(target, s.Ui.CurrentPath)
    Assert.Contains("gorilla.vb", s.Console.RowText(1))
    Assert.Contains("Saved gorilla.vb", s.StatusLine())
  End Sub

  <Fact>
  Public Sub SavingOverAnExistingFileAsksFirst()
    Dim s As New Session()
    Dim target = NewFile("existing.vb", "original")

    s.Menu("f", "a")
    s.Type(target)
    s.Press(VirtualKey.VK_RETURN)

    Assert.Contains("existing.vb already exists.", s.Screen_())
    Assert.Contains("Do you want to replace it?", s.Screen_())
    Assert.Equal("original", File.ReadAllText(target))

    s.Type("n")
    Assert.Equal("original", File.ReadAllText(target))
    Assert.Null(s.Ui.CurrentPath)

    s.Menu("f", "a")
    s.Type(target)
    s.Press(VirtualKey.VK_RETURN)
    s.Type("y")
    Assert.Equal(s.Initial, File.ReadAllText(target))
    Assert.Equal(target, s.Ui.CurrentPath)
  End Sub

  <Fact>
  Public Sub SaveWritesStraightToTheCurrentFileOfAnOpenedProgram()
    Dim s As New Session()
    Dim target = NewFile("opened.vb", "Module Opened" & vbCrLf & "End Module")

    s.Menu("f", "o")
    s.Type(target)
    s.Press(VirtualKey.VK_RETURN)
    Assert.Equal(target, s.Ui.CurrentPath)

    s.Edit("Module Opened" & vbCrLf & "' edited" & vbCrLf & "End Module")
    s.Menu("f", "s")                                      ' File > Save: no dialog

    Assert.Null(s.Ui.PopupControl)
    Assert.Equal("Module Opened" & vbCrLf & "' edited" & vbCrLf & "End Module", File.ReadAllText(target))

    ' saved, so leaving needs no question
    s.Menu("f", "x")
    Assert.Equal({"exit"}, s.Exited.ToArray())
  End Sub

  ' ---- New SUB / FUNCTION

  <Fact>
  Public Sub NewSubAddsAnEmptySubAtTheEndOfTheModule()
    Dim s As New Session()

    s.Menu("e", "s")
    Assert.Contains(" New SUB ", s.Screen_())
    Assert.Contains("Name:", s.Screen_())

    s.Type("Hello")
    s.Press(VirtualKey.VK_RETURN)

    Dim text = s.Ui.Editor.Text
    Assert.Contains("Sub Hello()", text)
    Assert.True(text.IndexOf("Sub Hello()") > text.IndexOf("End Sub"))             ' after the existing SUB
    Assert.True(text.IndexOf("Sub Hello()") < text.IndexOf("End Module"))          ' inside the module
    Assert.Equal(s.LineOf("Sub Hello()") + 1, s.Ui.EditorView.CursorRow)           ' the cursor is in the new SUB's body
  End Sub

  <Fact>
  Public Sub NewFunctionAddsAFunctionThatReturnsAValue()
    Dim s As New Session()

    s.Menu("e", "f")
    Assert.Contains(" New FUNCTION ", s.Screen_())
    s.Type("Twice")
    s.Press(VirtualKey.VK_RETURN)

    Dim text = s.Ui.Editor.Text
    Assert.Contains("Function Twice() As Integer", text)
    Assert.True(text.IndexOf("Return 0") > text.IndexOf("Function Twice"))
    Assert.Contains("End Function", text)
  End Sub

  <Fact>
  Public Sub ABadNameKeepsTheNewSubDialogOpen()
    Dim s As New Session()

    s.Menu("e", "s")
    s.Type("1abc")
    s.Press(VirtualKey.VK_RETURN)

    Assert.Contains("Identifier expected", s.Screen_())
    Assert.DoesNotContain("1abc()", s.Ui.Editor.Text)

    s.Press(VirtualKey.VK_ESCAPE)
    Assert.Equal(s.Initial, s.Ui.Editor.Text)
  End Sub

  ' ---- Find / Change

  <Fact>
  Public Sub FindMovesTheCursorToTheMatchAndHighlightsIt()
    Dim s As New Session()

    s.Menu("s", "f")
    Assert.Contains(" Find ", s.Screen_())
    Assert.Contains("Find What:", s.Screen_())
    Assert.Contains("[ ] Match Upper/Lowercase", s.Screen_())
    Assert.Contains("[ ] Whole Word", s.Screen_())

    s.Type("total")
    s.Press(VirtualKey.VK_RETURN)

    Assert.Equal(2, s.Ui.EditorView.CursorRow)
    Assert.Equal(s.Ui.Editor.Lines(2).Text.IndexOf("total"), s.Ui.EditorView.CursorColumn)
    Assert.Null(s.Ui.PopupControl)
    ' the match is highlighted (black on gray) in the editor
    Dim row = s.Console.GetText().Split(ControlChars.Lf).ToList().FindIndex(Function(l) l.Contains("Dim total As Integer"))
    Dim column = s.Console.RowText(row).IndexOf("total")
    Assert.Equal(ConsoleColor.Gray, s.Console.BackgroundAt(column, row))
    Assert.Equal(ConsoleColor.DarkBlue, s.Console.BackgroundAt(column + 6, row))
  End Sub

  <Fact>
  Public Sub F3RepeatsTheLastFindAndWrapsRoundTheProgram()
    Dim s As New Session()

    s.Menu("s", "f")
    s.Type("total")
    s.Press(VirtualKey.VK_RETURN)
    Assert.Equal(2, s.Ui.EditorView.CursorRow)

    s.Press(VirtualKey.VK_F3)
    Assert.Equal(3, s.Ui.EditorView.CursorRow)               ' PRINT(total)

    s.Press(VirtualKey.VK_F3)
    Assert.Equal(2, s.Ui.EditorView.CursorRow)               ' wrapped
  End Sub

  <Fact>
  Public Sub FindStartsWithTheWordUnderTheCursor()
    Dim s As New Session()
    Dim inside = s.Ui.Editor.Lines(2).Text.IndexOf("total") + 2
    s.Ui.Post(Sub() s.Ui.EditorView.MoveCursor(2, inside))      ' inside "total"
    s.Run()

    s.Menu("s", "f")

    Assert.Contains("total", s.Console.RowText(s.Console.GetText().Split(ControlChars.Lf).ToList().FindIndex(Function(l) l.Contains("Find What:"))))
  End Sub

  <Fact>
  Public Sub MatchCaseAndWholeWordNarrowTheSearch()
    Dim s As New Session("Module Untitled" & vbCrLf & "  TOTAL totals Total" & vbCrLf & "End Module")
    Dim line = s.Ui.Editor.Lines(1).Text
    Dim upper = line.IndexOf("TOTAL", StringComparison.Ordinal)
    Dim mixed = line.IndexOf("Total", StringComparison.Ordinal)

    ' whole word, ignoring case: finds TOTAL, then Total (not "totals")
    s.Menu("s", "f")
    s.Type("total")
    s.Press(VirtualKey.VK_TAB)
    s.Press(VirtualKey.VK_TAB)
    s.Press(VirtualKey.VK_SPACE)                    ' Whole Word
    s.Press(VirtualKey.VK_RETURN)
    Assert.Equal(1, s.Ui.EditorView.CursorRow)
    Assert.Equal(upper, s.Ui.EditorView.CursorColumn)
    s.Press(VirtualKey.VK_F3)
    Assert.Equal(mixed, s.Ui.EditorView.CursorColumn)

    ' match case: only "Total"
    s.Menu("s", "f")
    s.Type("Total")
    s.Press(VirtualKey.VK_TAB)
    s.Press(VirtualKey.VK_SPACE)                    ' Match Upper/Lowercase
    s.Press(VirtualKey.VK_RETURN)
    Assert.Equal(mixed, s.Ui.EditorView.CursorColumn)
  End Sub

  <Fact>
  Public Sub FindingNothingSaysMatchNotFound()
    Dim s As New Session()

    s.Menu("s", "f")
    s.Type("zzzzz")
    s.Press(VirtualKey.VK_RETURN)

    Assert.Contains("Match not found", s.Screen_())
    s.Press(VirtualKey.VK_RETURN)                    ' OK
    Assert.Null(s.Ui.PopupControl)
  End Sub

  <Fact>
  Public Sub ChangeAllReplacesEveryMatch()
    Dim s As New Session()

    s.Menu("s", "c")
    Assert.Contains(" Change ", s.Screen_())
    Assert.Contains("Change To:", s.Screen_())
    Assert.Contains("< Find and Verify >", s.Screen_())
    Assert.Contains("< Change All >", s.Screen_())

    s.Type("total")
    s.Press(VirtualKey.VK_TAB)
    s.Type("sum")
    s.Press(VirtualKey.VK_TAB)                       ' Match Upper/Lowercase
    s.Press(VirtualKey.VK_TAB)                       ' Whole Word
    s.Press(VirtualKey.VK_TAB)                       ' Find and Verify
    s.Press(VirtualKey.VK_TAB)                       ' Change All
    s.Press(VirtualKey.VK_RETURN)

    Assert.Contains("Dim sum As Integer = 1", s.Ui.Editor.Text)
    Assert.Contains("PRINT(sum)", s.Ui.Editor.Text)
    Assert.DoesNotContain("total", s.Ui.Editor.Text)
    Assert.Contains("2 occurrences changed", s.StatusLine())

    s.Console.QueueKey(VirtualKey.VK_BACK, ChrW(8), ControlKeyState.LEFT_ALT_PRESSED) ' Alt+Backspace
    s.Run()
    Assert.Contains("PRINT(total)", s.Ui.Editor.Text)
    Assert.DoesNotContain("sum", s.Ui.Editor.Text)
  End Sub

  <Fact>
  Public Sub FindAndVerifyAsksAboutEachMatch()
    Dim s As New Session()

    s.Menu("s", "c")
    s.Type("total")
    s.Press(VirtualKey.VK_TAB)
    s.Type("sum")
    s.Press(VirtualKey.VK_RETURN)                    ' the default button: Find and Verify

    Assert.Contains("Change this occurrence?", s.Screen_())
    s.Type("c")                                      ' Change the first
    Assert.Contains("Change this occurrence?", s.Screen_())
    s.Type("s")                                      ' Skip the second
    Assert.Contains("Change complete", s.Screen_())
    s.Press(VirtualKey.VK_RETURN)

    Assert.Contains("Dim sum As Integer = 1", s.Ui.Editor.Text)
    Assert.Contains("PRINT(total)", s.Ui.Editor.Text)

    s.Console.QueueKey(VirtualKey.VK_BACK, ChrW(8), ControlKeyState.LEFT_ALT_PRESSED) ' Alt+Backspace
    s.Run()
    Assert.Contains("Dim total As Integer = 1", s.Ui.Editor.Text)
  End Sub

  ' ---- SUBs

  Private Const TwoSubs As String = "Module Demo" & vbCrLf &
                                    "  Sub Main()" & vbCrLf &
                                    "  End Sub" & vbCrLf &
                                    vbCrLf &
                                    "  Sub Helper()" & vbCrLf &
                                    "    Dim x = 1" & vbCrLf &
                                    "  End Sub" & vbCrLf &
                                    vbCrLf &
                                    "  Function Twice(n As Integer) As Integer" & vbCrLf &
                                    "    Return n * 2" & vbCrLf &
                                    "  End Function" & vbCrLf &
                                    "End Module"

  <Fact>
  Public Sub TheSubsDialogListsTheModuleAndItsProcedures()
    Dim s As New Session(TwoSubs)

    s.Press(VirtualKey.VK_F2)

    Dim text = s.Screen_()
    Assert.Contains(" SUBs ", text)
    Assert.Contains("Choose program item to edit", text)
    Assert.Contains("Untitled is the Main Module", text)
    Assert.Contains("Main", text)
    Assert.Contains("Helper", text)
    Assert.Contains("Twice", text)
    Assert.Contains("< Edit in Active >", text)
    Assert.Contains("< Delete >", text)

    s.Press(VirtualKey.VK_DOWN)
    s.Press(VirtualKey.VK_DOWN)
    Assert.Contains("Helper is a SUB in Untitled", s.Screen_())
    s.Press(VirtualKey.VK_DOWN)
    Assert.Contains("Twice is a FUNCTION in Untitled", s.Screen_())
  End Sub

  <Fact>
  Public Sub EditInActiveJumpsToTheChosenProcedure()
    Dim s As New Session(TwoSubs)

    s.Menu("v", "s")                                 ' View > SUBs
    s.Press(VirtualKey.VK_DOWN)
    s.Press(VirtualKey.VK_DOWN)                      ' Helper
    s.Press(VirtualKey.VK_RETURN)

    Assert.Null(s.Ui.PopupControl)
    Assert.Equal(s.LineOf("Sub Helper"), s.Ui.EditorView.CursorRow)
  End Sub

  <Fact>
  Public Sub DeleteRemovesAProcedureAfterConfirmingAndNeverTheMainModule()
    Dim s As New Session(TwoSubs)

    s.Press(VirtualKey.VK_F2)
    s.Type("d")                                      ' the main module is selected
    Assert.Contains("cannot be deleted", s.Screen_())
    Assert.Contains("Helper", s.Ui.Editor.Text)

    s.Press(VirtualKey.VK_DOWN)
    s.Press(VirtualKey.VK_DOWN)                      ' Helper
    s.Type("d")
    Assert.Contains("Delete SUB Helper?", s.Screen_())
    s.Type("y")

    Assert.DoesNotContain("Helper", s.Ui.Editor.Text)
    Assert.Contains("Sub Main()", s.Ui.Editor.Text)
    Assert.Contains("Function Twice", s.Ui.Editor.Text)

    s.Console.QueueKey(VirtualKey.VK_BACK, ChrW(8), ControlKeyState.LEFT_ALT_PRESSED) ' Alt+Backspace
    s.Run()
    Assert.Contains("Sub Helper()", s.Ui.Editor.Text)
  End Sub

  ' ---- Run with a syntax error

  <Fact>
  Public Sub RunningAProgramWithASyntaxErrorShowsTheSyntaxErrorBoxAndMovesToIt()
    Dim s As New Session("Module Untitled" & vbCrLf & "  Sub Main()" & vbCrLf & "    Dim x As Integer = " & vbCrLf & "  End Sub" & vbCrLf & "End Module")

    s.Press(VirtualKey.VK_F5)

    Assert.Contains("Syntax error", s.Screen_())
    Assert.Contains("<  OK  >", s.Screen_())
    Assert.Contains("< Help >", s.Screen_())
    Assert.NotNull(s.Ui.PopupControl)

    s.Press(VirtualKey.VK_RETURN)

    Assert.Null(s.Ui.PopupControl)
    Assert.Equal(2, s.Ui.EditorView.CursorRow)              ' the line with the error
    Assert.True(s.StatusLine().Trim().Length > 0)
  End Sub

  ' ---- Print, Help Path, About

  <Fact>
  Public Sub PrintShowsQBasicsRadioButtons()
    Dim s As New Session()

    s.Menu("f", "p")

    Dim text = s.Screen_()
    Assert.Contains(" Print ", text)
    Assert.Contains("( ) Selected Text Only", text)
    Assert.Contains("( ) Current Window", text)
    Assert.Contains("(" & ChrW(&H2022) & ") Entire Program", text)
    Assert.Contains("< OK >", text)

    s.Press(VirtualKey.VK_RETURN)
    Assert.Null(s.Ui.PopupControl)
    Assert.Contains("Printing is not available yet", s.StatusLine())
  End Sub

  <Fact>
  Public Sub AboutShowsTheNameVersionAndCopyright()
    Dim s As New Session()

    s.Menu("h", "a")

    Dim platform = If(OperatingSystem.IsWindows(), "Windows", If(OperatingSystem.IsLinux(), "Linux", "macOS"))
    Assert.Contains("QuickVB for " & platform, s.Screen_())
    Assert.Contains("Version 0.1", s.Screen_())
    Assert.Contains("Copyright (C) Dartmouth Didn't, 1964-2026.", s.Screen_())
    Assert.Contains("<  OK  >", s.Screen_())

    s.Press(VirtualKey.VK_RETURN)
    Assert.Null(s.Ui.PopupControl)
  End Sub

  ' ---- Options > Set Paths

  <Fact>
  Public Sub SetPathsHasFourBoxesWithQB45sLabels()
    Dim s As New Session()

    s.Menu("o", "p")

    Dim text = s.Screen_()
    Assert.Contains(" Set Paths ", text)
    Assert.Contains("Search Path For:", text)
    Assert.Contains("Executable files:", text)
    Assert.Contains("(.EXE, .COM)", text)
    Assert.Contains("Include files:", text)
    Assert.Contains("(.BI, .BAS)", text)
    Assert.Contains("Library files:", text)
    Assert.Contains("(.LIB, .QLB)", text)
    Assert.Contains("Help file:", text)
    Assert.Contains("(.HLP)", text)
    Assert.Contains("< OK >", text)

    s.Press(VirtualKey.VK_ESCAPE)
    Assert.Null(s.Ui.PopupControl)
  End Sub

  <Fact>
  Public Sub SetPathsRemembersFoldersAndRejectsOnesThatDoNotExist()
    Dim s As New Session()

    s.Menu("o", "p")
    s.Type(Path.Combine(Me._root, "nowhere"))
    s.Press(VirtualKey.VK_RETURN)
    Assert.True(s.Screen_().Contains("Path not found:"), s.Screen_())
    Assert.NotNull(s.Ui.PopupControl)

    ' replace it with a folder that exists: select all by clearing, then type
    For i = 1 To Path.Combine(Me._root, "nowhere").Length
      s.Press(VirtualKey.VK_BACK)
    Next
    s.Type(Me._root)
    s.Press(VirtualKey.VK_RETURN)
    Assert.Null(s.Ui.PopupControl)
    Assert.Contains("Search paths set", s.StatusLine())

    s.Menu("o", "p")                                   ' it is remembered
    Assert.Contains(Me._root.Substring(Me._root.Length - 20), s.Screen_())
  End Sub

  ' ---- Debug: Add Watch, Watchpoint, Instant Watch

  <Fact>
  Public Sub AddWatchAsksForAnExpression()
    Dim s As New Session()

    s.Menu("d", "a")

    Assert.Contains(" Add Watch ", s.Screen_())
    Assert.Contains("Enter expression to be added to Watch window:", s.Screen_())
    Assert.Contains("< OK >", s.Screen_())

    s.Type("total * 2")
    s.Press(VirtualKey.VK_RETURN)

    Assert.Null(s.Ui.PopupControl)
    Assert.Contains("Watch added: total * 2", s.StatusLine())
  End Sub

  <Fact>
  Public Sub AddWatchStartsWithTheWordUnderTheCursor()
    Dim s As New Session()
    Dim inside = s.Ui.Editor.Lines(2).Text.IndexOf("total") + 2
    s.Ui.Post(Sub() s.Ui.EditorView.MoveCursor(2, inside))
    s.Run()

    s.Menu("d", "a")

    s.Press(VirtualKey.VK_RETURN)
    Assert.Contains("Watch added: total", s.StatusLine())
  End Sub

  <Fact>
  Public Sub AWatchNeedsAnExpression()
    Dim s As New Session()

    s.Menu("d", "a")
    s.Press(VirtualKey.VK_BACK)                       ' nothing to delete: still empty
    s.Press(VirtualKey.VK_RETURN)

    Assert.NotNull(s.Ui.PopupControl)
    Assert.Contains("Enter an expression", s.Screen_())
  End Sub

  <Fact>
  Public Sub WatchpointBreaksWhenTheExpressionIsTrue()
    Dim s As New Session()

    s.Menu("d", "w")
    Assert.Contains(" Watchpoint ", s.Screen_())
    Assert.Contains("Enter expression (will break when TRUE):", s.Screen_())

    s.Type("total > 3")
    s.Press(VirtualKey.VK_RETURN)
    Assert.Contains("Watchpoint added: total > 3", s.StatusLine())
  End Sub

  <Fact>
  Public Sub InstantWatchSaysInvalidExpressionWhileThereIsNoRunningProgram()
    Dim s As New Session()

    s.Press(VirtualKey.VK_F9)                         ' F9 alone is Toggle Breakpoint
    Assert.Null(s.Ui.PopupControl)

    s.Menu("d", "i")
    Assert.Contains("Invalid expression for Instant Watch", s.Screen_())
    Assert.Contains("<  OK  >", s.Screen_())
    Assert.Contains("< Help >", s.Screen_())

    s.Press(VirtualKey.VK_RETURN)
    Assert.Null(s.Ui.PopupControl)

    ' Shift+F9 does the same
    s.Console.QueueKey(VirtualKey.VK_F9, ChrW(0), ControlKeyState.SHIFT_PRESSED)
    s.Run()
    Assert.Contains("Invalid expression for Instant Watch", s.Screen_())
  End Sub

  ' ---- Run: Modify COMMAND$

  <Fact>
  Public Sub ModifyCommandTakesTheTextForTheProgram()
    Dim s As New Session()

    s.Menu("r", "c")
    Assert.Contains(" Modify COMMAND$ ", s.Screen_())
    Assert.Contains("Enter text for COMMAND$:", s.Screen_())

    s.Type("one two")
    s.Press(VirtualKey.VK_RETURN)
    Assert.Contains("COMMAND$ set", s.StatusLine())

    ' the text is shown again next time, and can be cleared
    s.Menu("r", "c")
    Assert.Contains("one two", s.Screen_())
    For i = 1 To 7
      s.Press(VirtualKey.VK_BACK)
    Next
    s.Press(VirtualKey.VK_RETURN)
    Assert.Null(s.Ui.PopupControl)
  End Sub

  ' ---- Run: Make EXE File / Make Library

  Private Const HelloProgram As String = "Module Hello" & vbCrLf &
                                         "  Sub Main()" & vbCrLf &
                                         "    Console.WriteLine(""hello from the exe"")" & vbCrLf &
                                         "  End Sub" & vbCrLf &
                                         "End Module"

  <Fact>
  Public Sub MakeExeFileShowsQB45sBoxWithDotNetOptions()
    Dim s As New Session(HelloProgram)

    s.Menu("r", "x")

    Dim text = s.Screen_()
    Assert.Contains(" Make EXE File ", text)
    Assert.Contains("EXE File Name:", text)
    Assert.Contains("[ ] Produce Debug Code", text)
    Assert.Contains("Produce:", text)
    Assert.Contains("(" & ChrW(&H2022) & ") EXE Requiring .NET Runtime", text)
    Assert.Contains("( ) Stand-Alone EXE File", text)
    Assert.Contains("< Make EXE >", text)
    Assert.Contains("< Make EXE and Exit >", text)
    Assert.Contains("< Cancel >", text)
    Assert.Contains("< Help >", text)
  End Sub

  <Fact>
  Public Sub MakeExeFileWritesALauncherThatRuns()
    Dim s As New Session(HelloProgram)
    Dim target = Path.Combine(Me._root, "hello")

    s.Menu("r", "x")
    s.Type(target)
    s.Press(VirtualKey.VK_RETURN)

    Assert.True(s.Ui.PopupControl Is Nothing, s.Screen_())
    Assert.Contains("Made", s.StatusLine())
    Assert.True(File.Exists(target & QuickVBWorkspace.ExecutableSuffix))
    Assert.True(File.Exists(target & ".dll"))
    Assert.True(File.Exists(target & ".runtimeconfig.json"))
    Assert.False(File.Exists(Path.Combine(Me._root, "QB.Compatibility.dll")))   ' it uses nothing of the libraries
    Assert.False(File.Exists(target & ".pdb"))                      ' no debug code asked for

    Dim info As New ProcessStartInfo(target & QuickVBWorkspace.ExecutableSuffix) With {.RedirectStandardOutput = True, .UseShellExecute = False, .CreateNoWindow = True}
    Using proc = Process.Start(info)
      Dim output = proc.StandardOutput.ReadToEnd()
      proc.WaitForExit()
      Assert.Equal(0, proc.ExitCode)
      Assert.Contains("hello from the exe", output)
    End Using
  End Sub

  <Fact>
  Public Sub MakeExeFileCanIncludeDebugCodeAndLeaveAfterwards()
    Dim s As New Session(HelloProgram)
    Dim target = Path.Combine(Me._root, "debugged")

    s.Menu("r", "x")
    s.Type(target)
    s.Press(VirtualKey.VK_TAB)                        ' Produce Debug Code
    s.Press(VirtualKey.VK_SPACE)
    s.Type("x")                                       ' hot key: Make EXE and Exit

    Assert.True(File.Exists(target & QuickVBWorkspace.ExecutableSuffix))
    Assert.True(File.Exists(target & ".pdb"))
    Assert.Equal({"exit"}, s.Exited.ToArray())
  End Sub

  <Fact>
  Public Sub StandAloneExeFilesAreNotAvailableYet()
    Dim s As New Session(HelloProgram)
    Dim target = Path.Combine(Me._root, "standalone")

    s.Menu("r", "x")
    s.Type(target)
    s.Press(VirtualKey.VK_TAB)
    s.Press(VirtualKey.VK_TAB)                        ' the radio group
    s.Press(VirtualKey.VK_DOWN)                       ' Stand-Alone EXE File
    s.Press(VirtualKey.VK_RETURN)

    Assert.Contains("Stand-alone EXE files are not available yet", s.Screen_())
    Assert.False(File.Exists(target & QuickVBWorkspace.ExecutableSuffix))
  End Sub

  <Fact>
  Public Sub MakeExeFileWithASyntaxErrorShowsTheErrorAndWritesNothing()
    Dim s As New Session("Module M" & vbCrLf & "  Sub Main()" & vbCrLf & "    Dim x As Integer = " & vbCrLf & "  End Sub" & vbCrLf & "End Module")
    Dim target = Path.Combine(Me._root, "broken")

    s.Menu("r", "x")
    s.Type(target)
    s.Press(VirtualKey.VK_RETURN)

    Assert.Contains("Syntax error", s.Screen_())
    Assert.False(File.Exists(target & QuickVBWorkspace.ExecutableSuffix))
  End Sub

  <Fact>
  Public Sub MakeLibraryWritesAClassLibrary()
    Dim s As New Session(HelloProgram)
    Dim target = Path.Combine(Me._root, "mylib")

    s.Menu("r", "l")
    Assert.Contains(" Make Library ", s.Screen_())
    Assert.Contains("[ ] Produce Debug Code", s.Screen_())
    Assert.Contains("< Make Library >", s.Screen_())
    Assert.Contains("< Make Library and Exit >", s.Screen_())

    s.Type(target)
    s.Press(VirtualKey.VK_RETURN)

    Assert.Null(s.Ui.PopupControl)
    Assert.True(File.Exists(target & ".dll"))
    Assert.False(File.Exists(target & ".pdb"))
    Dim name = System.Reflection.AssemblyName.GetAssemblyName(target & ".dll")
    Assert.NotNull(name)
  End Sub

  ' ---- Options > Display

  <Fact>
  Public Sub DisplayChangesTheEditorColoursScrollBarsAndTabStops()
    Dim s As New Session()

    s.Menu("o", "d")
    Assert.Contains(" Display ", s.Screen_())
    Assert.Contains("Normal Text", s.Screen_())

    ' background list: choose Black (Home), foreground list: choose Light Green (page down from White... use End then Up)
    s.Press(VirtualKey.VK_TAB)
    s.Press(VirtualKey.VK_TAB)
    s.Press(VirtualKey.VK_HOME)                      ' background: Black
    s.Press(VirtualKey.VK_TAB)                       ' Scroll Bars
    s.Press(VirtualKey.VK_SPACE)                     ' off
    s.Press(VirtualKey.VK_TAB)                       ' Tab Stops
    s.Type("2")
    s.Press(VirtualKey.VK_RETURN)

    Assert.Null(s.Ui.PopupControl)
    ' plain text in the editor is now on black ("Module" is a keyword, so look at a spot in the line without colours)
    Dim row = 2
    Assert.Equal(ConsoleColor.Black, s.Console.BackgroundAt(40, row))
    Assert.Equal(ConsoleColor.Black, s.Console.BackgroundAt(1, row))
    ' no scroll bar arrows at the right edge of the editor
    Assert.NotEqual(ChrW(&H2191), s.Console.CharAt(s.Console.Width - 1, 2))

    ' tab stops of 2: Tab from column 1 of a line of text goes to column 2
    s.Ui.Post(Sub() s.Ui.EditorView.MoveCursor(1, 1))
    s.Run()
    s.Console.QueueKey(VirtualKey.VK_TAB, ControlChars.Tab)
    s.Run()
    Assert.Equal(2, s.Ui.EditorView.CursorColumn)
  End Sub

  ' ---- samples

  <Fact>
  Public Sub TheSamplesBoxListsTheSamples()
    Dim s As New Session()

    s.Menu("f", "e")

    Dim text = s.Screen_()
    Assert.True(text.Contains(" Samples "), text)
    Assert.True(text.Contains("Nibbles"), text)
    Assert.True(text.Contains("Gorilla"), text)
    Assert.Contains("< Open >", text)
    Assert.Contains("< Save As... >", text)
    Assert.Contains("Steer the snake", text)
  End Sub

  <Fact>
  Public Sub OpeningASampleLoadsItAsAProgramWithoutAFile()
    Dim s As New Session()

    s.Menu("f", "e")
    s.Press(VirtualKey.VK_DOWN)   ' Gorilla
    s.Press(VirtualKey.VK_RETURN) ' Open
    s.Type("n")                   ' the program that was there has changes: not saved

    Assert.Null(s.Ui.CurrentPath)
    Assert.True(s.Ui.Editor.Text.ToUpperInvariant().Contains("GORILLA"), s.Screen_())
    Assert.Contains("Imports QB", s.Ui.Editor.Text)
    Assert.Contains("Gorilla", s.Console.RowText(1))
  End Sub

  <Fact>
  Public Sub ASampleIsSavedWhereThePersonChooses()
    Dim s As New Session()
    Dim target = Path.Combine(Me._root, "my nibbles.vb")

    s.Menu("f", "e")
    s.Type("s")                   ' Save As...
    Assert.Contains(" Save As ", s.Screen_())
    Assert.Contains("NIBBLES.vb", s.Screen_())   ' the name it starts with
    s.Type(target)
    s.Press(VirtualKey.VK_RETURN)

    Assert.Equal(SampleLibrary.All()(0).Text(), File.ReadAllText(target))
    Assert.Contains("Saved my nibbles.vb", s.StatusLine())
    Assert.Equal(s.Initial, s.Ui.Editor.Text)     ' the program being edited is untouched
  End Sub

End Class
