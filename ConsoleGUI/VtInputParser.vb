' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Imports System.Text

Namespace ConsoleGUI

  ''' <summary>
  ''' Turns the bytes a terminal sends (UTF-8 text, control characters, CSI / SS3 escape sequences, SGR mouse reports and bracketed paste) into the same
  ''' <see cref="KeyEvent"/>s and <see cref="MouseEvent"/>s the Windows console produces, so the rest of ConsoleGUI doesn't care which OS it runs on.
  ''' </summary>
  Public NotInheritable Class VtInputParser

    Private Const Esc As Byte = &H1B

    Private ReadOnly _pending As New List(Of Byte)()
    Private ReadOnly _output As New List(Of ConsoleEvent)()

    Private _inPaste As Boolean
    Private _lastWasCarriageReturn As Boolean

    ''' <summary>True while a partial sequence (for example a lone Esc, which may start an Alt+key or a function key) is waiting for more bytes.</summary>
    Public ReadOnly Property HasPending As Boolean
      Get
        Return Me._pending.Count > 0
      End Get
    End Property

    ''' <summary>Parses more input and returns the events completed by it.</summary>
    Public Function Feed(data() As Byte, count As Integer) As List(Of ConsoleEvent)

      For i = 0 To count - 1
        Me._pending.Add(data(i))
      Next

      Do While Me._pending.Count > 0 AndAlso Me.ParseOne()
      Loop

      Return Me.TakeOutput()

    End Function

    ''' <summary>Called when no more bytes arrived in time: a lone Esc is the Escape key; any other unfinished sequence is dropped.</summary>
    Public Function Flush() As List(Of ConsoleEvent)

      If Me._pending.Count = 1 AndAlso Me._pending(0) = Esc Then
        Me.EmitKey(VirtualKey.VK_ESCAPE, ChrW(27), 0)
      End If

      Me._pending.Clear()
      Me._inPaste = False

      Return Me.TakeOutput()

    End Function

    Private Function TakeOutput() As List(Of ConsoleEvent)
      Dim result As New List(Of ConsoleEvent)(Me._output)
      Me._output.Clear()
      Return result
    End Function

    ' ---- parsing

    ''' <summary>Consumes one complete item from the head of the pending bytes; False when more bytes are needed.</summary>
    Private Function ParseOne() As Boolean

      Dim b = Me._pending(0)

      If Me._inPaste Then Return Me.ParsePasted(b)

      If b = Esc Then Return Me.ParseEscape()

      If b >= &H80 Then Return Me.ParseText(0)

      Me.Consume(1)
      Me.EmitAscii(b, 0)

      Return True

    End Function

    Private Sub Consume(count As Integer)
      Me._pending.RemoveRange(0, count)
    End Sub

    ''' <summary>Decodes the UTF-8 character that starts at <paramref name="offset"/>; False when it isn't complete yet.</summary>
    Private Function ParseText(offset As Integer) As Boolean

      Dim text As String = Nothing
      Dim length = 0

      If Not Me.TryDecode(offset, text, length) Then Return False

      Me.Consume(offset + length)

      For Each c In text
        Me.EmitChar(c, 0)
      Next

      Return True

    End Function

    Private Function TryDecode(offset As Integer, ByRef text As String, ByRef length As Integer) As Boolean

      Dim lead = Me._pending(offset)

      If lead < &H80 Then
        length = 1
      ElseIf lead >= &HF0 Then
        length = 4
      ElseIf lead >= &HE0 Then
        length = 3
      ElseIf lead >= &HC0 Then
        length = 2
      Else
        ' a stray continuation byte
        length = 1
        text = ""
        Return True
      End If

      If offset + length > Me._pending.Count Then Return False

      text = Encoding.UTF8.GetString(Me._pending.GetRange(offset, length).ToArray())

      Return True

    End Function

    Private Function ParseEscape() As Boolean

      If Me._pending.Count < 2 Then Return False

      Dim next1 = Me._pending(1)

      Select Case next1

        Case AscW("["c)
          Return Me.ParseCsi()

        Case AscW("O"c)
          If Me._pending.Count < 3 Then Return False
          Dim final = Me._pending(2)
          Me.Consume(3)
          Me.EmitSs3(ChrW(final))
          Return True

        Case Esc
          ' Esc Esc is the Escape key (typed twice, or Alt+Esc)
          Me.Consume(1)
          Me.EmitKey(VirtualKey.VK_ESCAPE, ChrW(27), 0)
          Return True

        Case Else
          ' Alt + the next key
          If next1 >= &H80 Then
            Dim text As String = Nothing
            Dim length = 0
            If Not Me.TryDecode(1, text, length) Then Return False
            Me.Consume(1 + length)
            For Each c In text
              Me.EmitChar(c, ControlKeyState.LEFT_ALT_PRESSED)
            Next
          Else
            Me.Consume(2)
            Me.EmitAscii(next1, ControlKeyState.LEFT_ALT_PRESSED)
          End If
          Return True

      End Select

    End Function

    Private Function ParseCsi() As Boolean

      ' Esc [ parameters(0x30-0x3F) intermediates(0x20-0x2F) final(0x40-0x7E)
      Dim index = 2

      Do While index < Me._pending.Count AndAlso Me._pending(index) >= &H20 AndAlso Me._pending(index) <= &H3F
        index += 1
      Loop

      If index >= Me._pending.Count Then Return False

      Dim final = ChrW(Me._pending(index))

      If Me._pending(index) < &H40 OrElse Me._pending(index) > &H7E Then
        ' not a valid sequence: drop what we have
        Me.Consume(index + 1)
        Return True
      End If

      Dim parameters = Encoding.ASCII.GetString(Me._pending.GetRange(2, index - 2).ToArray())

      Me.Consume(index + 1)
      Me.EmitCsi(parameters, final)

      Return True

    End Function

    Private Function ParsePasted(b As Byte) As Boolean

      Static endMarker As Byte() = {Esc, AscW("["c), AscW("2"c), AscW("0"c), AscW("1"c), AscW("~"c)}

      If b = Esc Then

        Dim available = Math.Min(Me._pending.Count, endMarker.Length)

        For i = 0 To available - 1
          If Me._pending(i) <> endMarker(i) Then
            Me.Consume(1) ' a stray Esc inside the pasted text
            Return True
          End If
        Next

        If available < endMarker.Length Then Return False

        Me.Consume(endMarker.Length)
        Me._inPaste = False
        Return True

      End If

      If b = 13 OrElse b = 10 Then
        Me.Consume(1)
        ' CR LF (or a lone LF / CR) is one line break
        If Not (b = 10 AndAlso Me._lastWasCarriageReturn) Then
          Me.EmitKey(VirtualKey.VK_RETURN, ChrW(13), 0)
        End If
        Me._lastWasCarriageReturn = (b = 13)
        Return True
      End If

      Me._lastWasCarriageReturn = False

      If b = 9 Then
        Me.Consume(1)
        Me.EmitKey(VirtualKey.VK_TAB, ChrW(9), 0)
        Return True
      End If

      If b < &H20 Then
        Me.Consume(1)
        Return True
      End If

      Return Me.ParseText(0)

    End Function

    ' ---- translation to events

    Private Sub EmitAscii(b As Byte, extra As ControlKeyState)

      Select Case b
        Case 13
          Me.EmitKey(VirtualKey.VK_RETURN, ChrW(13), extra)
        Case 10
          Me.EmitKey(VirtualKey.VK_J, ChrW(10), extra Or ControlKeyState.LEFT_CTRL_PRESSED)
        Case 9
          Me.EmitKey(VirtualKey.VK_TAB, ChrW(9), extra)
        Case 8, &H7F
          Me.EmitKey(VirtualKey.VK_BACK, ChrW(8), extra)
        Case 27
          Me.EmitKey(VirtualKey.VK_ESCAPE, ChrW(27), extra)
        Case 0
          Me.EmitKey(VirtualKey.VK_SPACE, ChrW(0), extra Or ControlKeyState.LEFT_CTRL_PRESSED) ' Ctrl+Space (or Ctrl+@)
        Case 1 To 26
          Me.EmitKey(CType(VirtualKey.VK_A + b - 1, VirtualKey), ChrW(b), extra Or ControlKeyState.LEFT_CTRL_PRESSED)
        Case 28 To 31
          Me.EmitKey(0, ChrW(b), extra Or ControlKeyState.LEFT_CTRL_PRESSED) ' Ctrl+\ ] ^ _
        Case Else
          Me.EmitChar(ChrW(b), extra)
      End Select

      If b <> 13 Then Me._lastWasCarriageReturn = False

    End Sub

    Private Sub EmitChar(c As Char, extra As ControlKeyState)

      Dim vk As VirtualKey = 0
      Dim state = extra

      If c >= "a"c AndAlso c <= "z"c Then
        vk = CType(VirtualKey.VK_A + AscW(c) - AscW("a"c), VirtualKey)
      ElseIf c >= "A"c AndAlso c <= "Z"c Then
        vk = CType(VirtualKey.VK_A + AscW(c) - AscW("A"c), VirtualKey)
        state = state Or ControlKeyState.SHIFT_PRESSED
      ElseIf c >= "0"c AndAlso c <= "9"c Then
        vk = CType(VirtualKey.VK_0 + AscW(c) - AscW("0"c), VirtualKey)
      ElseIf c = " "c Then
        vk = VirtualKey.VK_SPACE
      End If

      Me.EmitKey(vk, c, state)

    End Sub

    Private Sub EmitSs3(final As Char)

      Select Case final
        Case "A"c : Me.EmitKey(VirtualKey.VK_UP, ChrW(0), 0)
        Case "B"c : Me.EmitKey(VirtualKey.VK_DOWN, ChrW(0), 0)
        Case "C"c : Me.EmitKey(VirtualKey.VK_RIGHT, ChrW(0), 0)
        Case "D"c : Me.EmitKey(VirtualKey.VK_LEFT, ChrW(0), 0)
        Case "H"c : Me.EmitKey(VirtualKey.VK_HOME, ChrW(0), 0)
        Case "F"c : Me.EmitKey(VirtualKey.VK_END, ChrW(0), 0)
        Case "P"c : Me.EmitKey(VirtualKey.VK_F1, ChrW(0), 0)
        Case "Q"c : Me.EmitKey(VirtualKey.VK_F2, ChrW(0), 0)
        Case "R"c : Me.EmitKey(VirtualKey.VK_F3, ChrW(0), 0)
        Case "S"c : Me.EmitKey(VirtualKey.VK_F4, ChrW(0), 0)
        Case "M"c : Me.EmitKey(VirtualKey.VK_RETURN, ChrW(13), 0) ' keypad Enter
      End Select
    End Sub

    Private Sub EmitCsi(parameters As String, final As Char)

      If parameters.StartsWith("<", StringComparison.Ordinal) Then
        If final = "M"c OrElse final = "m"c Then Me.EmitMouse(parameters.Substring(1), final = "M"c)
        Return
      End If

      Dim parts = parameters.Split(";"c)
      Dim first = ParseNumber(parts(0), 1)
      Dim modifier = If(parts.Length > 1, ParseNumber(parts(1), 1), 1) - 1
      Dim state As ControlKeyState = 0

      If (modifier And 1) <> 0 Then state = state Or ControlKeyState.SHIFT_PRESSED
      If (modifier And 2) <> 0 Then state = state Or ControlKeyState.LEFT_ALT_PRESSED
      If (modifier And 4) <> 0 Then state = state Or ControlKeyState.LEFT_CTRL_PRESSED

      Dim vk As VirtualKey = 0

      Select Case final
        Case "A"c : vk = VirtualKey.VK_UP
        Case "B"c : vk = VirtualKey.VK_DOWN
        Case "C"c : vk = VirtualKey.VK_RIGHT
        Case "D"c : vk = VirtualKey.VK_LEFT
        Case "H"c : vk = VirtualKey.VK_HOME
        Case "F"c : vk = VirtualKey.VK_END
        Case "P"c : vk = VirtualKey.VK_F1
        Case "Q"c : vk = VirtualKey.VK_F2
        Case "R"c : vk = VirtualKey.VK_F3
        Case "S"c : vk = VirtualKey.VK_F4
        Case "Z"c : vk = VirtualKey.VK_TAB : state = state Or ControlKeyState.SHIFT_PRESSED
        Case "~"c
          If first = 200 Then
            Me._inPaste = True
            Me._lastWasCarriageReturn = False
            Return
          End If
          vk = FunctionKeyFromNumber(first)
      End Select

      If vk <> 0 Then Me.EmitKey(vk, If(vk = VirtualKey.VK_TAB, ChrW(9), ChrW(0)), state)

    End Sub

    Private Shared Function ParseNumber(text As String, defaultValue As Integer) As Integer
      Dim value As Integer
      Return If(Integer.TryParse(text, value), value, defaultValue)
    End Function

    Private Shared Function FunctionKeyFromNumber(number As Integer) As VirtualKey

      Select Case number
        Case 1, 7 : Return VirtualKey.VK_HOME
        Case 2 : Return VirtualKey.VK_INSERT
        Case 3 : Return VirtualKey.VK_DELETE
        Case 4, 8 : Return VirtualKey.VK_END
        Case 5 : Return VirtualKey.VK_PRIOR
        Case 6 : Return VirtualKey.VK_NEXT
        Case 11 To 15 : Return CType(VirtualKey.VK_F1 + number - 11, VirtualKey)
        Case 17 To 21 : Return CType(VirtualKey.VK_F6 + number - 17, VirtualKey)
        Case 23 To 24 : Return CType(VirtualKey.VK_F11 + number - 23, VirtualKey)
        Case Else : Return 0
      End Select

    End Function

    Private Sub EmitMouse(parameters As String, pressed As Boolean)

      Dim parts = parameters.Split(";"c)
      If parts.Length < 3 Then Return

      Dim code = ParseNumber(parts(0), 0)
      Dim x = ParseNumber(parts(1), 1) - 1
      Dim y = ParseNumber(parts(2), 1) - 1

      Dim state As ControlKeyState = 0
      If (code And 4) <> 0 Then state = state Or ControlKeyState.SHIFT_PRESSED
      If (code And 8) <> 0 Then state = state Or ControlKeyState.LEFT_ALT_PRESSED
      If (code And 16) <> 0 Then state = state Or ControlKeyState.LEFT_CTRL_PRESSED

      Dim buttons As ButtonState = 0

      If (code And 64) <> 0 Then

        ' wheel: 64 up, 65 down (and 66 / 67 left / right)
        If Not pressed Then Return

        Dim flags = If((code And 2) <> 0, MouseEventFlags.MOUSE_HWHEELED, MouseEventFlags.MOUSE_WHEELED)
        Dim delta = If((code And 1) = 0, 120, -120) ' positive is up / right, in the high word like Windows reports it

        Me._output.Add(New MouseEvent(CShort(x), CShort(y), CType(CUInt(delta And &HFFFF) * 65536UI, ButtonState), state, flags))
        Return

      End If

      Select Case code And 3
        Case 0 : buttons = ButtonState.FROM_LEFT_1ST_BUTTON_PRESSED
        Case 1 : buttons = ButtonState.FROM_LEFT_2ND_BUTTON_PRESSED
        Case 2 : buttons = ButtonState.RIGHTMOST_BUTTON_PRESSED
      End Select

      Dim moved = (code And 32) <> 0

      If Not pressed Then buttons = 0 ' a release: nothing is down any more

      Me._output.Add(New MouseEvent(CShort(x), CShort(y), buttons, state, If(moved, MouseEventFlags.MOUSE_MOVED, CType(0, MouseEventFlags))))

    End Sub

    ''' <summary>A key press and release; Alt is reported the way Windows does it, as a VK_MENU press around the key.</summary>
    Private Sub EmitKey(vk As VirtualKey, c As Char, state As ControlKeyState)

      Dim alt = (state And ControlKeyState.LEFT_ALT_PRESSED) <> 0

      If alt Then Me._output.Add(New KeyEvent(True, ChrW(0), 1, VirtualKey.VK_MENU, ControlKeyState.LEFT_ALT_PRESSED))

      Me._output.Add(New KeyEvent(True, c, 1, vk, state))
      Me._output.Add(New KeyEvent(False, c, 1, vk, state))

      If alt Then Me._output.Add(New KeyEvent(False, ChrW(0), 1, VirtualKey.VK_MENU, 0))

    End Sub

  End Class

End Namespace
