Option Explicit On
Option Strict On
Option Infer On

Imports System.IO
Imports System.Text.RegularExpressions

''' <summary>Finds the Visual Basic source files of a .vbproj, .sln or .slnx file.</summary>
Public Module ProjectFiles

  Public Function IsProjectFile(path As String) As Boolean
    Select Case IO.Path.GetExtension(path).ToLowerInvariant()
      Case ".vbproj", ".sln", ".slnx"
        Return True
      Case Else
        Return False
    End Select
  End Function

  ''' <summary>The .vb files a project or solution compiles, in path order (full paths).</summary>
  Public Function SourceFiles(path As String) As List(Of String)

    Dim result As New List(Of String)()
    Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

    For Each projectPath In ProjectsOf(path)
      For Each file In SourcesOfProject(projectPath)
        If seen.Add(file) Then result.Add(file)
      Next
    Next

    Return result

  End Function

  Private Function ProjectsOf(path As String) As List(Of String)

    Dim extension = IO.Path.GetExtension(path).ToLowerInvariant()

    If extension = ".vbproj" Then Return New List(Of String) From {path}

    Dim projects As New List(Of String)()
    Dim pattern = If(extension = ".slnx", "Path\s*=\s*""([^""]+\.vbproj)""", "Project\(""[^""]*""\)\s*=\s*""[^""]*"",\s*""([^""]+\.vbproj)""")
    Dim folder = IO.Path.GetDirectoryName(IO.Path.GetFullPath(path))

    For Each match As Match In Regex.Matches(File.ReadAllText(path), pattern, RegexOptions.IgnoreCase)
      Dim project = IO.Path.GetFullPath(IO.Path.Combine(folder, match.Groups(1).Value.Replace("\"c, IO.Path.DirectorySeparatorChar)))
      If File.Exists(project) Then projects.Add(project)
    Next

    Return projects

  End Function

  Private Function SourcesOfProject(projectPath As String) As List(Of String)

    Dim text = File.ReadAllText(projectPath)
    Dim folder = IO.Path.GetDirectoryName(IO.Path.GetFullPath(projectPath))
    Dim files As New List(Of String)()

    ' an SDK style project compiles every .vb file below its folder (apart from bin and obj) unless it says otherwise
    Dim sdkStyle = Regex.IsMatch(text, "<Project\s+[^>]*Sdk\s*=", RegexOptions.IgnoreCase)
    Dim defaults = sdkStyle AndAlso Not Regex.IsMatch(text, "<EnableDefaultCompileItems>\s*false\s*<", RegexOptions.IgnoreCase)

    If defaults Then
      Dim bin = IO.Path.Combine(folder, "bin") & IO.Path.DirectorySeparatorChar
      Dim obj = IO.Path.Combine(folder, "obj") & IO.Path.DirectorySeparatorChar
      For Each file In Directory.EnumerateFiles(folder, "*.vb", SearchOption.AllDirectories)
        If Not file.StartsWith(bin, StringComparison.OrdinalIgnoreCase) AndAlso Not file.StartsWith(obj, StringComparison.OrdinalIgnoreCase) Then files.Add(file)
      Next
    End If

    For Each match As Match In Regex.Matches(text, "<Compile\s+Include\s*=\s*""([^""]+)""", RegexOptions.IgnoreCase)
      Dim file = IO.Path.GetFullPath(IO.Path.Combine(folder, match.Groups(1).Value.Replace("\"c, IO.Path.DirectorySeparatorChar)))
      If System.IO.File.Exists(file) AndAlso Not files.Contains(file, StringComparer.OrdinalIgnoreCase) Then files.Add(file)
    Next

    For Each match As Match In Regex.Matches(text, "<Compile\s+Remove\s*=\s*""([^""]+)""", RegexOptions.IgnoreCase)
      Dim removed = IO.Path.GetFullPath(IO.Path.Combine(folder, match.Groups(1).Value.Replace("\"c, IO.Path.DirectorySeparatorChar)))
      files.RemoveAll(Function(f) String.Equals(f, removed, StringComparison.OrdinalIgnoreCase))
    Next

    files.Sort(StringComparer.OrdinalIgnoreCase)

    Return files

  End Function

End Module
