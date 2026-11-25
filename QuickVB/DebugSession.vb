Option Explicit On
Option Strict On
Option Infer On

Imports System.Globalization
Imports System.IO
Imports System.IO.Pipes
Imports System.Text
Imports System.Threading

''' <summary>What the program being debugged is told to do when it is let go again.</summary>
Public NotInheritable Class DebugCommand

  ''' <summary>Nothing (carry on with the step under way), "run", "into" (the next statement), "over" (the next statement of this procedure), "out" (the statement after this procedure) or "to" (a line).</summary>
  Public Property Mode As String = "run"
  Public Property TargetFile As Integer
  Public Property TargetLine As Integer

  ''' <summary>The breakpoints as (file, line) pairs; both numbers start at 0.</summary>
  Public Property Breakpoints As IEnumerable(Of Tuple(Of Integer, Integer)) = Enumerable.Empty(Of Tuple(Of Integer, Integer))()

  ''' <summary>True when the program should stop in front of every statement so that watchpoints can be tested.</summary>
  Public Property Watching As Boolean

  ''' <summary>True when an error nothing handles stops the program where it happened.</summary>
  Public Property BreakOnErrors As Boolean

  Friend Function ToLines() As String()
    Dim lines As New List(Of String)()
    lines.Add("BP" & vbTab & String.Join(";", Me.Breakpoints.Select(Function(b) b.Item1 & ":" & b.Item2)))
    lines.Add("WP" & vbTab & If(Me.Watching, "1", "0"))
    lines.Add("ERR" & vbTab & If(Me.BreakOnErrors, "1", "0"))
    If Me.Mode Is Nothing Then
      ' keep going as before
    ElseIf Me.Mode = "to" Then
      lines.Add("MODE" & vbTab & "to" & vbTab & Me.TargetFile & vbTab & Me.TargetLine)
    Else
      lines.Add("MODE" & vbTab & Me.Mode)
    End If
    lines.Add("GO")
    Return lines.ToArray()
  End Function

End Class

''' <summary>A variable of the program being debugged: a number (Long or Double), a String, a Boolean, Nothing or an array of those.</summary>
Public NotInheritable Class DebugVariable
  Public Property Name As String
  Public Property Value As Object
End Class

''' <summary>The program stopped: where, why and what its variables hold.</summary>
Public NotInheritable Class PauseInfo
  Public Property File As Integer
  Public Property Line As Integer
  Public Property Depth As Integer
  ''' <summary>"step", "break" (a breakpoint) or "watch" (stopped to test the watchpoints).</summary>
  Public Property Reason As String
  Public ReadOnly Property Variables As New List(Of DebugVariable)()
  ''' <summary>The procedures that were running, the outermost first.</summary>
  Public ReadOnly Property Frames As New List(Of DebugFrame)()
  ''' <summary>The message of the error when <see cref="Reason"/> is "error".</summary>
  Public Property ErrorMessage As String
End Class

''' <summary>A procedure that was running when the program stopped, and the statement it had got to.</summary>
Public NotInheritable Class DebugFrame
  Public Property Name As String
  Public Property File As Integer
  Public Property Line As Integer
End Class

''' <summary>
''' Runs a program that was compiled for debugging (see <see cref="Instrumenter"/>) in a process of its own and talks to the
''' <c>QuickBasic.DebugHost</c> inside it. The program owns the terminal; QuickVB hears from it when it stops.
''' The events are raised on a background thread.
''' </summary>
Public NotInheritable Class DebugSession
  Implements IDisposable

  Private ReadOnly _dllPath As String
  Private ReadOnly _workingDirectory As String
  Private ReadOnly _commandLine As String
  Private ReadOnly _pipeName As String = "quickvb-" & Guid.NewGuid().ToString("N")
  Private ReadOnly _cancel As New CancellationTokenSource()
  Private ReadOnly _writeLock As New Object()

  Private _process As Process
  Private _server As NamedPipeServerStream
  Private _writer As StreamWriter
  Private _first As DebugCommand
  Private _ended As Integer

  Public Property OnPaused As Action(Of PauseInfo)
  Public Property OnEnded As Action(Of Integer)

  Public Sub New(dllPath As String, workingDirectory As String, commandLine As String)
    Me._dllPath = dllPath
    Me._workingDirectory = workingDirectory
    Me._commandLine = commandLine
  End Sub

  ''' <summary>Starts the program; it stops (or not) as <paramref name="first"/> says.</summary>
  Public Sub Start(first As DebugCommand)

    Me._first = first
    Me._server = New NamedPipeServerStream(Me._pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous)

    Dim arguments = """" & Me._dllPath & """" & If(String.IsNullOrEmpty(Me._commandLine), "", " " & Me._commandLine)
    Dim info As New ProcessStartInfo(QuickVBWorkspace.DotnetHost(), arguments) With {.UseShellExecute = False, .WorkingDirectory = Me._workingDirectory}
    info.Environment(QuickBasic.DebugHost.PipeVariable) = Me._pipeName

    Try
      Me._process = Process.Start(info)
    Catch ex As System.ComponentModel.Win32Exception
      Me._server.Dispose()
      Throw New InvalidOperationException(QuickVBWorkspace.DotnetMissingMessage, ex)
    End Try
    Me._process.EnableRaisingEvents = True
    AddHandler Me._process.Exited, Sub(sender, e) Me._cancel.Cancel()

    Dim worker As New Thread(AddressOf Me.Listen) With {.IsBackground = True, .Name = "Debugger"}
    worker.Start()

  End Sub

  ''' <summary>Lets the program that stopped carry on.</summary>
  Public Sub Go(command As DebugCommand)
    Me.Send(command)
  End Sub

  Private Sub Send(command As DebugCommand)
    SyncLock Me._writeLock
      If Me._writer Is Nothing Then Return
      Try
        For Each line In command.ToLines()
          Me._writer.WriteLine(line)
        Next
      Catch ex As IOException
        ' the program is gone
      End Try
    End SyncLock
  End Sub

  ''' <summary>Stops the program.</summary>
  Public Sub Kill()
    Try
      If Me._process IsNot Nothing AndAlso Not Me._process.HasExited Then
        Me._process.Kill(True)
        ' the program's files stay locked until it is gone
        Me._process.WaitForExit(5000)
      End If
    Catch ex As Exception When TypeOf ex Is InvalidOperationException OrElse TypeOf ex Is System.ComponentModel.Win32Exception
      ' it ended by itself
    End Try
  End Sub

  Private Sub Listen()

    Dim exitCode = 0

    Try

      ' the program connects when it reaches its first statement; a program that ends without one never does
      Try
        Me._server.WaitForConnectionAsync(Me._cancel.Token).Wait()
      Catch ex As AggregateException
        Return
      End Try

      Dim utf8 As New UTF8Encoding(False)
      Dim reader As New StreamReader(Me._server, utf8)
      Me._writer = New StreamWriter(Me._server, utf8) With {.AutoFlush = True, .NewLine = vbLf}

      Dim hello = reader.ReadLine()
      If hello Is Nothing Then Return

      Me.Send(Me._first)

      Do
        Dim line = reader.ReadLine()
        If line Is Nothing Then Exit Do

        Dim parts = line.Split(ControlChars.Tab)
        If parts(0) <> "PAUSE" OrElse parts.Length < 5 Then Continue Do

        Dim pause As New PauseInfo With {.File = ToInt(parts(1)), .Line = ToInt(parts(2)), .Depth = ToInt(parts(3)), .Reason = parts(4)}

        Do
          Dim item = reader.ReadLine()
          If item Is Nothing OrElse item = "END" Then Exit Do
          Dim fields = item.Split(ControlChars.Tab)
          If fields(0) = "VAR" AndAlso fields.Length >= 4 Then
            pause.Variables.Add(New DebugVariable With {.Name = fields(1), .Value = ParseValue(fields)})
          ElseIf fields(0) = "FRAME" AndAlso fields.Length >= 4 Then
            pause.Frames.Add(New DebugFrame With {.Name = fields(1), .File = ToInt(fields(2)), .Line = ToInt(fields(3))})
          ElseIf fields(0) = "ERROR" AndAlso fields.Length >= 2 Then
            pause.ErrorMessage = Uri.UnescapeDataString(fields(1))
          End If
        Loop

        Dim handler = Me.OnPaused
        If handler IsNot Nothing Then handler(pause)
      Loop

    Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is ObjectDisposedException

    Finally

      If Me._process IsNot Nothing Then
        Try
          Me._process.WaitForExit()
          exitCode = Me._process.ExitCode
        Catch ex As InvalidOperationException
        End Try
      End If

      If Interlocked.Exchange(Me._ended, 1) = 0 Then
        Dim handler = Me.OnEnded
        If handler IsNot Nothing Then handler(exitCode)
      End If

    End Try

  End Sub

  Private Shared Function ToInt(text As String) As Integer
    Dim value As Integer
    Integer.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, value)
    Return value
  End Function

  Private Shared Function ParseScalar(type As String, text As String) As Object

    Select Case type
      Case "int"
        Dim value As Long
        Long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, value)
        Return value
      Case "num"
        Dim value As Double
        Double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, value)
        Return value
      Case "bool"
        Return text = "1"
      Case "str", "obj"
        Return Uri.UnescapeDataString(text)
      Case Else
        Return Nothing
    End Select

  End Function

  Private Shared Function ParseValue(fields() As String) As Object

    ' VAR, name, type, text [, length]
    If fields(2) = "arr" Then
      Dim items As New List(Of Object)()
      Dim joined = Uri.UnescapeDataString(fields(3))
      If joined.Length > 0 Then
        For Each item In joined.Split("|"c)
          Dim at = item.IndexOf(":"c)
          items.Add(If(at < 0, Nothing, ParseScalar(item.Substring(0, at), item.Substring(at + 1))))
        Next
      End If
      Return items.ToArray()
    End If

    Return ParseScalar(fields(2), fields(3))

  End Function

  Public Sub Dispose() Implements IDisposable.Dispose
    Me.Kill()
    Me._cancel.Cancel()
    Me._server?.Dispose()
  End Sub

End Class
