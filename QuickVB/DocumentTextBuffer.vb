' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Imports ConsoleGUI
Imports System.Threading
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Classification
Imports Microsoft.CodeAnalysis.Formatting

Public Class DocumentTextBuffer
  Inherits TextBuffer

  Public Property DocumentId As DocumentId

  Public ReadOnly Property Document As Document
    Get
      Return App.TheWorkspace?.CurrentSolution?.GetDocument(Me.DocumentId)
    End Get
  End Property

  Public Sub Colorize(Optional diagnostics As IReadOnlyList(Of Diagnostic) = Nothing, Optional cancel As CancellationToken = Nothing)

    MyScreen.GetScreen.AssertOnUIThread()

    Dim workspace = App.TheWorkspace

    Dim text = Me.Document.GetTextAsync(cancel).Result
    Dim rootNode = Me.Document.GetSyntaxRootAsync(cancel).Result
    If rootNode Is Nothing Then Exit Sub

    Dim spans = Classifier.GetClassifiedSpansAsync(Me.Document, rootNode.FullSpan).Result

    For Each line In Me.Lines
      Dim stickySpans = line.ColorSpans.Where(Function(span) span.IsSticky).ToList()
      line.ColorSpans.Clear()
      line.ColorSpans.AddRange(stickySpans)
    Next

    For Each span In spans

      Dim color As ConsoleColor

      If Me.Document.GetSyntaxRootAsync.Result.Language = LanguageNames.VisualBasic Then
        Select Case span.ClassificationType
          Case ClassificationTypeNames.Keyword
            color = ConsoleColor.Cyan
          Case ClassificationTypeNames.Comment
            color = ConsoleColor.Green
          Case ClassificationTypeNames.NumericLiteral, ClassificationTypeNames.StringLiteral, ClassificationTypeNames.VerbatimStringLiteral
            color = ConsoleColor.Red
          Case ClassificationTypeNames.ClassName, ClassificationTypeNames.ModuleName, ClassificationTypeNames.StructName, ClassificationTypeNames.DelegateName,
               ClassificationTypeNames.EnumName, ClassificationTypeNames.InterfaceName, ClassificationTypeNames.TypeParameterName
            color = ConsoleColor.DarkCyan
          Case Else
            Continue For
        End Select
      Else
        Select Case span.ClassificationType
          Case ClassificationTypeNames.Keyword
            color = ConsoleColor.White
          Case ClassificationTypeNames.Comment
            color = ConsoleColor.DarkGreen
          Case ClassificationTypeNames.NumericLiteral, ClassificationTypeNames.StringLiteral, ClassificationTypeNames.VerbatimStringLiteral
            color = ConsoleColor.DarkRed
          Case ClassificationTypeNames.Identifier
            color = ConsoleColor.Green
          Case Else
            Continue For
        End Select
      End If

      ' TODO: Handle multi-line spans:

      Dim spanStart = text.Lines.GetLinePosition(span.TextSpan.Start)
      Dim spanEnd = text.Lines.GetLinePosition(span.TextSpan.End)

      Dim colorSpan As New ColorSpan With {.Start = spanStart.Character,
                                           .Length = spanEnd.Character - spanStart.Character,
                                           .BackgroundColor = ConsoleColor.DarkBlue,
                                           .ForegroundColor = color}

      Me.Lines(spanStart.Line).ColorSpans.Add(colorSpan)

    Next

    If diagnostics IsNot Nothing Then

      For Each diagnostic In diagnostics.Where(Function(d) d.Location.SourceTree Is Me.Document.GetSyntaxTreeAsync.Result)

        ' TODO: Handle multi-line errors

        Dim span = diagnostic.Location.SourceSpan
        Dim startLine = text.Lines.GetLineFromPosition(span.Start)
        Dim startCol = span.Start - startLine.Start
        Dim endCol = span.End - startLine.Start

        Dim cs = New ColorSpan With {.Start = startCol,
                                     .Length = endCol - startCol,
                                     .IsSticky = True,
                                     .ForegroundColor = ConsoleColor.Gray}

        If diagnostic.Severity = DiagnosticSeverity.Error Then
          cs.BackgroundColor = ConsoleColor.DarkRed
        Else
          cs.BackgroundColor = ConsoleColor.DarkGreen
        End If

        Me.Lines(startLine.LineNumber).ColorSpans.Add(cs)

      Next

      MyScreen.GetScreen().Invalidate()

    End If

  End Sub

  Private _formatting As Boolean = False

  Public Sub Format()

    If Me._formatting Then Exit Sub

    MyScreen.GetScreen.AssertOnUIThread()

    Me._formatting = True

    Dim workspace = App.TheWorkspace

    Dim oldSolution = workspace.CurrentSolution
    Dim oldDocument = oldSolution.GetDocument(Me.DocumentId)
    Dim formattedDocument = Formatter.FormatAsync(oldDocument).Result
    Dim newSolution = formattedDocument.Project.Solution

    workspace.TryApplyChanges(newSolution)

    Me._formatting = False

  End Sub

End Class