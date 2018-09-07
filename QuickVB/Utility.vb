' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Formatting
Imports Microsoft.CodeAnalysis.Text

Module Utility

  <Extension>
  Public Function GetPositionFromLineAndColumn(text As SourceText, lineNum As Integer, column As Integer) As Integer

    If lineNum < 0 Then Return 0
    If lineNum >= text.Lines.Count Then Return text.Length

    Dim line = text.Lines(lineNum)

    Return Math.Min(line.Start + column, line.End)

  End Function

  <Extension>
  Public Function GetCleanedSolution(newSolution As Solution, oldSolution As Solution) As Solution

    Dim cleanedSolution = newSolution

    If cleanedSolution IsNot Nothing Then

      Dim solutionChange = newSolution.GetChanges(oldSolution)

      For Each projectChanges In solutionChange.GetProjectChanges()

        Dim documentsToClean = projectChanges.GetChangedDocuments().Concat(projectChanges.GetAddedDocuments())

        For Each documentId In documentsToClean

          Dim newDocument = projectChanges.NewProject.GetDocument(documentId)
          Dim cleanedDocument = Formatter.FormatAsync(newDocument).Result

          cleanedSolution = cleanedDocument.Project.Solution

        Next

      Next

    End If

    Return cleanedSolution

  End Function

  Public Sub CopyDirectory(source As DirectoryInfo, target As DirectoryInfo, Optional excludeFolderFilters As String() = Nothing)

    If source.FullName.ToLower() = target.FullName.ToLower() Then Return

    If Not Directory.Exists(target.FullName) Then
      Directory.CreateDirectory(target.FullName)
    End If

    For Each fi In source.GetFiles()
      fi.CopyTo(Path.Combine(target.FullName, fi.Name), True)
    Next

    For Each diSourceSubDir In source.GetDirectories()

      If excludeFolderFilters IsNot Nothing Then

        Dim filtered As Boolean = False

        For Each ExcludeFolderFilter In excludeFolderFilters
          If String.Equals(diSourceSubDir.Name, ExcludeFolderFilter, StringComparison.InvariantCultureIgnoreCase) Then
            filtered = True
            Exit For
          End If
        Next

        If filtered Then Continue For

      End If

      Dim nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name)

      CopyDirectory(diSourceSubDir, nextTargetSubDir, excludeFolderFilters)

    Next

  End Sub

End Module