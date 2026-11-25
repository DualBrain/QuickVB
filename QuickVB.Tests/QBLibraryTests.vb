Option Explicit On
Option Strict On
Option Infer On

Imports System.IO
Imports System.Reflection
Imports Microsoft.CodeAnalysis
Imports Xunit

Public Class QBLibraryTests

  Private Shared Function Build(text As String, Optional name As String = "Demo") As (Workspace As QuickVBWorkspace, Diagnostics As List(Of Diagnostic))
    Dim workspace As New QuickVBWorkspace()
    Dim projectId = workspace.CreateProject(name, name)
    Dim documentId = workspace.CreateDocument(projectId, name & ".vb")
    workspace.ChangedDocumentTextInternal(documentId, text)
    Return (workspace, workspace.BuildProgram())
  End Function

  Private Shared Function BinFile(name As String) As String
    Return Path.Combine(AppFolders.ProcessWorkFolder, "bin", name)
  End Function

  <Fact>
  Public Sub TheLibrariesAreEmbeddedInQuickVB()
    Dim names = GetType(QBLibrary).Assembly.GetManifestResourceNames()
    Assert.Contains("QB.Compatibility.dll", names)
    Assert.Contains("QB.Compatibility.Sdl.dll", names)
    Assert.Contains("SDL2-CS.dll", names)
    Assert.Contains("QuickVB.Runtime.dll", names)
  End Sub

  <Fact>
  Public Sub TheNativeWindowLibraryOfEveryPlatformIsEmbedded()
    Dim names = GetType(QBLibrary).Assembly.GetManifestResourceNames()
    Assert.Contains("native/win-x64/SDL2.dll", names)
    Assert.Contains("native/linux-x64/libSDL2.so", names)
    Assert.Contains("native/osx-x64/libSDL2.dylib", names)
  End Sub

  <Fact>
  Public Sub TheSamplesAreEmbedded()
    Assert.Equal({"Nibbles", "Gorilla"}, SampleLibrary.All().Select(Function(s) s.Name).ToArray())
    For Each sample In SampleLibrary.All()
      Assert.Contains("Module Program", sample.Text())
    Next
  End Sub

  <Fact>
  Public Sub TheEmbeddedCopiesAreTheLibrariesQuickVBUses()
    Assert.Equal("QB.Compatibility", Assembly.Load(QBLibrary.Image("QB.Compatibility.dll")).GetName().Name)
    Assert.NotNull(Assembly.Load(QBLibrary.Image("QuickVB.Runtime.dll")).GetType("QuickVB.Runtime.DebugHost"))
  End Sub

  <Fact>
  Public Sub QuickBasicStatementsAreFoundInTheLibraryItself()
    Dim workspace As New QuickVBWorkspace()
    Dim projectId = workspace.CreateProject("Demo", "Demo")
    Dim keywords = QBLibrary.Keywords(workspace.CurrentSolution.GetProject(projectId).GetCompilationAsync().Result)

    Assert.Equal("QB.Video", keywords("LOCATE"))
    Assert.Equal("QB.Video", keywords("locate"))
    Assert.Equal("QB.Video", keywords("PRINT"))
    Assert.Equal("QB.Graphics", keywords("PSET"))
  End Sub

  <Fact>
  Public Sub AProgramThatUsesNothingOfTheLibrariesCarriesNone()
    Dim built = Build("Module Demo" & vbLf & "  Sub Main()" & vbLf & "  End Sub" & vbLf & "End Module")

    Assert.Empty(built.Diagnostics.Where(Function(d) d.Severity = DiagnosticSeverity.Error))
    Assert.True(File.Exists(BinFile("Demo.dll")))
    Assert.False(File.Exists(BinFile("QB.Compatibility.dll")))
    Assert.False(File.Exists(BinFile("QB.Compatibility.Sdl.dll")))
  End Sub

  <Fact>
  Public Sub AProgramThatImportsQBCarriesTheLibraryButNotTheWindow()
    Dim built = Build("Imports QB.Video" & vbLf & "Module Demo" & vbLf & "  Sub Main()" & vbLf & "    CLS()" & vbLf & "    PRINT(""hi"")" & vbLf & "  End Sub" & vbLf & "End Module")

    Assert.Empty(built.Diagnostics.Where(Function(d) d.Severity = DiagnosticSeverity.Error))
    Assert.True(File.Exists(BinFile("QB.Compatibility.dll")))
    Assert.False(File.Exists(BinFile("QB.Compatibility.Sdl.dll")))
    Assert.False(File.Exists(BinFile("SDL2-CS.dll")))
  End Sub

  <Fact>
  Public Sub AProgramThatDrawsAlsoCarriesTheWindowLibraries()
    Dim built = Build("Imports QB.Graphics" & vbLf & "Module Demo" & vbLf & "  Sub Main()" & vbLf & "    SCREEN(1)" & vbLf & "    PSET(10, 10)" & vbLf & "  End Sub" & vbLf & "End Module")

    Assert.Empty(built.Diagnostics.Where(Function(d) d.Severity = DiagnosticSeverity.Error))
    Assert.True(File.Exists(BinFile("QB.Compatibility.dll")))
    Assert.True(File.Exists(BinFile("QB.Compatibility.Sdl.dll")))
    Assert.True(File.Exists(BinFile("SDL2-CS.dll")))
  End Sub

  <Fact>
  Public Sub ACallToAStatementTheProgramDoesNotImportIsNoticed()
    Dim built = Build("Module Demo" & vbLf & "  Sub Main()" & vbLf & "    CLS()" & vbLf & "    LOCATE(1, 1)" & vbLf & "  End Sub" & vbLf & "End Module")

    Dim missing = QBLibrary.MissingImports(built.Workspace.CurrentSolution.Projects.First().GetCompilationAsync().Result)

    Assert.Equal({"QB.Video"}, missing.Single().Value)
  End Sub

  <Fact>
  Public Sub ScreenWithAGraphicsModeAsksForGraphicsNotVideo()
    Dim built = Build("Module Demo" & vbLf & "  Sub Main()" & vbLf & "    SCREEN(9)" & vbLf & "    PRINT(""hi"")" & vbLf & "  End Sub" & vbLf & "End Module")

    Dim missing = QBLibrary.MissingImports(built.Workspace.CurrentSolution.Projects.First().GetCompilationAsync().Result)

    Assert.Equal({"QB.Graphics"}, missing.Single().Value)   ' QB.Graphics has PRINT too
  End Sub

  <Fact>
  Public Sub ScreenZeroIsText()
    Dim built = Build("Module Demo" & vbLf & "  Sub Main()" & vbLf & "    SCREEN(0)" & vbLf & "  End Sub" & vbLf & "End Module")

    Dim missing = QBLibrary.MissingImports(built.Workspace.CurrentSolution.Projects.First().GetCompilationAsync().Result)

    Assert.Equal({"QB.Video"}, missing.Single().Value)
  End Sub

  <Fact>
  Public Sub ImportingVideoDoesNotHideAProgramThatDraws()
    Dim built = Build("Imports QB.Video" & vbLf & "Module Demo" & vbLf & "  Sub Main()" & vbLf & "    SCREEN(9)" & vbLf & "    PRINT(""hi"")" & vbLf & "  End Sub" & vbLf & "End Module")

    Dim missing = QBLibrary.MissingImports(built.Workspace.CurrentSolution.Projects.First().GetCompilationAsync().Result)

    Assert.Equal({"QB.Graphics"}, missing.Single().Value)
  End Sub

  <Fact>
  Public Sub NothingIsAskedForWhenGraphicsIsAlreadyImported()
    Dim built = Build("Imports QB.Graphics" & vbLf & "Module Demo" & vbLf & "  Sub Main()" & vbLf & "    SCREEN(9)" & vbLf & "    PRINT(""hi"")" & vbLf & "  End Sub" & vbLf & "End Module")

    Assert.Empty(QBLibrary.MissingImports(built.Workspace.CurrentSolution.Projects.First().GetCompilationAsync().Result))
  End Sub

  <Fact>
  Public Sub KeywordsAreWrittenInTheirProperCase()
    Dim tree = Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree.ParseText("module program" & vbLf & "  sub Main()" & vbLf & "    dim x as integer = 1 'end sub" & vbLf & "  end sub" & vbLf & "end module")

    Dim changed = tree.GetText().WithChanges(Casing.KeywordChanges(tree.GetRoot())).ToString()

    Assert.Equal("Module program" & vbLf & "  Sub Main()" & vbLf & "    Dim x As Integer = 1 'end sub" & vbLf & "  End Sub" & vbLf & "End Module", changed)
  End Sub

  <Fact>
  Public Sub ANameTakesTheCasingItWasDeclaredWith()
    Dim built = Build("Imports QB.Video" & vbLf & "Module Demo" & vbLf & "  Sub Main()" & vbLf & "    Dim Total As Integer = 1" & vbLf & "    print(total)" & vbLf & "    cls()" & vbLf & "  End Sub" & vbLf & "End Module")

    Dim fixes = Casing.NameFixes(built.Workspace.CurrentSolution.Projects.First().GetCompilationAsync().Result).Single().Value

    Assert.Equal({"print>PRINT", "total>Total", "cls>CLS"}.OrderBy(Function(x) x), fixes.Select(Function(f) f.Old & ">" & f.[New]).OrderBy(Function(x) x))
  End Sub

  <Fact>
  Public Sub AProgramThatDrawsWaitsForAKeyWhenItEnds()
    Dim built = Build("Imports QB.Graphics" & vbLf & "Module Demo" & vbLf & "  Sub Main()" & vbLf & "    SCREEN(9)" & vbLf & "    PRINT(""hi"")" & vbLf & "  End Sub" & vbLf & "End Module")
    Dim compilation = built.Workspace.CurrentSolution.Projects.First().GetCompilationAsync().Result

    Dim changed = QuickVBWorkspace.WaitForKeyAtEnd(compilation)

    Dim lines = changed.SyntaxTrees.Single().GetText().Lines
    Assert.Equal(compilation.SyntaxTrees.Single().GetText().Lines.Count, lines.Count)   ' no line moved
    Assert.Equal("    QB.Video.WaitForKeyAtExit = True : SCREEN(9)", lines(3).ToString())
    Assert.DoesNotContain(changed.GetDiagnostics(), Function(d) d.Severity = Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
  End Sub

  <Fact>
  Public Sub OnlyRunningAProgramMakesItWaitForAKey()
    Dim built = Build("Imports QB.Graphics" & vbLf & "Module Demo" & vbLf & "  Sub Main()" & vbLf & "    SCREEN(9)" & vbLf & "  End Sub" & vbLf & "End Module")
    Dim marker = System.Text.Encoding.ASCII.GetBytes("WaitForKeyAtExit")

    ' built as for Make EXE File: the program just ends
    Assert.False(Contains(File.ReadAllBytes(BinFile("Demo.dll")), marker))

    ' built to run from the editor: it waits
    built.Workspace.BuildProgram(True)
    Assert.True(Contains(File.ReadAllBytes(BinFile("Demo.dll")), marker))
  End Sub

  Private Shared Function Contains(bytes As Byte(), part As Byte()) As Boolean
    Return bytes.AsSpan().IndexOf(part.AsSpan()) >= 0
  End Function

  <Fact>
  Public Sub ABareStatementCountsToo()
    Dim built = Build("Module Demo" & vbLf & "  Sub Main()" & vbLf & "    CLS" & vbLf & "  End Sub" & vbLf & "End Module")

    Dim missing = QBLibrary.MissingImports(built.Workspace.CurrentSolution.Projects.First().GetCompilationAsync().Result)

    Assert.Equal({"QB.Video"}, missing.Single().Value)
  End Sub

  <Fact>
  Public Sub NothingIsAskedForWhenTheImportIsThereOrTheProgramDeclaresTheName()
    Dim imported = Build("Imports QB.Video" & vbLf & "Module Demo" & vbLf & "  Sub Main()" & vbLf & "    CLS()" & vbLf & "  End Sub" & vbLf & "End Module")
    Assert.Empty(QBLibrary.MissingImports(imported.Workspace.CurrentSolution.Projects.First().GetCompilationAsync().Result))

    Dim own = Build("Module Demo" & vbLf & "  Sub CLS()" & vbLf & "  End Sub" & vbLf & "  Sub Main()" & vbLf & "    CLS()" & vbLf & "  End Sub" & vbLf & "End Module")
    Assert.Empty(QBLibrary.MissingImports(own.Workspace.CurrentSolution.Projects.First().GetCompilationAsync().Result))

    Dim plain = Build("Module Demo" & vbLf & "  Sub Main()" & vbLf & "    Console.WriteLine(1)" & vbLf & "  End Sub" & vbLf & "End Module")
    Assert.Empty(QBLibrary.MissingImports(plain.Workspace.CurrentSolution.Projects.First().GetCompilationAsync().Result))
  End Sub

  <Fact>
  Public Sub AProgramUsingTheLibraryRunsOnItsOwn()
    Dim built = Build("Imports QB.Video" & vbLf & "Module Demo" & vbLf & "  Sub Main()" & vbLf & "    PRINT(""Hello from QB"")" & vbLf & "  End Sub" & vbLf & "End Module")
    Assert.Empty(built.Diagnostics.Where(Function(d) d.Severity = DiagnosticSeverity.Error))

    Dim info As New System.Diagnostics.ProcessStartInfo(QuickVBWorkspace.DotnetHost(), """" & BinFile("Demo.dll") & """") With {.RedirectStandardOutput = True, .RedirectStandardInput = True, .UseShellExecute = False, .CreateNoWindow = True}
    Using proc = System.Diagnostics.Process.Start(info)
      proc.StandardInput.Close()
      Dim output = proc.StandardOutput.ReadToEnd()
      Assert.True(proc.WaitForExit(30000))
      Assert.Equal(0, proc.ExitCode)
      Assert.Contains("Hello from QB", output)
    End Using
  End Sub

  <Fact>
  Public Sub TheNativeWindowLibraryTravelsWithAProgramThatDraws()
    Build("Imports QB.Graphics" & vbLf & "Module Demo" & vbLf & "  Sub Main()" & vbLf & "    PSET(1, 1)" & vbLf & "  End Sub" & vbLf & "End Module")

    Dim native = If(OperatingSystem.IsWindows(), "SDL2.dll", If(OperatingSystem.IsMacOS(), "libSDL2.dylib", "libSDL2.so"))
    Assert.True(File.Exists(BinFile(native)))
  End Sub

  ' the sample games, ported to QB.Compatibility, show the whole thing working: they must compile in QuickVB as they are
  <Theory>
  <InlineData("Gorilla", "GORILLA.vb", True)>
  <InlineData("Nibbles", "NIBBLES.vb", False)>
  Public Sub TheSampleGamesCompileInQuickVB(folderName As String, fileName As String, draws As Boolean)
    Dim folder = AppContext.BaseDirectory
    Dim source As String = Nothing
    Do While folder IsNot Nothing
      Dim candidate = Path.Combine(folder, folderName, fileName)
      If File.Exists(candidate) Then source = candidate : Exit Do
      folder = Path.GetDirectoryName(folder)
    Loop
    If source Is Nothing Then Return   ' running without the source tree

    Dim built = Build(File.ReadAllText(source), folderName)

    Assert.Empty(built.Diagnostics.Where(Function(d) d.Severity = DiagnosticSeverity.Error))
    Assert.Empty(QBLibrary.MissingImports(built.Workspace.CurrentSolution.Projects.First().GetCompilationAsync().Result))
    Assert.True(File.Exists(BinFile("QB.Compatibility.dll")))
    Assert.Equal(draws, File.Exists(BinFile("QB.Compatibility.Sdl.dll")))
  End Sub

  Private Shared Function SubsystemOf(path As String) As System.Reflection.PortableExecutable.Subsystem
    Using stream = File.OpenRead(path)
      Using reader As New System.Reflection.PortableExecutable.PEReader(stream)
        Return reader.PEHeaders.PEHeader.Subsystem
      End Using
    End Using
  End Function

  <Fact>
  Public Sub AProgramThatDrawsIsBuiltAsAWindowsProgram()
    Dim built = Build("Imports QB.Graphics" & vbLf & "Module Demo" & vbLf & "  Sub Main()" & vbLf & "    SCREEN(1)" & vbLf & "    PSET(10, 10)" & vbLf & "  End Sub" & vbLf & "End Module")

    Assert.Empty(built.Diagnostics.Where(Function(d) d.Severity = DiagnosticSeverity.Error))
    Assert.True(built.Workspace.IsWindowedProgram)
    Assert.Equal(System.Reflection.PortableExecutable.Subsystem.WindowsGui, SubsystemOf(BinFile("Demo.dll")))
  End Sub

  <Fact>
  Public Sub ATextProgramStaysAConsoleProgram()
    Dim built = Build("Imports QB.Video" & vbLf & "Module Demo" & vbLf & "  Sub Main()" & vbLf & "    PRINT(""hi"")" & vbLf & "  End Sub" & vbLf & "End Module")

    Assert.False(built.Workspace.IsWindowedProgram)
    Assert.Equal(System.Reflection.PortableExecutable.Subsystem.WindowsCui, SubsystemOf(BinFile("Demo.dll")))
  End Sub

  <Fact>
  Public Sub ALauncherForAProgramThatDrawsIsAWindowsProgramToo()
    Dim image = {CByte(&H4D), CByte(&H5A)}.Concat(New Byte(&H200) {}).ToArray()   ' a minimal "executable": MZ, a PE header at 0x80
    BitConverter.GetBytes(&H80).CopyTo(image, &H3C)
    image(&H80) = &H50 : image(&H81) = &H45
    image(&H80 + 24 + 68) = 3

    QuickVBWorkspace.SetWindowsSubsystem(image)
    Assert.Equal(2, image(&H80 + 24 + 68))

    Dim notAnExecutable = {CByte(1), CByte(2), CByte(3)}.Concat(New Byte(&H200) {}).ToArray()
    QuickVBWorkspace.SetWindowsSubsystem(notAnExecutable)   ' left alone
    Assert.Equal(CByte(0), notAnExecutable(&H80 + 24 + 68))
  End Sub

End Class
