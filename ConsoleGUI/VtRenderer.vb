' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Imports System.Text

Namespace ConsoleGUI

  Public Enum VtColorMode
    ''' <summary>The 16 standard ANSI colours: the terminal's own colour scheme decides what they look like.</summary>
    Ansi16
    ''' <summary>24 bit colour using a <see cref="VtPalette"/>, so the IDE looks the same in every terminal.</summary>
    TrueColor
  End Enum

  ''' <summary>The RGB values used for the 16 <see cref="ConsoleColor"/>s in <see cref="VtColorMode.TrueColor"/>.</summary>
  Public NotInheritable Class VtPalette

    Private ReadOnly _rgb(15) As Integer

    ''' <param name="rgb">16 colours as 0xRRGGBB, indexed by <see cref="ConsoleColor"/>.</param>
    Public Sub New(rgb() As Integer)
      If rgb Is Nothing OrElse rgb.Length <> 16 Then Throw New ArgumentException("A palette needs 16 colours.", NameOf(rgb))
      Array.Copy(rgb, Me._rgb, 16)
    End Sub

    Public Function GetRgb(color As ConsoleColor) As (R As Integer, G As Integer, B As Integer)
      Dim value = Me._rgb(CInt(color) And 15)
      Return ((value >> 16) And &HFF, (value >> 8) And &HFF, value And &HFF)
    End Function

    ''' <summary>The classic VGA text mode colours of MS-DOS (blue is 0000AA, "dark yellow" is brown), as QuickBASIC showed them.</summary>
    Public Shared ReadOnly Property Vga As New VtPalette({&H0, &HAA, &HAA00, &HAAAA, &HAA0000, &HAA00AA, &HAA5500, &HAAAAAA,
                                                          &H555555, &H5555FF, &H55FF55, &H55FFFF, &HFF5555, &HFF55FF, &HFFFF55, &HFFFFFF})

  End Class

  ''' <summary>
  ''' Turns the changed spans of a cell buffer into VT/ANSI escape sequences: one cursor move per span and a colour change only where the colours change.
  ''' Pure text in, text out, so it is testable without a terminal.
  ''' </summary>
  Public NotInheritable Class VtRenderer

    Private Const Esc As String = ChrW(27)

    ''' <summary>Switches to the alternate screen (the terminal's previous contents come back on exit), disables auto-wrap and clears.</summary>
    Public Const EnterScreen As String = Esc & "[?1049h" & Esc & "[?7l" & Esc & "[0m" & Esc & "[2J" & Esc & "[H"

    ''' <summary>Undoes <see cref="EnterScreen"/> and leaves the cursor visible.</summary>
    Public Const LeaveScreen As String = Esc & "[0m" & Esc & "[?7h" & Esc & "[?25h" & Esc & "[?1049l"

    Public Const ShowCursor As String = Esc & "[?25h"
    Public Const HideCursor As String = Esc & "[?25l"

    Private ReadOnly _mode As VtColorMode
    Private ReadOnly _palette As VtPalette
    Private ReadOnly _synchronized As Boolean

    ''' <param name="synchronizedOutput">Wrap each frame in "synchronized output" markers so terminals that support them draw it atomically (others ignore them).</param>
    Public Sub New(Optional colorMode As VtColorMode = VtColorMode.TrueColor, Optional palette As VtPalette = Nothing, Optional synchronizedOutput As Boolean = True)
      Me._mode = colorMode
      Me._palette = If(palette, VtPalette.Vga)
      Me._synchronized = synchronizedOutput
    End Sub

    Public ReadOnly Property ColorMode As VtColorMode
      Get
        Return Me._mode
      End Get
    End Property

    Public Shared Function MoveCursor(column As Integer, row As Integer) As String
      Return Esc & "[" & (row + 1) & ";" & (column + 1) & "H"
    End Function

    ''' <summary>The terminal's colour index (0 black, 1 red, 2 green, 3 yellow, 4 blue ...) for a ConsoleColor, which orders the red and blue bits the other way round.</summary>
    Friend Shared Function AnsiIndex(color As ConsoleColor) As Integer
      Dim c = CInt(color) And 15
      Return (c And 8) Or ((c And 1) << 2) Or (c And 2) Or ((c And 4) >> 2)
    End Function

    Private Sub AppendColors(sb As StringBuilder, fg As ConsoleColor, bg As ConsoleColor)

      sb.Append(Esc).Append("["c)

      If Me._mode = VtColorMode.TrueColor Then
        Dim f = Me._palette.GetRgb(fg)
        Dim b = Me._palette.GetRgb(bg)
        sb.Append("38;2;").Append(f.R).Append(";"c).Append(f.G).Append(";"c).Append(f.B)
        sb.Append(";48;2;").Append(b.R).Append(";"c).Append(b.G).Append(";"c).Append(b.B)
      Else
        Dim f = AnsiIndex(fg)
        Dim b = AnsiIndex(bg)
        sb.Append(If(f < 8, 30 + f, 90 + f - 8)).Append(";"c).Append(If(b < 8, 40 + b, 100 + b - 8))
      End If

      sb.Append("m"c)

    End Sub

    ''' <summary>
    ''' The escape sequence string that updates a screen to the given buffers. <paramref name="invalidSpans"/> are the changed ranges
    ''' (End is exclusive); Nothing redraws every cell.
    ''' </summary>
    Public Function Render(bufferChars() As Char, bufferFg() As ConsoleColor, bufferBg() As ConsoleColor, width As Integer, height As Integer, invalidSpans As List(Of RowSpan)) As String

      Dim sb As New StringBuilder()

      If Me._synchronized Then sb.Append(Esc).Append("[?2026h")

      Dim lastFg As Integer = -1
      Dim lastBg As Integer = -1

      Dim spans As IEnumerable(Of RowSpan) = invalidSpans

      If spans Is Nothing Then
        Dim all As New List(Of RowSpan)()
        For row = 0 To height - 1
          all.Add(New RowSpan(row, 0, width))
        Next
        spans = all
      End If

      For Each span In spans

        sb.Append(MoveCursor(span.Start, span.Row))

        For column = span.Start To span.End - 1

          Dim index = span.Row * width + column
          Dim fg = bufferFg(index)
          Dim bg = bufferBg(index)

          If CInt(fg) <> lastFg OrElse CInt(bg) <> lastBg Then
            lastFg = CInt(fg)
            lastBg = CInt(bg)
            Me.AppendColors(sb, fg, bg)
          End If

          Dim ch = bufferChars(index)
          ' Control characters (including ESC) must never reach the terminal as such.
          sb.Append(If(ch < " "c OrElse (ch >= ChrW(127) AndAlso ch < ChrW(160)), " "c, ch))

        Next

      Next

      sb.Append(Esc).Append("[0m")

      If Me._synchronized Then sb.Append(Esc).Append("[?2026l")

      Return sb.ToString()

    End Function

    ''' <summary>The colour mode to use: the COLORTERM / QUICKVB_COLORS environment variables, else true colour on any Windows terminal that has virtual terminal processing.</summary>
    Public Shared Function DetectColorMode() As VtColorMode

      Dim setting = If(Environment.GetEnvironmentVariable("QUICKVB_COLORS"), "").Trim().ToLowerInvariant()

      Select Case setting
        Case "16", "ansi", "ansi16", "terminal" : Return VtColorMode.Ansi16
        Case "truecolor", "24bit", "rgb" : Return VtColorMode.TrueColor
      End Select

      Dim colorTerm = If(Environment.GetEnvironmentVariable("COLORTERM"), "").ToLowerInvariant()
      If colorTerm.Contains("truecolor") OrElse colorTerm.Contains("24bit") Then Return VtColorMode.TrueColor

      Return If(OperatingSystem.IsWindows(), VtColorMode.TrueColor, VtColorMode.Ansi16)

    End Function

  End Class

End Namespace
