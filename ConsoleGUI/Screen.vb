' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

'Imports System.Text
Imports System.Threading
'Imports System.Threading.Tasks

Namespace ConsoleGUI

  Public Class Screen
    Inherits ContainerControl
    Implements IDisposable

    Public Property ActiveControl() As Control
    Public Property PopupControl() As Control

    Private ReadOnly _console As Win32Console
    Private _disposed As Boolean = False
    Private _cursorHideCount As Integer

    Private _backbuffer As New BackBuffer()
    Private _invalid As Boolean = False


    Public Event BeforeRender As EventHandler
    Public Event BeforeKeyDown As EventHandler(Of KeyEvent)
    Public Event AfterKeyDown As EventHandler(Of KeyEvent)
    Public Event KeyUp As EventHandler(Of KeyEvent)
    Public Event MouseDown As EventHandler(Of MouseEvent)
    Public Event Resize As EventHandler(Of BufferSizeEvent)

    Public Property Theme() As ControlTheme

    Private Shared _screen As Screen = Nothing

    Public Shared Sub NavigateTo(screen As Screen)
      _screen = screen
      screen.DoIOLoop()
    End Sub

    Public Shared Function GetScreen() As Screen
      Return _screen
    End Function

    Protected Sub New()

      Me.ActiveControl = Me

      ' TODO: Save existing console buffer before resizing to fit window,
      ' TODO: then restore it in Dispose()

      Console.CursorSize = 8
      Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight)

      Me._console = New Win32Console()
      Me._console.EnableInputEvents()

      Dim [dim] = Me._console.GetBufferDimensions()

      Me.Left = 0
      Me.Top = 0

      Me.Width = [dim].Width
      Me.Height = [dim].Height

    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
      If Not Me._disposed Then
        Me._console.Dispose()
        Me._disposed = True
      End If
    End Sub

    Protected Property UIThread() As Thread

    Public Sub AssertOnUIThread()
      If Me.UIThread IsNot Nothing AndAlso
         Thread.CurrentThread IsNot Me.UIThread Then
        Throw New Exception("Not on UI thread")
      End If
    End Sub

    Public Sub DoIOLoop()

      Me.UIThread = Thread.CurrentThread

      Dim [dim] = Me._console.GetBufferDimensions()

      Me._backbuffer.ResetBufferSize([dim], Me.ForegroundColor, Me.BackgroundColor)

      Me.Left = 0
      Me.Top = 0
      Me.Width = [dim].Width
      Me.Height = [dim].Height

      Me._invalid = True

      Do

        RaiseEvent BeforeRender(Me, EventArgs.Empty)

        If Me._invalid Then
          Me.Render(Me)
          Me._backbuffer.CopyToConsole(Me._console)
          Me._invalid = False
        End If

        Dim thePopupControl = Me.PopupControl
        Dim theActiveControl = Me.ActiveControl

        Dim focusControl As Control

        If thePopupControl IsNot Nothing Then
          focusControl = thePopupControl
        Else
          focusControl = theActiveControl
        End If

        theActiveControl.PlaceCursor(Me)

        ' Get event
        ' Route event for handling
        Dim consoleEvent = Me._console.GetNextEvent()

        If TypeOf consoleEvent Is KeyEvent Then

          Dim keyEvent = CType(consoleEvent, KeyEvent)

          If keyEvent.KeyDown Then
            If (Not keyEvent.Handled) AndAlso thePopupControl IsNot Nothing Then
              thePopupControl.OnBeforeKeyDown(Me, keyEvent)
            End If
            If Not keyEvent.Handled Then
              RaiseEvent BeforeKeyDown(Me, keyEvent)
            End If
            If (Not keyEvent.Handled) AndAlso theActiveControl IsNot Nothing Then
              theActiveControl.OnBeforeKeyDown(Me, keyEvent)
            End If
            If (Not keyEvent.Handled) AndAlso thePopupControl IsNot Nothing Then
              thePopupControl.OnAfterKeyDown(Me, keyEvent)
            End If
            If Not keyEvent.Handled Then
              RaiseEvent AfterKeyDown(Me, keyEvent)
            End If
            If (Not keyEvent.Handled) AndAlso theActiveControl IsNot Nothing Then
              theActiveControl.OnAfterKeyDown(Me, keyEvent)
            End If
          Else
            If Not keyEvent.Handled Then
              RaiseEvent KeyUp(Me, keyEvent)
            End If
            If (Not keyEvent.Handled) AndAlso thePopupControl IsNot Nothing Then
              thePopupControl.OnKeyUp(Me, keyEvent)
            End If
            If (Not keyEvent.Handled) AndAlso theActiveControl IsNot Nothing Then
              theActiveControl.OnKeyUp(Me, keyEvent)
            End If
          End If

        ElseIf TypeOf consoleEvent Is MouseEvent Then

          Dim mouseEvent = CType(consoleEvent, MouseEvent)

          If mouseEvent.MouseEventFlags = 0 AndAlso mouseEvent.ButtonState <> 0 Then ' pressed
            RaiseEvent MouseDown(Me, mouseEvent)
            Dim hit = Me.HitTest(mouseEvent.X, mouseEvent.Y)
            If hit IsNot Nothing Then
              hit.OnMouseDown(Me, mouseEvent)
            End If
          End If

        ElseIf TypeOf consoleEvent Is BufferSizeEvent Then

          Dim bufferSizeEvent = TryCast(consoleEvent, BufferSizeEvent)
          Me._backbuffer.ResetBufferSize(bufferSizeEvent.Dimensions, Me.ForegroundColor, Me.BackgroundColor)

          Me.Width = bufferSizeEvent.Dimensions.Width
          Me.Height = bufferSizeEvent.Dimensions.Height

          ' The buffer size event is for the actual size of the output
          ' buffer and not the visible window into the buffer. So
          ' we shouldn't resize to the entire buffer size.  Ideally
          ' we'd resize with the visible window portion of the buffer
          ' with the top-left corner anchored to the left most column
          ' and the row where the cursor was when the process started.
          ' Complicated.

          RaiseEvent Resize(Me, bufferSizeEvent)

        ElseIf TypeOf consoleEvent Is DispatchEvent Then

          Dim dispatchEvent = TryCast(consoleEvent, DispatchEvent)

          dispatchEvent.Dispatch()

        End If

      Loop

    End Sub

    Private Class DispatchEvent
      Inherits ConsoleEvent

      Private ReadOnly _action As Action

      Public Sub New(action As Action)
        Me._action = action
      End Sub

      Public Sub Dispatch()
        Me._action()
      End Sub

    End Class

    Public Sub Post(a As Action)
      Dim dispatchEvent = New DispatchEvent(a)
      Me._console.PostEvent(dispatchEvent)
    End Sub

    Private Class InvalidateEvent
      Inherits ConsoleEvent

    End Class

    Public Sub Invalidate()
      If Not Me._invalid Then
        SyncLock Me
          If Not Me._invalid Then
            Me._invalid = True
            Me._console.PostEvent(New InvalidateEvent())
          End If
        End SyncLock
      End If
    End Sub

    Public Sub RestoreScreen()
      Me._backbuffer.RestoreBufferToScreen(Me._console)
    End Sub

    Public Sub DrawString(column As Integer, row As Integer, str As String, fg As ConsoleColor, bg As ConsoleColor)

      ' Clipping is expected to occur at a higher level

      If column + str.Length > Me._backbuffer.Width OrElse
         column < 0 OrElse
         row >= Me._backbuffer.Height OrElse
         row < 0 Then
        Throw New ArgumentOutOfRangeException()
      End If

      For i = 0 To str.Length - 1
        Me._backbuffer.SetChar(column + i, row, str.Chars(i), fg, bg)
      Next

    End Sub

    Public Sub RecolorLine(column As Integer, row As Integer, count As Integer, fg As ConsoleColor, bg As ConsoleColor)

      ' Clipping is expected to occur at a higher level

      If column + count >= Me._backbuffer.Width OrElse
         column < 0 OrElse
         row >= Me._backbuffer.Height OrElse
         row < 0 Then
        Throw New ArgumentOutOfRangeException()
      End If

      For i = 0 To count - 1
        Me._backbuffer.SetColor(column + i, row, fg, bg)
      Next

    End Sub

    Friend Sub DrawTopLeftCorner(column As Integer, row As Integer, fg As ConsoleColor, bg As ConsoleColor)
      If Me.Theme = ControlTheme.Basic Then
        Me._backbuffer.SetChar(column, row, "┌"c, fg, bg)
      Else
        Me._backbuffer.SetChar(column, row, "╔"c, fg, bg)
      End If
    End Sub

    Friend Sub DrawTopRightCorner(column As Integer, row As Integer, fg As ConsoleColor, bg As ConsoleColor)
      If Me.Theme = ControlTheme.Basic Then
        Me._backbuffer.SetChar(column, row, "┐"c, fg, bg)
      Else
        Me._backbuffer.SetChar(column, row, "╗"c, fg, bg)
      End If
    End Sub

    Friend Sub DrawBottomLeftCorner(column As Integer, row As Integer, fg As ConsoleColor, bg As ConsoleColor)
      If Me.Theme = ControlTheme.Basic Then
        Me._backbuffer.SetChar(column, row, "└"c, fg, bg)
      Else
        Me._backbuffer.SetChar(column, row, "╚"c, fg, bg)
      End If
    End Sub

    Friend Sub DrawBottomRightCorner(column As Integer, row As Integer, fg As ConsoleColor, bg As ConsoleColor)
      If Me.Theme = ControlTheme.Basic Then
        Me._backbuffer.SetChar(column, row, "┘"c, fg, bg)
      Else
        Me._backbuffer.SetChar(column, row, "╝"c, fg, bg)
      End If
    End Sub

    Friend Sub DrawLeftTee(column As Integer, row As Integer, fg As ConsoleColor, bg As ConsoleColor)
      If Me.Theme = ControlTheme.Basic Then
        Me._backbuffer.SetChar(column, row, "├"c, fg, bg)
      Else
        Me._backbuffer.SetChar(column, row, "╠"c, fg, bg)
      End If
    End Sub

    Friend Sub DrawRightTee(column As Integer, row As Integer, fg As ConsoleColor, bg As ConsoleColor)
      If Me.Theme = ControlTheme.Basic Then
        Me._backbuffer.SetChar(column, row, "┤"c, fg, bg)
      Else
        Me._backbuffer.SetChar(column, row, "╣"c, fg, bg)
      End If
    End Sub

    Friend Sub DrawHorizontalLine(left As Integer, right As Integer, row As Integer, fg As ConsoleColor, bg As ConsoleColor)
      If Me.Theme = ControlTheme.Basic Then
        Me.DrawHorizontalLine(left, right, row, "─"c, fg, bg)
      Else
        Me.DrawHorizontalLine(left, right, row, "═"c, fg, bg)
      End If
    End Sub

    Friend Sub DrawHorizontalLine(left As Integer, right As Integer, row As Integer, ch As Char, fg As ConsoleColor, bg As ConsoleColor)
      For column As Integer = left To right
        Me._backbuffer.SetChar(column, row, ch, fg, bg)
      Next column
    End Sub

    Friend Sub DrawVerticalLine(column As Integer, top As Integer, bottom As Integer, ch As Char, fg As ConsoleColor, bg As ConsoleColor)
      For row As Integer = top To bottom
        Me._backbuffer.SetChar(column, row, ch, fg, bg)
      Next row
    End Sub

    Friend Sub DrawVerticalLine(column As Integer, top As Integer, bottom As Integer, fg As ConsoleColor, bg As ConsoleColor)
      If Me.Theme = ControlTheme.Basic Then
        Me.DrawVerticalLine(column, top, bottom, "│"c, fg, bg)
      Else
        Me.DrawVerticalLine(column, top, bottom, "║"c, fg, bg)
      End If
    End Sub

    Friend Sub DrawBoxFill(left As Integer, top As Integer, right As Integer, bottom As Integer, fg As ConsoleColor, bg As ConsoleColor)

      Me.DrawBox(left, top, right, bottom, fg, bg)

      If right - left > 1 Then
        For row = top + 1 To bottom - 1
          Me.DrawHorizontalLine(left + 1, right - 1, row, " "c, fg, bg)
        Next
      End If

    End Sub

    Friend Sub DrawShadow(left As Integer, top As Integer, right As Integer, bottom As Integer)
      For row = top To bottom
        Dim column As Integer = left
        Do While column <= right
          Me._backbuffer.ShadowChar(column, row)
          column += 1
        Loop
      Next
    End Sub

    Friend Sub DrawBox(left As Integer, top As Integer, right As Integer, bottom As Integer, fg As ConsoleColor, bg As ConsoleColor)

      If right < left Then
        Dim temp As Integer = left
        left = right
        right = temp
      End If

      If bottom < top Then
        Dim temp As Integer = top
        top = bottom
        bottom = top
      End If

      If left < 0 OrElse right >= Me.Width OrElse top < 0 OrElse bottom >= Me.Height Then
        Return
      End If

      If left = right AndAlso top = bottom Then
        Me._backbuffer.SetChar(left, top, " "c, fg, bg)
      ElseIf left = right Then
        Me.DrawVerticalLine(left, top, bottom, fg, bg)
      ElseIf top = bottom Then
        Me.DrawHorizontalLine(left, right, top, fg, bg)
      Else

        Me.DrawTopLeftCorner(left, top, fg, bg)
        Me.DrawTopRightCorner(right, top, fg, bg)
        Me.DrawBottomLeftCorner(left, bottom, fg, bg)
        Me.DrawBottomRightCorner(right, bottom, fg, bg)

        If left + 1 <= right - 1 Then
          Me.DrawHorizontalLine(left + 1, right - 1, top, fg, bg)
          Me.DrawHorizontalLine(left + 1, right - 1, bottom, fg, bg)
        End If

        If top + 1 <= bottom - 1 Then
          Me.DrawVerticalLine(left, top + 1, bottom - 1, fg, bg)
          Me.DrawVerticalLine(right, top + 1, bottom - 1, fg, bg)
        End If

      End If

    End Sub

    Friend Sub DrawLeftArrow(column As Integer, row As Integer, fg As ConsoleColor, bg As ConsoleColor)
      If Me.Theme = ControlTheme.Basic Then
        Me._backbuffer.SetChar(column, row, "←"c, fg, bg)
      Else
        Me._backbuffer.SetChar(column, row, ChrW(&H11), fg, bg)
      End If
    End Sub

    Friend Sub DrawRightArrow(column As Integer, row As Integer, fg As ConsoleColor, bg As ConsoleColor)
      If Me.Theme = ControlTheme.Basic Then
        Me._backbuffer.SetChar(column, row, "→"c, fg, bg)
      Else
        Me._backbuffer.SetChar(column, row, ChrW(&H10), fg, bg)
      End If
    End Sub

    Friend Sub DrawUpArrow(column As Integer, row As Integer, fg As ConsoleColor, bg As ConsoleColor)
      If Me.Theme = ControlTheme.Basic Then
        Me._backbuffer.SetChar(column, row, "↑"c, fg, bg)
      Else
        Me._backbuffer.SetChar(column, row, ChrW(&H1E), fg, bg)
      End If
    End Sub

    Friend Sub DrawDownArrow(column As Integer, row As Integer, fg As ConsoleColor, bg As ConsoleColor)
      If Me.Theme = ControlTheme.Basic Then
        Me._backbuffer.SetChar(column, row, "↓"c, fg, bg)
      Else
        Me._backbuffer.SetChar(column, row, ChrW(&H1F), fg, bg)
      End If
    End Sub

    Friend Sub DrawLightShade(column As Integer, row As Integer, fg As ConsoleColor, bg As ConsoleColor)
      Me._backbuffer.SetChar(column, row, "░"c, fg, bg)
    End Sub

    Friend Sub DrawBlock(column As Integer, row As Integer, fg As ConsoleColor, bg As ConsoleColor)
      Me._backbuffer.SetChar(column, row, "█"c, fg, bg)
    End Sub

    Friend Sub DrawMiniBlock(column As Integer, row As Integer, fg As ConsoleColor, bg As ConsoleColor)
      Me._backbuffer.SetChar(column, row, "■"c, fg, bg)
    End Sub

    Friend Sub SetCursorPosition(x As Integer, y As Integer)
      Console.SetCursorPosition(x, y)
    End Sub

    Friend Sub HideCursor()

      If Me._cursorHideCount = 0 Then
        Console.CursorVisible = False
      End If

      Me._cursorHideCount += 1

    End Sub

    Friend Sub ShowCursor()

      If Me._cursorHideCount > 0 Then
        Me._cursorHideCount -= 1
      End If

      If Me._cursorHideCount = 0 Then
        Console.CursorVisible = True
      End If

    End Sub

  End Class

  Public Enum ControlTheme
    Basic
    CSharp
  End Enum

End Namespace