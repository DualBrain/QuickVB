Option Explicit On
Option Strict On
Option Infer On

Imports ConsoleGUI
Imports Microsoft.CodeAnalysis

' Debugging: breakpoints, Step / Procedure Step / Run to Cursor, Watch, Watchpoint and Instant Watch.
'
' The program is compiled with the debugger's calls in it (see Instrumenter) and runs in a process of its own that owns the terminal
' while it runs. When it stops, QuickVB takes the terminal back and shows the statement; the keys then tell the program what to do next.
Partial Public Class MyScreen

  Private _session As DebugSession
  Private _callsMenu As Menu
  Private _breakOnErrorsItem As MenuItem
  Private _breakOnErrors As Boolean
  Private _rightMouse As String = "help"
  Private _sessionFiles As List(Of String)
  Private _lastPause As PauseInfo
  Private _paused As Boolean

  ' the terminal belongs to the program (QuickVB's screen is not showing)
  Private _consoleGivenAway As Boolean

  Private _statementFileName As String
  Private _statementRow As Integer = -1

  ''' <summary>Waits for a key after the program has ended (tests replace it: there is no terminal to read from).</summary>
  Public Property WaitAfterProgram As Action = AddressOf WaitForKeyOnTheOutputScreen

  Private Shared Sub WaitForKeyOnTheOutputScreen()
    Try
      Console.SetCursorPosition(0, Math.Max(0, Console.WindowHeight - 1))
      Console.Write("Press any key to continue")
      Console.ReadKey(True)
    Catch ex As Exception When TypeOf ex Is InvalidOperationException OrElse TypeOf ex Is System.IO.IOException
      ' no terminal to wait on
    End Try
  End Sub

  ''' <summary>True while a program that is being debugged is stopped and QuickVB is showing where.</summary>
  Friend ReadOnly Property IsPaused As Boolean
    Get
      Return Me._paused
    End Get
  End Property

  Friend ReadOnly Property HasDebugSession As Boolean
    Get
      Return Me._session IsNot Nothing
    End Get
  End Property

  ' ---- breakpoints

  Private Sub SyncBreakpointsFromEditor()
    If Me._active Is Nothing Then Return
    Dim rows As New HashSet(Of Integer)()
    Dim lines = Me.DocumentBuffer.Lines
    For row = 0 To lines.Count - 1
      If (lines(row).Marks And LineMarks.Breakpoint) <> 0 Then rows.Add(row)
    Next
    Me._active.Breakpoints = rows
  End Sub

  ''' <summary>Puts the marks of each window's file on its lines: the breakpoints and the statement the program stopped at.</summary>
  Private Sub ApplyMarks()

    For Each slot In Me.EditorSlots()

      Dim file = slot.Item2
      If file Is Nothing Then Continue For

      Dim lines = slot.Item1.Buffer.Lines

      For Each line In lines
        line.Marks = 0
      Next

      For Each row In file.Breakpoints
        If row < lines.Count Then lines(row).Marks = lines(row).Marks Or LineMarks.Breakpoint
      Next

      If Me._paused AndAlso Me._statementFileName = file.Name AndAlso Me._statementRow >= 0 AndAlso Me._statementRow < lines.Count Then
        lines(Me._statementRow).Marks = lines(Me._statementRow).Marks Or LineMarks.CurrentStatement
      End If

    Next

  End Sub

  Private Sub ClearAllMarks()
    For Each slot In Me.EditorSlots()
      For Each line In slot.Item1.Buffer.Lines
        line.Marks = 0
      Next
    Next
  End Sub

  ''' <summary>Debug &gt; Toggle Breakpoint (F9): the line with the cursor.</summary>
  Public Sub ToggleBreakpoint()

    If Me._active Is Nothing OrElse Not Object.ReferenceEquals(Me.ActiveBufferView, Me.DocumentBufferView) Then Return

    Dim row = Me.DocumentBufferView.CursorRow
    Dim lines = Me.DocumentBuffer.Lines
    If row < 0 OrElse row >= lines.Count Then Return

    lines(row).Marks = lines(row).Marks Xor LineMarks.Breakpoint
    Me.SyncBreakpointsFromEditor()
    Me.ApplyMarks()
    Me.Invalidate()

  End Sub

  Public Sub ClearAllBreakpoints()

    For Each file In Me._files
      file.Breakpoints = New HashSet(Of Integer)()
    Next

    Me.ApplyMarks()
    Me.Invalidate()

  End Sub

  Private Function HasBreakpoints() As Boolean
    Me.SyncBreakpointsFromEditor()
    Return Me._files.Any(Function(f) f.Breakpoints.Count > 0)
  End Function

  ''' <summary>A program only runs under the debugger when something asks for it: a breakpoint or a watchpoint.</summary>
  Private Function NeedsDebugger() As Boolean
    Return Me.HasBreakpoints() OrElse Me._watchpoints.Count > 0
  End Function

  ' ---- starting, continuing and ending

  ''' <summary>Run &gt; Start / Continue (F5): continues a stopped program, otherwise runs it (under the debugger when there are breakpoints).</summary>
  Public Sub ContinueOrRun()

    If Me._session IsNot Nothing Then
      If Me._paused Then Me.ResumeSession("run")
    ElseIf Me.NeedsDebugger() Then
      Me.StartDebugging("run", False)
    Else
      Me.RunProgram()
    End If

  End Sub

  ''' <summary>Run &gt; Start (Shift+F5): from the beginning, whatever was happening.</summary>
  Public Sub StartFromTheBeginning()

    Me.EndSession()

    If Me.NeedsDebugger() Then
      Me.StartDebugging("run", False)
    Else
      Me.RunProgram()
    End If

  End Sub

  ''' <summary>Run &gt; Restart: the program starts again and stops in front of its first statement.</summary>
  Public Sub RestartProgram()
    Me.EndSession()
    Me.StartDebugging("into", False)
  End Sub

  ''' <summary>F8 (<c>into</c>), F10 (<c>over</c>): a statement at a time, starting the program when it is not running.</summary>
  Public Sub StepProgram(mode As String)
    If Me._session IsNot Nothing Then
      If Me._paused Then Me.ResumeSession(mode)
    Else
      Me.StartDebugging("into", False)
    End If
  End Sub

  ''' <summary>F7: runs up to the line with the cursor.</summary>
  Public Sub RunToCursor()
    If Me._session IsNot Nothing Then
      If Me._paused Then Me.ResumeSession("to", True)
    Else
      Me.StartDebugging("to", True)
    End If
  End Sub

  Private Function BuildCommand(mode As String, toCursor As Boolean) As DebugCommand

    Me.SyncBreakpointsFromEditor()

    Dim breakpoints As New List(Of Tuple(Of Integer, Integer))()

    For index = 0 To Me._sessionFiles.Count - 1
      Dim name = Me._sessionFiles(index)
      Dim file = Me._files.FirstOrDefault(Function(f) f.Name = name)
      If file IsNot Nothing Then
        For Each row In file.Breakpoints
          breakpoints.Add(Tuple.Create(index, row))
        Next
      End If
    Next

    Dim command As New DebugCommand With {.Mode = mode, .Breakpoints = breakpoints, .Watching = Me._watchpoints.Count > 0, .BreakOnErrors = Me._breakOnErrors}

    If toCursor AndAlso Me._active IsNot Nothing Then
      command.TargetFile = Math.Max(0, Me._sessionFiles.IndexOf(Me._active.Name))
      command.TargetLine = Me.DocumentBufferView.CursorRow
    End If

    Return command

  End Function

  ''' <summary>The program runs in the folder its main file is in (otherwise the folder QuickVB was started in).</summary>
  Private Sub PrepareToRun()
    Dim main = Me.MainFile()
    Dim path = If(main IsNot Nothing, main.Path, Nothing)
    App.TheWorkspace.ProgramDirectory = If(path IsNot Nothing, IO.Path.GetDirectoryName(IO.Path.GetFullPath(path)), Nothing)
  End Sub

  Private Sub StartDebugging(mode As String, toCursor As Boolean)

    If Not Me.RoslynEnabled Then
      Me.SetStatus("NOTE: Please enable Roslyn on the Options menu first.", True)
      Return
    End If

    Me.EndSession()
    Me.ClearFindHighlight()
    Me.PushEditorTextToWorkspace()
    Me.UpdateMainModule()
    Me.PrepareToRun()

    Dim workspace = App.TheWorkspace

    ' errors are reported against the program as it is written, so it is built that way first
    Dim diagnostics = workspace.BuildProgram()
    Dim firstError = diagnostics.FirstOrDefault(Function(d) d.Severity = DiagnosticSeverity.Error)

    If firstError IsNot Nothing Then
      Me.UpdateDiagnostics(diagnostics)
      Me.ShowSyntaxError(firstError)
      Return
    End If

    Dim names As New List(Of String)()
    Dim debugDiagnostics = workspace.BuildDebugProgram(names, True)

    ' a variable the debugger can't read must not stop the program from being debugged
    If debugDiagnostics.Any(Function(d) d.Severity = DiagnosticSeverity.Error) Then
      names.Clear()
      debugDiagnostics = workspace.BuildDebugProgram(names, False)
    End If

    If debugDiagnostics.Any(Function(d) d.Severity = DiagnosticSeverity.Error) Then
      Me.ShowMessage("Unable to debug this program")
      Return
    End If

    Me._sessionFiles = names

    Dim session = workspace.CreateDebugSession(Me._commandLine)
    session.OnPaused = Sub(pause) Me.Post(Sub() Me.HandlePause(session, pause))
    session.OnEnded = Sub(code) Me.Post(Sub() Me.HandleSessionEnded(session))

    Me._session = session
    Me._paused = False

    Dim command = Me.BuildCommand(mode, toCursor)

    Me.SuspendConsole()
    Me._consoleGivenAway = True
    Try
      Console.Clear()
    Catch ex As System.IO.IOException
      ' not a terminal
    End Try

    Try
      session.Start(command)
    Catch ex As InvalidOperationException
      Me._session = Nothing
      Me._consoleGivenAway = False
      Me.ReactivateConsole()
      Me.SetStatus(ex.Message, True)
      Me.ShowMessage(ex.Message)
    End Try

  End Sub

  Private Sub ResumeSession(mode As String, Optional toCursor As Boolean = False)

    Dim command = Me.BuildCommand(mode, toCursor)

    Me.ClearStatementMark()
    Me._paused = False
    Me.DocumentBufferView.IsReadOnly = False
    Me.UpdateDiagnostics(Me._diagnostics)
    Me.UpdateCallsMenu()

    Me.SuspendConsole()
    Me._consoleGivenAway = True

    Me._session.Go(command)

  End Sub

  ''' <summary>Called (on the UI thread) when the program stops.</summary>
  Private Sub HandlePause(session As DebugSession, pause As PauseInfo)

    If session IsNot Me._session Then Return

    Me._lastPause = pause

    Dim watchpoint As String = Nothing

    If pause.Reason = "watch" Then

      watchpoint = Me._watchpoints.FirstOrDefault(Function(expression) Me.IsTrue(expression))

      If watchpoint Is Nothing Then
        ' none of the watchpoints holds: the program carries on as if it had not been stopped
        session.Go(New DebugCommand With {.Mode = Nothing, .Breakpoints = Me.BuildCommand("run", False).Breakpoints, .Watching = True, .BreakOnErrors = Me._breakOnErrors})
        Return
      End If

    End If

    If Me._consoleGivenAway Then
      Me.ReactivateConsole()
      Me._consoleGivenAway = False
    End If

    Me._paused = True

    Dim name = If(pause.File >= 0 AndAlso pause.File < Me._sessionFiles.Count, Me._sessionFiles(pause.File), Nothing)
    Dim file = Me._files.FirstOrDefault(Function(f) f.Name = name)

    Me.ClearStatementMark()

    If file IsNot Nothing Then
      Me.ActivateFile(file)
      Me._statementFileName = file.Name
      Me._statementRow = pause.Line
      ' the cursor goes to the start of the statement
      Dim text = Me.DocumentBuffer.Lines(Math.Min(pause.Line, Me.DocumentBuffer.Lines.Count - 1)).Text
      Me.DocumentBufferView.MoveCursor(pause.Line, text.Length - text.TrimStart().Length)
      Me.ApplyMarks()
    End If

    Me.ActiveBufferView = Me.DocumentBufferView
    Me.ActiveControl = Me.DocumentBufferView
    Me.DocumentBufferView.IsReadOnly = True

    Me.UpdateWatchWindow()

    Me.UpdateCallsMenu()

    If pause.Reason = "error" Then
      Me.SetStatus("Error: " & pause.ErrorMessage, True)
      Me.ShowMessage(pause.ErrorMessage)
    ElseIf watchpoint IsNot Nothing Then
      Me.SetStatus("Watchpoint: " & watchpoint, True)
    Else
      Me.SetStatus("F5=Continue  F8=Step  F10=Procedure Step  F7=To Cursor  F4=Output Screen", False)
    End If

    Me.Invalidate()

  End Sub

  ''' <summary>Called (on the UI thread) when the program has ended.</summary>
  Private Sub HandleSessionEnded(session As DebugSession)

    If session IsNot Me._session Then Return

    Me._session.Dispose()
    Me._session = Nothing
    Me._paused = False
    Me.ClearStatementMark()
    Me.DocumentBufferView.IsReadOnly = False

    If Me._consoleGivenAway Then
      Me.WaitAfterProgram.Invoke()
      Me.ReactivateConsole()
      Me._consoleGivenAway = False
    End If

    Me.UpdateDiagnostics(Me._diagnostics)
    Me.UpdateCallsMenu()
    Me.SetStatus(Nothing, False)
    Me.Invalidate()

  End Sub

  ''' <summary>Stops the program that is being debugged, if there is one.</summary>
  Public Sub EndSession()

    Dim session = Me._session
    If session Is Nothing Then Return

    Me._session = Nothing
    Me._paused = False
    Me.ClearStatementMark()
    Me.DocumentBufferView.IsReadOnly = False

    session.Dispose()

    If Me._consoleGivenAway Then
      Me.ReactivateConsole()
      Me._consoleGivenAway = False
    End If

    Me.UpdateDiagnostics(Me._diagnostics)
    Me.UpdateCallsMenu()
    Me.Invalidate()

  End Sub

  Private Sub ClearStatementMark()
    For Each slot In Me.EditorSlots()
      For Each line In slot.Item1.Buffer.Lines
        line.Marks = line.Marks And Not LineMarks.CurrentStatement
      Next
    Next
    Me._statementRow = -1
    Me._statementFileName = Nothing
  End Sub

  ''' <summary>View &gt; Output Screen (F4): what the program has written, until a key is pressed.</summary>
  Public Sub ShowOutputScreen()

    If Me._consoleGivenAway Then Return

    Me.SuspendConsole()
    Me.WaitAfterProgram.Invoke()
    Me.ReactivateConsole()

  End Sub

  ' ---- watches

  Private Function LookupVariable(name As String) As Tuple(Of Boolean, Object)

    If Me._lastPause Is Nothing Then Return Tuple.Create(False, CType(Nothing, Object))

    ' a QuickBASIC type character (x%, name$) is not part of the name
    Dim plain = name.TrimEnd("$"c, "%"c, "&"c, "!"c, "#"c)

    For Each variable In Me._lastPause.Variables
      If String.Equals(variable.Name, plain, StringComparison.OrdinalIgnoreCase) Then Return Tuple.Create(True, variable.Value)
    Next

    Return Tuple.Create(False, CType(Nothing, Object))

  End Function

  Private Function IsTrue(expression As String) As Boolean
    Try
      Dim value = ExpressionEvaluator.Evaluate(expression, AddressOf Me.LookupVariable)
      Return (TypeOf value Is Boolean AndAlso CBool(value)) OrElse (TypeOf value Is Long AndAlso CLng(value) <> 0)
    Catch ex As EvaluationException
      Return False
    End Try
  End Function

  Private Function Describe(expression As String) As String
    Try
      Return ExpressionEvaluator.Format(ExpressionEvaluator.Evaluate(expression, AddressOf Me.LookupVariable))
    Catch ex As EvaluationException
      Return "<" & ex.Message & ">"
    End Try
  End Function

  ''' <summary>While the program is stopped the bottom window shows the watch expressions and their values.</summary>
  Private Sub UpdateWatchWindow()

    If Not Me._paused OrElse Me._watches.Count = 0 Then Return

    Me.DiagnosticsBuffer.Text = String.Join(vbCrLf, Me._watches.Select(Function(w) w & ": " & Me.Describe(w)))
    Me.DiagnosticsPane.Title = "Watch"

  End Sub

  Public Sub ShowDeleteWatchDialog()

    If Me._watches.Count = 0 Then
      Me.ShowMessage("No watch expressions")
      Return
    End If

    Dim d As New FormDialog() With {.Title = "Delete Watch", .Size = FormDialogSize.Wide, .BottomPadding = 0}
    d.AddText("Choose the watch expression to delete", centered:=False)
    Dim list = d.AddList(Me._watches.ToList())
    d.AddText("", centered:=False)

    d.AddButton("< Delete >", "D"c,
                Sub()
                  Me._watches.RemoveAt(list.SelectedIndex)
                  Me.RefreshWatchWindow()
                End Sub, isDefault:=True)
    d.AddButton("< Cancel >", "C"c, isCancel:=True)
    d.AddButton("< Help >", "H"c, Sub() Me.ShowDialogHelp(), closes:=False)
    d.Show(Me)

  End Sub

  Public Sub DeleteAllWatches()
    Me._watches.Clear()
    Me.RefreshWatchWindow()
  End Sub

  Private Sub RefreshWatchWindow()
    If Me._paused AndAlso Me._watches.Count > 0 Then
      Me.UpdateWatchWindow()
    Else
      Me.UpdateDiagnostics(Me._diagnostics)
    End If
    Me.Invalidate()
  End Sub

  ''' <summary>Debug &gt; Instant Watch (Shift+F9): the value of the selected text or the word at the cursor.</summary>
  Public Sub ShowInstantWatch()

    If Not Me._paused Then
      Me.ShowMessage("Invalid expression for Instant Watch")
      Return
    End If

    Dim view = Me.DocumentBufferView
    Dim expression = If(view.HasTextSelection AndAlso Not view.SelectedText.Contains(vbLf), view.SelectedText.Trim(), Me.WordAtCursor())

    Dim value As String

    Try
      value = ExpressionEvaluator.Format(ExpressionEvaluator.Evaluate(expression, AddressOf Me.LookupVariable))
    Catch ex As EvaluationException
      Me.ShowMessage("Invalid expression for Instant Watch")
      Return
    End Try

    Dim d As New FormDialog() With {.Title = "Instant Watch", .Size = FormDialogSize.Medium, .TopPadding = 0}
    d.AddText("Expression:  " & expression, centered:=False)
    d.AddText("Value:       " & value, centered:=False)
    d.AddButton("< Add Watch >", "A"c,
                Sub()
                  Me._watches.Add(expression)
                  Me.RefreshWatchWindow()
                End Sub, isDefault:=True)
    d.AddButton("< Cancel >", "C"c, isCancel:=True)
    d.AddButton("< Help >", "H"c, Sub() Me.ShowDialogHelp(), closes:=False)
    d.Show(Me)

  End Sub

  ' ---- Debug > Break on Errors, Calls, View > Next Statement

  Public Sub ToggleBreakOnErrors()
    Me._breakOnErrors = Not Me._breakOnErrors
    Me._breakOnErrorsItem.Name = If(Me._breakOnErrors, "Break on &Errors    " & ChrW(&H221A), "Break on &Errors")
    Me.MainMenu.Layout()
    Me.SetStatus(If(Me._breakOnErrors, "Break on Errors is on", "Break on Errors is off"), False)
    Me.Invalidate()
  End Sub

  ''' <summary>The Calls menu lists the procedures that are running while the program is stopped (the newest first); otherwise it names the program.</summary>
  Private Sub UpdateCallsMenu()

    If Me._callsMenu Is Nothing Then Return

    Dim items As New List(Of MenuItem)()

    If Me._paused AndAlso Me._lastPause IsNot Nothing AndAlso Me._lastPause.Frames.Count > 0 Then

      Dim frames = Me._lastPause.Frames
      Dim number = 1

      For index = frames.Count - 1 To 0 Step -1
        Dim frame = frames(index)
        Dim label = If(number <= 9, "&" & number & " ", "  ") & frame.Name
        items.Add(New MenuItem(Name:=label, Action:=Sub() Me.Post(Sub() Me.ShowFrame(frame))))
        number += 1
      Next

    Else

      Dim main = Me.MainFile()
      Dim title = If(main Is Nothing, "Untitled", Me.TitleOf(main))
      items.Add(New MenuItem(Name:="&1 " & title, Action:=Sub() Me.Post(Sub() Me.ActivateFile(main))))

    End If

    Me._callsMenu.SetMenuItems(items.ToArray())

  End Sub

  Private Sub ShowFrame(frame As DebugFrame)

    Dim name = If(frame.File >= 0 AndAlso frame.File < Me._sessionFiles.Count, Me._sessionFiles(frame.File), Nothing)
    Dim file = Me._files.FirstOrDefault(Function(f) f.Name = name)
    If file Is Nothing Then Return

    Me.ActivateFile(file)
    Dim lines = Me.DocumentBuffer.Lines
    Dim row = Math.Min(frame.Line, lines.Count - 1)
    Me.DocumentBufferView.MoveCursor(row, lines(row).Text.Length - lines(row).Text.TrimStart().Length)
    Me.Invalidate()

  End Sub

  ''' <summary>View &gt; Next Statement: back to the statement the program stopped at.</summary>
  Public Sub ShowNextStatement()

    If Not Me._paused OrElse Me._statementRow < 0 Then
      Me.SetStatus("The program is not stopped", False)
      Return
    End If

    Dim file = Me._files.FirstOrDefault(Function(f) f.Name = Me._statementFileName)
    If file Is Nothing Then Return

    Me.ActivateFile(file)
    Dim text = Me.DocumentBuffer.Lines(Math.Min(Me._statementRow, Me.DocumentBuffer.Lines.Count - 1)).Text
    Me.DocumentBufferView.MoveCursor(Me._statementRow, text.Length - text.TrimStart().Length)
    Me.Invalidate()

  End Sub

  ' ---- Options > Right Mouse

  Public Sub ShowRightMouseDialog()

    Dim d As New FormDialog() With {.Title = "Right Mouse", .Size = FormDialogSize.Medium, .TopPadding = 0}
    d.AddText("Action of the right mouse button:", centered:=False)
    Dim choice = d.AddRadios(If(Me._rightMouse = "cursor", 1, 0), {"Context-sensitive help", "Run to the cursor"})

    d.AddButton("< OK >", "O"c,
                Sub()
                  Me._rightMouse = If(choice.SelectedIndex = 1, "cursor", "help")
                  Me.SaveSettings()
                End Sub, isDefault:=True)
    d.AddButton("< Cancel >", "C"c, isCancel:=True)
    d.AddButton("< Help >", "H"c, Sub() Me.ShowDialogHelp(), closes:=False)
    d.Show(Me)

  End Sub

  ' ---- File > Merge, DOS Shell

  Private ReadOnly MergeDialog As New FileDialog With {.Title = "Merge", .Pattern = "*.bas;*.vb", .OnHelp = Sub() Me.ShowDialogHelp()}

  Public Sub ShowMergeDialog()
    Me.MergeDialog.Show(Me, If(Me._currentPath IsNot Nothing, IO.Path.GetDirectoryName(Me._currentPath), Me._lastOpenDirectory), AddressOf Me.MergeFile)
  End Sub

  ''' <summary>File &gt; Merge: the text of another file goes into the program at the cursor.</summary>
  Public Sub MergeFile(path As String)

    Dim text As String

    Try
      text = IO.File.ReadAllText(path)
    Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
      Me.SetStatus("Unable to open " & IO.Path.GetFileName(path) & ": " & ex.Message, True)
      Return
    End Try

    Me.DocumentBufferView.Edit(Sub() Me.DocumentBufferView.InsertTextAtCursor(text))
    Me.SetStatus("Merged " & IO.Path.GetFileName(path), False)
    Me.Invalidate()

  End Sub

  ''' <summary>The program File &gt; DOS Shell starts (the command shell of the system unless a test says otherwise).</summary>
  Public Property ShellFileName As String
  Public Property ShellArguments As String = ""

  Public Sub RunShell()

    Dim shell = Me.ShellFileName
    If shell Is Nothing Then
      shell = If(OperatingSystem.IsWindows(), If(Environment.GetEnvironmentVariable("ComSpec"), "cmd.exe"), If(Environment.GetEnvironmentVariable("SHELL"), "/bin/sh"))
    End If

    Me.SuspendConsole()

    Try
      Console.Clear()
      Console.WriteLine("Type EXIT to return to QuickVB.")
    Catch ex As System.IO.IOException
      ' not a terminal
    End Try

    Try
      Dim info As New ProcessStartInfo(shell, Me.ShellArguments) With {.UseShellExecute = False}
      If Me._currentPath IsNot Nothing Then info.WorkingDirectory = IO.Path.GetDirectoryName(Me._currentPath)
      Using proc = Process.Start(info)
        proc.WaitForExit()
      End Using
    Catch ex As System.ComponentModel.Win32Exception
      Me.SetStatus("Unable to start " & shell & ": " & ex.Message, True)
    Catch ex As System.IO.DirectoryNotFoundException
      Me.SetStatus("Unable to start " & shell & ": " & ex.Message, True)
    End Try

    Me.ReactivateConsole()

  End Sub

  ' ---- Options > Syntax Checking, Full Menus

  Private _syntaxCheckingItem As MenuItem
  Private _fullMenusItem As MenuItem
  Private _syntaxChecking As Boolean = True
  Private _fullMenus As Boolean = True

  ' What the short menus leave out: the commands for programs of several files, the debugger's more advanced commands and other extras.
  Private Shared ReadOnly AdvancedCommands As String() = {"&Merge", "Sa&ve All", "&Create File", "&Load File", "&Unload File", "&DOS Shell", "N&ext SUB", "S&plit",
                                                         "&Next Statement", "Included", "S&witch to", "&Restart", "Modify &COMMAND", "Make &Library", "Set &Main",
                                                         "&Watchpoint", "&Delete Watch", "Delete All W", "&Trace", "&History", "&Clear All", "Break on", "Set Next"}

  Public Sub ToggleSyntaxChecking()
    Me._syntaxChecking = Not Me._syntaxChecking
    Me.ApplyMenuSettings()
    If Me._syntaxChecking Then
      If Me.RoslynEnabled Then Me.OnLineCommit(Me.DocumentBuffer)
    Else
      Me._diagnostics = Nothing
      Me.UpdateDiagnostics(Nothing)
    End If
    Me.SaveSettings()
    Me.Invalidate()
  End Sub

  Public Sub ToggleFullMenus()
    Me._fullMenus = Not Me._fullMenus
    Me.ApplyMenuSettings()
    Me.SaveSettings()
    Me.Invalidate()
  End Sub

  ''' <summary>Shows the check marks of the options and, with Full Menus off, only the commands a beginner needs.</summary>
  Private Sub ApplyMenuSettings()

    Me._syntaxCheckingItem.Name = "&Syntax Checking" & If(Me._syntaxChecking, "    " & ChrW(&H221A), "")
    Me._fullMenusItem.Name = "&Full Menus" & If(Me._fullMenus, "    " & ChrW(&H221A), "")

    For Each topItem In Me.MainMenu.MenuItems

      If topItem.Name IsNot Nothing AndAlso topItem.Name.Contains("Calls") Then topItem.Visible = Me._fullMenus

      If topItem.DropDownMenu Is Nothing Then Continue For

      Dim items = topItem.DropDownMenu.MenuItems
      For Each item In items
        If Not item.Separator AndAlso item.Name IsNot Nothing Then
          item.Visible = Me._fullMenus OrElse Not AdvancedCommands.Any(Function(k) item.Name.Contains(k.Replace("&", "")))
        End If
      Next

      ' a separator needs a visible command on both sides of it
      Dim previousVisible = False
      Dim pendingSeparator As MenuItem = Nothing
      For Each item In items
        If item.Separator Then
          item.Visible = False
          If previousVisible Then pendingSeparator = item
        ElseIf item.Visible Then
          If pendingSeparator IsNot Nothing Then pendingSeparator.Visible = True
          pendingSeparator = Nothing
          previousVisible = True
        End If
      Next

      topItem.DropDownMenu.Layout()

    Next

    Me.MainMenu.Layout()

  End Sub

End Class
