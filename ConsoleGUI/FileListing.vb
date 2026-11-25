' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Imports System.IO

Namespace ConsoleGUI

  Public Enum FileEntryKind
    ''' <summary>Goes to the parent folder (or to the list of drives from a drive root).</summary>
    Up
    Folder
    File
    ''' <summary>A drive root, shown when browsing "above" the drive roots.</summary>
    Drive
  End Enum

  Public NotInheritable Class FileEntry

    Public Sub New(kind As FileEntryKind, name As String, path As String)
      Me.Kind = kind
      Me.Name = name
      Me.Path = path
    End Sub

    Public ReadOnly Property Kind As FileEntryKind
    Public ReadOnly Property Name As String
    Public ReadOnly Property Path As String

    ''' <summary>How the entry is shown in a list.</summary>
    Public ReadOnly Property Display As String
      Get
        Select Case Me.Kind
          Case FileEntryKind.Up : Return ".."
          Case FileEntryKind.Folder : Return Me.Name
          Case FileEntryKind.Drive : Return "[-" & Me.Name.TrimEnd(":"c) & "-]"
          Case Else : Return Me.Name
        End Select
      End Get
    End Property

  End Class

  ''' <summary>Builds the entries of a folder for a file dialog: ".." first, then folders, then the files that match a filter.</summary>
  Public NotInheritable Class FileListing

    Private Sub New()
    End Sub

    ''' <summary>
    ''' The entries of <paramref name="directory"/> ("" lists the drives). <paramref name="pattern"/> is a list of wildcard patterns separated by ';'
    ''' (for example "*.bas;*.vb"); Nothing or "" or "*.*" shows every file. Throws IOException / UnauthorizedAccessException like the file system does.
    ''' </summary>
    Public Shared Function Read(directory As String, Optional pattern As String = Nothing) As List(Of FileEntry)

      Dim entries As New List(Of FileEntry)()

      If String.IsNullOrEmpty(directory) Then
        For Each drive In System.IO.DriveInfo.GetDrives()
          entries.Add(New FileEntry(FileEntryKind.Drive, drive.Name.TrimEnd(System.IO.Path.DirectorySeparatorChar), drive.RootDirectory.FullName))
        Next
        Return entries
      End If

      entries.Add(New FileEntry(FileEntryKind.Up, "..", ParentOf(directory)))

      Dim info As New DirectoryInfo(directory)

      For Each d In info.EnumerateDirectories().Where(Function(x) Not IsHidden(x)).OrderBy(Function(x) x.Name, StringComparer.OrdinalIgnoreCase)
        entries.Add(New FileEntry(FileEntryKind.Folder, d.Name, d.FullName))
      Next

      Dim patterns = SplitPatterns(pattern)

      For Each f In info.EnumerateFiles().Where(Function(x) Not IsHidden(x) AndAlso Matches(x.Name, patterns)).OrderBy(Function(x) x.Name, StringComparer.OrdinalIgnoreCase)
        entries.Add(New FileEntry(FileEntryKind.File, f.Name, f.FullName))
      Next

      Return entries

    End Function

    ''' <summary>The folder above <paramref name="directory"/>; "" (the drive list) above a drive root.</summary>
    Public Shared Function ParentOf(directory As String) As String
      Dim parent = System.IO.Path.GetDirectoryName(directory.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar))
      If parent Is Nothing Then
        ' A root: on Windows go up to the drive list, elsewhere stay put.
        Return If(OperatingSystem.IsWindows(), "", directory)
      End If
      Return parent
    End Function

    Public Shared Function SplitPatterns(pattern As String) As String()
      If String.IsNullOrWhiteSpace(pattern) Then Return Array.Empty(Of String)()
      Return pattern.Split({";"c, ","c}, StringSplitOptions.RemoveEmptyEntries).Select(Function(p) p.Trim()).Where(Function(p) p.Length > 0).ToArray()
    End Function

    ''' <summary>True when <paramref name="name"/> matches any pattern (no patterns, or "*.*" / "*", match everything).</summary>
    Public Shared Function Matches(name As String, patterns As IEnumerable(Of String)) As Boolean
      If patterns Is Nothing OrElse Not patterns.Any() Then Return True
      For Each p In patterns
        If p = "*" OrElse p = "*.*" OrElse WildcardMatch(name, p) Then Return True
      Next
      Return False
    End Function

    ''' <summary>Case-insensitive match supporting '*' and '?'.</summary>
    Public Shared Function WildcardMatch(text As String, pattern As String) As Boolean
      Dim t = 0
      Dim p = 0
      Dim starP = -1
      Dim starT = 0
      Do While t < text.Length
        If p < pattern.Length AndAlso (pattern(p) = "?"c OrElse Char.ToUpperInvariant(pattern(p)) = Char.ToUpperInvariant(text(t))) Then
          t += 1
          p += 1
        ElseIf p < pattern.Length AndAlso pattern(p) = "*"c Then
          starP = p
          starT = t
          p += 1
        ElseIf starP >= 0 Then
          p = starP + 1
          starT += 1
          t = starT
        Else
          Return False
        End If
      Loop
      Do While p < pattern.Length AndAlso pattern(p) = "*"c
        p += 1
      Loop
      Return p = pattern.Length
    End Function

    Private Shared Function IsHidden(info As FileSystemInfo) As Boolean
      Return (info.Attributes And (FileAttributes.Hidden Or FileAttributes.System)) <> 0
    End Function

  End Class

End Namespace
