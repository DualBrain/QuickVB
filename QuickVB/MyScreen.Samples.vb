Option Explicit On
Option Strict On
Option Infer On

Imports ConsoleGUI

Partial Public Class MyScreen

  Private ReadOnly SampleSaveDialog As New FileDialog With {.Mode = FileDialogMode.Save, .Pattern = "*.vb", .DefaultExtension = ".vb", .OnHelp = Sub() Me.ShowDialogHelp()}

  ''' <summary>File &gt; Samples: the programs that come with QuickVB, to open or to save somewhere to keep.</summary>
  Public Sub ShowSamplesDialog()

    Dim samples = SampleLibrary.All()

    Dim d As New FormDialog() With {.Title = "Samples", .Size = FormDialogSize.Wide, .BottomPadding = 0}
    d.AddText("Choose a sample program", centered:=False)
    Dim list = d.AddList(samples.Select(Function(s) s.Name))
    list.BaseRows = Math.Max(4, samples.Count + 2)   ' the rows inside the frame, and the frame
    list.GrowsWithWindow = False
    Dim info = d.AddText(samples(0).Description, centered:=False)
    list.SelectionChanged = Sub() info.Text = samples(list.SelectedIndex).Description

    d.AddButton("< Open >", "O"c, Sub() Me.OpenSample(samples(list.SelectedIndex)), isDefault:=True)
    d.AddButton("< Save As... >", "S"c, Sub() Me.Post(Sub() Me.ShowSaveSampleDialog(samples(list.SelectedIndex))))
    d.AddButton("< Cancel >", "C"c, isCancel:=True)
    d.AddButton("< Help >", "H"c, Sub() Me.ShowDialogHelp(), closes:=False)
    d.Show(Me)

  End Sub

  ''' <summary>Makes the sample the program being edited (asking first when the program there has changes that are not saved).</summary>
  Public Sub OpenSample(sample As Sample)

    If Not Me.HasUnsavedChanges Then
      Me.LoadSample(sample)
      Return
    End If

    Dim d As New FormDialog()
    d.AddText("Loaded file is not saved. Save it now?")
    d.AddButton("< Yes >", "Y"c, Sub() Me.SaveAll(Sub() Me.LoadSample(sample)), isDefault:=True)
    d.AddButton("< No >", "N"c, Sub() Me.LoadSample(sample))
    d.AddButton("<Cancel>", "C"c, isCancel:=True)
    d.Show(Me)

  End Sub

  Private Sub LoadSample(sample As Sample)

    ' the sample is a program without a file yet: Save asks where to keep it
    App.NewUntitledWorkspace()
    Me.ViewDocument("Untitled", "Untitled.vb", sample.Text(), title:=sample.Name)
    If Me.RoslynEnabled Then Me.OnLineCommit(Me.DocumentBuffer)
    Me._savedText = Me.DocumentBuffer.Text
    Me._diagnostics = Nothing
    Me.UpdateDiagnostics(Nothing)
    Me._lastOpenDirectory = Nothing
    Me.SetStatus("Opened the " & sample.Name & " sample. Use Save As to keep a copy.", False)

  End Sub

  ''' <summary>Writes a sample to a file of the person's choosing, without opening it.</summary>
  Public Sub ShowSaveSampleDialog(sample As Sample)
    Me.SampleSaveDialog.Title = "Save As"
    Me.SampleSaveDialog.SuggestedName = sample.FileName
    Me.SampleSaveDialog.Show(Me, Me._lastOpenDirectory, Sub(path) Me.ConfirmSaveSample(sample, path))
  End Sub

  Private Sub ConfirmSaveSample(sample As Sample, path As String)

    If IO.File.Exists(path) Then
      Dim d As New FormDialog()
      d.AddText(IO.Path.GetFileName(path) & " already exists.")
      d.AddText("Do you want to replace it?")
      d.AddButton("< Yes >", "Y"c, Sub() Me.WriteSample(sample, path), isDefault:=True)
      d.AddButton("< No >", "N"c, isCancel:=True)
      d.Show(Me)
    Else
      Me.WriteSample(sample, path)
    End If

  End Sub

  Private Sub WriteSample(sample As Sample, path As String)

    Try
      IO.File.WriteAllText(path, sample.Text(), New System.Text.UTF8Encoding(False))
    Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
      Me.SetStatus("Unable to save " & IO.Path.GetFileName(path) & ": " & ex.Message, True)
      Return
    End Try

    Me._lastOpenDirectory = IO.Path.GetDirectoryName(path)
    Me.SetStatus("Saved " & IO.Path.GetFileName(path) & ". Open it with Open Program.", False)
    Me.Invalidate()

  End Sub

End Class
