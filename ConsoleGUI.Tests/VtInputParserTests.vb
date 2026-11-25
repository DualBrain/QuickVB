Option Explicit On
Option Strict On
Option Infer On

Imports System.Text
Imports ConsoleGUI
Imports Xunit

Public Class VtInputParserTests

  Private Shared Function Parse(text As String) As List(Of ConsoleEvent)
    Dim bytes = Encoding.UTF8.GetBytes(text)
    Return New VtInputParser().Feed(bytes, bytes.Length)
  End Function

  Private Shared Function KeyDowns(events As List(Of ConsoleEvent)) As List(Of KeyEvent)
    Return events.OfType(Of KeyEvent)().Where(Function(k) k.KeyDown).ToList()
  End Function

  Private Const Esc As String = ChrW(27)

  <Fact>
  Public Sub PlainLettersAreKeysWithVirtualKeyCodes()
    Dim keys = KeyDowns(Parse("aB1 "))
    Assert.Equal(4, keys.Count)
    Assert.Equal(VirtualKey.VK_A, keys(0).VirtualKey)
    Assert.Equal("a"c, keys(0).Character)
    Assert.Equal(VirtualKey.VK_B, keys(1).VirtualKey)
    Assert.True((keys(1).ControlKeyState And ControlKeyState.SHIFT_PRESSED) <> 0)
    Assert.Equal(VirtualKey.VK_1, keys(2).VirtualKey)
    Assert.Equal(VirtualKey.VK_SPACE, keys(3).VirtualKey)
  End Sub

  <Fact>
  Public Sub EveryKeyDownIsFollowedByAKeyUp()
    Dim events = Parse("a")
    Assert.Equal(2, events.Count)
    Assert.True(CType(events(0), KeyEvent).KeyDown)
    Assert.False(CType(events(1), KeyEvent).KeyDown)
  End Sub

  <Fact>
  Public Sub ArrowKeysInBothEncodings()
    Dim keys = KeyDowns(Parse(Esc & "[A" & Esc & "[B" & Esc & "OC" & Esc & "OD"))
    Assert.Equal({VirtualKey.VK_UP, VirtualKey.VK_DOWN, VirtualKey.VK_RIGHT, VirtualKey.VK_LEFT}, keys.Select(Function(k) k.VirtualKey).ToArray())
  End Sub

  <Fact>
  Public Sub ModifiersOnArrowKeys()
    Dim key = KeyDowns(Parse(Esc & "[1;6C")).Single() ' shift + ctrl + right
    Assert.Equal(VirtualKey.VK_RIGHT, key.VirtualKey)
    Assert.True((key.ControlKeyState And ControlKeyState.SHIFT_PRESSED) <> 0)
    Assert.True((key.ControlKeyState And ControlKeyState.LEFT_CTRL_PRESSED) <> 0)
    Assert.True((key.ControlKeyState And ControlKeyState.LEFT_ALT_PRESSED) = 0)
  End Sub

  <Fact>
  Public Sub NavigationAndFunctionKeys()
    Dim keys = KeyDowns(Parse(Esc & "[3~" & Esc & "[5~" & Esc & "[6~" & Esc & "[2~" & Esc & "[H" & Esc & "[F" & Esc & "[15~" & Esc & "[21~" & Esc & "[24~" & Esc & "OP"))
    Assert.Equal({VirtualKey.VK_DELETE, VirtualKey.VK_PRIOR, VirtualKey.VK_NEXT, VirtualKey.VK_INSERT, VirtualKey.VK_HOME, VirtualKey.VK_END,
                  VirtualKey.VK_F5, VirtualKey.VK_F10, VirtualKey.VK_F12, VirtualKey.VK_F1}, keys.Select(Function(k) k.VirtualKey).ToArray())
  End Sub

  <Fact>
  Public Sub ShiftTabAndOtherControlKeys()
    Dim keys = KeyDowns(Parse(Esc & "[Z" & vbCr & vbTab & ChrW(127) & ChrW(1)))
    Assert.Equal(VirtualKey.VK_TAB, keys(0).VirtualKey)
    Assert.True((keys(0).ControlKeyState And ControlKeyState.SHIFT_PRESSED) <> 0)
    Assert.Equal(VirtualKey.VK_RETURN, keys(1).VirtualKey)
    Assert.Equal(VirtualKey.VK_TAB, keys(2).VirtualKey)
    Assert.Equal(VirtualKey.VK_BACK, keys(3).VirtualKey)
    Assert.Equal(VirtualKey.VK_A, keys(4).VirtualKey) ' Ctrl+A
    Assert.True((keys(4).ControlKeyState And ControlKeyState.LEFT_CTRL_PRESSED) <> 0)
  End Sub

  <Fact>
  Public Sub AltLetterIsReportedLikeWindowsDoesIt()
    Dim events = Parse(Esc & "f")
    Dim keys = events.OfType(Of KeyEvent)().ToList()

    ' Alt down, F down, F up, Alt up
    Assert.Equal(4, keys.Count)
    Assert.Equal(VirtualKey.VK_MENU, keys(0).VirtualKey)
    Assert.True(keys(0).KeyDown)
    Assert.Equal(VirtualKey.VK_F, keys(1).VirtualKey)
    Assert.True((keys(1).ControlKeyState And ControlKeyState.LEFT_ALT_PRESSED) <> 0)
    Assert.Equal("f"c, keys(1).Character)
    Assert.Equal(VirtualKey.VK_MENU, keys(3).VirtualKey)
    Assert.False(keys(3).KeyDown)
  End Sub

  <Fact>
  Public Sub ALoneEscapeNeedsTheTimeoutToBecomeTheEscapeKey()
    Dim parser As New VtInputParser()
    Dim bytes = {CByte(27)}

    Assert.Empty(parser.Feed(bytes, 1))
    Assert.True(parser.HasPending)

    Dim keys = KeyDowns(parser.Flush())
    Assert.Equal(VirtualKey.VK_ESCAPE, keys.Single().VirtualKey)
    Assert.False(parser.HasPending)
  End Sub

  <Fact>
  Public Sub SequencesSplitAcrossReadsAreJoined()
    Dim parser As New VtInputParser()
    Dim first = Encoding.ASCII.GetBytes(Esc & "[1;")
    Dim second = Encoding.ASCII.GetBytes("5A")

    Assert.Empty(parser.Feed(first, first.Length))

    Dim key = KeyDowns(parser.Feed(second, second.Length)).Single()
    Assert.Equal(VirtualKey.VK_UP, key.VirtualKey)
    Assert.True((key.ControlKeyState And ControlKeyState.LEFT_CTRL_PRESSED) <> 0)
  End Sub

  <Fact>
  Public Sub Utf8TextArrivesAsCharacters()
    Dim keys = KeyDowns(Parse("é€"))
    Assert.Equal({"é"c, "€"c}, keys.Select(Function(k) k.Character).ToArray())
  End Sub

  <Fact>
  Public Sub MousePressMoveAndRelease()
    Dim events = Parse(Esc & "[<0;10;5M" & Esc & "[<32;11;5M" & Esc & "[<0;11;5m").OfType(Of MouseEvent)().ToList()

    Assert.Equal(3, events.Count)

    Assert.Equal(9, events(0).X) ' reports are 1 based
    Assert.Equal(4, events(0).Y)
    Assert.Equal(ButtonState.FROM_LEFT_1ST_BUTTON_PRESSED, events(0).ButtonState)
    Assert.Equal(CType(0, MouseEventFlags), events(0).MouseEventFlags)

    Assert.Equal(MouseEventFlags.MOUSE_MOVED, events(1).MouseEventFlags)
    Assert.Equal(ButtonState.FROM_LEFT_1ST_BUTTON_PRESSED, events(1).ButtonState)

    Assert.Equal(CType(0, ButtonState), events(2).ButtonState)
  End Sub

  <Fact>
  Public Sub RightButtonAndWheel()
    Dim events = Parse(Esc & "[<2;1;1M" & Esc & "[<64;3;4M" & Esc & "[<65;3;4M").OfType(Of MouseEvent)().ToList()

    Assert.Equal(ButtonState.RIGHTMOST_BUTTON_PRESSED, events(0).ButtonState)

    Assert.Equal(MouseEventFlags.MOUSE_WHEELED, events(1).MouseEventFlags)
    Assert.Equal(120UI, CUInt(events(1).ButtonState) >> 16)

    Assert.Equal(65416UI, CUInt(events(2).ButtonState) >> 16) ' -120 as the high word
  End Sub

  <Fact>
  Public Sub BracketedPasteIsTypedTextWithoutInterpretingControlSequences()
    Dim keys = KeyDowns(Parse(Esc & "[200~ab" & vbCrLf & "c" & Esc & "[A" & Esc & "[201~x"))

    ' the pasted Esc[A is not an arrow key; after the paste, typing continues normally
    Assert.Equal(VirtualKey.VK_A, keys(0).VirtualKey)
    Assert.Equal(VirtualKey.VK_B, keys(1).VirtualKey)
    Assert.Equal(VirtualKey.VK_RETURN, keys(2).VirtualKey)
    Assert.Equal(VirtualKey.VK_C, keys(3).VirtualKey)
    Assert.DoesNotContain(keys, Function(k) k.VirtualKey = VirtualKey.VK_UP)
    Assert.Equal(VirtualKey.VK_X, keys.Last().VirtualKey)
    Assert.Equal(1, keys.Where(Function(k) k.VirtualKey = VirtualKey.VK_RETURN).Count())
  End Sub

End Class
