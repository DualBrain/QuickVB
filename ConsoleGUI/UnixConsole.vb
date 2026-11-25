' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Imports System.Collections.Concurrent
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading

Namespace ConsoleGUI

  ''' <summary>
  ''' The input side of the console on Linux and macOS: the terminal is put in raw mode, the bytes it sends are parsed by <see cref="VtInputParser"/>
  ''' (keys, mouse reports, pasted text) and a size change (SIGWINCH) becomes a <see cref="BufferSizeEvent"/>.
  ''' Output is done by <see cref="VtConsole"/>, which wraps this driver.
  ''' </summary>
  Friend NotInheritable Class UnixConsole
    Implements IConsoleDriver

    ' xterm mouse reporting: button events (1000), motion while a button is down (1002), SGR coordinates (1006); and bracketed paste (2004).
    Private Const MouseOn As String = ChrW(27) & "[?1000h" & ChrW(27) & "[?1002h" & ChrW(27) & "[?1006h" & ChrW(27) & "[?2004h"
    Private Const MouseOff As String = ChrW(27) & "[?2004l" & ChrW(27) & "[?1006l" & ChrW(27) & "[?1002l" & ChrW(27) & "[?1000l"

    Private ReadOnly _events As New BlockingCollection(Of ConsoleEvent)()
    Private ReadOnly _parser As New VtInputParser()
    Private ReadOnly _gate As New Object()

    Private _original() As Byte
    Private _rawMode As Boolean
    Private _paused As Boolean
    Private _disposed As Boolean
    Private _reader As Thread
    Private _resizeRegistration As PosixSignalRegistration

    Private ReadOnly _terminationRegistrations As New List(Of PosixSignalRegistration)()

    Public Sub New()
      AddHandler AppDomain.CurrentDomain.ProcessExit, Sub(sender, e) Me.LeaveRawMode()

      ' "kill" and a closed terminal end the program the orderly way, which gives the terminal back (the default would not)
      For Each signal In {PosixSignal.SIGTERM, PosixSignal.SIGHUP}
        Me._terminationRegistrations.Add(PosixSignalRegistration.Create(signal,
          Sub(context)
            context.Cancel = True
            Environment.Exit(If(context.Signal = PosixSignal.SIGHUP, 129, 143))
          End Sub))
      Next
    End Sub

    ' ---- IConsoleDriver

    Public Sub EnableInputEvents() Implements IConsoleDriver.EnableInputEvents

      Me.EnterRawMode()

      If Me._reader Is Nothing Then

        Me._resizeRegistration = PosixSignalRegistration.Create(PosixSignal.SIGWINCH,
          Sub(context) Me.PostEvent(New BufferSizeEvent(Console.WindowWidth, Console.WindowHeight)))

        Me._reader = New Thread(AddressOf Me.ReadLoop) With {.IsBackground = True, .Name = "Terminal input"}
        Me._reader.Start()

      End If

    End Sub

    Public Function GetBufferDimensions() As Dimensions Implements IConsoleDriver.GetBufferDimensions
      Return New Dimensions(Console.WindowWidth, Console.WindowHeight)
    End Function

    Public Function GetNextEvent() As ConsoleEvent Implements IConsoleDriver.GetNextEvent

      Dim ev As ConsoleEvent = Nothing

      ' Blocks until an event arrives; False once the input has ended.
      If Me._events.TryTake(ev, Timeout.Infinite) Then Return ev

      Return Nothing

    End Function

    Public Sub PostEvent([event] As ConsoleEvent) Implements IConsoleDriver.PostEvent
      Try
        Me._events.Add([event])
      Catch ex As InvalidOperationException
        ' the input has ended
      End Try
    End Sub

    Public Sub Suspend() Implements IConsoleDriver.Suspend

      SyncLock Me._gate
        Me._paused = True ' the reader doesn't touch the terminal while we hold the gate, or after it
        Me._parser.Flush()
      End SyncLock

      Me.LeaveRawMode()

    End Sub

    Public Sub Reactivate() Implements IConsoleDriver.Reactivate

      Me.EnterRawMode()

      SyncLock Me._gate
        Me._paused = False
      End SyncLock

    End Sub

    ' Drawing is done by VtConsole.
    Public Sub WriteOutputBuffer(bufferChars() As Char, bufferFg() As ConsoleColor, bufferBg() As ConsoleColor, width As Integer, height As Integer, invalidSpans As List(Of RowSpan)) Implements IConsoleDriver.WriteOutputBuffer
      Throw New NotSupportedException("UnixConsole only reads input; wrap it in a VtConsole to draw.")
    End Sub

    Public Sub SetCursorPosition(x As Integer, y As Integer) Implements IConsoleDriver.SetCursorPosition
    End Sub

    Public Sub SetCursorVisible(visible As Boolean) Implements IConsoleDriver.SetCursorVisible
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose

      If Not Me._disposed Then
        Me._disposed = True
        Me.LeaveRawMode()
        Me._resizeRegistration?.Dispose()
        For Each registration In Me._terminationRegistrations
          registration.Dispose()
        Next
        Me._events.CompleteAdding()
      End If

    End Sub

    ' ---- reading

    Private Sub ReadLoop()

      Dim buffer(255) As Byte
      Dim fds(0) As PollFd
      fds(0).fd = 0
      fds(0).events = POLLIN

      Do While Not Me._disposed

        Dim idle As Boolean

        SyncLock Me._gate

          idle = Me._paused

          If Not idle Then

            ' wait a little while for the rest of a sequence that has started, otherwise a lone Esc is the Escape key
            Dim ready = poll(fds, New UIntPtr(1UI), If(Me._parser.HasPending, 30, 100))

            If ready > 0 Then

              Dim count = CInt(read(0, buffer, New IntPtr(buffer.Length)))

              If count <= 0 Then
                Me._events.CompleteAdding()
                Return
              End If

              Me.PostAll(Me._parser.Feed(buffer, count))

            ElseIf ready = 0 AndAlso Me._parser.HasPending Then
              Me.PostAll(Me._parser.Flush())
            End If

          End If

        End SyncLock

        If idle Then Thread.Sleep(20)

      Loop

    End Sub

    Private Sub PostAll(events As List(Of ConsoleEvent))
      For Each ev In events
        Me.PostEvent(ev)
      Next
    End Sub

    ' ---- terminal mode

    Private Sub EnterRawMode()

      If Me._rawMode Then Return

      Dim layout = TermiosLayout.Current
      Dim attributes(255) As Byte

      If tcgetattr(0, attributes) <> 0 Then
        Throw New InvalidOperationException("The terminal's settings can't be read (is input a terminal?).")
      End If

      Me._original = CType(attributes.Clone(), Byte())

      layout.Write(attributes, layout.InputFlags, layout.Read(attributes, layout.InputFlags) And Not (layout.IXON Or layout.ICRNL Or layout.INLCR Or layout.IGNCR Or layout.ISTRIP Or layout.IXOFF))
      layout.Write(attributes, layout.LocalFlags, layout.Read(attributes, layout.LocalFlags) And Not (layout.ISIG Or layout.ICANON Or layout.ECHO Or layout.IEXTEN))
      attributes(layout.ControlCharacters + layout.VMIN) = 1
      attributes(layout.ControlCharacters + layout.VTIME) = 0

      If tcsetattr(0, 0, attributes) <> 0 Then ' TCSANOW
        Throw New InvalidOperationException("The terminal can't be switched to raw mode.")
      End If

      Me._rawMode = True
      Me.WriteToTerminal(MouseOn)

    End Sub

    Private Sub LeaveRawMode()

      If Not Me._rawMode Then Return

      Me._rawMode = False

      Try
        Me.WriteToTerminal(MouseOff)
      Catch ex As System.IO.IOException
        ' the terminal is already gone
      End Try

      If Me._original IsNot Nothing Then tcsetattr(0, 0, Me._original)

    End Sub

    Private Sub WriteToTerminal(text As String)
      Using output = Console.OpenStandardOutput()
        Dim bytes = Encoding.ASCII.GetBytes(text)
        output.Write(bytes, 0, bytes.Length)
        output.Flush()
      End Using
    End Sub

    ' ---- libc

    Private Const POLLIN As Short = 1

    <StructLayout(LayoutKind.Sequential)>
    Private Structure PollFd
      Public fd As Integer
      Public events As Short
      Public revents As Short
    End Structure

    <DllImport("libc", SetLastError:=True)>
    Private Shared Function poll(<[In], Out> fds() As PollFd, nfds As UIntPtr, timeout As Integer) As Integer
    End Function

    <DllImport("libc", SetLastError:=True)>
    Private Shared Function read(fd As Integer, <Out> buffer() As Byte, count As IntPtr) As IntPtr
    End Function

    <DllImport("libc", SetLastError:=True)>
    Private Shared Function tcgetattr(fd As Integer, <Out> termios() As Byte) As Integer
    End Function

    <DllImport("libc", SetLastError:=True)>
    Private Shared Function tcsetattr(fd As Integer, optionalActions As Integer, termios() As Byte) As Integer
    End Function

    ''' <summary>Where the fields of <c>struct termios</c> are on this OS (Linux uses 32 bit flags; macOS uses 64 bit flags).</summary>
    Private NotInheritable Class TermiosLayout

      Public Shared ReadOnly Current As TermiosLayout = If(OperatingSystem.IsMacOS(), Mac(), Linux())

      Public InputFlags As Integer
      Public LocalFlags As Integer
      Public ControlCharacters As Integer
      Public FlagSize As Integer
      Public VMIN As Integer
      Public VTIME As Integer
      Public IXON, ICRNL, INLCR, IGNCR, ISTRIP, IXOFF As ULong
      Public ISIG, ICANON, ECHO, IEXTEN As ULong

      Private Shared Function Linux() As TermiosLayout
        Return New TermiosLayout With {.InputFlags = 0, .LocalFlags = 12, .ControlCharacters = 17, .FlagSize = 4, .VMIN = 6, .VTIME = 5,
                                       .IXON = &H400, .ICRNL = &H100, .INLCR = &H40, .IGNCR = &H80, .ISTRIP = &H20, .IXOFF = &H1000,
                                       .ISIG = 1, .ICANON = 2, .ECHO = 8, .IEXTEN = &H8000}
      End Function

      Private Shared Function Mac() As TermiosLayout
        Return New TermiosLayout With {.InputFlags = 0, .LocalFlags = 24, .ControlCharacters = 32, .FlagSize = 8, .VMIN = 16, .VTIME = 17,
                                       .IXON = &H200, .ICRNL = &H100, .INLCR = &H40, .IGNCR = &H80, .ISTRIP = &H20, .IXOFF = &H400,
                                       .ISIG = &H80, .ICANON = &H100, .ECHO = 8, .IEXTEN = &H400}
      End Function

      Public Function Read(attributes() As Byte, offset As Integer) As ULong
        Return If(Me.FlagSize = 4, BitConverter.ToUInt32(attributes, offset), BitConverter.ToUInt64(attributes, offset))
      End Function

      Public Sub Write(attributes() As Byte, offset As Integer, value As ULong)
        Dim bytes = If(Me.FlagSize = 4, BitConverter.GetBytes(CUInt(value And &HFFFFFFFFUL)), BitConverter.GetBytes(value))
        Array.Copy(bytes, 0, attributes, offset, bytes.Length)
      End Sub

    End Class

  End Class

End Namespace
