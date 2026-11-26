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

  ''' <summary>
  ''' The Option settings of the first project of a .vbproj, .sln or .slnx (<c>OptionExplicit</c>, <c>OptionStrict</c>, <c>OptionInfer</c> and
  ''' <c>OptionCompare</c> properties); what a project doesn't say is left at the defaults: Explicit On, Strict On, Infer On, Compare Binary.
  ''' </summary>
  Public Function OptionsOf(path As String) As ProgramOptions

    Dim options As New ProgramOptions()

    Try
      Dim project = ProjectsOf(path).FirstOrDefault()
      If project Is Nothing Then Return options

      Dim text = File.ReadAllText(project)
      Dim value = PropertyOf(text, "OptionExplicit")
      If value IsNot Nothing Then options.OptionExplicit = IsOn(value)
      value = PropertyOf(text, "OptionStrict")
      If value IsNot Nothing Then options.OptionStrict = IsOn(value)
      value = PropertyOf(text, "OptionInfer")
      If value IsNot Nothing Then options.OptionInfer = IsOn(value)
      For Each match As Match In Regex.Matches(text, "<Import\s+Include\s*=\s*""([^""]+)""", RegexOptions.IgnoreCase)
        options.GlobalImports.Add(match.Groups(1).Value.Trim())
      Next
      value = PropertyOf(text, "OptionCompare")
      If value IsNot Nothing Then options.CompareText = String.Equals(value, "Text", StringComparison.OrdinalIgnoreCase)
    Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
      ' a project that can't be read: the defaults
    End Try

    Return options

  End Function

  Private Function PropertyOf(text As String, name As String) As String
    Dim match = Regex.Match(text, "<" & name & ">\s*([^<\s]+)\s*</" & name & ">", RegexOptions.IgnoreCase)
    Return If(match.Success, match.Groups(1).Value, Nothing)
  End Function

  Private Function IsOn(value As String) As Boolean
    Return Not (String.Equals(value, "Off", StringComparison.OrdinalIgnoreCase) OrElse String.Equals(value, "False", StringComparison.OrdinalIgnoreCase))
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

''' <summary>
''' The Option settings a program is compiled with: what its project says, or QuickVB's defaults when there is no project
''' (Option Explicit On, Option Strict On, Option Infer On, Option Compare Binary).
''' </summary>
Public NotInheritable Class ProgramOptions
  Public Property OptionExplicit As Boolean = True
  Public Property OptionStrict As Boolean = True
  Public Property OptionInfer As Boolean = True

  ''' <summary>True for Option Compare Text; False (the default) is Option Compare Binary.</summary>
  Public Property CompareText As Boolean = False

  ''' <summary>The namespaces the project imports for all its files (<c>&lt;Import Include="..." /&gt;</c>).</summary>
  Public Property GlobalImports As New List(Of String)()
End Class
