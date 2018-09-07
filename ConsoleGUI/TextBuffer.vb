' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Imports System.Collections.ObjectModel
'Imports System.Runtime.CompilerServices
'Imports System.Text
'Imports System.Threading.Tasks

Namespace ConsoleGUI

  Public Class TextBuffer

    Private _lines As ObservableCollection(Of TextBufferLine)

    Public Event BufferChanged As EventHandler

    Public Sub New()
      Me.InitLines(Enumerable.Empty(Of TextBufferLine)())
    End Sub

    Private Sub InitLines(lines As IEnumerable(Of TextBufferLine))
      Me._lines = New ObservableCollection(Of TextBufferLine)(lines)
      AddHandler Me._lines.CollectionChanged, AddressOf Me.Lines_CollectionChanged
    End Sub

    Private Sub Lines_CollectionChanged(sender As Object, e As System.Collections.Specialized.NotifyCollectionChangedEventArgs)
      RaiseEvent BufferChanged(Me, Nothing)
    End Sub

    Public Property Text() As String
      Get
        Return String.Join(vbCrLf, Me._lines.Select(Function(l) l.Text))
      End Get
      Set(value As String)

        Dim textLines = value.Split({vbCrLf, vbCr, vbLf}, StringSplitOptions.None).Select(Function(s) New TextBufferLine With {.Text = s})

        Me.InitLines(textLines)

        RaiseEvent BufferChanged(Me, Nothing)

        'Screen.GetScreen.Invalidate()

      End Set
    End Property

    Public ReadOnly Property Lines() As IReadOnlyList(Of TextBufferLine)
      Get
        Return New ReadOnlyObservableCollection(Of TextBufferLine)(Me._lines)
      End Get
    End Property

    Public Sub InsertText(row As Integer, col As Integer, text As String)

      If row < Me.Lines.Count Then

        Dim line = Me._lines(row)

        Dim oldText = line.Text
        Dim newText = oldText.SubstringPadRight(0, col, col) & text & oldText.SubstringPadRight(col, 0)

        line.Text = newText

        Dim spans = line.ColorSpans

        'For s As Integer = 0 To spans.Count - 1

        '  'If s > spans.Count - 1 Then ' Not sure why this needs to be here?  Somehow s ending up greater than what it should be.
        '  '  Exit For
        '  'End If

        '  Dim span = spans(s)

        '  If span.Contains(col) Then
        '    'line.ColorSpans.RemoveAt(s--); ' Why the s--?????
        '    line.ColorSpans.RemoveAt(s) : Continue For
        '    'line.ColorSpans.RemoveAt(s)
        '    's -= 1 ' Possibly due to this??????? (See above.)
        '  ElseIf col < span.Start Then
        '    span.Start += text.Length
        '  End If

        'Next s

        Dim s = 0
        Do Until s > spans.Count - 1

          Dim span = spans(s)

          If span.Contains(col) Then
            line.ColorSpans.RemoveAt(s)
            s -= 1
          ElseIf col < span.Start Then
            span.Start += text.Length
          End If

          s += 1

        Loop

        RaiseEvent BufferChanged(Me, Nothing)

      Else
        Me.InsertLine(row, New String(" "c, col) & text)
      End If

    End Sub

    Public Sub AppendText(row As Integer, text As String)
      Dim oldText = Me._lines(row).Text
      Me.InsertText(row, oldText.Length, text)
    End Sub

    Public Sub RemoveText(row As Integer, col As Integer, count As Integer)

      Dim line = Me._lines(row)

      Dim oldText = line.Text
      Dim newText = oldText.SubstringPadRight(0, col, col) + oldText.SubstringPadRight(col + count, 0)
      line.Text = newText

      Dim spans = line.ColorSpans

      'For s As Integer = 0 To spans.Count - 1

      '  Dim span = spans(s)

      '  If span.Contains(col) Then
      '    'line.ColorSpans.RemoveAt(s--); ' Why the s--?????
      '    line.ColorSpans.RemoveAt(s)
      '    s -= 1
      '  ElseIf col < span.Start Then
      '    span.Start -= count
      '  End If
      'Next s

      Dim s = 0
      Do Until s > spans.Count - 1

        Dim span = spans(s)

        If span.Contains(col) Then
          line.ColorSpans.RemoveAt(s)
          s -= 1
        ElseIf col < span.Start Then
          span.Start -= count
        End If

        s += 1

      Loop

      RaiseEvent BufferChanged(Me, Nothing)

    End Sub

    Public Function TruncateText(row As Integer, length As Integer) As String
      Dim oldText = Me._lines(row).Text
      Me.RemoveText(row, length, oldText.Length - length)
      Return oldText.SubstringPadRight(length, 0)
    End Function

    Public Sub InsertLine(row As Integer, text As String)
      Dim line = New TextBufferLine With {.Text = text}
      Me._lines.Insert(row, line)
    End Sub

    Public Sub RemoveLine(row As Integer)
      Me._lines.RemoveAt(row)
    End Sub

    Public Function GetLineOrDefault(l As Integer) As TextBufferLine
      Dim line As TextBufferLine
      If l < Me._lines.Count Then
        line = Me._lines(l)
      Else
        line = New TextBufferLine()
      End If
      Return line
    End Function

  End Class

  Public Class TextBufferLine

    ' TODO: Text should only really be set from the TextBuffer methods

    Public Property Text() As String
    Public Property ColorSpans() As List(Of ColorSpan)

    Friend Sub New()
      Me.Text = ""
      Me.ColorSpans = New List(Of ColorSpan)()
    End Sub

  End Class

  Public Class ColorSpan

    Public Start As Integer
    Public Length As Integer
    Public ForegroundColor As ConsoleColor
    Public BackgroundColor As ConsoleColor
    Public IsSticky As Boolean

    Public ReadOnly Property [End]() As Integer
      Get
        Return Me.Start + Me.Length - 1
      End Get
    End Property

    Public Function Contains(position As Integer) As Boolean
      Return (position >= Me.Start AndAlso position <= Me.[End])
    End Function

    Public Function IntersectsWith(start As Integer, [end] As Integer) As Boolean
      Return Me.Contains(start) OrElse
             Me.Contains([end]) OrElse
             (start < Me.Start AndAlso
              [end] > Me.End)
    End Function

  End Class

End Namespace