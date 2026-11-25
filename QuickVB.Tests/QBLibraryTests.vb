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

  ' GORILLA.BAS, ported to QB.Compatibility, is the sample that shows the whole thing working: it must compile in QuickVB as it is
  <Fact>
  Public Sub TheGorillaSampleCompilesInQuickVBAndCarriesTheWindowLibraries()
    Dim folder = AppContext.BaseDirectory
    Dim source As String = Nothing
    Do While folder IsNot Nothing
      Dim candidate = Path.Combine(folder, "Gorilla", "GORILLA.vb")
      If File.Exists(candidate) Then source = candidate : Exit Do
      folder = Path.GetDirectoryName(folder)
    Loop
    If source Is Nothing Then Return   ' running without the source tree

    Dim built = Build(File.ReadAllText(source), "Gorilla")

    Assert.Empty(built.Diagnostics.Where(Function(d) d.Severity = DiagnosticSeverity.Error))
    Assert.Empty(QBLibrary.MissingImports(built.Workspace.CurrentSolution.Projects.First().GetCompilationAsync().Result))
    Assert.True(File.Exists(BinFile("QB.Compatibility.Sdl.dll")))
  End Sub

End Class
