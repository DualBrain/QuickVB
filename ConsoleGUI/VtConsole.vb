' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text

Namespace ConsoleGUI

  ''' <summary>
  ''' A console driver that draws with VT/ANSI escape sequences (true colour, UTF-8, alternate screen, synchronized output) instead of
  ''' WriteConsoleOutput, and takes its events from another driver (the Win32 console input records on Windows).
  ''' </summary>
  Public NotInheritable Class VtConsole
    Implements IConsoleDriver

    Private ReadOnly _input As IConsoleDriver
    Private ReadOnly _output As Stream
    Private ReadOnly _renderer As VtRenderer
    Private ReadOnly _windowSize As Func(Of Dimensions)
    Private ReadOnly _utf8 As New UTF8Encoding(False)

    Private _entered As Boolean
    Private _disposed As Boolean

    ''' <param name="input">Supplies the events (keys, mouse, resize) and the event queue.</param>
    ''' <param name="output">Where the escape sequences are written (standard output on a real terminal).</param>
    ''' <param name="windowSize">The size of the visible terminal window; this is the screen size, whatever the buffer size is.</param>
    Public Sub New(input As IConsoleDriver, output As Stream, renderer As VtRenderer, windowSize As Func(Of Dimensions))
      Me._input = input
      Me._output = output
      Me._renderer = renderer
      Me._windowSize = windowSize
    End Sub

    ''' <summary>
    ''' A driver for the real terminal on top of <paramref name="input"/>, or Nothing when VT output isn't available (or QUICKVB_RENDERER=win32 asks for the
    ''' classic console renderer). The terminal is restored when the process exits, however it exits.
    ''' </summary>
    Friend Shared Function TryCreateForTerminal(input As IConsoleDriver) As VtConsole

      If OperatingSystem.IsWindows() AndAlso String.Equals(Environment.GetEnvironmentVariable("QUICKVB_RENDERER"), "win32", StringComparison.OrdinalIgnoreCase) Then Return Nothing
      If Not TryEnableVirtualTerminal() Then Return Nothing

      ' The escape sequences and the box characters are written as UTF-8; a console left on its OEM code page (437 ...) would show them as
      ' something else and put the cursor in the wrong place.
      Dim restoreEncoding = UseUtf8Output()

      Dim driver As New VtConsole(input,
                                  Console.OpenStandardOutput(),
                                  New VtRenderer(VtRenderer.DetectColorMode()),
                                  Function() New Dimensions(Console.WindowWidth, Console.WindowHeight))
      driver._restoreEncoding = restoreEncoding
      driver.RestoreTerminalOnExit()
      Return driver

    End Function

    ''' <summary>Makes the console decode what is written to it as UTF-8 (Windows; elsewhere it is UTF-8 already). Returns what puts it back, or Nothing.</summary>
    Private Shared Function UseUtf8Output() As Action

      If Not OperatingSystem.IsWindows() Then Return Nothing

      Try
        Dim previous = Console.OutputEncoding
        If previous.CodePage = 65001 Then Return Nothing
        Console.OutputEncoding = New UTF8Encoding(False)
        Return Sub()
                 Try
                   Console.OutputEncoding = previous
                 Catch ex As IOException
                   ' the console is gone
                 End Try
               End Sub
      Catch ex As IOException
        Return Nothing
      End Try

    End Function

    Private _restoreEncoding As Action

    Private Sub RestoreTerminalOnExit()
      AddHandler AppDomain.CurrentDomain.ProcessExit, Sub(sender, e) Me.GiveBackTerminal()
      AddHandler AppDomain.CurrentDomain.UnhandledException, Sub(sender, e) Me.GiveBackTerminal()
    End Sub

    ''' <summary>Leaves the screen and puts the console's code page back.</summary>
    Private Sub GiveBackTerminal()
      Me.LeaveScreen()
      Dim restore = Me._restoreEncoding
      Me._restoreEncoding = Nothing
      If restore IsNot Nothing Then restore()
    End Sub

    Private Sub Write(text As String)
      Dim bytes = Me._utf8.GetBytes(text)
      Me._output.Write(bytes, 0, bytes.Length)
      Me._output.Flush()
    End Sub

    Private Sub LeaveScreen()
      If Me._entered Then
        Me._entered = False
        Try
          Me.Write(VtRenderer.LeaveScreen)
        Catch ex As IOException
          ' The terminal is already gone.
        End Try
      End If
    End Sub

    ' ---- IConsoleDriver

    Public Function GetBufferDimensions() As Dimensions Implements IConsoleDriver.GetBufferDimensions
      Return Me._windowSize()
    End Function

    Public Sub WriteOutputBuffer(bufferChars() As Char, bufferFg() As ConsoleColor, bufferBg() As ConsoleColor, width As Integer, height As Integer, invalidSpans As List(Of RowSpan)) Implements IConsoleDriver.WriteOutputBuffer
      Me.Write(Me._renderer.Render(bufferChars, bufferFg, bufferBg, width, height, invalidSpans))
    End Sub

    Public Sub EnableInputEvents() Implements IConsoleDriver.EnableInputEvents
      Me._input.EnableInputEvents()
      If Not Me._entered Then
        Me._entered = True
        Me.Write(VtRenderer.EnterScreen)
      End If
    End Sub

    Public Function GetNextEvent() As ConsoleEvent Implements IConsoleDriver.GetNextEvent

      Dim ev = Me._input.GetNextEvent()

      ' The input side reports the size of the console buffer; on screen only the visible window counts.
      If TypeOf ev Is BufferSizeEvent Then
        Dim size = Me._windowSize()
        Return New BufferSizeEvent(size.Width, size.Height)
      End If

      Return ev

    End Function

    Public Sub PostEvent([event] As ConsoleEvent) Implements IConsoleDriver.PostEvent
      Me._input.PostEvent([event])
    End Sub

    Public Sub SetCursorPosition(x As Integer, y As Integer) Implements IConsoleDriver.SetCursorPosition
      Me.Write(VtRenderer.MoveCursor(x, y))
    End Sub

    Public Sub SetCursorVisible(visible As Boolean) Implements IConsoleDriver.SetCursorVisible
      Me.Write(If(visible, VtRenderer.ShowCursor, VtRenderer.HideCursor))
    End Sub

    Public Sub Suspend() Implements IConsoleDriver.Suspend
      Me.LeaveScreen()
      Me._input.Suspend()
    End Sub

    Public Sub Reactivate() Implements IConsoleDriver.Reactivate
      Me._input.Reactivate()
      If Not Me._entered Then
        Me._entered = True
        Me.Write(VtRenderer.EnterScreen)
      End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
      If Not Me._disposed Then
        Me._disposed = True
        Me.GiveBackTerminal()
        Me._input.Dispose()
      End If
    End Sub

    ' ---- Windows: switch the console to interpret escape sequences

    Private Const STD_OUTPUT_HANDLE As Integer = -11
    Private Const ENABLE_PROCESSED_OUTPUT As UInteger = &H1
    Private Const ENABLE_VIRTUAL_TERMINAL_PROCESSING As UInteger = &H4

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function GetStdHandle(nStdHandle As Integer) As IntPtr
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function GetConsoleMode(hConsoleHandle As IntPtr, ByRef lpMode As UInteger) As Boolean
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function SetConsoleMode(hConsoleHandle As IntPtr, dwMode As UInteger) As Boolean
    End Function

    ''' <summary>True when the terminal understands escape sequences (enabling that on Windows consoles; other platforms' terminals always do).</summary>
    Friend Shared Function TryEnableVirtualTerminal() As Boolean

      If Not OperatingSystem.IsWindows() Then Return True

      Dim handle = GetStdHandle(STD_OUTPUT_HANDLE)
      Dim mode As UInteger

      If handle = IntPtr.Zero OrElse handle = New IntPtr(-1) OrElse Not GetConsoleMode(handle, mode) Then Return False

      Return SetConsoleMode(handle, mode Or ENABLE_PROCESSED_OUTPUT Or ENABLE_VIRTUAL_TERMINAL_PROCESSING)

    End Function

  End Class

End Namespace
