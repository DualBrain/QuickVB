' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Namespace ConsoleGUI

  ''' <summary>
  ''' A small modal notice in the style of QBasic's welcome box: centred lines of text in a single-line frame, a "&lt; ... &gt;" action for Enter,
  ''' a separator, and a second action for Esc. Call <see cref="Show"/>; the callbacks run after the dialog has closed.
  ''' </summary>
  Public Class NoticeDialog
    Inherits Control

    Private _screen As Screen
    Private _previousPopup As Control
    Private _onEnter As Action
    Private _onEscape As Action
    Private _enterRow, _escapeRow As Integer

    Public Sub New()
      Me.ForegroundColor = ConsoleColor.Black
      Me.BackgroundColor = ConsoleColor.Gray
      Me.Lines = New List(Of String)()
      Me.EnterText = "Press Enter to continue"
      Me.EscapeText = "Press ESC to clear this dialog box"
    End Sub

    ''' <summary>The text, one centred line each (an empty string is a blank line).</summary>
    Public Property Lines As IList(Of String)

    ''' <summary>The action shown above the separator; Enter (or a click on it) chooses it.</summary>
    Public Property EnterText As String

    ''' <summary>The action shown below the separator; Esc (or a click on it) chooses it.</summary>
    Public Property EscapeText As String

    Public ReadOnly Property IsOpen As Boolean
      Get
        Return Me._screen IsNot Nothing
      End Get
    End Property

    Public Overrides ReadOnly Property StatusHint As String
      Get
        Return Screen.DialogStatusHint
      End Get
    End Property

    Public Sub Show(screen As Screen, Optional onEnter As Action = Nothing, Optional onEscape As Action = Nothing)

      Me._screen = screen
      Me._onEnter = onEnter
      Me._onEscape = onEscape

      Me._previousPopup = screen.PopupControl
      screen.Controls.Add(Me)
      screen.PopupControl = Me
      screen.HideCursor()
      screen.Invalidate()

    End Sub

    Private Sub Close(callback As Action)
      Dim screen = Me._screen
      Me._screen = Nothing
      screen.Controls.Remove(Me)
      screen.PopupControl = Me._previousPopup
      screen.ShowCursor()
      screen.Invalidate()
      If callback IsNot Nothing Then callback()
    End Sub

    Public Overrides Sub OnBeforeKeyDown(sender As Object, e As KeyEvent)

      ' Modal: nothing below the dialog sees keys while it is open.
      e.Handled = True

      If Me._screen Is Nothing Then Return

      Select Case e.VirtualKey
        Case VirtualKey.VK_RETURN : Me.Close(Me._onEnter)
        Case VirtualKey.VK_ESCAPE : Me.Close(Me._onEscape)
      End Select

    End Sub

    Public Overrides Sub OnKeyUp(sender As Object, e As KeyEvent)
      e.Handled = True
    End Sub

    Public Overrides Sub OnMouseDown(sender As Object, e As MouseEvent)
      If Me._screen Is Nothing Then Return
      If e.X < Me.Left OrElse e.X > Me.Right Then Return
      If e.Y = Me._enterRow Then
        Me.Close(Me._onEnter)
      ElseIf e.Y = Me._escapeRow Then
        Me.Close(Me._onEscape)
      End If
    End Sub

    Private Shared Function Clip(text As String, width As Integer) As String
      Return If(text.Length > width, text.Substring(0, Math.Max(0, width)), text)
    End Function

    ''' <summary>Draws "&lt; text &gt;" centred on a row with the angle brackets in bright white.</summary>
    Private Sub DrawAction(screen As Screen, row As Integer, text As String)
      Dim shown = Clip(text, Me.Width - 8)
      Dim left = Me.Left + (Me.Width - (shown.Length + 4)) \ 2
      screen.DrawString(left, row, "<", ConsoleColor.White, Me.BackgroundColor)
      screen.DrawString(left + 2, row, shown, Me.ForegroundColor, Me.BackgroundColor)
      screen.DrawString(left + 3 + shown.Length, row, ">", ConsoleColor.White, Me.BackgroundColor)
    End Sub

    Public Overrides Sub Render(screen As Screen)

      If Not Me.Visible Then Return

      Dim lineCount = Me.Lines.Count
      Me.Width = Math.Min(58, screen.Width - 2)
      Me.Height = lineCount + 7
      Me.Left = Math.Max(0, (screen.Width - Me.Width) \ 2)
      Me.Top = Math.Max(0, (screen.Height - Me.Height) \ 2)

      ' Too small to show (the keyboard still works).
      If Me.Width < 20 OrElse Me.Bottom + 1 >= screen.Height OrElse Me.Right + 2 >= screen.Width Then Return

      Dim fg = Me.ForegroundColor
      Dim bg = Me.BackgroundColor

      screen.DrawShadow(Me.Left + 2, Me.Top + 1, Me.Right + 2, Me.Bottom + 1)
      screen.DrawBoxFill(Me.Left, Me.Top, Me.Right, Me.Bottom, fg, bg)
      screen.DrawBox(Me.Left, Me.Top, Me.Right, Me.Bottom, fg, bg)

      For i = 0 To lineCount - 1
        Dim text = Clip(Me.Lines(i), Me.Width - 4)
        screen.DrawString(Me.Left + (Me.Width - text.Length) \ 2, Me.Top + 2 + i, text, fg, bg)
      Next

      Me._enterRow = Me.Top + lineCount + 3
      Me.DrawAction(screen, Me._enterRow, Me.EnterText)

      screen.DrawLeftTee(Me.Left, Me._enterRow + 1, fg, bg)
      screen.DrawHorizontalLine(Me.Left + 1, Me.Right - 1, Me._enterRow + 1, fg, bg)
      screen.DrawRightTee(Me.Right, Me._enterRow + 1, fg, bg)

      Me._escapeRow = Me._enterRow + 2
      Me.DrawAction(screen, Me._escapeRow, Me.EscapeText)

    End Sub

  End Class

End Namespace
