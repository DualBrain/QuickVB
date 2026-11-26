Option Explicit On
Option Strict On
Option Infer On

Imports ConsoleGUI
Imports Microsoft.CodeAnalysis.VisualBasic

' File > Save As Project: the program as a .vbproj (and its source) that runs with "dotnet run".
Partial Public Class MyScreen

  Private ReadOnly ProjectDialog As New FileDialog With {.Mode = FileDialogMode.Save, .Title = "Save As Project", .Pattern = "*.vbproj", .DefaultExtension = ".vbproj", .OnHelp = Sub() Me.ShowDialogHelp()}

  Public Sub ShowSaveAsProjectDialog()
    Me.ProjectDialog.Show(Me, If(Me._currentPath IsNot Nothing, IO.Path.GetDirectoryName(Me._currentPath), Me._lastOpenDirectory),
                          AddressOf Me.ConfirmProject, Nothing)
  End Sub

  Friend Sub ConfirmProjectForTest(path As String)
    Me.ConfirmProject(path)
  End Sub

  Private Sub ConfirmProject(path As String)

    If IO.File.Exists(path) Then
      Dim d As New FormDialog()
      d.AddText(IO.Path.GetFileName(path) & " already exists.")
      d.AddText("Do you want to replace it?")
      d.AddButton("< Yes >", "Y"c, Sub() Me.WarnOfOtherSources(path), isDefault:=True)
      d.AddButton("< No >", "N"c, isCancel:=True)
      d.Show(Me)
    Else
      Me.WarnOfOtherSources(path)
    End If

  End Sub

  ' A project compiles every .vb file below its folder: say so when there are some that are not the program's
  Private Sub WarnOfOtherSources(path As String)

    Dim others = Me.OtherSources(path)

    If others.Count = 0 Then
      Me.WriteProject(path)
      Return
    End If

    Dim shown = String.Join(", ", others.Take(3)) & If(others.Count > 3, ", ... (" & others.Count & " files)", "")
    Dim d As New FormDialog()
    d.AddText("The folder has other .vb files, which the project will compile too:")
    d.AddText(shown)
    d.AddText("Save the project anyway?")
    d.AddButton("< Yes >", "Y"c, Sub() Me.WriteProject(path))
    d.AddButton("< No >", "N"c, isDefault:=True, isCancel:=True)
    d.Show(Me)

  End Sub

  ' the .vb files of the project's folder (and below it) that are not files of this program
  Private Function OtherSources(path As String) As List(Of String)

    Dim folder = IO.Path.GetDirectoryName(IO.Path.GetFullPath(path))
    Dim name = IO.Path.GetFileNameWithoutExtension(path)

    Dim own As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    If Me._files.Count = 0 Then
      own.Add(IO.Path.Combine(folder, If(Me._currentPath IsNot Nothing, IO.Path.GetFileName(Me._currentPath), name & ".vb")))
    Else
      For Each file In Me._files
        own.Add(IO.Path.Combine(folder, If(file.Path IsNot Nothing, IO.Path.GetFileName(file.Path), If(Me._files.Count = 1, name & ".vb", file.Name))))
      Next
    End If

    Dim result As New List(Of String)()
    If Not IO.Directory.Exists(folder) Then Return result

    Dim bin = IO.Path.Combine(folder, "bin") & IO.Path.DirectorySeparatorChar
    Dim obj = IO.Path.Combine(folder, "obj") & IO.Path.DirectorySeparatorChar

    Try
      For Each file In IO.Directory.EnumerateFiles(folder, "*.vb", IO.SearchOption.AllDirectories)
        If file.StartsWith(bin, StringComparison.OrdinalIgnoreCase) OrElse file.StartsWith(obj, StringComparison.OrdinalIgnoreCase) Then Continue For
        If Not own.Contains(file) Then result.Add(IO.Path.GetRelativePath(folder, file))
      Next
    Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
      ' a folder that can't be listed: no warning
    End Try

    Return result

  End Function

  ''' <summary>Writes the project file at <paramref name="path"/> and saves the program's source next to it.</summary>
  Public Sub WriteProject(path As String)

    Dim folder = IO.Path.GetDirectoryName(IO.Path.GetFullPath(path))
    Dim name = IO.Path.GetFileNameWithoutExtension(path)

    If Me._files.Count = 0 Then
      ' a single program: its source is written next to the project (the file it already is, or one named like the project)
      Dim source = If(Me._currentPath IsNot Nothing, IO.Path.GetFileName(Me._currentPath), name & ".vb")
      Dim target = IO.Path.Combine(folder, source)

      If IO.File.Exists(target) AndAlso Not String.Equals(target, Me._currentPath, StringComparison.OrdinalIgnoreCase) Then
        Me.SetStatus(source & " already exists in that folder.", True)
        Return
      End If

      Me._afterSave = Sub() Me.WriteProjectFile(path)
      Me.WriteProgram(target)
      Return
    End If

    ' the files of a program: those that have a place already must be in the folder of the project, the others are written there
    Dim targets As New List(Of Tuple(Of ProgramFile, String))()

    For Each file In Me._files
      Dim target As String
      If file.Path IsNot Nothing Then
        If Not String.Equals(IO.Path.GetDirectoryName(IO.Path.GetFullPath(file.Path)), folder, StringComparison.OrdinalIgnoreCase) Then
          Me.SetStatus("Save " & IO.Path.GetFileName(file.Path) & " in the folder of the project first (File > Save As).", True)
          Return
        End If
        target = file.Path
      Else
        target = IO.Path.Combine(folder, If(Me._files.Count = 1, name & ".vb", file.Name))
        If IO.File.Exists(target) Then
          Me.SetStatus(IO.Path.GetFileName(target) & " already exists in that folder.", True)
          Return
        End If
      End If
      targets.Add(Tuple.Create(file, target))
    Next

    For Each entry In targets
      If Not Me.WriteFile(entry.Item1, entry.Item2) Then Return
    Next

    Me.WriteProjectFile(path)

  End Sub

  Private Sub WriteProjectFile(path As String)

    Dim project = App.TheWorkspace.CurrentSolution.Projects.First()
    Dim options = CType(project.CompilationOptions, VisualBasicCompilationOptions)

    Dim programOptions As New ProgramOptions() With {
      .OptionExplicit = options.OptionExplicit,
      .OptionStrict = options.OptionStrict = OptionStrict.On,
      .OptionInfer = options.OptionInfer,
      .CompareText = options.OptionCompareText}

    Dim draws = False
    Try
      draws = QBLibrary.LibrariesUsedBy(project.GetCompilationAsync().Result).Contains(QBLibrary.Sdl)
    Catch ex As Exception When TypeOf ex Is InvalidOperationException OrElse TypeOf ex Is ArgumentException
      ' without a compilation to ask: a console program
    End Try

    Dim imports_ = options.GlobalImports.Select(Function(g) g.Clause.ToString().Trim())

    Try
      IO.File.WriteAllText(path, ProjectWriter.Text(programOptions, imports_, draws, ProjectWriter.LibraryVersion()), New System.Text.UTF8Encoding(False))
    Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
      Me.SetStatus("Unable to save " & IO.Path.GetFileName(path) & ": " & ex.Message, True)
      Return
    End Try

    Me.SetStatus("Saved " & IO.Path.GetFileName(path) & " (dotnet run in its folder runs it)", False)

  End Sub

End Class
