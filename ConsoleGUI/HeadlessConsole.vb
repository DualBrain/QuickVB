' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Imports System.Collections.Concurrent
Imports System.Text

Namespace ConsoleGUI

  ''' <summary>
  ''' An in-memory console: records what a <see cref="Screen"/> writes (honouring the dirty spans, exactly like a real console would)
  ''' and plays back queued key / mouse / resize events. When the queue is empty the I/O loop ends.
  ''' </summary>
  Public NotInheritable Class HeadlessConsole
    Implements IConsoleDriver

    Private ReadOnly _events As New ConcurrentQueue(Of ConsoleEvent)()
    Private _width As Integer
    Private _height As Integer
    Private _chars() As Char
    Private _fg() As ConsoleColor
    Private _bg() As ConsoleColor

    Public Sub New(Optional width As Integer = 80, Optional height As Integer = 25)
      ' tests must not touch the real clipboard
      TextClipboard.UseSystemClipboard = False
      Me.Resize(width, height)
    End Sub

    ''' <summary>Number of times output was written, and the number of dirty spans in the most recent write (0 for a full write).</summary>
    Public Property WriteCount As Integer
    Public Property LastSpanCount As Integer

    Public Property CursorX As Integer
    Public Property CursorY As Integer
    Public Property CursorVisible As Boolean = True

    Public ReadOnly Property Width As Integer
      Get
        Return Me._width
      End Get
    End Property

    Public ReadOnly Property Height As Integer
      Get
        Return Me._height
      End Get
    End Property

    ''' <summary>Changes the console size (clears it); use <see cref="QueueResize"/> to also tell the screen.</summary>
    Public Sub Resize(width As Integer, height As Integer)
      Me._width = width
      Me._height = height
      Me._chars = New Char(width * height - 1) {}
      Me._fg = New ConsoleColor(width * height - 1) {}
      Me._bg = New ConsoleColor(width * height - 1) {}
      For i = 0 To Me._chars.Length - 1
        Me._chars(i) = " "c
      Next
    End Sub

    ' ---- what is on screen

    Public Function RowText(row As Integer) As String
      Return New String(Me._chars, row * Me._width, Me._width)
    End Function

    ''' <summary>The whole screen as text, one line per row, trailing spaces trimmed.</summary>
    Public Function GetText() As String
      Dim sb As New StringBuilder()
      For row = 0 To Me._height - 1
        sb.AppendLine(Me.RowText(row).TrimEnd())
      Next
      Return sb.ToString()
    End Function

    Public Function CharAt(column As Integer, row As Integer) As Char
      Return Me._chars(row * Me._width + column)
    End Function

    Public Function ForegroundAt(column As Integer, row As Integer) As ConsoleColor
      Return Me._fg(row * Me._width + column)
    End Function

    Public Function BackgroundAt(column As Integer, row As Integer) As ConsoleColor
      Return Me._bg(row * Me._width + column)
    End Function

    Public Function Contains(text As String) As Boolean
      Return Me.GetText().Contains(text)
    End Function

    ' ---- input

    Public Sub QueueEvent([event] As ConsoleEvent)
      Me._events.Enqueue([event])
    End Sub

    Public Sub QueueKey(key As VirtualKey, Optional character As Char = ChrW(0), Optional state As ControlKeyState = 0)
      Me.QueueEvent(New KeyEvent(True, character, 1US, key, state))
      Me.QueueEvent(New KeyEvent(False, character, 1US, key, state))
    End Sub

    ''' <summary>Queues typed characters (letters, digits, space).</summary>
    Public Sub QueueText(text As String)
      For Each ch In text
        Dim vk As VirtualKey
        If Char.IsLetter(ch) Then
          vk = CType(AscW(Char.ToUpperInvariant(ch)), VirtualKey)
        ElseIf Char.IsDigit(ch) Then
          vk = CType(AscW(ch), VirtualKey)
        ElseIf ch = " "c Then
          vk = VirtualKey.VK_SPACE
        Else
          vk = CType(0, VirtualKey)
        End If
        Me.QueueKey(vk, ch, If(Char.IsUpper(ch), ControlKeyState.SHIFT_PRESSED, CType(0, ControlKeyState)))
      Next
    End Sub

    Public Sub QueueClick(x As Integer, y As Integer)
      Me.QueueEvent(New MouseEvent(CShort(x), CShort(y), ButtonState.FROM_LEFT_1ST_BUTTON_PRESSED, CType(0, ControlKeyState), CType(0, MouseEventFlags)))
    End Sub

    ''' <summary>Queues any mouse event: a press (buttons set, no flags), a release (no buttons), a drag (MOUSE_MOVED with buttons) and so on.</summary>
    Public Sub QueueMouse(x As Integer, y As Integer, buttons As ButtonState, Optional flags As MouseEventFlags = 0, Optional state As ControlKeyState = 0)
      Me.QueueEvent(New MouseEvent(CShort(x), CShort(y), buttons, state, flags))
    End Sub

    ''' <summary>Queues a turn of the wheel: positive is up, 120 per notch.</summary>
    Public Sub QueueWheel(x As Integer, y As Integer, delta As Integer)
      Me.QueueMouse(x, y, CType(CUInt(delta And &HFFFF) * 65536UI, ButtonState), MouseEventFlags.MOUSE_WHEELED)
    End Sub

    Public Sub QueueResize(width As Integer, height As Integer)
      Me.Resize(width, height)
      Me.QueueEvent(New BufferSizeEvent(width, height))
    End Sub

    ' ---- IConsoleDriver

    Private Function GetBufferDimensions() As Dimensions Implements IConsoleDriver.GetBufferDimensions
      Return New Dimensions(Me._width, Me._height)
    End Function

    Private Sub WriteOutputBuffer(bufferChars() As Char, bufferFg() As ConsoleColor, bufferBg() As ConsoleColor, width As Integer, height As Integer, invalidSpans As List(Of RowSpan)) Implements IConsoleDriver.WriteOutputBuffer

      Me.WriteCount += 1

      If width <> Me._width OrElse height <> Me._height Then Me.Resize(width, height)

      If invalidSpans Is Nothing Then
        Me.LastSpanCount = 0
        Array.Copy(bufferChars, Me._chars, bufferChars.Length)
        Array.Copy(bufferFg, Me._fg, bufferFg.Length)
        Array.Copy(bufferBg, Me._bg, bufferBg.Length)
      Else
        Me.LastSpanCount = invalidSpans.Count
        For Each span In invalidSpans
          Dim first = span.Row * width + span.Start
          Dim count = span.End - span.Start
          Array.Copy(bufferChars, first, Me._chars, first, count)
          Array.Copy(bufferFg, first, Me._fg, first, count)
          Array.Copy(bufferBg, first, Me._bg, first, count)
        Next
      End If

    End Sub

    Private Sub EnableInputEvents() Implements IConsoleDriver.EnableInputEvents
    End Sub

    Private Function GetNextEvent() As ConsoleEvent Implements IConsoleDriver.GetNextEvent
      Dim ev As ConsoleEvent = Nothing
      If Me._events.TryDequeue(ev) Then Return ev
      Return Nothing
    End Function

    Private Sub PostEvent([event] As ConsoleEvent) Implements IConsoleDriver.PostEvent
      Me._events.Enqueue([event])
    End Sub

    Private Sub SetCursorPosition(x As Integer, y As Integer) Implements IConsoleDriver.SetCursorPosition
      Me.CursorX = x
      Me.CursorY = y
    End Sub

    Private Sub SetCursorVisible(visible As Boolean) Implements IConsoleDriver.SetCursorVisible
      Me.CursorVisible = visible
    End Sub

    Private Sub Suspend() Implements IConsoleDriver.Suspend
    End Sub

    Private Sub Reactivate() Implements IConsoleDriver.Reactivate
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
    End Sub

  End Class

End Namespace
