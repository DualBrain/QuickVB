Option Explicit On
Option Strict On
Option Infer On

Imports System.IO
Imports Xunit

' Where QuickVB keeps settings and builds: never in the folder it is installed in.
Public Class AppFoldersTests
  Implements IDisposable

  Private ReadOnly _root As String = Path.Combine(Path.GetTempPath(), "QuickVB.Folders." & Guid.NewGuid().ToString("N"))
  Private ReadOnly _savedVariable As String = Environment.GetEnvironmentVariable(AppFolders.HomeVariable)

  Public Sub New()
    Directory.CreateDirectory(Me._root)
    Environment.SetEnvironmentVariable(AppFolders.HomeVariable, Nothing)
    AppFolders.HomeOverride = Nothing
  End Sub

  Public Sub Dispose() Implements IDisposable.Dispose
    Environment.SetEnvironmentVariable(AppFolders.HomeVariable, Me._savedVariable)
    AppFolders.HomeOverride = Nothing
    File.Delete(Path.Combine(AppContext.BaseDirectory, AppFolders.PortableMarker))
    Try
      Directory.Delete(Me._root, True)
    Catch ex As IOException
    End Try
  End Sub

  <Fact>
  Public Sub WithoutAnyOverrideTheUsersOwnFoldersAreUsed()
    Assert.Null(AppFolders.PortableHome)
    Assert.Equal(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuickVB", "settings.ini"), AppFolders.SettingsPath)
    Assert.StartsWith(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QuickVB"), AppFolders.WorkRoot)
    Assert.False(AppFolders.SettingsPath.StartsWith(AppContext.BaseDirectory, StringComparison.OrdinalIgnoreCase))
    Assert.False(AppFolders.WorkRoot.StartsWith(AppContext.BaseDirectory, StringComparison.OrdinalIgnoreCase))
  End Sub

  <Fact>
  Public Sub TheHomeVariableKeepsEverythingInOneFolder()
    Environment.SetEnvironmentVariable(AppFolders.HomeVariable, Me._root)

    Assert.Equal(Me._root, AppFolders.PortableHome)
    Assert.Equal(Path.Combine(Me._root, "settings.ini"), AppFolders.SettingsPath)
    Assert.Equal(Path.Combine(Me._root, "work"), AppFolders.WorkRoot)
  End Sub

  <Fact>
  Public Sub APortableMarkerNextToTheProgramKeepsEverythingInADataFolderThere()
    File.WriteAllText(Path.Combine(AppContext.BaseDirectory, AppFolders.PortableMarker), "")

    Assert.Equal(Path.Combine(AppContext.BaseDirectory, "data"), AppFolders.PortableHome)
    Assert.Equal(Path.Combine(AppContext.BaseDirectory, "data", "settings.ini"), AppFolders.SettingsPath)
  End Sub

  <Fact>
  Public Sub TheHomeVariableWinsOverThePortableMarker()
    File.WriteAllText(Path.Combine(AppContext.BaseDirectory, AppFolders.PortableMarker), "")
    Environment.SetEnvironmentVariable(AppFolders.HomeVariable, Me._root)

    Assert.Equal(Me._root, AppFolders.PortableHome)
  End Sub

  <Fact>
  Public Sub StaleBuildFoldersAreClearedAwayButNotOneInUse()
    AppFolders.HomeOverride = Me._root
    Dim work = AppFolders.WorkRoot
    Directory.CreateDirectory(work)

    Dim dead = Path.Combine(work, "2147483000-deadbeef")        ' no such process
    Dim mine = Path.Combine(work, Environment.ProcessId & "-cafe0001")
    Dim old = Path.Combine(work, "not-a-process-folder")
    Dim fresh = Path.Combine(work, "also-not-a-process-folder")
    Directory.CreateDirectory(dead)
    Directory.CreateDirectory(mine)
    Directory.CreateDirectory(old)
    Directory.CreateDirectory(fresh)
    File.WriteAllText(Path.Combine(dead, "x.txt"), "x")
    Directory.SetLastWriteTimeUtc(old, Date.UtcNow.AddDays(-3))

    AppFolders.RemoveStaleWorkFolders()

    Assert.False(Directory.Exists(dead))
    Assert.True(Directory.Exists(mine))                          ' a running QuickVB's folder
    Assert.False(Directory.Exists(old))
    Assert.True(Directory.Exists(fresh))                         ' too recent to know
  End Sub

  <Fact>
  Public Sub ProgramsAreBuiltInTheWorkFolderNotWhereQuickVBIsInstalled()
    Dim workspace As New QuickVBWorkspace()
    Dim projectId = workspace.CreateProject("Demo", "Demo")
    Dim documentId = workspace.CreateDocument(projectId, "Demo.vb")
    workspace.ChangedDocumentTextInternal(documentId, "Module Demo" & vbLf & "  Sub Main()" & vbLf & "  End Sub" & vbLf & "End Module")

    Assert.Empty(workspace.BuildProgram().Where(Function(d) d.Severity = Microsoft.CodeAnalysis.DiagnosticSeverity.Error))

    Dim built = Path.Combine(AppFolders.ProcessWorkFolder, "bin", "Demo.dll")
    Assert.True(File.Exists(built))
    Assert.True(File.Exists(Path.Combine(AppFolders.ProcessWorkFolder, "bin", "QuickBasic.dll")))
    Assert.False(built.StartsWith(AppContext.BaseDirectory, StringComparison.OrdinalIgnoreCase))
  End Sub

  <Fact>
  Public Sub AProgramRunsInTheFolderItWasGiven()
    Dim workspace As New QuickVBWorkspace()
    Dim projectId = workspace.CreateProject("Demo", "Demo")
    Dim documentId = workspace.CreateDocument(projectId, "Demo.vb")
    workspace.ChangedDocumentTextInternal(documentId, "Module Demo" & vbLf & "  Sub Main()" & vbLf & "    IO.File.WriteAllText(""ran.txt"", ""x"")" & vbLf & "  End Sub" & vbLf & "End Module")
    workspace.ProgramDirectory = Me._root

    Dim names As New List(Of String)()
    Assert.Empty(workspace.BuildDebugProgram(names, True).Where(Function(d) d.Severity = Microsoft.CodeAnalysis.DiagnosticSeverity.Error))

    Dim finished As New Threading.ManualResetEventSlim()
    Using session = workspace.CreateDebugSession("")
      session.OnEnded = Sub(code) finished.Set()
      session.Start(New DebugCommand With {.Mode = "run"})
      Assert.True(finished.Wait(30000), "the program did not end")
    End Using

    Assert.True(File.Exists(Path.Combine(Me._root, "ran.txt")))
  End Sub

End Class
