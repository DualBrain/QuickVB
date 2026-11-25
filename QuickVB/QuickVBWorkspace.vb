' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Imports ConsoleGUI
Imports System.IO
Imports System.Threading
Imports Microsoft.CodeAnalysis
Imports System.Reflection
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.Host.Mef
Imports Microsoft.CodeAnalysis.Emit
Imports Microsoft.CodeAnalysis.VisualBasic

Public Class QuickVBWorkspace
  Inherits Workspace

  Private ReadOnly _bufferMap As New Dictionary(Of DocumentId, List(Of TextBuffer))

  Public Sub New()
    MyBase.New(MefHostServices.Create(HostAssemblies), WorkspaceKind.Host)
  End Sub

  Private Shared ReadOnly HostAssemblies As Assembly() = {GetType(Workspace).Assembly,
                                                          Assembly.Load("Microsoft.CodeAnalysis.VisualBasic.Workspaces")}

  ' References for the code being edited must be reference assemblies (not the
  ' implementation assemblies this process happens to be running on).
  Private Shared _references As List(Of MetadataReference)
  Private Shared _desktopReferenced As Boolean

  Private Shared Function GetMetadataReferences() As IEnumerable(Of MetadataReference)

    If _references Is Nothing Then

      Dim refs As New List(Of MetadataReference)(Basic.Reference.Assemblies.Net100.References.All)
      Dim names As New HashSet(Of String)(refs.Select(Function(r) IO.Path.GetFileName(r.Display)), StringComparer.OrdinalIgnoreCase)

      ' Windows Forms / Drawing reference assemblies (Microsoft.WindowsDesktop.App.Ref pack).
      Dim dotnetRoot = IO.Path.GetFullPath(IO.Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "..", "..", ".."))
      Dim packRoot = IO.Path.Combine(dotnetRoot, "packs", "Microsoft.WindowsDesktop.App.Ref")

      If IO.Directory.Exists(packRoot) Then
        Dim refFolder = IO.Directory.GetDirectories(packRoot).
                          Select(Function(d) IO.Path.Combine(d, "ref", "net10.0")).
                          Where(Function(d) IO.Directory.Exists(d)).
                          OrderByDescending(Function(d) d).FirstOrDefault()
        If refFolder IsNot Nothing Then
          For Each file In IO.Directory.GetFiles(refFolder, "*.dll")
            If names.Add(IO.Path.GetFileName(file)) Then refs.Add(MetadataReference.CreateFromFile(file))
          Next
          _desktopReferenced = True
        End If
      End If

      refs.Add(MetadataReference.CreateFromImage(QuickBasicLibrary.Image()))

      _references = refs

    End If

    Return _references

  End Function

  Function CreateProject(name As String, assemblyName As String) As ProjectId
    Dim projId = ProjectId.CreateNewId(name)
    Dim metadataReferences = GetMetadataReferences()
    Dim opts As New VisualBasic.VisualBasicCompilationOptions(OutputKind.ConsoleApplication,
                                                              globalImports:=VisualBasic.GlobalImport.Parse({"System",
                                                                                                             "Microsoft.VisualBasic",
                                                                                                             "QuickBasic.Constants",
                                                                                                             "QuickBasic.Compatibility"}),
                                                              optionExplicit:=False,
                                                              optionStrict:=VisualBasic.OptionStrict.Off,
                                                              optionInfer:=False,
                                                              platform:=Platform.AnyCpu)
    Dim projInfo = ProjectInfo.Create(projId,
                                      VersionStamp.Default,
                                      name,
                                      assemblyName,
                                      LanguageNames.VisualBasic,
                                      compilationOptions:=opts,
                                      metadataReferences:=metadataReferences)
    Me.OnProjectAdded(projInfo)
    Return projId
  End Function

  Function CreateDocument(projId As ProjectId, name As String) As DocumentId
    Dim docId = DocumentId.CreateNewId(projId, name)
    ' the file path is what tells the syntax trees (and so the diagnostics) of the program's files apart
    Me.OnDocumentAdded(DocumentInfo.Create(docId, name, filePath:=name))
    Return docId
  End Function

  ' ---- the files of the program (all of them are compiled together into the one project)

  Private Function ProgramProject() As Project
    Return Me.CurrentSolution.Projects.First()
  End Function

  ''' <summary>Adds a file to the program.</summary>
  Public Function AddProgramDocument(name As String, text As String) As DocumentId
    Dim id = Me.CreateDocument(Me.ProgramProject().Id, name)
    Me.ChangedDocumentTextInternal(id, text)
    Return id
  End Function

  Public Sub RemoveProgramDocument(name As String)
    Dim document = Me.ProgramProject().Documents.FirstOrDefault(Function(d) d.Name = name)
    If document IsNot Nothing Then
      Me._bufferMap.Remove(document.Id)
      Me.OnDocumentRemoved(document.Id)
    End If
  End Sub

  ''' <summary>The current text of a file of the program (Nothing when there is no such file).</summary>
  Public Function GetProgramDocumentText(name As String) As String
    Dim document = Me.ProgramProject().Documents.FirstOrDefault(Function(d) d.Name = name)
    Return If(document Is Nothing, Nothing, document.GetTextAsync().Result.ToString())
  End Function

  ''' <summary>Chooses the module or class whose Main starts the program (Nothing: let the compiler find the only Main).</summary>
  Public Sub SetMainType(typeName As String)
    Dim project = Me.ProgramProject()
    Dim options = CType(project.CompilationOptions, VisualBasicCompilationOptions)
    If String.Equals(options.MainTypeName, typeName, StringComparison.Ordinal) Then Return
    Me.OnCompilationOptionsChanged(project.Id, options.WithMainTypeName(typeName))
  End Sub

  ' TODO: Investigate SourceTextContainer here
  Sub RegisterTextBuffer(document As Document, buffer As TextBuffer)

    If Not Me._bufferMap.ContainsKey(document.Id) Then
      Me._bufferMap(document.Id) = New List(Of TextBuffer)
    End If

    Me._bufferMap(document.Id).Add(buffer)

    buffer.Text = document.GetTextAsync.Result.ToString()

  End Sub

  Sub UnregisterTextBuffer(documentId As DocumentId, buffer As TextBuffer)

    If Me._bufferMap.ContainsKey(documentId) Then

      Me._bufferMap(documentId).Remove(buffer)

      If Me._bufferMap(documentId).Count = 0 Then
        Me._bufferMap.Remove(documentId)
      End If

    End If

  End Sub

  Sub UpdateTextBuffers(document As Document)

    Dim text = document.GetTextAsync.Result.ToString()

    For Each buffer In Me._bufferMap(document.Id)
      buffer.Text = text
    Next

  End Sub

  Public Overrides Function CanApplyChange(feature As ApplyChangesKind) As Boolean
    Return feature = ApplyChangesKind.ChangeDocument
  End Function

  Protected Overrides Sub ApplyDocumentTextChanged(documentId As DocumentId, newText As SourceText)

    Me.OnDocumentTextChanged(documentId, newText, PreservationMode.PreserveIdentity)

    Me.UpdateTextBuffers(Me.CurrentSolution.GetDocument(documentId))

  End Sub

  Sub ChangedDocumentTextInternal(documentId As DocumentId, text As String)
    Me.OnDocumentTextChanged(documentId, SourceText.From(text, System.Text.Encoding.UTF8), PreservationMode.PreserveIdentity)
  End Sub

  ' Compilation:

  Public Function BuildProgram() As List(Of Diagnostic)
    Return Me.CompileProgram(False)
  End Function

  ''' <summary>Where programs are built: a folder of this running QuickVB's own (see <see cref="AppFolders"/>), so nothing is written to the install folder.</summary>
  Private Shared ReadOnly Property BinFolder As String
    Get
      Return IO.Path.Combine(AppFolders.ProcessWorkFolder, "bin")
    End Get
  End Property

  ''' <summary>The folder a program runs in, so that the files it opens by a plain name are found next to its source (Nothing: where QuickVB was started).</summary>
  Public Property ProgramDirectory As String

  Private Function RunFolder() As String
    Return If(Me.ProgramDirectory IsNot Nothing AndAlso Directory.Exists(Me.ProgramDirectory), Me.ProgramDirectory, Directory.GetCurrentDirectory())
  End Function

  ''' <param name="transform">Changes the compilation before it is built (the debugger adds its calls this way).</param>
  Public Function CompileProgram(background As Boolean, Optional cancel As CancellationToken = Nothing, Optional transform As Func(Of Compilation, Compilation) = Nothing) As List(Of Diagnostic)

    Dim diagnostics As New List(Of Diagnostic)

    If Not background Then
      If Directory.Exists(BinFolder) Then Directory.Delete(BinFolder, True)
      Directory.CreateDirectory(BinFolder)

      ' Programs written in QuickVB use the QuickBasic compatibility library (PRINT, etc.); it must sit next to them.
      File.WriteAllBytes(System.IO.Path.Combine(BinFolder, "QuickBasic.dll"), QuickBasicLibrary.Image())
    End If

    ' Builds all .DLLs before all .EXEs

    ' TODO: Proper dependency tracking here

    For Each project In Me.CurrentSolution.Projects.OrderByDescending(Function(p) p.CompilationOptions.OutputKind)

      Dim compilation = project.GetCompilationAsync(cancel).Result
      If transform IsNot Nothing Then compilation = transform(compilation)
      Dim projectDiagnostics = compilation.GetDiagnostics(cancel)

      If background Then
        diagnostics.AddRange(projectDiagnostics)
      Else
        ' Output is always a .NET assembly (.dll); console programs are launched with "dotnet".
        Dim path = Me.EnsureExtension(IO.Path.Combine(BinFolder, project.AssemblyName), ".dll")
        If File.Exists(path) Then File.Delete(path)
        Dim pdbPath = IO.Path.Combine(BinFolder, project.AssemblyName & ".pdb")
        If File.Exists(pdbPath) Then File.Delete(pdbPath)

        Using stream = File.OpenWrite(path), pdbStream = File.OpenWrite(pdbPath)
          Dim result = compilation.Emit(stream, pdbStream, options:=New EmitOptions(debugInformationFormat:=DebugInformationFormat.PortablePdb, pdbFilePath:=pdbPath), cancellationToken:=cancel)
          stream.Close()
          pdbStream.Close()

          If project.CompilationOptions.OutputKind <> OutputKind.DynamicallyLinkedLibrary Then
            Me.WriteRuntimeConfig(IO.Path.Combine(BinFolder, project.AssemblyName & ".runtimeconfig.json"))
          End If

          diagnostics.AddRange(result.Diagnostics)
        End Using
      End If

    Next

    Return diagnostics.OrderByDescending(Function(diagnostic) diagnostic.Severity).ToList

  End Function

  Private Sub WriteRuntimeConfig(path As String)

    Dim version = Environment.Version.Major & ".0.0"

    File.WriteAllText(path, "{" & vbLf &
                            "  ""runtimeOptions"": {" & vbLf &
                            "    ""tfm"": ""net" & Environment.Version.Major & ".0""," & vbLf &
                            "    ""frameworks"": [" & vbLf &
                            "      { ""name"": ""Microsoft.NETCore.App"", ""version"": """ & version & """ }" &
                            If(_desktopReferenced, "," & vbLf & "      { ""name"": ""Microsoft.WindowsDesktop.App"", ""version"": """ & version & """ }", "") & vbLf &
                            "    ]" & vbLf &
                            "  }" & vbLf &
                            "}")

  End Sub

  Private Function EnsureExtension(path As String, ext As String) As String
    If path.ToLowerInvariant().EndsWith(ext.ToLowerInvariant()) Then
      Return path
    Else
      Return path + ext
    End If
  End Function

  ' tests point this at something that isn't there
  Friend Shared Property DotnetHostOverride As String

  ''' <summary>
  ''' The program that runs the programs written in QuickVB: the "dotnet" that started QuickVB, or the one in the .NET installation QuickVB is
  ''' running on, or else whatever "dotnet" the PATH finds.
  ''' </summary>
  Friend Shared Function DotnetHost() As String

    If Not String.IsNullOrEmpty(DotnetHostOverride) Then Return DotnetHostOverride

    Dim fileName = If(OperatingSystem.IsWindows(), "dotnet.exe", "dotnet")

    Dim current = Environment.ProcessPath
    If current IsNot Nothing AndAlso String.Equals(IO.Path.GetFileName(current), fileName, StringComparison.OrdinalIgnoreCase) Then Return current

    Dim root = IO.Path.GetFullPath(IO.Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "..", "..", ".."))
    Dim installed = IO.Path.Combine(root, fileName)
    If File.Exists(installed) Then Return installed

    Return "dotnet"

  End Function

  Public Const DotnetMissingMessage As String = "Unable to start 'dotnet': the .NET runtime must be installed and on the PATH to run programs."

  ''' <summary>Builds and runs the program. Returns the build's diagnostics; when any is an error the program was not run.</summary>
  Public Function RunProgram(screen As Screen, Optional commandLine As String = "") As List(Of Diagnostic)

    Dim noDiagnostics As New List(Of Diagnostic)()

    ' TODO: Run default project rather than only .EXE project

    Dim project = (From p In Me.CurrentSolution.Projects Where p.CompilationOptions.OutputKind <> OutputKind.DynamicallyLinkedLibrary).Single()

    Dim path = Me.EnsureExtension(IO.Path.Combine(BinFolder, project.AssemblyName), ".dll")

    Dim diagnostics = Me.BuildProgram()

    Dim errorDiagnostics = diagnostics.Where(Function(diagnostic) diagnostic.Severity = DiagnosticSeverity.Error)

    If errorDiagnostics.Any() Then Return diagnostics

    screen.SuspendConsole()
    Try
      Console.Clear()
    Catch ex As IOException
      ' not a terminal
    End Try

    Dim info As New ProcessStartInfo(DotnetHost(), """" & path & """" & If(String.IsNullOrEmpty(commandLine), "", " " & commandLine)) With {.CreateNoWindow = False,
                                                 .UseShellExecute = False,
                                                 .WorkingDirectory = Me.RunFolder()}

    Dim proc As Process

    Try
      proc = Process.Start(info)
    Catch ex As System.ComponentModel.Win32Exception
      screen.ReactivateConsole()
      Throw New InvalidOperationException(DotnetMissingMessage, ex)
    End Try

    proc.WaitForExit()

    Console.SetCursorPosition(0, Console.WindowHeight - 1)
    Console.Write("Press any key to continue")
    Console.ReadKey()

    screen.ReactivateConsole()

    Return diagnostics

  End Function

  ' ---- debugging

  ''' <summary>
  ''' Builds the program with the debugger's calls in it. <paramref name="fileNames"/> receives the file names in the order the calls
  ''' number them. Returns the diagnostics of the build; the program is only usable when none of them is an error.
  ''' </summary>
  Public Function BuildDebugProgram(fileNames As List(Of String), captureVariables As Boolean) As List(Of Diagnostic)
    Return Me.CompileProgram(False, Nothing, Function(compilation) Instrumenter.Instrument(compilation, captureVariables, fileNames))
  End Function

  ''' <summary>A session for the program that <see cref="BuildDebugProgram"/> built (it is not started yet).</summary>
  Public Function CreateDebugSession(commandLine As String) As DebugSession

    Dim project = (From p In Me.CurrentSolution.Projects Where p.CompilationOptions.OutputKind <> OutputKind.DynamicallyLinkedLibrary).Single()
    Dim path = Me.EnsureExtension(IO.Path.Combine(BinFolder, project.AssemblyName), ".dll")

    Return New DebugSession(path, Me.RunFolder(), commandLine)

  End Function

  ' ---- Make EXE File / Make Library -----------------------------------------------------

  ' The apphost template of the .NET SDK contains this text where the name of the program's .dll goes.
  Private Const AppHostPlaceholder As String = "c3ab8ff13720e8ad9047dd39466b3c8974e592c2fa383d4a3960714caef0c4f2"

  ''' <summary>The suffix of an executable on this system (".exe" on Windows, none elsewhere).</summary>
  Public Shared ReadOnly Property ExecutableSuffix As String
    Get
      Return If(OperatingSystem.IsWindows(), ".exe", "")
    End Get
  End Property

  ''' <summary>The runtime identifier of this machine as the .NET host packs name it, for example win-x64 or linux-arm64.</summary>
  Private Shared Function HostRuntimeIdentifier() As String

    Dim os = If(OperatingSystem.IsWindows(), "win", If(OperatingSystem.IsMacOS(), "osx", "linux"))
    Dim architecture As String

    Select Case System.Runtime.InteropServices.RuntimeInformation.OSArchitecture
      Case System.Runtime.InteropServices.Architecture.Arm64 : architecture = "arm64"
      Case System.Runtime.InteropServices.Architecture.Arm : architecture = "arm"
      Case System.Runtime.InteropServices.Architecture.X86 : architecture = "x86"
      Case Else : architecture = "x64"
    End Select

    Return os & "-" & architecture

  End Function

  ''' <summary>The apphost (launcher) template that ships with the .NET SDK, or Nothing when it can't be found.</summary>
  Private Shared Function FindAppHostTemplate() As String

    Dim dotnetRoot = IO.Path.GetFullPath(IO.Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "..", "..", ".."))
    Dim rid = HostRuntimeIdentifier()
    Dim packs = IO.Path.Combine(dotnetRoot, "packs", "Microsoft.NETCore.App.Host." & rid)

    If Not Directory.Exists(packs) Then Return Nothing

    Dim fileName = If(OperatingSystem.IsWindows(), "apphost.exe", "apphost")

    Return Directory.GetDirectories(packs).
             OrderByDescending(Function(d) d).
             Select(Function(d) IO.Path.Combine(d, "runtimes", rid, "native", fileName)).
             FirstOrDefault(Function(f) File.Exists(f))

  End Function

  Private Shared Function IndexOfBytes(data() As Byte, pattern() As Byte) As Integer
    For i = 0 To data.Length - pattern.Length
      Dim match = True
      For j = 0 To pattern.Length - 1
        If data(i + j) <> pattern(j) Then
          match = False
          Exit For
        End If
      Next
      If match Then Return i
    Next
    Return -1
  End Function

  ''' <summary>Writes a launcher .exe that starts <paramref name="dllName"/> (next to it) on the installed .NET runtime.</summary>
  Private Shared Sub WriteAppHost(exePath As String, dllName As String)

    Dim template = FindAppHostTemplate()
    If template Is Nothing Then Throw New FileNotFoundException("The .NET apphost template was not found (is the .NET SDK installed?).")

    Dim data = File.ReadAllBytes(template)
    Dim placeholder = System.Text.Encoding.UTF8.GetBytes(AppHostPlaceholder)
    Dim at = IndexOfBytes(data, placeholder)
    If at < 0 Then Throw New InvalidDataException("The apphost template has no program name to fill in.")

    Dim name = System.Text.Encoding.UTF8.GetBytes(dllName)
    Array.Clear(data, at, Math.Max(placeholder.Length, name.Length + 1))
    Array.Copy(name, 0, data, at, name.Length)

    File.WriteAllBytes(exePath, data)

    ' a launcher has to be executable on Linux and macOS
    If Not OperatingSystem.IsWindows() Then
      File.SetUnixFileMode(exePath, UnixFileMode.UserRead Or UnixFileMode.UserWrite Or UnixFileMode.UserExecute Or UnixFileMode.GroupRead Or UnixFileMode.GroupExecute Or UnixFileMode.OtherRead Or UnixFileMode.OtherExecute)
    End If

  End Sub

  ''' <summary>
  ''' Builds the program and writes it as <paramref name="destination"/> (an .exe launcher) with its .dll, runtime configuration and the QuickBasic
  ''' library beside it. Returns the build's diagnostics; nothing is written when any is an error.
  ''' </summary>
  Public Function MakeExecutable(destination As String, includeDebugSymbols As Boolean) As List(Of Diagnostic)

    Dim diagnostics = Me.BuildProgram()
    If diagnostics.Any(Function(d) d.Severity = DiagnosticSeverity.Error) Then Return diagnostics

    Dim project = (From p In Me.CurrentSolution.Projects Where p.CompilationOptions.OutputKind <> OutputKind.DynamicallyLinkedLibrary).Single()
    Dim baseName = IO.Path.GetFileNameWithoutExtension(destination)
    Dim folder = IO.Path.GetDirectoryName(IO.Path.GetFullPath(destination))
    Dim built = IO.Path.Combine(BinFolder, project.AssemblyName)

    Directory.CreateDirectory(folder)

    File.Copy(Me.EnsureExtension(built, ".dll"), IO.Path.Combine(folder, baseName & ".dll"), True)
    File.Copy(built & ".runtimeconfig.json", IO.Path.Combine(folder, baseName & ".runtimeconfig.json"), True)
    File.Copy(IO.Path.Combine(BinFolder, "QuickBasic.dll"), IO.Path.Combine(folder, "QuickBasic.dll"), True)
    If includeDebugSymbols Then File.Copy(built & ".pdb", IO.Path.Combine(folder, baseName & ".pdb"), True)

    WriteAppHost(IO.Path.Combine(folder, baseName & ExecutableSuffix), baseName & ".dll")

    Return diagnostics

  End Function

  ''' <summary>Compiles the program as a class library, <paramref name="destination"/> (a .dll). Returns the diagnostics; nothing is written when any is an error.</summary>
  Public Function MakeLibrary(destination As String, includeDebugSymbols As Boolean) As List(Of Diagnostic)

    Dim project = Me.CurrentSolution.Projects.First()
    Dim compilation = project.GetCompilationAsync().Result
    compilation = compilation.WithOptions(compilation.Options.WithOutputKind(OutputKind.DynamicallyLinkedLibrary))

    Dim path = IO.Path.GetFullPath(destination)
    Directory.CreateDirectory(IO.Path.GetDirectoryName(path))

    Dim errors = compilation.GetDiagnostics().Where(Function(d) d.Severity = DiagnosticSeverity.Error).ToList()
    If errors.Any() Then Return compilation.GetDiagnostics().ToList()

    Using stream = File.Create(path)
      If includeDebugSymbols Then
        Dim pdbPath = IO.Path.ChangeExtension(path, ".pdb")
        Using pdbStream = File.Create(pdbPath)
          Return compilation.Emit(stream, pdbStream, options:=New EmitOptions(debugInformationFormat:=DebugInformationFormat.PortablePdb, pdbFilePath:=pdbPath)).Diagnostics.ToList()
        End Using
      Else
        Return compilation.Emit(stream).Diagnostics.ToList()
      End If
    End Using

  End Function

  Dim _backgroundCompileCancelSource As CancellationTokenSource = Nothing

  ''' <summary>Compiles in the background and delivers the diagnostics on <paramref name="screen"/>'s UI thread.</summary>
  Public Sub BackgroundCompile(screen As Screen, onCompile As Action(Of IReadOnlyList(Of Diagnostic), CancellationToken))

    If Me._backgroundCompileCancelSource IsNot Nothing Then
      Me._backgroundCompileCancelSource.Cancel()
    End If

    Task.Run(Sub()

               Try

                 Me._backgroundCompileCancelSource = New CancellationTokenSource()
                 Dim cancel = Me._backgroundCompileCancelSource.Token

                 Dim diagnostics = Me.CompileProgram(True, cancel)
                 Dim backgroundDiagnostics = diagnostics.AsReadOnly()

                 screen.Post(Sub()

                                             Try
                                               onCompile(backgroundDiagnostics, cancel)
                                             Catch e As OperationCanceledException
                                               ' Let next background compilation update errors
                                             End Try

                                             Me._backgroundCompileCancelSource = Nothing

                                           End Sub)

               Catch e As OperationCanceledException
                 ' Let next background compilation update errors
               End Try

             End Sub)
  End Sub

End Class