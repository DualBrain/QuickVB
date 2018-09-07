' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

'Imports System.Text
'Imports System.Threading.Tasks

Namespace ConsoleGUI

  Public Class RowChangedEventArgs
    Inherits EventArgs

    Public Property OldRow() As Integer
    Public Property NewRow() As Integer

    Public Sub New(oldRow As Integer, newRow As Integer)
      Me.OldRow = oldRow
      Me.NewRow = newRow
    End Sub

  End Class

  Public Class SelectionChangedEventArgs
    Inherits EventArgs

    Public Property OldSelection() As ViewSpan
    Public Property NewSelection() As ViewSpan

    Public Sub New(oldSelection As ViewSpan, newSelection As ViewSpan)
      Me.OldSelection = oldSelection
      Me.NewSelection = newSelection
    End Sub

  End Class

  Public Class TextBufferView
    Inherits Control

    Private _buffer As TextBuffer

    Public Property Buffer() As TextBuffer
      Get
        Return Me._buffer
      End Get
      Set(value As TextBuffer)
        If Me._buffer IsNot Nothing Then
          RemoveHandler Me._buffer.BufferChanged, AddressOf Me._buffer_BufferChanged
        End If
        Me._buffer = value
        AddHandler Me._buffer.BufferChanged, AddressOf Me._buffer_BufferChanged
      End Set
    End Property

    Private Sub _buffer_BufferChanged(sender As Object, e As EventArgs)
      Dim screen = ConsoleGUI.Screen.GetScreen()
      If screen IsNot Nothing Then
        screen.Invalidate()
      End If
    End Sub

    Public Property WindowLeft() As Integer
    Public Property WindowTop() As Integer

    Public ReadOnly Property WindowRight() As Integer
      Get
        Return Me.WindowLeft + Me.Width - 1
      End Get
    End Property

    Public ReadOnly Property WindowBottom() As Integer
      Get
        Return Me.WindowTop + Me.Height - 1
      End Get
    End Property

    Public Property CursorRow() As Integer
      Get
        Return Me.Selection.StartRow
      End Get
      Private Set(value As Integer)
        Me.Selection = New ViewSpan(value, Me.CursorColumn)
      End Set
    End Property

    Public Property CursorColumn() As Integer
      Get
        Return Me.Selection.StartColumn
      End Get
      Private Set(value As Integer)
        Me.Selection = New ViewSpan(Me.CursorRow, value)
      End Set
    End Property

    Private _selection As ViewSpan

    Public Property Selection() As ViewSpan
      Get
        Return Me._selection
      End Get
      Private Set(value As ViewSpan)
        Me._selection = value
        Me.NotifyCursorMoved(Screen.GetScreen())
      End Set
    End Property

    Public Sub MoveCursor(row As Integer, col As Integer)
      Me.CursorRow = row
      Me.CursorColumn = col
      Me.NotifyCursorMoved(Screen.GetScreen())
    End Sub

    Public Event RowChanged As EventHandler(Of RowChangedEventArgs)
    Public Event SelectionChanged As EventHandler(Of SelectionChangedEventArgs)

    Public Property ScrollBars() As Boolean = True
    Public Property ScrollBarsVisible() As Boolean = True

    Public Overrides Sub Render(screen As Screen)

      Dim cr As Integer = Me.ScreenTop
      Dim wr As Integer = Me.WindowTop

      Do While cr <= Me.ScreenBottom

        Dim line = Me.Buffer.GetLineOrDefault(wr)

        Dim windowLineWidth = Math.Min(line.Text.Length - Me.WindowLeft, Me.Width)

        Dim screenLine = line.Text.SubstringPadRight(Me.WindowLeft, windowLineWidth, Me.Width)

        screen.DrawString(Me.ScreenLeft, cr, screenLine, Me.ForegroundColor, Me.BackgroundColor)

        For Each span In line.ColorSpans

          If span.IntersectsWith(Me.WindowLeft, Me.WindowRight) Then

            Dim l = Me.Left - Me.WindowLeft + span.Start
            Dim r = Me.Left - Me.WindowLeft + span.End

            If l < Me.ScreenLeft Then
              l = Me.ScreenLeft
            End If
            If r > Me.ScreenRight Then
              r = Me.ScreenRight
            End If

            screen.RecolorLine(l, cr, r - l + 1, span.ForegroundColor, span.BackgroundColor)

          End If

        Next

        cr += 1
        wr += 1

      Loop

      Me.RenderScrollBars(screen)

    End Sub

    Private Sub RenderScrollBars(screen As Screen)

      If Me._ScrollBars AndAlso Me.ScrollBarsVisible Then

        Dim fg, bg As ConsoleColor

        If screen.Theme = ControlTheme.Basic Then
          fg = ConsoleColor.Black
          bg = ConsoleColor.Gray
        Else
          fg = ConsoleColor.Blue
          bg = ConsoleColor.DarkCyan
        End If

        screen.DrawLeftArrow(Me.ScreenLeft, Me.ScreenBottom + 1, fg, bg)

        For x = Me.ScreenLeft + 1 To Me.ScreenRight - 1
          screen.DrawLightShade(x, Me.ScreenBottom + 1, fg, bg)
        Next

        screen.DrawRightArrow(Me.ScreenRight, Me.ScreenBottom + 1, fg, bg)

        Dim blockColumn = Utility.GetRangeValue(Me.ScreenLeft + 1, Me.ScreenRight - 1, (CDbl(Me.WindowLeft) / 255))

        If screen.Theme = ControlTheme.Basic Then
          screen.DrawBlock(blockColumn, Me.ScreenBottom + 1, fg, bg)
        Else
          screen.DrawMiniBlock(blockColumn, Me.ScreenBottom + 1, fg, bg)
        End If

        screen.DrawUpArrow(Me.ScreenRight + 1, Me.ScreenTop, fg, bg)

        For y = Me.ScreenTop + 1 To Me.ScreenBottom - 1
          screen.DrawLightShade(Me.ScreenRight + 1, y, fg, bg)
        Next

        screen.DrawDownArrow(Me.ScreenRight + 1, Me.ScreenBottom, fg, bg)

        Dim blockRow = Utility.GetRangeValue(Me.ScreenTop + 1, Me.ScreenBottom - 1, (CDbl(Me.CursorRow) / (Me.Buffer.Lines.Count - 1)))

        If screen.Theme = ControlTheme.Basic Then
          screen.DrawBlock(Me.ScreenRight + 1, blockRow, fg, bg)
        Else
          screen.DrawMiniBlock(Me.ScreenRight + 1, blockRow, fg, bg)
        End If

      End If

    End Sub

    Public Overrides Sub OnBeforeKeyDown(sender As Object, e As KeyEvent)

      Dim screen = CType(sender, Screen)

      Dim lineText = Me.Buffer.GetLineOrDefault(Me.CursorRow).Text

      If e.ControlKeyState.IsControlPressed() Then
        Select Case e.VirtualKey
          Case VirtualKey.VK_HOME
            Me.MoveCursor(0, 0)
          Case VirtualKey.VK_END
            Me.MoveCursor(Me.Buffer.Lines.Count, 0)
        End Select
      Else
        Select Case e.VirtualKey
          Case VirtualKey.VK_DOWN
            Me.CursorRow += 1
          Case VirtualKey.VK_UP
            Me.CursorRow -= 1
          Case VirtualKey.VK_RIGHT
            Me.CursorColumn += 1
          Case VirtualKey.VK_LEFT
            Me.CursorColumn -= 1
          Case VirtualKey.VK_HOME
            Dim preSpace = lineText.Length - lineText.TrimStart().Length
            If Me.CursorColumn <> preSpace Then
              Me.CursorColumn = preSpace
            Else
              Me.CursorColumn = 0
            End If
          Case VirtualKey.VK_END
            Me.CursorColumn = lineText.Length
          Case VirtualKey.VK_PRIOR
            Me.CursorRow -= Me.Height
          Case VirtualKey.VK_NEXT
            Me.CursorRow += Me.Height
          Case VirtualKey.VK_ESCAPE
          Case VirtualKey.VK_TAB, VirtualKey.VK_BACK, VirtualKey.VK_DELETE
            Me.CharacterEdit(e, screen)
          Case Else
            If e.Character <> ControlChars.NullChar Then
              Me.CharacterEdit(e, screen)
            End If
        End Select
      End If

      Me.NotifyCursorMoved(screen)

    End Sub

    Private lastNotifySpan As New ViewSpan(-1, -1)

    Private Sub NotifyCursorMoved(screen As Screen)

      Me.UpdateWindow(screen)

      If Me.CursorRow <> Me.lastNotifySpan.StartRow Then
        RaiseEvent RowChanged(Me, New RowChangedEventArgs(Me.lastNotifySpan.StartRow, Me.CursorRow))
      End If

      If Me.Selection <> Me.lastNotifySpan Then
        RaiseEvent SelectionChanged(Me, New SelectionChangedEventArgs(Me.lastNotifySpan, Me.Selection))
      End If

      Me.lastNotifySpan = Me.Selection

    End Sub

    Private Sub CharacterEdit(ch As Char, screen As Screen)

      If Me.CursorRow < 0 OrElse Me.CursorRow > Me.Buffer.Lines.Count OrElse Me.CursorColumn < 0 Then
        Return
      End If

      If ch = ControlChars.NullChar Then
        Return
      End If

      Dim lineText = Me.Buffer.GetLineOrDefault(Me.CursorRow).Text
      Dim preSpace = lineText.Length - lineText.TrimStart().Length

      Select Case ch

        Case CChar(vbTab) 'CChar(ConsoleKey.Tab)

          Dim newColumn = (Me.CursorColumn + 4) - (Me.CursorColumn Mod 4)

          If preSpace < lineText.Length Then
            Me.Buffer.InsertText(Me.CursorRow, Me.CursorColumn, New String(" "c, newColumn - Me.CursorColumn))
          End If

          Me.CursorColumn = newColumn

        Case CChar(vbBack) ' Chr(9) 'CChar(ConsoleKey.Backspace)

          If Me.CursorColumn > 0 Then

            Dim count As Integer

            If Me.CursorColumn <= preSpace OrElse lineText.Trim().Length = 0 Then
              count = ((Me.CursorColumn - 1) Mod 4) + 1
            Else
              count = 1
            End If

            Me.Buffer.RemoveText(Me.CursorRow, Me.CursorColumn - count, count)
            Me.CursorColumn -= count

          Else
            If Me.CursorRow > 0 Then
              Me.MoveCursor(Me.CursorRow - 1, Me.Buffer.Lines(Me.CursorRow - 1).Text.Length)
              If Me.CursorRow < Me.Buffer.Lines.Count - 1 Then
                Me.Buffer.AppendText(Me.CursorRow, Me.Buffer.Lines(Me.CursorRow + 1).Text)
                Me.Buffer.RemoveLine(Me.CursorRow + 1)
              End If
            End If
          End If

        Case Chr(13) 'CChar(ConsoleKey.Enter)

          If Me.CursorRow >= Me.Buffer.Lines.Count Then
            Me.Buffer.InsertLine(Me.CursorRow, "")
          End If

          Dim padding = Me.Buffer.Lines(Me.CursorRow).Text.Length - Me.Buffer.Lines(Me.CursorRow).Text.TrimStart(" "c).Length
          Dim truncatedText = Me.Buffer.TruncateText(Me.CursorRow, Me.CursorColumn)
          Me.Buffer.InsertLine(Me.CursorRow + 1, New String(" "c, padding) & truncatedText)
          Me.MoveCursor(Me.CursorRow + 1, padding)

        Case Else

          Me.Buffer.InsertText(Me.CursorRow, Me.CursorColumn, ch.ToString())
          Me.CursorColumn += 1

      End Select

    End Sub

    Public Sub CharacterEdit(e As KeyEvent, screen As Screen)

      Me._windowUpdatesSuspended = True

      Try

        If Me.CursorRow < 0 OrElse Me.CursorRow > Me.Buffer.Lines.Count OrElse Me.CursorColumn < 0 Then
          Return
        End If

        Select Case e.VirtualKey
          Case VirtualKey.VK_DELETE
            If Me.CursorColumn < Me.Buffer.GetLineOrDefault(Me.CursorRow).Text.Length Then
              Me.Buffer.RemoveText(Me.CursorRow, Me.CursorColumn, 1)
            Else
              If Me.CursorRow < Me.Buffer.Lines.Count - 1 Then
                Me.Buffer.AppendText(Me.CursorRow, Me.Buffer.Lines(Me.CursorRow + 1).Text)
                Me.Buffer.RemoveLine(Me.CursorRow + 1)
              End If
            End If
          Case Else
            Me.CharacterEdit(e.Character, screen)
        End Select

      Finally
        Me._windowUpdatesSuspended = False
        Me.UpdateWindow(screen)
      End Try

    End Sub

    Private _windowUpdatesSuspended As Boolean = False

    Private Sub UpdateWindow(screen As Screen)

      If Me._windowUpdatesSuspended Then
        Return
      End If

      Me._windowUpdatesSuspended = True

      Try

        Dim scrolled = False

        Dim windowBottom = Me.WindowTop + Me.Height - 1
        Dim windowRight = Me.WindowLeft + Me.Width - 1

        If Me.CursorRow < 0 Then
          Me.CursorRow = 0
        End If

        If Me.CursorRow > Me.Buffer.Lines.Count Then
          Me.CursorRow = Me.Buffer.Lines.Count
        End If

        If Me.CursorColumn < 0 Then
          Me.CursorColumn = 0
        End If

        If Me.CursorRow < Me.WindowTop Then
          Me.WindowTop = Me.CursorRow
          scrolled = True
        ElseIf Me.CursorRow > Me.WindowBottom Then
          Me.WindowTop = Me.CursorRow - Me.Height + 1
          scrolled = True
        End If

        If Me.CursorColumn < Me.WindowLeft Then
          Me.WindowLeft = Me.CursorColumn
          scrolled = True
        ElseIf Me.CursorColumn > Me.WindowRight Then
          Me.WindowLeft = Me.CursorColumn - Me.Width + 1
          scrolled = True
        End If

        If scrolled Then
          screen.Invalidate()
        End If

      Finally
        Me._windowUpdatesSuspended = False
      End Try

    End Sub

    Public Overrides Sub OnMouseDown(sender As Object, e As MouseEvent)
      Me.MoveCursor(e.Y - Me.ScreenTop, e.X - Me.ScreenLeft)
    End Sub

    Public Overrides Sub PlaceCursor(screen As Screen)
      Me.UpdateWindow(screen)
      screen.SetCursorPosition(Me.ScreenLeft + (Me.CursorColumn - Me.WindowLeft), Me.ScreenTop + (Me.CursorRow - Me.WindowTop))
    End Sub
  End Class

  Public Structure ViewSpan

    Public Sub New(row As Integer, col As Integer)
      'Me.New()
      Me.StartRow = row
      Me.StartColumn = col
      Me.EndRow = row
      Me.EndColumn = col
    End Sub

    Public Sub New(startRow As Integer, startCol As Integer, endRow As Integer, endCol As Integer)
      'Me.New()
      Me.StartRow = startRow
      Me.StartColumn = startCol
      Me.EndRow = endRow
      Me.EndColumn = endCol
    End Sub

    Public Property StartRow() As Integer
    Public Property StartColumn() As Integer
    Public Property EndRow() As Integer
    Public Property EndColumn() As Integer

    Public Overrides Function Equals(other As Object) As Boolean
      Return TypeOf other Is ViewSpan AndAlso
             Me = CType(other, ViewSpan)
    End Function

    Public Shared Operator =(x As ViewSpan, y As ViewSpan) As Boolean
      Return x.StartRow = y.StartRow AndAlso
             x.StartColumn = y.StartColumn AndAlso
             x.EndRow = y.EndRow AndAlso
             x.EndColumn = y.EndColumn
    End Operator

    Public Shared Operator <>(x As ViewSpan, y As ViewSpan) As Boolean
      Return Not (x = y)
    End Operator

    Public Overrides Function GetHashCode() As Integer
      Return Me.StartRow Xor
             Me.StartColumn Xor
             Me.EndRow Xor
             Me.EndColumn
    End Function

  End Structure

End Namespace