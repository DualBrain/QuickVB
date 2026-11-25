' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Imports ConsoleGUI
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Text

Public Class DocumentTextBufferView
  Inherits TextBufferView

  Public Shadows Property Buffer As DocumentTextBuffer
    Get
      Return CType(MyBase.Buffer, DocumentTextBuffer)
    End Get
    Set(value As DocumentTextBuffer)
      MyBase.Buffer = value
    End Set
  End Property

  ' There is nothing to edit until the first document has been loaded (keys and clicks can arrive before that at start-up).
  Private ReadOnly Property HasDocument As Boolean
    Get
      Return Me.Buffer IsNot Nothing AndAlso Me.Buffer.Document IsNot Nothing
    End Get
  End Property

  Public Overrides Sub OnBeforeKeyDown(sender As Object, e As KeyEvent)
    If Me.HasDocument Then MyBase.OnBeforeKeyDown(sender, e)
  End Sub

  Public Overrides Sub OnMouseDown(sender As Object, e As MouseEvent)
    If Me.HasDocument Then MyBase.OnMouseDown(sender, e)
  End Sub

  Sub LoadDocument(projectName As String, documentName As String)

    Dim workspace = App.TheWorkspace

    If workspace IsNot Nothing AndAlso Me.Buffer.DocumentId IsNot Nothing Then
      workspace.UnregisterTextBuffer(Me.Buffer.DocumentId, Me.Buffer)
    End If

    Dim project = workspace.CurrentSolution.Projects.Where(Function(proj) proj.Name = projectName).Single()
    Dim document = project.Documents.Where(Function(doc) doc.Name = documentName).Single()
    Me.Buffer.DocumentId = document.Id

    workspace.RegisterTextBuffer(document, Me.Buffer)

  End Sub

  Dim _completionList As CompletionList = Nothing

  Public Sub MaybeTriggerCompletionList(triggerChar As Char?)

    Dim theScreen = Screen.GetScreen()

    If CompletionList.CheckIfActive(Me._completionList) Then Me._completionList.Deactivate(theScreen)

    Dim document = Me.Buffer.Document
    If document Is Nothing Then Exit Sub

    Me._completionList = CompletionList.TryCreate(document, Me, theScreen, triggerChar)

    If Me._completionList IsNot Nothing Then Me._completionList.Activate(theScreen, Nothing)

  End Sub

  ReadOnly Property IsCompletionListActive As Boolean
    Get
      Return CompletionList.CheckIfActive(Me._completionList)
    End Get
  End Property

  Dim _activeDiagnostics As List(Of Diagnostic)

  Public Sub UpdateDiagnostics(allDiagnostics As IEnumerable(Of Diagnostic))

    Dim theScreen = CType(Screen.GetScreen(), MyScreen)

    If allDiagnostics Is Nothing Then
      theScreen.SetStatus(Nothing, False)
      Exit Sub
    End If

    Dim document = Me.Buffer.Document
    If document Is Nothing Then Exit Sub

    Dim position = document.GetTextAsync.Result.GetPositionFromLineAndColumn(Me.CursorRow, Me.CursorColumn)
    Dim span = New TextSpan(position, 0)

    Dim tree = document.GetSyntaxTreeAsync.Result

    Me._activeDiagnostics = (From diagnostic In allDiagnostics
                             Where diagnostic.Location.IsInSource AndAlso
                                   diagnostic.Location.SourceTree.FilePath = tree.FilePath AndAlso
                                   diagnostic.Location.SourceSpan.IntersectsWith(position)).ToList()

    If Me._activeDiagnostics.Any() Then

      Dim descriptions = Me._activeDiagnostics.Select(Function(diagnostic) diagnostic.GetMessage())

      Dim description As String
      If descriptions.Count = 1 Then
        description = descriptions.First
      Else
        Dim moreDiagnostics = String.Format(" (+ {0} more...)", Me._activeDiagnostics.Count - 1)
        description = descriptions.First + moreDiagnostics
      End If

      theScreen.SetStatus(description, True)

    Else
      theScreen.SetStatus(Nothing, False)
    End If

  End Sub

End Class