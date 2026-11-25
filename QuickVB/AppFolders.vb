Option Explicit On
Option Strict On
Option Infer On

Imports System.IO

''' <summary>
''' Where QuickVB keeps things. The folder it is installed in is never written to (it may be read-only, shared or in a tool store):
''' settings go to the user's configuration folder and each running QuickVB builds in a folder of its own under the user's data folder.
''' Set the QUICKVB_HOME environment variable, or put a file named <c>quickvb.portable</c> next to the program, to keep everything
''' together in one folder instead (QUICKVB_HOME itself, or a <c>data</c> folder beside the program).
''' </summary>
Public Module AppFolders

  Public Const HomeVariable As String = "QUICKVB_HOME"
  Public Const PortableMarker As String = "quickvb.portable"

  ' tests point this somewhere of their own
  Friend Property HomeOverride As String

  Private _processWorkFolder As String

  ''' <summary>The folder everything is kept in when QuickVB is told so (see above); Nothing when the user's own folders are used.</summary>
  Public ReadOnly Property PortableHome As String
    Get

      If Not String.IsNullOrEmpty(HomeOverride) Then Return HomeOverride

      Dim fromEnvironment = Environment.GetEnvironmentVariable(HomeVariable)
      If Not String.IsNullOrWhiteSpace(fromEnvironment) Then Return Path.GetFullPath(fromEnvironment)

      If File.Exists(Path.Combine(AppContext.BaseDirectory, PortableMarker)) Then Return Path.Combine(AppContext.BaseDirectory, "data")

      Return Nothing

    End Get
  End Property

  ''' <summary>The file the Options &gt; Display choices are kept in.</summary>
  Public ReadOnly Property SettingsPath As String
    Get
      Dim home = PortableHome
      If home IsNot Nothing Then Return Path.Combine(home, "settings.ini")
      Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuickVB", "settings.ini")
    End Get
  End Property

  ''' <summary>The folder that holds the build folders of the QuickVB programs that are running.</summary>
  Public ReadOnly Property WorkRoot As String
    Get
      Dim home = PortableHome
      If home IsNot Nothing Then Return Path.Combine(home, "work")
      Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QuickVB", "work")
    End Get
  End Property

  ''' <summary>This running QuickVB's own folder (programs are built under it); created on first use.</summary>
  Public ReadOnly Property ProcessWorkFolder As String
    Get
      If _processWorkFolder Is Nothing Then
        Dim folder = Path.Combine(WorkRoot, Environment.ProcessId & "-" & Guid.NewGuid().ToString("N").Substring(0, 8))
        Directory.CreateDirectory(folder)
        _processWorkFolder = folder
        AddHandler AppDomain.CurrentDomain.ProcessExit, Sub(sender, e) RemoveProcessWorkFolder()
      End If
      Return _processWorkFolder
    End Get
  End Property

  ''' <summary>Deletes this QuickVB's build folder (it is run again from scratch the next time).</summary>
  Public Sub RemoveProcessWorkFolder()
    Dim folder = _processWorkFolder
    If folder Is Nothing Then Return
    _processWorkFolder = Nothing
    TryDelete(folder)
  End Sub

  ''' <summary>Removes the build folders that earlier runs of QuickVB left behind (one that crashed, for example).</summary>
  Public Sub RemoveStaleWorkFolders()

    Dim root = WorkRoot
    If Not Directory.Exists(root) Then Return

    For Each folder In Directory.GetDirectories(root)

      If String.Equals(folder, _processWorkFolder, StringComparison.OrdinalIgnoreCase) Then Continue For

      Dim name = Path.GetFileName(folder)
      Dim dash = name.IndexOf("-"c)
      Dim pid As Integer

      If dash > 0 AndAlso Integer.TryParse(name.Substring(0, dash), pid) Then
        If Not IsRunning(pid) Then TryDelete(folder)
      ElseIf Directory.GetLastWriteTimeUtc(folder) < Date.UtcNow.AddDays(-1) Then
        TryDelete(folder)
      End If

    Next

  End Sub

  Private Function IsRunning(pid As Integer) As Boolean
    Try
      Using p = Process.GetProcessById(pid)
        Return Not p.HasExited
      End Using
    Catch ex As ArgumentException
      Return False
    Catch ex As InvalidOperationException
      Return False
    End Try
  End Function

  Private Sub TryDelete(folder As String)
    Try
      Directory.Delete(folder, True)
    Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
      ' still in use; the next run clears it
    End Try
  End Sub

End Module
