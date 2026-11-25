Option Explicit On
Option Strict On
Option Infer On

Imports System.Text.RegularExpressions
Imports ConsoleGUI
Imports Microsoft.CodeAnalysis

' The files of the program: QuickBASIC's Load File / Create File / Unload File / Set Main Module / SUBs / Switch to ...
' All the files are documents of the one project, so they are compiled together; one of them at a time is shown in the editor.
Partial Public Class MyScreen

  Private NotInheritable Class ProgramFile
    ''' <summary>The document name in the workspace (also the file path the compiler reports in diagnostics).</summary>
    Public Name As String
    ''' <summary>What the editor's title says when the file has no path yet.</summary>
    Public Title As String
    Public Path As String
    Public SavedText As String = ""
    ' where the cursor was when the file was last shown
    Public Row As Integer
    Public Column As Integer
    ''' <summary>The lines (0 based) that have a breakpoint; kept up to date from the editor while the file is not shown.</summary>
    Public Breakpoints As New HashSet(Of Integer)()
  End Class

  Private ReadOnly _files As New List(Of ProgramFile)()
  Private _explicitMain As ProgramFile
  Private _workspaceOfFiles As QuickVBWorkspace

  ' The file in the editor: its path and the text it had when it was loaded or saved.
  Private Property _currentPath As String
    Get
      Return If(Me._active Is Nothing, Nothing, Me._active.Path)
    End Get
    Set(value As String)
      If Me._active IsNot Nothing Then Me._active.Path = value
    End Set
  End Property

  Private Property _savedText As String
    Get
      Return If(Me._active Is Nothing, "", Me._active.SavedText)
    End Get
    Set(value As String)
      If Me._active IsNot Nothing Then Me._active.SavedText = value
    End Set
  End Property

  Private Sub ResetFiles()
    Me.EndSession()
    Me._files.Clear()
    If Me._splitVisible Then Me.CloseSplit()
    Me._activeMain = Nothing
    Me._activeSplit = Nothing
    Me._explicitMain = Nothing
    Me._workspaceOfFiles = App.TheWorkspace
  End Sub

  Private Function TitleOf(file As ProgramFile) As String
    Return If(file.Path IsNot Nothing, IO.Path.GetFileName(file.Path), If(file.Title, file.Name))
  End Function

  Private Function ProgramName() As String
    Return App.TheWorkspace.CurrentSolution.Projects.First().Name
  End Function

  ''' <summary>The text of a file: what is in the editor for the active file, what the workspace holds for the others.</summary>
  Private Function FileText(file As ProgramFile) As String
    For Each slot In Me.EditorSlots()
      If slot.Item2 Is file Then Return slot.Item1.Buffer.Text
    Next
    Return If(App.TheWorkspace.GetProgramDocumentText(file.Name), "")
  End Function

  Private Function IsProgramBlank() As Boolean
    Return Me._files.All(Function(f) Me.FileText(f).Trim().Length = 0)
  End Function

  Private Function IsDirty(file As ProgramFile) As Boolean
    Return Not String.Equals(Me.FileText(file), file.SavedText, StringComparison.Ordinal)
  End Function

  Private ReadOnly Property HasUnsavedChanges As Boolean
    Get
      If Me._files.Count = 0 Then Return Not String.Equals(Me.DocumentBuffer.Text, "", StringComparison.Ordinal)
      Return Me._files.Any(Function(f) Me.IsDirty(f))
    End Get
  End Property

  ''' <summary>The names of the files of the program, in the order they were loaded.</summary>
  Friend ReadOnly Property FileNames As IReadOnlyList(Of String)
    Get
      Return Me._files.Select(Function(f) f.Name).ToList()
    End Get
  End Property

  Friend ReadOnly Property ActiveFileName As String
    Get
      Return If(Me._active Is Nothing, Nothing, Me._active.Name)
    End Get
  End Property

  ' ---- bookkeeping used by ViewDocument

  Private Function RegisterFile(documentName As String, title As String) As ProgramFile

    If Not Object.ReferenceEquals(Me._workspaceOfFiles, App.TheWorkspace) Then Me.ResetFiles()

    Dim file = Me._files.FirstOrDefault(Function(f) f.Name = documentName)

    If file Is Nothing Then
      file = New ProgramFile With {.Name = documentName}
      Me._files.Add(file)
    End If

    If title IsNot Nothing Then file.Title = title

    Return file

  End Function

  ' ---- showing a file

  Private Sub PushEditorTextToWorkspace()
    For Each slot In Me.EditorSlots()
      Dim buffer = slot.Item1.Buffer
      If buffer.DocumentId IsNot Nothing AndAlso App.TheWorkspace IsNot Nothing Then
        App.TheWorkspace.ChangedDocumentTextInternal(buffer.DocumentId, buffer.Text)
      End If
    Next
  End Sub

  ''' <summary>Shows another file of the program in the editor (what was typed into the file being left stays in the program).</summary>
  Private Sub ActivateFile(file As ProgramFile)

    If file Is Nothing OrElse file Is Me._active Then Return

    Dim view = Me.DocumentBufferView

    If Me._active IsNot Nothing Then
      Me._active.Row = view.CursorRow
      Me._active.Column = view.CursorColumn
      Me.SyncBreakpointsFromEditor()
      Me.PushEditorTextToWorkspace()
    End If

    ' the window is showing the new file from here on (a file open in two windows is kept the same in both)
    Dim previous = Me._active
    Me._active = file

    Try
      view.LoadDocument(Me.ProgramName(), file.Name)
    Catch ex As Exception
      Me._active = previous
      Me.SetStatus("Unable to show " & file.Name & ": " & ex.Message, True)
      Return
    End Try
    Me.DocumentPane.Title = Me.TitleOf(file)

    view.ClearUndo()
    view.ClearTextSelection()
    Me.ClearAllMarks()
    Me.ApplyMarks()
    view.WindowTop = 0
    view.WindowLeft = 0
    view.MoveCursor(file.Row, file.Column)

    Me._lineDirty = False

    If Me._diagnostics IsNot Nothing Then Me.UpdateDiagnostics(Me._diagnostics)

    Me.Invalidate()

  End Sub

  Private Function FindFile(path As String) As ProgramFile
    Dim full = IO.Path.GetFullPath(path)
    Return Me._files.FirstOrDefault(Function(f) f.Path IsNot Nothing AndAlso String.Equals(IO.Path.GetFullPath(f.Path), full, StringComparison.OrdinalIgnoreCase))
  End Function

  ''' <summary>Adds a file to the program without showing it.</summary>
  Private Function AddFile(name As String, path As String, text As String) As ProgramFile
    Dim file = New ProgramFile With {.Name = name, .Path = path, .SavedText = text}
    Me._files.Add(file)
    App.TheWorkspace.AddProgramDocument(name, text)
    Return file
  End Function

  Private Function UniqueName(name As String) As String
    If Not Me._files.Any(Function(f) String.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase)) Then Return name
    Dim stem = IO.Path.GetFileNameWithoutExtension(name)
    Dim extension = IO.Path.GetExtension(name)
    Dim n = 2
    Do While Me._files.Any(Function(f) String.Equals(f.Name, stem & " (" & n & ")" & extension, StringComparison.OrdinalIgnoreCase))
      n += 1
    Loop
    Return stem & " (" & n & ")" & extension
  End Function

  ' ---- File > Load File / Create File / Unload File

  Private ReadOnly LoadDialog As New FileDialog With {.Title = "Load File", .Pattern = "*.bas;*.vb", .OnHelp = Sub() Me.ShowDialogHelp()}

  Public Sub ShowLoadFileDialog()
    Me.LoadDialog.Show(Me, If(Me._currentPath IsNot Nothing, IO.Path.GetDirectoryName(Me._currentPath), Me._lastOpenDirectory), AddressOf Me.LoadFile)
  End Sub

  ''' <summary>File &gt; Load File: adds a file to the program (or shows it when it is already part of it).</summary>
  Public Sub LoadFile(path As String)

    Dim existing = Me.FindFile(path)
    If existing IsNot Nothing Then
      Me.ActivateFile(existing)
      Return
    End If

    Dim text As String

    Try
      text = IO.File.ReadAllText(path)
    Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
      Me.SetStatus("Unable to open " & IO.Path.GetFileName(path) & ": " & ex.Message, True)
      Return
    End Try

    Me._lastOpenDirectory = IO.Path.GetDirectoryName(path)

    Dim file = Me.AddFile(Me.UniqueName(IO.Path.GetFileName(path)), path, text)
    Me.ActivateFile(file)
    Me.FinishLoad(file)
    Me.UpdateMainModule()
    Me.SetStatus("Loaded " & IO.Path.GetFileName(path), False)

  End Sub

  ' Roslyn formats a program when it is loaded; that is not a change the user made.
  Private Sub FinishLoad(file As ProgramFile)
    If Me.RoslynEnabled Then Me.OnLineCommit(Me.DocumentBuffer)
    If file Is Me._active Then file.SavedText = Me.DocumentBuffer.Text
  End Sub

  Public Sub ShowCreateFileDialog()

    Dim d As New FormDialog() With {.Title = "Create File", .Size = FormDialogSize.Medium, .TopPadding = 0}
    Dim name = d.AddInput("File Name:", "")

    d.AddButton("< OK >", "O"c,
                Sub() Me.CreateFile(name.Text.Trim()),
                Function()
                  Dim stem = IO.Path.GetFileNameWithoutExtension(name.Text.Trim())
                  If Not IsValidIdentifier(stem) Then
                    d.Message = "Bad file name"
                    Return False
                  End If
                  Return True
                End Function,
                isDefault:=True)
    d.AddButton("< Cancel >", "C"c, isCancel:=True)
    d.AddButton("< Help >", "H"c, Sub() Me.ShowDialogHelp(), closes:=False)
    d.Show(Me)

  End Sub

  ''' <summary>File &gt; Create File: a new, empty module in the program (it is written when it is saved).</summary>
  Public Sub CreateFile(fileName As String)

    If Not IO.Path.HasExtension(fileName) Then fileName &= ".vb"

    Dim directory = If(Me._currentPath IsNot Nothing, IO.Path.GetDirectoryName(Me._currentPath), If(Me._lastOpenDirectory, IO.Directory.GetCurrentDirectory()))
    Dim path = IO.Path.Combine(directory, fileName)

    If Me.FindFile(path) IsNot Nothing OrElse Me._files.Any(Function(f) String.Equals(f.Name, fileName, StringComparison.OrdinalIgnoreCase)) Then
      Me.ShowMessage("File already loaded")
      Return
    End If

    If IO.File.Exists(path) Then
      Me.ShowMessage("File already exists")
      Return
    End If

    Dim text = "Module " & IO.Path.GetFileNameWithoutExtension(fileName) & vbCrLf & "End Module"
    Dim file = Me.AddFile(fileName, path, "")

    Me.ActivateFile(file)
    Me.DocumentBuffer.Text = text
    If Me.RoslynEnabled Then Me.OnLineCommit(Me.DocumentBuffer)

    Me.UpdateMainModule()
    Me.SetStatus("Created " & fileName, False)

  End Sub

  Public Sub ShowUnloadFileDialog()

    If Me._files.Count < 2 Then
      Me.ShowMessage("Cannot unload the only file")
      Return
    End If

    Dim d As New FormDialog() With {.Title = "Unload File", .Size = FormDialogSize.Wide, .BottomPadding = 0}
    d.AddText("Choose file to unload", centered:=False)
    Dim list = d.AddList(Me._files.Select(Function(f) Me.TitleOf(f)).ToList())
    d.AddText("", centered:=False)
    Dim files = Me._files.ToList()

    d.AddButton("< Unload >", "U"c, Sub() Me.UnloadFile(files(list.SelectedIndex)), isDefault:=True)
    d.AddButton("< Cancel >", "C"c, isCancel:=True)
    d.AddButton("< Help >", "H"c, Sub() Me.ShowDialogHelp(), closes:=False)
    d.Show(Me)

  End Sub

  ''' <summary>File &gt; Unload File: takes a file out of the program, asking what to do about changes that were not saved.</summary>
  Private Sub UnloadFile(file As ProgramFile)

    If Not Me._files.Contains(file) Then Return

    If Not Me.IsDirty(file) Then
      Me.RemoveFile(file)
      Return
    End If

    Dim d As New FormDialog()
    d.AddText(Me.TitleOf(file) & " is not saved. Save it now?")
    d.AddButton("< Yes >", "Y"c,
                Sub()
                  Me.ActivateFile(file)
                  Me.SaveProgram(Sub() Me.RemoveFile(file))
                End Sub, isDefault:=True)
    d.AddButton("< No >", "N"c, Sub() Me.RemoveFile(file))
    d.AddButton("<Cancel>", "C"c, isCancel:=True)
    d.Show(Me)

  End Sub

  Private Sub RemoveFile(file As ProgramFile)

    If Me._files.Count < 2 Then
      Me.ShowMessage("Cannot unload the only file")
      Return
    End If

    Dim index = Me._files.IndexOf(file)

    Dim replacement = Me._files(If(index = 0, 1, index - 1))

    For Each slot In Me.EditorSlots()
      If slot.Item2 Is file Then
        Dim view = slot.Item1
        Me.InView(view, Sub() Me.ActivateFile(replacement))
      End If
    Next

    Me._files.Remove(file)
    If Me._explicitMain Is file Then Me._explicitMain = Nothing
    App.TheWorkspace.RemoveProgramDocument(file.Name)

    Me.UpdateMainModule()
    Me.SetStatus("Unloaded " & Me.TitleOf(file), False)
    Me.Invalidate()

  End Sub

  ' ---- File > Save All

  ''' <summary>Writes every file with unsaved changes (the one without a name yet asks for it), then carries on with <paramref name="afterSave"/>.</summary>
  Public Sub SaveAll(Optional afterSave As Action = Nothing)

    Dim unnamed As ProgramFile = Nothing

    For Each file In Me._files.Where(Function(f) Me.IsDirty(f)).ToList()

      If file.Path Is Nothing Then
        If unnamed Is Nothing Then unnamed = file
        Continue For
      End If

      If Not Me.WriteFile(file, file.Path) Then Return

    Next

    If unnamed IsNot Nothing Then
      Me.ActivateFile(unnamed)
      Me.SaveProgram(afterSave)
    Else
      Me.SetStatus("Saved all files", False)
      If afterSave IsNot Nothing Then afterSave()
    End If

  End Sub

  Private Function WriteFile(file As ProgramFile, path As String) As Boolean

    Dim text = Me.FileText(file)

    Try
      IO.File.WriteAllText(path, text, New System.Text.UTF8Encoding(False))
    Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
      Me.SetStatus("Unable to save " & IO.Path.GetFileName(path) & ": " & ex.Message, True)
      Return False
    End Try

    file.Path = path
    file.SavedText = text
    If file Is Me._active Then Me.DocumentPane.Title = IO.Path.GetFileName(path)

    Return True

  End Function

  ' ---- the procedures of all the files

  Private NotInheritable Class ProcedureInfo
    Public Name As String
    Public Kind As String
    Public Row As Integer
    Public File As ProgramFile
  End Class

  Private Shared ReadOnly ProcedurePattern As New Regex(
    "^\s*(?:(?:Public|Private|Friend|Protected|Shared|Overrides|Overridable|MustOverride|Async|Iterator)\s+)*(?<kind>Sub|Function)\s+(?<name>[A-Za-z_]\w*)",
    RegexOptions.IgnoreCase)

  Private Function FindProcedures(file As ProgramFile) As List(Of ProcedureInfo)
    Dim result As New List(Of ProcedureInfo)()
    Dim lines = Me.FileText(file).Split({vbCrLf, vbCr, vbLf}, StringSplitOptions.None)
    For row = 0 To lines.Length - 1
      Dim m = ProcedurePattern.Match(lines(row))
      If m.Success Then
        result.Add(New ProcedureInfo With {.Name = m.Groups("name").Value, .Kind = m.Groups("kind").Value.ToUpperInvariant(), .Row = row, .File = file})
      End If
    Next
    Return result
  End Function

  Private Function FindProcedures() As List(Of ProcedureInfo)
    Return If(Me._active Is Nothing, New List(Of ProcedureInfo)(), Me.FindProcedures(Me._active))
  End Function

  Private Function ModuleTitle(file As ProgramFile) As String
    Return Me.TitleOf(file)
  End Function

  ' ---- the main module

  Private Shared ReadOnly MainPattern As New Regex("^\s*(?:(?:Public|Friend|Shared)\s+)*Sub\s+Main\b", RegexOptions.IgnoreCase Or RegexOptions.Multiline)
  Private Shared ReadOnly TypePattern As New Regex("^\s*(?:(?:Public|Friend|Partial|NotInheritable|MustInherit)\s+)*(?:Module|Class)\s+(?<name>[A-Za-z_]\w*)", RegexOptions.IgnoreCase Or RegexOptions.Multiline)

  ''' <summary>The file whose Main starts the program: the one chosen with Set Main Module, otherwise the first with a Main.</summary>
  Private Function MainFile() As ProgramFile
    If Me._explicitMain IsNot Nothing AndAlso Me._files.Contains(Me._explicitMain) Then Return Me._explicitMain
    Return If(Me._files.FirstOrDefault(Function(f) MainPattern.IsMatch(Me.FileText(f))), Me._files.FirstOrDefault())
  End Function

  ''' <summary>With several files the compiler has to be told which module's Main to use.</summary>
  Private Sub UpdateMainModule()

    If App.TheWorkspace Is Nothing OrElse Not App.TheWorkspace.CurrentSolution.Projects.Any() Then Return

    Dim typeName As String = Nothing

    If Me._files.Count > 1 Then
      Dim main = Me.MainFile()
      If main IsNot Nothing Then
        Dim m = TypePattern.Match(Me.FileText(main))
        If m.Success Then typeName = m.Groups("name").Value
      End If
    End If

    App.TheWorkspace.SetMainType(typeName)

  End Sub

  Public Sub ShowSetMainModuleDialog()

    Dim d As New FormDialog() With {.Title = "Set Main Module", .Size = FormDialogSize.Wide, .BottomPadding = 0}
    d.AddText("Choose the module that holds the main program", centered:=False)
    Dim files = Me._files.ToList()
    Dim list = d.AddList(files.Select(Function(f) Me.TitleOf(f)).ToList())
    d.AddText("", centered:=False)
    list.SelectedIndex = Math.Max(0, files.IndexOf(Me.MainFile()))

    d.AddButton("< OK >", "O"c,
                Sub()
                  Me._explicitMain = files(list.SelectedIndex)
                  Me.UpdateMainModule()
                  Me.SetStatus(Me.TitleOf(Me._explicitMain) & " is the Main Module", False)
                End Sub, isDefault:=True)
    d.AddButton("< Cancel >", "C"c, isCancel:=True)
    d.AddButton("< Help >", "H"c, Sub() Me.ShowDialogHelp(), closes:=False)
    d.Show(Me)

  End Sub

  ' ---- View > SUBs / Switch to

  ''' <summary>View &gt; SUBs...: choose a file, SUB or FUNCTION of the program to jump to, or delete.</summary>
  Public Sub ShowSubsDialog()

    Dim entries As New List(Of ProcedureInfo)()
    Dim items As New List(Of String)()
    Dim main = Me.MainFile()

    For Each file In Me._files
      entries.Add(New ProcedureInfo With {.File = file})
      items.Add(Me.ModuleTitle(file))
      For Each p In Me.FindProcedures(file)
        entries.Add(p)
        items.Add(p.Name)
      Next
    Next

    Dim d As New FormDialog() With {.Title = "SUBs", .Size = FormDialogSize.Wide, .BottomPadding = 0}
    d.AddText("Choose program item to edit", centered:=False)
    Dim list = d.AddList(items)
    Dim info = d.AddText("", centered:=False)

    Dim describe = Function(entry As ProcedureInfo) As String
                     If entry.Name Is Nothing Then
                       Return Me.ModuleTitle(entry.File) & If(entry.File Is main, " is the Main Module", " is a Module")
                     End If
                     Return entry.Name & " is a " & entry.Kind & " in " & Me.ModuleTitle(entry.File)
                   End Function

    ' start on the procedure or file being edited
    Dim current = entries.FindIndex(Function(x) x.File Is Me._active AndAlso x.Name Is Nothing)
    If current >= 0 Then list.SelectedIndex = current
    info.Text = describe(entries(list.SelectedIndex))
    list.SelectionChanged = Sub() info.Text = describe(entries(list.SelectedIndex))

    d.AddButton("< Edit in Active >", "E"c,
                Sub()
                  Dim entry = entries(list.SelectedIndex)
                  Me.ActivateFile(entry.File)
                  Me.ActiveDocumentBufferView.MoveCursor(entry.Row, 0)
                  Me.Invalidate()
                End Sub,
                isDefault:=True)
    d.AddButton("< Delete >", "D"c,
                Sub() Me.ConfirmDeleteProcedure(entries(list.SelectedIndex)),
                Function()
                  If entries(list.SelectedIndex).Name Is Nothing Then
                    info.Text = "A module cannot be deleted here; use Unload File"
                    Return False
                  End If
                  Return True
                End Function)
    d.AddButton("< Cancel >", "C"c, isCancel:=True)
    d.AddButton("< Help >", "H"c, Sub() Me.ShowDialogHelp(), closes:=False)
    d.Show(Me)

  End Sub

  Public Sub ShowSwitchToDialog()

    Dim d As New FormDialog() With {.Title = "Switch to", .Size = FormDialogSize.Wide, .BottomPadding = 0}
    d.AddText("Choose the file to edit", centered:=False)
    Dim files = Me._files.ToList()
    Dim list = d.AddList(files.Select(Function(f) Me.TitleOf(f)).ToList())
    d.AddText("", centered:=False)
    list.SelectedIndex = Math.Max(0, files.IndexOf(Me._active))

    d.AddButton("< Switch >", "S"c, Sub() Me.ActivateFile(files(list.SelectedIndex)), isDefault:=True)
    d.AddButton("< Cancel >", "C"c, isCancel:=True)
    d.AddButton("< Help >", "H"c, Sub() Me.ShowDialogHelp(), closes:=False)
    d.Show(Me)

  End Sub

  ' for tests: shows the nth file
  Friend Sub ShowSwitchToDialogForTests(index As Integer)
    Me.ActivateFile(Me._files(index))
  End Sub

  ''' <summary>Shows the file a diagnostic was reported in.</summary>
  Private Sub ShowFileOf(diagnostic As Diagnostic)
    If diagnostic.Location.IsInSource Then
      Dim path = diagnostic.Location.SourceTree.FilePath
      Me.ActivateFile(Me._files.FirstOrDefault(Function(f) f.Name = path))
    End If
  End Sub

  ' ---- File > New Program

  ''' <summary>File &gt; New Program: an empty program (after settling any unsaved changes).</summary>
  Public Sub NewProgram()

    If Not Me.HasUnsavedChanges Then
      Me.StartNewProgram()
      Return
    End If

    Dim d As New FormDialog()
    d.AddText("Loaded file is not saved. Save it now?")
    d.AddButton("< Yes >", "Y"c, Sub() Me.SaveAll(AddressOf Me.StartNewProgram), isDefault:=True)
    d.AddButton("< No >", "N"c, Sub() Me.StartNewProgram())
    d.AddButton("<Cancel>", "C"c, isCancel:=True)
    d.Show(Me)

  End Sub

  Private Sub StartNewProgram()
    App.NewUntitledWorkspace()
    Me.ViewDocument("Untitled", "Untitled.vb", "", title:="Untitled")
    If Me.RoslynEnabled Then Me.OnLineCommit(Me.DocumentBuffer)
    Me._savedText = Me.DocumentBuffer.Text
    Me._diagnostics = Nothing
    Me.UpdateDiagnostics(Nothing)
    Me._lastOpenDirectory = Nothing
    Me.SetStatus(Nothing, False)
  End Sub

  ' ---- opening a project or solution

  ''' <summary>Opens a .vbproj, .sln or .slnx: all its source files become the files of the program.</summary>
  Private Sub OpenProjectFile(projectPath As String)

    Dim files As List(Of String)
    Dim texts As New Dictionary(Of String, String)()

    Try
      files = ProjectFiles.SourceFiles(projectPath)
      For Each file In files
        texts(file) = IO.File.ReadAllText(file)
      Next
    Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
      Me.SetStatus("Unable to open " & IO.Path.GetFileName(projectPath) & ": " & ex.Message, True)
      Return
    End Try

    If files.Count = 0 Then
      Me.ShowMessage("No Visual Basic files found in " & IO.Path.GetFileName(projectPath))
      Return
    End If

    Dim root = IO.Path.GetDirectoryName(IO.Path.GetFullPath(projectPath))
    Dim relativeName = Function(path As String) IO.Path.GetRelativePath(root, path)
    Dim first = If(files.FirstOrDefault(Function(f) MainPattern.IsMatch(texts(f))), files(0))

    Me._lastOpenDirectory = root

    App.OpenWorkspace(IO.Path.GetFileNameWithoutExtension(projectPath), {relativeName(first)})
    Me.ResetFiles()
    Dim firstFile = New ProgramFile With {.Name = relativeName(first), .Path = first}
    Me._files.Add(firstFile)

    Me.SetStatus(Nothing, False)
    Me.ViewDocument(Me.DocumentBufferView, Me.DocumentPane, Me.ProgramName(), firstFile.Name, texts(first), title:=firstFile.Name)

    For Each path In files
      If Not String.Equals(path, first, StringComparison.Ordinal) Then Me.AddFile(relativeName(path), path, texts(path))
    Next

    Me.UpdateMainModule()
    If Me.RoslynEnabled Then Me.OnLineCommit(Me.DocumentBuffer)

  End Sub

End Class
