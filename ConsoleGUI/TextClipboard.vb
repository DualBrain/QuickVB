' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Imports System.Runtime.InteropServices

Namespace ConsoleGUI

  ''' <summary>
  ''' The Clipboard of the editor. Text is always kept inside the program; when <see cref="UseSystemClipboard"/> is set it is also exchanged with the
  ''' operating system's clipboard (the Windows clipboard, pbcopy / pbpaste on macOS, wl-clipboard, xclip or xsel on Linux when one is installed).
  ''' </summary>
  Public NotInheritable Class TextClipboard

    Private Shared _text As String = ""

    ''' <summary>Share the clipboard with other programs (screens that run without a real terminal turn this off).</summary>
    Public Shared Property UseSystemClipboard As Boolean = True

    Private Sub New()
    End Sub

    Public Shared Sub SetText(text As String)

      _text = If(text, "")

      If UseSystemClipboard Then
        Try
          SetSystemText(_text)
        Catch ex As Exception
          ' no clipboard to talk to: the internal one still works
        End Try
      End If

    End Sub

    Public Shared Function GetText() As String

      If UseSystemClipboard Then
        Try
          Dim system = GetSystemText()
          If system IsNot Nothing Then Return system
        Catch ex As Exception
          ' fall back to the internal clipboard
        End Try
      End If

      Return _text

    End Function

    ' ---- the system clipboard

    Private Shared Sub SetSystemText(text As String)

      If OperatingSystem.IsWindows() Then
        SetWindowsText(text)
        Return
      End If

      Dim tool = FindTool(True)
      If tool Is Nothing Then Return

      Dim info As New ProcessStartInfo(tool.Item1, tool.Item2) With {.UseShellExecute = False,
                                                                     .RedirectStandardInput = True,
                                                                     .RedirectStandardOutput = True,
                                                                     .RedirectStandardError = True}
      Using proc = Process.Start(info)
        proc.StandardInput.Write(text)
        proc.StandardInput.Close()
        proc.WaitForExit(1500)
      End Using

    End Sub

    Private Shared Function GetSystemText() As String

      If OperatingSystem.IsWindows() Then Return GetWindowsText()

      Dim tool = FindTool(False)
      If tool Is Nothing Then Return Nothing

      Dim info As New ProcessStartInfo(tool.Item1, tool.Item2) With {.UseShellExecute = False,
                                                                     .RedirectStandardOutput = True,
                                                                     .RedirectStandardError = True}
      Using proc = Process.Start(info)
        Dim output = proc.StandardOutput.ReadToEndAsync()
        If Not proc.WaitForExit(1500) Then
          Try
            proc.Kill()
          Catch ex As InvalidOperationException
          End Try
          Return Nothing
        End If
        Return If(proc.ExitCode = 0, output.Result, Nothing)
      End Using

    End Function

    Private Shared Function FindTool(write As Boolean) As Tuple(Of String, String)

      Dim candidates As New List(Of Tuple(Of String, String))()

      If OperatingSystem.IsMacOS() Then
        candidates.Add(Tuple.Create(If(write, "pbcopy", "pbpaste"), ""))
      Else
        If Not String.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY")) Then
          candidates.Add(Tuple.Create(If(write, "wl-copy", "wl-paste"), If(write, "", "--no-newline")))
        End If
        If Not String.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")) Then
          candidates.Add(Tuple.Create("xclip", If(write, "-selection clipboard -in", "-selection clipboard -out")))
          candidates.Add(Tuple.Create("xsel", If(write, "--clipboard --input", "--clipboard --output")))
        End If
      End If

      Dim directories = If(Environment.GetEnvironmentVariable("PATH"), "").Split(IO.Path.PathSeparator)

      For Each candidate In candidates
        If directories.Any(Function(d) d.Length > 0 AndAlso IO.File.Exists(IO.Path.Combine(d, candidate.Item1))) Then Return candidate
      Next

      Return Nothing

    End Function

    ' ---- Windows

    Private Const CF_UNICODETEXT As UInteger = 13
    Private Const GMEM_MOVEABLE As UInteger = 2

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function OpenClipboard(owner As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function CloseClipboard() As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function EmptyClipboard() As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetClipboardData(format As UInteger, memory As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetClipboardData(format As UInteger) As IntPtr
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function GlobalAlloc(flags As UInteger, bytes As UIntPtr) As IntPtr
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function GlobalLock(memory As IntPtr) As IntPtr
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function GlobalUnlock(memory As IntPtr) As Boolean
    End Function

    Private Shared Function OpenWindowsClipboard() As Boolean
      ' another program may have it open for a moment
      For attempt = 1 To 5
        If OpenClipboard(IntPtr.Zero) Then Return True
        Threading.Thread.Sleep(10)
      Next
      Return False
    End Function

    Private Shared Sub SetWindowsText(text As String)

      If Not OpenWindowsClipboard() Then Return

      Try

        EmptyClipboard()

        Dim bytes = (text.Length + 1) * 2
        Dim memory = GlobalAlloc(GMEM_MOVEABLE, New UIntPtr(CUInt(bytes)))
        If memory = IntPtr.Zero Then Return

        Dim target = GlobalLock(memory)
        Marshal.Copy((text & ChrW(0)).ToCharArray(), 0, target, text.Length + 1)
        GlobalUnlock(memory)

        SetClipboardData(CF_UNICODETEXT, memory)

      Finally
        CloseClipboard()
      End Try

    End Sub

    Private Shared Function GetWindowsText() As String

      If Not OpenWindowsClipboard() Then Return Nothing

      Try

        Dim memory = GetClipboardData(CF_UNICODETEXT)
        If memory = IntPtr.Zero Then Return Nothing

        Dim source = GlobalLock(memory)
        If source = IntPtr.Zero Then Return Nothing

        Try
          Return Marshal.PtrToStringUni(source)
        Finally
          GlobalUnlock(memory)
        End Try

      Finally
        CloseClipboard()
      End Try

    End Function

  End Class

End Namespace
