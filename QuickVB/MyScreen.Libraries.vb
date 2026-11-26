Option Explicit On
Option Strict On
Option Infer On

Imports ConsoleGUI

Partial Public Class MyScreen

  ''' <summary>
  ''' When the code in an editor calls a QuickBASIC statement (LOCATE, CLS, ...) that nothing declares, the QB namespace that has it is imported
  ''' for the whole program (the project's imports: the source is not touched, and a project written out for it carries the same imports).
  ''' A file that has its own <c>Imports QB.Video</c> and needs QB.Graphics has that line changed instead, so the two are never both in effect.
  ''' </summary>
  Private Sub AddMissingQbImports()

    Dim workspace = App.TheWorkspace
    Dim missing = workspace.MissingQbImports
    If missing Is Nothing OrElse missing.Count = 0 Then Return

    workspace.MissingQbImports = New Dictionary(Of String, List(Of String))()

    Dim forProgram As New List(Of String)()

    For Each slot In Me.EditorSlots()

      Dim view = slot.Item1
      Dim file = slot.Item2
      Dim needed As List(Of String) = Nothing
      If file Is Nothing OrElse Not missing.TryGetValue(file.Name, needed) Then Continue For

      Dim buffer = TryCast(view.Buffer, DocumentTextBuffer)
      If buffer Is Nothing OrElse view.IsReadOnly Then Continue For

      ' QB.Graphics replaces QB.Video (it has all of it; having both makes SCREEN and the like ambiguous)
      Dim video = If(needed.Contains("QB.Graphics"), VideoImportRow(buffer), -1)
      If video >= 0 Then
        view.Edit(Sub()
                    buffer.RemoveLine(video)
                    buffer.InsertLine(video, "Imports QB.Graphics")
                  End Sub)
        needed = needed.Where(Function(n) n <> "QB.Graphics").ToList()
        Me.SetStatus("Changed Imports QB.Video to Imports QB.Graphics", False)
        Me.RefreshDocument(buffer)
      End If

      For Each name In needed
        If Not forProgram.Contains(name) Then forProgram.Add(name)
      Next

    Next

    ' the files that were not changed ask the compiler again once the program imports what they use
    If forProgram.Count = 0 Then Return

    Dim added = workspace.AddGlobalImports(forProgram)
    If added.Count = 0 Then Return

    Me.SetStatus("The program imports " & String.Join(", ", added), False)
    Me.OnLineCommit(Me.DocumentBuffer)

  End Sub

  ''' <summary>Writes the names of the program the way they are declared (print becomes PRINT), as QuickBASIC did when a line was entered.</summary>
  Private Sub ApplyNameCasing()

    Dim workspace = App.TheWorkspace
    Dim fixes = workspace.NameCasingFixes
    If fixes Is Nothing OrElse fixes.Count = 0 Then Return

    workspace.NameCasingFixes = New Dictionary(Of String, List(Of Casing.NameFix))()

    For Each slot In Me.EditorSlots()

      Dim view = slot.Item1
      Dim file = slot.Item2
      Dim list As List(Of Casing.NameFix) = Nothing
      If file Is Nothing OrElse Not fixes.TryGetValue(file.Name, list) Then Continue For

      Dim buffer = TryCast(view.Buffer, DocumentTextBuffer)
      If buffer Is Nothing OrElse view.IsReadOnly Then Continue For

      Dim changed = False

      view.Edit(Sub()
                  For Each entry In list
                    ' only where the text is still what was looked at (the line may have been edited since)
                    If entry.Line >= buffer.Lines.Count Then Continue For
                    Dim text = buffer.Lines(entry.Line).Text
                    If entry.Column + entry.Old.Length > text.Length Then Continue For
                    If Not String.Equals(text.Substring(entry.Column, entry.Old.Length), entry.Old, StringComparison.Ordinal) Then Continue For
                    buffer.RemoveText(entry.Line, entry.Column, entry.Old.Length)
                    buffer.InsertText(entry.Line, entry.Column, entry.[New])
                    changed = True
                  Next
                End Sub)

      If changed Then Me.OnCharacterChanged(buffer)

    Next

  End Sub

  ' the row of the file's Imports QB.Video line, or -1
  Private Shared Function VideoImportRow(buffer As TextBuffer) As Integer

    For i = 0 To buffer.Lines.Count - 1
      If String.Equals(buffer.Lines(i).Text.Trim(), "Imports QB.Video", StringComparison.OrdinalIgnoreCase) Then Return i
    Next

    Return -1

  End Function

  ' below the Option and Imports lines the file starts with (or below its opening comments when it has none)
  Private Shared Function ImportsRow(buffer As TextBuffer) As Integer

    Dim last = -1
    Dim end_ = 0

    Do While end_ < buffer.Lines.Count
      Dim text = buffer.Lines(end_).Text.Trim()
      If text.StartsWith("Option ", StringComparison.OrdinalIgnoreCase) OrElse text.StartsWith("Imports ", StringComparison.OrdinalIgnoreCase) Then
        last = end_
      ElseIf text.Length > 0 AndAlso Not text.StartsWith("'") Then
        Exit Do
      End If
      end_ += 1
    Loop

    If last >= 0 Then Return last + 1

    ' no Option or Imports yet: below the comments at the top, before the code
    Dim row = end_
    Do While row > 0 AndAlso buffer.Lines(row - 1).Text.Trim().Length = 0
      row -= 1
    Loop
    Return row

  End Function

End Class
