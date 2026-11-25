Option Explicit On
Option Strict On
Option Infer On

Imports ConsoleGUI

Partial Public Class MyScreen

  ''' <summary>
  ''' When the code in an editor calls a QuickBASIC statement (LOCATE, CLS, ...) that nothing declares, the QB namespace that has it is imported
  ''' at the top of the file (<c>Imports QB.Video</c>). Nothing takes the line out again: it is ordinary code that stays until the person removes it.
  ''' </summary>
  Private Sub AddMissingQbImports()

    Dim workspace = App.TheWorkspace
    Dim missing = workspace.MissingQbImports
    If missing Is Nothing OrElse missing.Count = 0 Then Return

    workspace.MissingQbImports = New Dictionary(Of String, List(Of String))()

    For Each slot In Me.EditorSlots()

      Dim view = slot.Item1
      Dim file = slot.Item2
      Dim needed As List(Of String) = Nothing
      If file Is Nothing OrElse Not missing.TryGetValue(file.Name, needed) Then Continue For

      Dim buffer = TryCast(view.Buffer, DocumentTextBuffer)
      If buffer Is Nothing OrElse view.IsReadOnly Then Continue For

      Dim row = ImportsRow(buffer)
      Dim cursorRow = view.CursorRow
      Dim cursorColumn = view.CursorColumn

      view.Edit(Sub()
                  For Each name In needed
                    buffer.InsertLine(row, "Imports " & name)
                    row += 1
                  Next
                End Sub)

      ' the cursor stays on the text it was on
      Dim firstRow = row - needed.Count
      If cursorRow >= firstRow Then view.MoveCursor(cursorRow + needed.Count, cursorColumn)

      Me.SetStatus("Added " & String.Join(", ", needed.Select(Function(n) "Imports " & n)), False)
      Me.RefreshDocument(buffer)

    Next

  End Sub

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
