Option Explicit On
Option Strict On
Option Infer On

Imports System.IO

''' <summary>A program that comes with QuickVB (File &gt; Samples).</summary>
Public NotInheritable Class Sample

  ''' <summary>What the list shows.</summary>
  Public ReadOnly Property Name As String

  ''' <summary>One line about the program.</summary>
  Public ReadOnly Property Description As String

  ''' <summary>The name of the file it is saved as to begin with.</summary>
  Public ReadOnly Property FileName As String

  Friend Sub New(name As String, description As String, fileName As String)
    Me.Name = name
    Me.Description = description
    Me.FileName = fileName
  End Sub

  ''' <summary>The program's text.</summary>
  Public Function Text() As String
    Using stream = GetType(Sample).Assembly.GetManifestResourceStream("samples/" & Me.FileName)
      If stream Is Nothing Then Return ""
      Using reader As New StreamReader(stream, New Text.UTF8Encoding(False))
        Return reader.ReadToEnd()
      End Using
    End Using
  End Function

End Class

''' <summary>The programs that come with QuickVB. They are embedded, so they are there however QuickVB was installed.</summary>
Public Module SampleLibrary

  Private ReadOnly _all As New List(Of Sample) From {
    New Sample("Nibbles", "Steer the snake round the screen and eat the numbers (text screen).", "NIBBLES.vb"),
    New Sample("Gorilla", "Two gorillas throw exploding bananas across a city skyline (draws in a window).", "GORILLA.vb")
  }

  ''' <summary>The samples, in the order the Samples box lists them.</summary>
  Public Function All() As IReadOnlyList(Of Sample)
    Return _all
  End Function

End Module
