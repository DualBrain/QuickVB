Option Explicit On
Option Strict On
Option Infer On

Imports System.IO
Imports System.Reflection.Metadata
Imports System.Reflection.PortableExecutable
Imports System.Text

''' <summary>Writes the .vbproj of a program, so that it builds and runs with <c>dotnet run</c> outside QuickVB.</summary>
Public Module ProjectWriter

  ''' <summary>
  ''' The text of a project file: the program's Option settings, the QB namespaces it imports for all its files and the QB.Compatibility
  ''' packages it needs (the SDL2 window one too when <paramref name="draws"/>, which also makes the program a Windows one: no console of its own).
  ''' </summary>
  Public Function Text(options As ProgramOptions, imports_ As IEnumerable(Of String), draws As Boolean, qbVersion As String) As String

    Dim sb As New StringBuilder()
    Dim nl = vbLf

    sb.Append("<Project Sdk=""Microsoft.NET.Sdk"">").Append(nl).Append(nl)
    sb.Append("  <PropertyGroup>").Append(nl)
    sb.Append("    <OutputType>").Append(If(draws, "WinExe", "Exe")).Append("</OutputType>").Append(nl)
    sb.Append("    <TargetFramework>net10.0</TargetFramework>").Append(nl)
    sb.Append("    <RootNamespace />").Append(nl)
    sb.Append("    <OptionExplicit>").Append(OnOff(options.OptionExplicit)).Append("</OptionExplicit>").Append(nl)
    sb.Append("    <OptionStrict>").Append(OnOff(options.OptionStrict)).Append("</OptionStrict>").Append(nl)
    sb.Append("    <OptionInfer>").Append(OnOff(options.OptionInfer)).Append("</OptionInfer>").Append(nl)
    sb.Append("    <OptionCompare>").Append(If(options.CompareText, "Text", "Binary")).Append("</OptionCompare>").Append(nl)
    sb.Append("  </PropertyGroup>").Append(nl).Append(nl)

    Dim names = imports_.Where(Function(n) n.StartsWith("QB.", StringComparison.OrdinalIgnoreCase)).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
    If names.Count > 0 Then
      sb.Append("  <ItemGroup>").Append(nl)
      For Each name In names
        sb.Append("    <Import Include=""").Append(name).Append(""" />").Append(nl)
      Next
      sb.Append("  </ItemGroup>").Append(nl).Append(nl)
    End If

    sb.Append("  <ItemGroup>").Append(nl)
    sb.Append("    <PackageReference Include=""QB.Compatibility"" Version=""").Append(qbVersion).Append(""" />").Append(nl)
    If draws Then sb.Append("    <PackageReference Include=""QB.Compatibility.Sdl"" Version=""").Append(qbVersion).Append(""" />").Append(nl)
    sb.Append("  </ItemGroup>").Append(nl).Append(nl)
    sb.Append("</Project>").Append(nl)

    Return sb.ToString()

  End Function

  Private Function OnOff(value As Boolean) As String
    Return If(value, "On", "Off")
  End Function

  ''' <summary>The version of the QB.Compatibility that travels inside QuickVB (the version of the package a project of its programs refers to).</summary>
  Public Function LibraryVersion() As String

    Dim bytes = QBLibrary.Image(QBLibrary.Core & ".dll")
    If bytes Is Nothing Then Return "0.1.2"

    Using reader As New PEReader(New MemoryStream(bytes))
      Dim version = reader.GetMetadataReader().GetAssemblyDefinition().Version
      Return version.Major & "." & version.Minor & "." & Math.Max(version.Build, 0)
    End Using

  End Function

End Module
