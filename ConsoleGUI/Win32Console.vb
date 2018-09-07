' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Imports System.Collections.Concurrent
Imports System.Runtime.InteropServices
Imports System.Threading
'Imports System.Threading.Tasks
Imports Microsoft.Win32.SafeHandles

Namespace ConsoleGUI

  Friend NotInheritable Class Win32Console
    Implements IDisposable

    Private _disposed As Boolean = False

    Private ReadOnly _handleIn As Integer
    Private ReadOnly _handleOut As Integer
    Private ReadOnly consoleEvent As New AutoResetEvent(False)
    Private ReadOnly clientEvent As New AutoResetEvent(False)
    Private ReadOnly clientEventQueue As New ConcurrentQueue(Of ConsoleEvent)()

    Private _needRestoreConsoleMode As Boolean = False
    Private _originalConsoleMode As ConsoleMode

    Friend Sub New()

      ' According to http://msdn.microsoft.com/en-us/library/windows/desktop/ms683231(v=vs.85).aspx
      ' it sounds like GetStdHandle isn't going to do the right thing if the user redirects stdin
      ' or stdout, e.g. by starting the app on the command line with "edit.exe > foo.txt < bar.txt".
      ' Instead it sounds like we should use CreateFile("CONIN$") and CreateFile("CONOUT$")...

      Me._handleIn = GetStdHandle(STD_INPUT_HANDLE)
      Me._handleOut = GetStdHandle(STD_OUTPUT_HANDLE)

      Me.consoleEvent.SafeWaitHandle = New SafeWaitHandle(New IntPtr(Me._handleIn), False)

    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose

      If Not Me._disposed Then

        If Me._needRestoreConsoleMode Then
          SetConsoleMode(Me._handleIn, Me._originalConsoleMode)
          Me._needRestoreConsoleMode = False
        End If

        Me._disposed = True

      End If

    End Sub

    Friend Sub PostEvent([event] As ConsoleEvent)
      Me.clientEventQueue.Enqueue([event])
      Me.clientEvent.Set()
    End Sub

    Friend Sub EnableInputEvents()

      If Not GetConsoleMode(Me._handleIn, Me._originalConsoleMode) Then
        Throw New Exception("GetConsoleMode failed")
      End If

      If Not SetConsoleMode(Me._handleIn, ConsoleMode.ENABLE_MOUSE_INPUT Or ConsoleMode.ENABLE_WINDOW_INPUT) Then
        Throw New Exception("SetConsoleMode failed")
      End If

      Me._needRestoreConsoleMode = True

    End Sub

    Friend Function GetBufferDimensions() As Dimensions

      Dim info As CONSOLE_SCREEN_BUFFER_INFO

      If Not GetConsoleScreenBufferInfo(Me._handleOut, info) Then
        Throw New Exception("GetConsoleScreenBufferInfo failed")
      End If

      Return New Dimensions(info.dwSize.x, info.dwSize.y)

    End Function

    Friend Sub WriteOutputBuffer(bufferChars() As Char, bufferFg() As ConsoleColor, bufferBg() As ConsoleColor, width As Integer, height As Integer, invalidSpans As List(Of RowSpan))

      ' This copying of the backbuffer data into a local CHAR_INFO array is a lot of
      ' copying that might be a perf issue.  If so consider storing a CHAR_INFO array
      ' directly in the BackBuffer class.

      Dim charinfos = New CHAR_INFO(bufferChars.Length - 1) {}

      For i = 0 To bufferChars.Length - 1
        charinfos(i).UnicodeChar = bufferChars(i)
        charinfos(i).Attributes = Me.CharInfoAttributeFromConsoleColors(bufferFg(i), bufferBg(i))
      Next

      Dim bufferSize As COORD

      bufferSize.x = CShort(width)
      bufferSize.y = CShort(height)

      Dim bufferCoord As COORD
      Dim writeRegion As SMALL_RECT

      If invalidSpans Is Nothing Then

        writeRegion.Top = 0
        writeRegion.Left = 0
        writeRegion.Bottom = CShort(height - 1)
        writeRegion.Right = CShort(width - 1)

        bufferCoord.x = 0
        bufferCoord.y = 0

        WriteConsoleOutput(Me._handleOut, charinfos, bufferSize, bufferCoord, writeRegion)

      Else

        For Each span In invalidSpans

          writeRegion.Top = CShort(Fix(span.Row))
          writeRegion.Bottom = CShort(Fix(span.Row))
          writeRegion.Left = CShort(Fix(span.Start))
          writeRegion.Right = CShort(Fix(span.End - 1))

          bufferCoord.y = writeRegion.Top
          bufferCoord.x = writeRegion.Left

          WriteConsoleOutput(Me._handleOut, charinfos, bufferSize, bufferCoord, writeRegion)

        Next

      End If

    End Sub

    Private Function CharInfoAttributeFromConsoleColors(fg As ConsoleColor, bg As ConsoleColor) As UShort
      Return CUShort(CUShort(fg) Or (CUShort(bg) << 4))
    End Function

    Friend Function GetNextEvent() As ConsoleEvent

      ' TODO: How to handle timing events?

      Dim ev As ConsoleEvent = Nothing

      Do While ev Is Nothing

        Dim whichEvent As Integer = WaitHandle.WaitAny({Me.consoleEvent, Me.clientEvent})

        Select Case whichEvent
          Case 0
            ev = Me.GetConsoleEvent()
          Case 1
            ev = Me.GetClientEvent()
        End Select

        If Not Me.clientEventQueue.IsEmpty Then
          Me.clientEvent.Set()
        End If

      Loop

      Return ev

    End Function

    Private Function GetClientEvent() As ConsoleEvent
      Dim ev As ConsoleEvent = Nothing
      If Me.clientEventQueue.TryDequeue(ev) Then
        Return ev
      End If
      Return Nothing
    End Function

    Private Function GetConsoleEvent() As ConsoleEvent

      ' CONSIDER: Any reason to buffer more than one record in our Win32Console class?

      Dim records = New INPUT_RECORD(0) {}
      Dim count As UInteger

      If Not ReadConsoleInput(Me._handleIn, records, CUInt(records.Length), count) Then
        Throw New Exception("ReadConsoleInput failed")
      End If

      Dim record = records(0)

      Select Case record.EventType
        Case EventType.KEY_EVENT
          Dim keyEvent = record.KeyEvent
          Return New KeyEvent(keyEvent.bKeyDown, keyEvent.UnicodeChar, keyEvent.wRepeatCount, keyEvent.wVirtualKeyCode, keyEvent.dwControlKeyState)
        Case EventType.MOUSE_EVENT
          Dim mouseEvent = record.MouseEvent
          Return New MouseEvent(mouseEvent.dwMousePosition.x, mouseEvent.dwMousePosition.y, mouseEvent.dwButtonState, mouseEvent.dwControlKeyState, mouseEvent.dwEventFlags)
        Case EventType.WINDOW_BUFFER_SIZE_EVENT
          Dim size As COORD = record.WindowBufferSizeEvent.dwSize
          Return New BufferSizeEvent(size.x, size.y)
        Case EventType.FOCUS_EVENT, EventType.MENU_EVENT
          ' Ignore.
          Return Nothing
        Case Else
          Throw New Exception("Invalid EventType enum value")
      End Select

    End Function

    Private Const STD_INPUT_HANDLE As Integer = -10
    Private Const STD_OUTPUT_HANDLE As Integer = -11

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function GetConsoleCP() As UInteger
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function GetConsoleMode(hConsoleHandle As Integer, ByRef lpMode As ConsoleMode) As Boolean
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function GetConsoleScreenBufferInfo(hConsoleHandle As Integer, ByRef lpConsoleScreenBufferInfo As CONSOLE_SCREEN_BUFFER_INFO) As Boolean
    End Function

    <DllImport("kernel32.dll", EntryPoint:="GetStdHandle", SetLastError:=True, CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function GetStdHandle(nStdHandle As Integer) As Integer
    End Function

    <DllImport("kernel32.dll", SetLastError:=True, EntryPoint:="ReadConsoleInputW", CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function ReadConsoleInput(hConsoleHandle As Integer, <Out()> lpBuffer() As INPUT_RECORD, nLength As UInteger, ByRef lpNumberOfEventsRead As UInteger) As Boolean
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function SetConsoleCP(wCodePageID As UInteger) As Boolean
    End Function

    <DllImport("kernel32.dll", EntryPoint:="SetConsoleCursorPosition", SetLastError:=True, CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function SetConsoleCursorPosition(hConsoleOutput As Integer, dwCursorPosition As COORD) As Integer
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function SetConsoleMode(hConsoleHandle As Integer, dwMode As ConsoleMode) As Boolean
    End Function

    <DllImport("kernel32.dll", SetLastError:=True, EntryPoint:="WriteConsoleOutputW", CharSet:=CharSet.Unicode)>
    Private Shared Function WriteConsoleOutput(hConsoleHandle As Integer, lpBuffer() As CHAR_INFO, dwBufferSize As COORD, dwBufferCoord As COORD, ByRef lpWriteRegion As SMALL_RECT) As Boolean
    End Function

    <Flags>
    Private Enum ConsoleMode As UInteger
      ENABLE_PROCESSED_INPUT = &H1
      ENABLE_LINE_INPUT = &H2
      ENABLE_ECHO_INPUT = &H4
      ENABLE_WINDOW_INPUT = &H8
      ENABLE_MOUSE_INPUT = &H10
      ENABLE_INSERT_MODE = &H20
      ENABLE_QUICK_EDIT_MODE = &H40
      ENABLE_EXTENDED_FLAGS = &H80
      ENABLE_AUTO_POSITION = &H100
      ENABLE_PROCESSED_OUTPUT = &H1
      ENABLE_WRAP_AT_EOL_OUTPUT = &H2
    End Enum

    <StructLayout(LayoutKind.Explicit, CharSet:=CharSet.Unicode)>
    Friend Structure CHAR_INFO
      <FieldOffset(0)>
      Friend UnicodeChar As Char
      <FieldOffset(0)>
      Friend AsciiChar As Byte
      <FieldOffset(2)>
      Friend Attributes As UShort
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Friend Structure COORD
      Public x As Short
      Public y As Short
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Friend Structure SMALL_RECT
      Public Left As Short
      Public Top As Short
      Public Right As Short
      Public Bottom As Short
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Friend Structure CONSOLE_SCREEN_BUFFER_INFO
      Public dwSize As COORD
      Public dwCursorPosition As COORD
      Public wAttributes As Integer
      Public srWindow As SMALL_RECT
      Public dwMaximumWindowSize As COORD
    End Structure

    <StructLayout(LayoutKind.Explicit)>
    Friend Structure INPUT_RECORD
      <FieldOffset(0)>
      Public EventType As EventType
      <FieldOffset(4)>
      Public KeyEvent As KEY_EVENT_RECORD
      <FieldOffset(4)>
      Public MouseEvent As MOUSE_EVENT_RECORD
      <FieldOffset(4)>
      Public WindowBufferSizeEvent As WINDOW_BUFFER_SIZE_RECORD
      <FieldOffset(4)>
      Public MenuEvent As MENU_EVENT_RECORD
      <FieldOffset(4)>
      Public FocusEvent As FOCUS_EVENT_RECORD
    End Structure

    Public Enum EventType As UShort
      KEY_EVENT = &H1
      MOUSE_EVENT = &H2
      WINDOW_BUFFER_SIZE_EVENT = &H4
      MENU_EVENT = &H8
      FOCUS_EVENT = &H10
    End Enum

    <StructLayout(LayoutKind.Explicit, CharSet:=CharSet.Unicode)>
    Friend Structure KEY_EVENT_RECORD
      <FieldOffset(0), MarshalAs(UnmanagedType.Bool)>
      Public bKeyDown As Boolean
      <FieldOffset(4), MarshalAs(UnmanagedType.U2)>
      Public wRepeatCount As UShort
      <FieldOffset(6), MarshalAs(UnmanagedType.U2)>
      Public wVirtualKeyCode As VirtualKey
      <FieldOffset(8), MarshalAs(UnmanagedType.U2)>
      Public wVirtualScanCode As UShort
      <FieldOffset(10)>
      Public UnicodeChar As Char
      <FieldOffset(12), MarshalAs(UnmanagedType.U4)>
      Public dwControlKeyState As ControlKeyState
    End Structure

    <StructLayout(LayoutKind.Explicit)>
    Friend Structure MOUSE_EVENT_RECORD
      <FieldOffset(0)>
      Public dwMousePosition As COORD
      <FieldOffset(4), MarshalAs(UnmanagedType.U4)>
      Public dwButtonState As ButtonState
      <FieldOffset(8), MarshalAs(UnmanagedType.U4)>
      Public dwControlKeyState As ControlKeyState
      <FieldOffset(12), MarshalAs(UnmanagedType.U4)>
      Public dwEventFlags As MouseEventFlags
    End Structure

    <StructLayout(LayoutKind.Explicit)>
    Friend Structure WINDOW_BUFFER_SIZE_RECORD
      <FieldOffset(0)>
      Public dwSize As COORD
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Friend Structure MENU_EVENT_RECORD
      Public dwCommandId As UInteger
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Friend Structure FOCUS_EVENT_RECORD
      Public bSetFocus As UInteger
    End Structure

  End Class

  Public Enum VirtualKey As UShort
    VK_LBUTTON = &H1
    VK_RBUTTON = &H2
    VK_CANCEL = &H3
    VK_MBUTTON = &H4
    VK_XBUTTON1 = &H5
    VK_XBUTTON2 = &H6
    VK_BACK = &H8
    VK_TAB = &H9
    VK_CLEAR = &HC
    VK_RETURN = &HD
    VK_SHIFT = &H10
    VK_CONTROL = &H11
    VK_MENU = &H12
    VK_PAUSE = &H13
    VK_CAPITAL = &H14
    VK_KANA = &H15
    VK_HANGEUL = &H15
    VK_HANGUL = &H15
    VK_JUNJA = &H17
    VK_FINAL = &H18
    VK_HANJA = &H19
    VK_KANJI = &H19
    VK_ESCAPE = &H1B
    VK_CONVERT = &H1C
    VK_NONCONVERT = &H1D
    VK_ACCEPT = &H1E
    VK_MODECHANGE = &H1F
    VK_SPACE = &H20
    VK_PRIOR = &H21
    VK_NEXT = &H22
    VK_END = &H23
    VK_HOME = &H24
    VK_LEFT = &H25
    VK_UP = &H26
    VK_RIGHT = &H27
    VK_DOWN = &H28
    VK_SELECT = &H29
    VK_PRINT = &H2A
    VK_EXECUTE = &H2B
    VK_SNAPSHOT = &H2C
    VK_INSERT = &H2D
    VK_DELETE = &H2E
    VK_HELP = &H2F
    VK_0 = &H30
    VK_1 = &H31
    VK_2 = &H32
    VK_3 = &H33
    VK_4 = &H34
    VK_5 = &H35
    VK_6 = &H36
    VK_7 = &H37
    VK_8 = &H38
    VK_9 = &H39
    VK_A = &H41
    VK_B = &H42
    VK_C = &H43
    VK_D = &H44
    VK_E = &H45
    VK_F = &H46
    VK_G = &H47
    VK_H = &H48
    VK_I = &H49
    VK_J = &H4A
    VK_K = &H4B
    VK_L = &H4C
    VK_M = &H4D
    VK_N = &H4E
    VK_O = &H4F
    VK_P = &H50
    VK_Q = &H51
    VK_R = &H52
    VK_S = &H53
    VK_T = &H54
    VK_U = &H55
    VK_V = &H56
    VK_W = &H57
    VK_X = &H58
    VK_Y = &H59
    VK_Z = &H5A
    VK_LWIN = &H5B
    VK_RWIN = &H5C
    VK_APPS = &H5D
    VK_SLEEP = &H5F
    VK_NUMPAD0 = &H60
    VK_NUMPAD1 = &H61
    VK_NUMPAD2 = &H62
    VK_NUMPAD3 = &H63
    VK_NUMPAD4 = &H64
    VK_NUMPAD5 = &H65
    VK_NUMPAD6 = &H66
    VK_NUMPAD7 = &H67
    VK_NUMPAD8 = &H68
    VK_NUMPAD9 = &H69
    VK_MULTIPLY = &H6A
    VK_ADD = &H6B
    VK_SEPARATOR = &H6C
    VK_SUBTRACT = &H6D
    VK_DECIMAL = &H6E
    VK_DIVIDE = &H6F
    VK_F1 = &H70
    VK_F2 = &H71
    VK_F3 = &H72
    VK_F4 = &H73
    VK_F5 = &H74
    VK_F6 = &H75
    VK_F7 = &H76
    VK_F8 = &H77
    VK_F9 = &H78
    VK_F10 = &H79
    VK_F11 = &H7A
    VK_F12 = &H7B
    VK_F13 = &H7C
    VK_F14 = &H7D
    VK_F15 = &H7E
    VK_F16 = &H7F
    VK_F17 = &H80
    VK_F18 = &H81
    VK_F19 = &H82
    VK_F20 = &H83
    VK_F21 = &H84
    VK_F22 = &H85
    VK_F23 = &H86
    VK_F24 = &H87
    VK_NUMLOCK = &H90
    VK_SCROLL = &H91
    VK_OEM_NEC_EQUAL = &H92
    VK_OEM_FJ_JISHO = &H92
    VK_OEM_FJ_MASSHOU = &H93
    VK_OEM_FJ_TOUROKU = &H94
    VK_OEM_FJ_LOYA = &H95
    VK_OEM_FJ_ROYA = &H96
    VK_LSHIFT = &HA0
    VK_RSHIFT = &HA1
    VK_LCONTROL = &HA2
    VK_RCONTROL = &HA3
    VK_LMENU = &HA4
    VK_RMENU = &HA5
    VK_BROWSER_BACK = &HA6
    VK_BROWSER_FORWARD = &HA7
    VK_BROWSER_REFRESH = &HA8
    VK_BROWSER_STOP = &HA9
    VK_BROWSER_SEARCH = &HAA
    VK_BROWSER_FAVORITES = &HAB
    VK_BROWSER_HOME = &HAC
    VK_VOLUME_MUTE = &HAD
    VK_VOLUME_DOWN = &HAE
    VK_VOLUME_UP = &HAF
    VK_MEDIA_NEXT_TRACK = &HB0
    VK_MEDIA_PREV_TRACK = &HB1
    VK_MEDIA_STOP = &HB2
    VK_MEDIA_PLAY_PAUSE = &HB3
    VK_LAUNCH_MAIL = &HB4
    VK_LAUNCH_MEDIA_SELECT = &HB5
    VK_LAUNCH_APP1 = &HB6
    VK_LAUNCH_APP2 = &HB7
    VK_OEM_1 = &HBA
    VK_OEM_PLUS = &HBB
    VK_OEM_COMMA = &HBC
    VK_OEM_MINUS = &HBD
    VK_OEM_PERIOD = &HBE
    VK_OEM_2 = &HBF
    VK_OEM_3 = &HC0
    VK_OEM_4 = &HDB
    VK_OEM_5 = &HDC
    VK_OEM_6 = &HDD
    VK_OEM_7 = &HDE
    VK_OEM_8 = &HDF
    VK_OEM_AX = &HE1
    VK_OEM_102 = &HE2
    VK_ICO_HELP = &HE3
    VK_ICO_00 = &HE4
    VK_PROCESSKEY = &HE5
    VK_ICO_CLEAR = &HE6
    VK_PACKET = &HE7
    VK_OEM_RESET = &HE9
    VK_OEM_JUMP = &HEA
    VK_OEM_PA1 = &HEB
    VK_OEM_PA2 = &HEC
    VK_OEM_PA3 = &HED
    VK_OEM_WSCTRL = &HEE
    VK_OEM_CUSEL = &HEF
    VK_OEM_ATTN = &HF0
    VK_OEM_FINISH = &HF1
    VK_OEM_COPY = &HF2
    VK_OEM_AUTO = &HF3
    VK_OEM_ENLW = &HF4
    VK_OEM_BACKTAB = &HF5
    VK_ATTN = &HF6
    VK_CRSEL = &HF7
    VK_EXSEL = &HF8
    VK_EREOF = &HF9
    VK_PLAY = &HFA
    VK_ZOOM = &HFB
    VK_NONAME = &HFC
    VK_PA1 = &HFD
    VK_OEM_CLEAR = &HFE
  End Enum

  <Flags>
  Public Enum ControlKeyState As UInteger
    ' dwControlKeyState bitmask
    RIGHT_ALT_PRESSED = &H1
    LEFT_ALT_PRESSED = &H2
    RIGHT_CTRL_PRESSED = &H4
    LEFT_CTRL_PRESSED = &H8
    SHIFT_PRESSED = &H10
    NUMLOCK_ON = &H20
    SCROLLLOCK_ON = &H40
    CAPSLOCK_ON = &H80
    ENHANCED_KEY = &H100
  End Enum

  <Flags>
  Public Enum ButtonState As UInteger
    FROM_LEFT_1ST_BUTTON_PRESSED = &H1
    RIGHTMOST_BUTTON_PRESSED = &H2
    FROM_LEFT_2ND_BUTTON_PRESSED = &H4
    FROM_LEFT_3RD_BUTTON_PRESSED = &H8
    FROM_LEFT_4TH_BUTTON_PRESSED = &H10
  End Enum

  <Flags>
  Public Enum MouseEventFlags As UInteger
    MOUSE_MOVED = &H1
    DOUBLE_CLICK = &H2
    MOUSE_WHEELED = &H4
    MOUSE_HWHEELED = &H8
  End Enum

End Namespace