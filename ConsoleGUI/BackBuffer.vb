' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

'Imports System.Text

Namespace ConsoleGUI

  Public Structure BackBuffer

    Private _frontbufferChars() As Char
    Private _frontbufferForegroundColors() As ConsoleColor
    Private _frontbufferBackgroundColors() As ConsoleColor
    Private _backbufferChars() As Char
    Private _backbufferForegroundColors() As ConsoleColor
    Private _backbufferBackgroundColors() As ConsoleColor
    Private _bufferDimensions As Dimensions

    Friend Sub ResetBufferSize([dim] As Dimensions, fgColor As ConsoleColor, bgColor As ConsoleColor)
      Me._bufferDimensions = [dim]
      Dim bufferSize As Integer = [dim].Width * [dim].Height
      Me._frontbufferChars = New Char(bufferSize - 1) {}
      Me._frontbufferForegroundColors = New ConsoleColor(bufferSize - 1) {}
      Me._frontbufferBackgroundColors = New ConsoleColor(bufferSize - 1) {}
      Me._backbufferChars = New Char(bufferSize - 1) {}
      Me._backbufferForegroundColors = New ConsoleColor(bufferSize - 1) {}
      Me._backbufferBackgroundColors = New ConsoleColor(bufferSize - 1) {}
      Me.ClearBuffer(ConsoleColor.Black, ConsoleColor.Black)
    End Sub

    Public Sub ClearBuffer(fgColor As ConsoleColor, bgColor As ConsoleColor)
      For i = 0 To Me._backbufferChars.Length - 1
        Me._frontbufferChars(i) = " "c
        Me._frontbufferForegroundColors(i) = fgColor
        Me._frontbufferBackgroundColors(i) = bgColor
        Me._backbufferChars(i) = " "c
        Me._backbufferForegroundColors(i) = fgColor
        Me._backbufferBackgroundColors(i) = bgColor
      Next
    End Sub

    Friend Sub SetChar(column As Integer, row As Integer, ch As Char, foreground As ConsoleColor, background As ConsoleColor)
      Dim index As Integer = column + row * Me._bufferDimensions.Width
      Me._backbufferChars(index) = ch
      Me.SetColor(column, row, foreground, background)
    End Sub

    Friend Sub SetColor(column As Integer, row As Integer, foreground As ConsoleColor, background As ConsoleColor)
      Dim index As Integer = column + row * Me._bufferDimensions.Width
      Me._backbufferForegroundColors(index) = foreground
      Me._backbufferBackgroundColors(index) = background
    End Sub

    Friend Sub ShadowChar(column As Integer, row As Integer)

      Dim index As Integer = column + row * Me._bufferDimensions.Width

      Dim foreground() As ConsoleColor
      Dim background() As ConsoleColor

      ' Some chars are drawn inverted, but we don't know at this point.
      ' Use black foreground color as a heuristic for this case.

      If Me._backbufferForegroundColors(index) = ConsoleColor.Black Then
        foreground = Me._backbufferBackgroundColors
        background = Me._backbufferForegroundColors
      Else
        foreground = Me._backbufferForegroundColors
        background = Me._backbufferBackgroundColors
      End If

      Dim fg As Integer = CInt(foreground(index))

      If fg = 7 Then
        fg = 8
      Else
        fg = fg And Not &H8
      End If

      foreground(index) = CType(fg, ConsoleColor)
      background(index) = ConsoleColor.Black

    End Sub

    Public ReadOnly Property Width() As Integer
      Get
        Return Me._bufferDimensions.Width
      End Get
    End Property

    Public ReadOnly Property Height() As Integer
      Get
        Return Me._bufferDimensions.Height
      End Get
    End Property

    Friend Sub CopyToConsole(console As Win32Console)

      Dim dirtyRows(Me._bufferDimensions.Height - 1) As Boolean

      For row = 0 To Me._bufferDimensions.Height - 1
        For column = 0 To Me._bufferDimensions.Width - 1
          If Me.CellChanged(row, column) Then
            dirtyRows(row) = True
            Exit For
          End If
        Next
      Next

      Dim spans = New List(Of RowSpan)()

      For row = 0 To Me._bufferDimensions.Height - 1
        For column = 0 To Me._bufferDimensions.Width - 1

          If Not Me.CellChanged(row, column) Then
            Continue For
          End If

          Dim start = column

          Do While column < Me._bufferDimensions.Width AndAlso Me.CellChanged(row, column)
            column += 1
          Loop

          spans.Add(New RowSpan(row, start, column))

        Next
      Next

      console.WriteOutputBuffer(Me._backbufferChars, Me._backbufferForegroundColors, Me._backbufferBackgroundColors, Me._bufferDimensions.Width, Me._bufferDimensions.Height, spans)

      Dim tempbufferChars = Me._frontbufferChars
      Dim tempbufferForegroundColors = Me._frontbufferForegroundColors
      Dim tempbufferBackgroundColors = Me._frontbufferBackgroundColors

      Me._frontbufferChars = Me._backbufferChars
      Me._frontbufferForegroundColors = Me._backbufferForegroundColors
      Me._frontbufferBackgroundColors = Me._backbufferBackgroundColors

      Me._backbufferChars = tempbufferChars
      Me._backbufferForegroundColors = tempbufferForegroundColors
      Me._backbufferBackgroundColors = tempbufferBackgroundColors

    End Sub

    Private Function CellChanged(row As Integer, column As Integer) As Boolean
      Dim index As Integer = column + row * Me._bufferDimensions.Width
      Return (Me._frontbufferChars(index) <> Me._backbufferChars(index) OrElse Me._frontbufferForegroundColors(index) <> Me._backbufferForegroundColors(index) OrElse Me._frontbufferBackgroundColors(index) <> Me._backbufferBackgroundColors(index))
    End Function

    Friend Sub RestoreBufferToScreen(console As Win32Console)
      console.WriteOutputBuffer(Me._frontbufferChars, Me._frontbufferForegroundColors, Me._frontbufferBackgroundColors, Me._bufferDimensions.Width, Me._bufferDimensions.Height, Nothing)
    End Sub

  End Structure

  Friend Class RowSpan

    Public Sub New(row As Integer, start As Integer, [end] As Integer)
      Me.Row = row
      Me.Start = start
      Me.End = [end]
    End Sub

    Public Property Row() As Integer
    Public Property Start() As Integer
    Public Property [End]() As Integer

  End Class

End Namespace