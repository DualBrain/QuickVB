Option Explicit On
Option Strict On
Option Infer On

Imports System.IO
Imports ConsoleGUI
Imports Xunit

Public Class ProjectFilesTests
  Implements IDisposable

  Private ReadOnly _root As String = Path.Combine(Path.GetTempPath(), "QuickVB.Projects." & Guid.NewGuid().ToString("N"))

  Public Sub New()
    Directory.CreateDirectory(Path.Combine(Me._root, "App", "obj"))
    Directory.CreateDirectory(Path.Combine(Me._root, "Lib"))
    File.WriteAllText(Path.Combine(Me._root, "App", "App.vbproj"), "<Project Sdk=""Microsoft.NET.Sdk""><PropertyGroup /></Project>")
    File.WriteAllText(Path.Combine(Me._root, "App", "Main.vb"), "Module Main" & vbCrLf & "End Module")
    File.WriteAllText(Path.Combine(Me._root, "App", "obj", "Generated.vb"), "' not mine")
    File.WriteAllText(Path.Combine(Me._root, "Lib", "Lib.vbproj"), "<Project Sdk=""Microsoft.NET.Sdk""><ItemGroup><Compile Remove=""Skip.vb"" /></ItemGroup></Project>")
    File.WriteAllText(Path.Combine(Me._root, "Lib", "Helper.vb"), "Module Helper" & vbCrLf & "End Module")
    File.WriteAllText(Path.Combine(Me._root, "Lib", "Skip.vb"), "' skipped")
    File.WriteAllText(Path.Combine(Me._root, "All.sln"), "Project(""{F184B08F-C81C-4C6B-8A5E-C68A6A7D2B1C}"") = ""App"", ""App\App.vbproj"", ""{1}""" & vbCrLf & "EndProject" & vbCrLf &
                      "Project(""{F184B08F-C81C-4C6B-8A5E-C68A6A7D2B1C}"") = ""Lib"", ""Lib\Lib.vbproj"", ""{2}""" & vbCrLf & "EndProject")
    File.WriteAllText(Path.Combine(Me._root, "All.slnx"), "<Solution><Project Path=""App/App.vbproj"" /><Project Path=""Lib/Lib.vbproj"" /></Solution>")
  End Sub

  Public Sub Dispose() Implements IDisposable.Dispose
    Try
      Directory.Delete(Me._root, True)
    Catch ex As IOException
    End Try
  End Sub

  Private Function Names(files As List(Of String)) As String()
    Return files.Select(Function(f) Path.GetFileName(f)).ToArray()
  End Function

  <Fact>
  Public Sub AnSdkProjectCompilesItsVbFilesExceptBinAndObj()
    Dim files = ProjectFiles.SourceFiles(Path.Combine(Me._root, "App", "App.vbproj"))
    Assert.Equal({"Main.vb"}, Names(files))
  End Sub

  <Fact>
  Public Sub CompileRemoveDropsAFile()
    Dim files = ProjectFiles.SourceFiles(Path.Combine(Me._root, "Lib", "Lib.vbproj"))
    Assert.Equal({"Helper.vb"}, Names(files))
  End Sub

  <Theory>
  <InlineData("All.sln")>
  <InlineData("All.slnx")>
  Public Sub ASolutionCollectsTheFilesOfItsProjects(solution As String)
    Dim files = ProjectFiles.SourceFiles(Path.Combine(Me._root, solution))
    Assert.Equal({"Main.vb", "Helper.vb"}, Names(files))
  End Sub

  <Fact>
  Public Sub OpeningAProjectWithOneFileOpensThatFile()
    Dim console As New HeadlessConsole(80, 25)
    Dim ui As New MyScreen(console)
    ui.Post(Sub()
              App.NewUntitledWorkspace()
              ui.ViewDocument("Untitled", "Untitled.vb", "Module Untitled" & vbCrLf & "End Module")
              ui.OpenProgram(Path.Combine(Me._root, "App", "App.vbproj"))
            End Sub)
    Screen.NavigateTo(ui)

    Assert.Contains("Module Main", ui.Editor.Text)
  End Sub

  <Fact>
  Public Sub OpeningASolutionLoadsEveryFileOfItsProjects()
    Dim console As New HeadlessConsole(80, 25)
    Dim ui As New MyScreen(console)
    ui.Post(Sub()
              App.NewUntitledWorkspace()
              ui.ViewDocument("Untitled", "Untitled.vb", "Module Untitled" & vbCrLf & "End Module")
              ui.OpenProgram(Path.Combine(Me._root, "All.sln"))
            End Sub)
    Screen.NavigateTo(ui)

    Assert.Equal({"App" & Path.DirectorySeparatorChar & "Main.vb", "Lib" & Path.DirectorySeparatorChar & "Helper.vb"}, ui.FileNames.OrderBy(Function(n) n).ToArray())
    Assert.Contains("Module Main", ui.Editor.Text)
  End Sub

End Class
