Option Explicit On
Option Strict On
Option Infer On

Imports System.Globalization
Imports System.IO
Imports System.IO.Pipes
Imports System.Text

''' <summary>
''' The part of the debugger that lives inside the program being debugged. QuickVB compiles a program for debugging with a call of
''' <see cref="Check"/> (and, when it says so, <see cref="Pause"/>) in front of every statement; this class decides whether to stop
''' there, and while stopped it talks to QuickVB over a pipe (the name is in the QUICKVB_DEBUG_PIPE environment variable).
''' A program that runs without QuickVB finds no pipe and runs at full speed.
''' </summary>
Public NotInheritable Class DebugHost

  Private Sub New()
  End Sub

  Public Const PipeVariable As String = "QUICKVB_DEBUG_PIPE"

  Private Enum StepMode
    Run
    Into
    Over
    Out
    ToLine
  End Enum

  Private Shared ReadOnly Gate As New Object()

  Private Shared _initialized As Boolean
  Private Shared _active As Boolean
  Private Shared _reader As StreamReader
  Private Shared _writer As StreamWriter
  Private Shared _pipe As NamedPipeClientStream

  Private Shared _mode As StepMode = StepMode.Into
  Private Shared _targetDepth As Integer
  Private Shared _targetFile As Integer = -1
  Private Shared _targetLine As Integer = -1
  Private Shared _watching As Boolean
  Private Shared _breakpoints As New HashSet(Of Long)()

  <ThreadStatic>
  Private Shared _depth As Integer

  ' the procedures that are running on this thread, and where each of them has got to
  Private NotInheritable Class Frame
    Public Name As String
    Public File As Integer
    Public Line As Integer
  End Class

  <ThreadStatic>
  Private Shared _frames As List(Of Frame)

  <ThreadStatic>
  Private Shared _lastFile As Integer

  <ThreadStatic>
  Private Shared _lastLine As Integer

  Private Shared _breakOnErrors As Boolean

  Private Shared Function Key(file As Integer, line As Integer) As Long
    Return CLng(file) * 1000000L + line
  End Function

  ''' <summary>Called when a procedure starts.</summary>
  Public Shared Sub Enter(procedureName As String)
    _depth += 1
    If _frames Is Nothing Then _frames = New List(Of Frame)()
    _frames.Add(New Frame With {.Name = procedureName})
  End Sub

  ''' <summary>Called when a procedure ends.</summary>
  Public Shared Sub Leave()
    If _depth > 0 Then _depth -= 1
    If _frames IsNot Nothing AndAlso _frames.Count > 0 Then _frames.RemoveAt(_frames.Count - 1)
  End Sub

  ''' <summary>True when the program should stop in front of the statement on <paramref name="line"/> (0 based) of file number <paramref name="file"/>.</summary>
  Public Shared Function Check(file As Integer, line As Integer) As Boolean

    If Not _initialized Then Connect()
    If Not _active Then Return False

    _lastFile = file
    _lastLine = line
    If _frames IsNot Nothing AndAlso _frames.Count > 0 Then
      Dim top = _frames(_frames.Count - 1)
      top.File = file
      top.Line = line
    End If

    Return Reason(file, line) IsNot Nothing

  End Function

  ' Why the program should stop in front of this statement: "break", "step" or "watch"; Nothing when it should not.
  Private Shared Function Reason(file As Integer, line As Integer) As String

    If _breakpoints.Contains(Key(file, line)) Then Return "break"

    Dim stepDone As Boolean

    Select Case _mode
      Case StepMode.Into
        stepDone = True
      Case StepMode.Over
        stepDone = _depth <= _targetDepth
      Case StepMode.Out
        stepDone = _depth < _targetDepth
      Case StepMode.ToLine
        stepDone = file = _targetFile AndAlso line = _targetLine
    End Select

    If stepDone Then Return "step"
    If _watching Then Return "watch"

    Return Nothing

  End Function

  Private Shared Sub Connect()

    SyncLock Gate

      If _initialized Then Return
      _initialized = True

      Dim name = Environment.GetEnvironmentVariable(PipeVariable)
      If String.IsNullOrEmpty(name) Then Return

      Try
        _pipe = New NamedPipeClientStream(".", name, PipeDirection.InOut)
        _pipe.Connect(5000)
        Dim utf8 As New UTF8Encoding(False)
        _reader = New StreamReader(_pipe, utf8)
        _writer = New StreamWriter(_pipe, utf8) With {.AutoFlush = True, .NewLine = vbLf}
        _active = True
        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf OnUnhandledException
        _writer.WriteLine("HELLO")
        ' QuickVB answers with the breakpoints and how to start, then GO
        ReadCommands()
      Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is TimeoutException OrElse TypeOf ex Is UnauthorizedAccessException
        _active = False
      End Try

    End SyncLock

  End Sub

  ''' <summary>Reads commands from QuickVB up to the GO that lets the program carry on.</summary>
  Private Shared Sub ReadCommands()

    Do
      Dim line = _reader.ReadLine()

      If line Is Nothing Then
        ' QuickVB went away: carry on without a debugger
        _active = False
        Return
      End If

      Dim parts = line.Split(ControlChars.Tab)

      Select Case parts(0)
        Case "GO"
          Return
        Case "BP"
          _breakpoints = ParseBreakpoints(If(parts.Length > 1, parts(1), ""))
        Case "WP"
          _watching = parts.Length > 1 AndAlso parts(1) = "1"
        Case "ERR"
          _breakOnErrors = parts.Length > 1 AndAlso parts(1) = "1"
        Case "MODE"
          ApplyMode(parts)
      End Select
    Loop

  End Sub

  Private Shared Function ParseBreakpoints(text As String) As HashSet(Of Long)
    Dim result As New HashSet(Of Long)()
    For Each item In text.Split({";"c}, StringSplitOptions.RemoveEmptyEntries)
      Dim pair = item.Split(":"c)
      Dim file, line As Integer
      If pair.Length = 2 AndAlso Integer.TryParse(pair(0), file) AndAlso Integer.TryParse(pair(1), line) Then result.Add(Key(file, line))
    Next
    Return result
  End Function

  Private Shared Sub ApplyMode(parts() As String)

    Select Case parts(1)
      Case "run" : _mode = StepMode.Run
      Case "into" : _mode = StepMode.Into
      Case "over" : _mode = StepMode.Over : _targetDepth = _depth
      Case "out" : _mode = StepMode.Out : _targetDepth = _depth
      Case "to"
        _mode = StepMode.ToLine
        If parts.Length > 3 Then
          Integer.TryParse(parts(2), _targetFile)
          Integer.TryParse(parts(3), _targetLine)
        End If
    End Select

  End Sub

  ''' <summary>Stops in front of a statement: tells QuickVB where, with the variables in scope, and waits to be told what to do next.</summary>
  Public Shared Sub Pause(file As Integer, line As Integer, names() As String, values() As Object)

    SyncLock Gate

      If Not _active Then Return

      Try

        Dim why = If(Reason(file, line), "step")

        _writer.WriteLine("PAUSE" & vbTab & file & vbTab & line & vbTab & _depth & vbTab & why)
        WriteFrames()

        If names IsNot Nothing Then
          For i = 0 To names.Length - 1
            Dim v As Object = Nothing
            If values IsNot Nothing AndAlso i < values.Length Then v = values(i)
            _writer.WriteLine("VAR" & vbTab & names(i) & vbTab & Describe(v))
          Next
        End If

        _writer.WriteLine("END")

        ' a stop only to test the watchpoints leaves a step that is under way as it was; any other stop ends it
        If why <> "watch" Then _mode = StepMode.Run
        ReadCommands()

      Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is ObjectDisposedException
        _active = False
      End Try

    End SyncLock

  End Sub

  Private Shared Sub WriteFrames()
    If _frames Is Nothing Then Return
    For Each f In _frames
      _writer.WriteLine("FRAME" & vbTab & f.Name & vbTab & f.File & vbTab & f.Line)
    Next
  End Sub

  ''' <summary>An error nothing handles: with Break on Errors the program stops where it happened instead of ending.</summary>
  Private Shared Sub OnUnhandledException(sender As Object, e As UnhandledExceptionEventArgs)

    If Not _active OrElse Not _breakOnErrors Then Return

    SyncLock Gate
      Try
        Dim message = TryCast(e.ExceptionObject, Exception)?.Message
        _writer.WriteLine("PAUSE" & vbTab & _lastFile & vbTab & _lastLine & vbTab & _depth & vbTab & "error")
        _writer.WriteLine("ERROR" & vbTab & Encode(If(message, e.ExceptionObject.ToString())))
        WriteFrames()
        _writer.WriteLine("END")
        ReadCommands()
      Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is ObjectDisposedException
        _active = False
      End Try
    End SyncLock

  End Sub

  ' ---- describing values: type TAB text, with the text percent-encoded

  Private Shared Function Encode(text As String) As String
    Return Uri.EscapeDataString(text)
  End Function

  Private Shared Function Scalar(value As Object) As String

    If value Is Nothing Then Return "nothing" & vbTab & ""

    Select Case Type.GetTypeCode(value.GetType())
      Case TypeCode.Boolean
        Return "bool" & vbTab & If(CBool(value), "1", "0")
      Case TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64
        Return "int" & vbTab & Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture)
      Case TypeCode.UInt64
        Return "num" & vbTab & Convert.ToDouble(value, CultureInfo.InvariantCulture).ToString("R", CultureInfo.InvariantCulture)
      Case TypeCode.Single, TypeCode.Double, TypeCode.Decimal
        Return "num" & vbTab & Convert.ToDouble(value, CultureInfo.InvariantCulture).ToString("R", CultureInfo.InvariantCulture)
      Case TypeCode.Char, TypeCode.String
        Return "str" & vbTab & Encode(value.ToString())
      Case Else
        Try
          Return "obj" & vbTab & Encode(value.ToString())
        Catch ex As Exception
          Return "obj" & vbTab & Encode("<" & value.GetType().Name & ">")
        End Try
    End Select

  End Function

  Private Shared Function Describe(value As Object) As String

    Dim list = TryCast(value, Array)

    If list IsNot Nothing AndAlso list.Rank = 1 Then
      Dim items As New List(Of String)()
      For i = 0 To Math.Min(list.Length, 100) - 1
        Dim parts = Scalar(list.GetValue(i)).Split(ControlChars.Tab)
        items.Add(parts(0) & ":" & parts(1))
      Next
      Return "arr" & vbTab & Encode(String.Join("|", items)) & vbTab & list.Length
    End If

    Return Scalar(value)

  End Function

End Class
