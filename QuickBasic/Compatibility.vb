Public Class Compatibility

  ' Used in Print(), ...
  Public Const SEMICOLON As String = " ;"
  Public Const COMMA As String = " ,"

  ' Used in Screen(), ...
  Private Shared m_screenMode As Integer = 0
  Private Shared m_window As System.Windows.Forms.Form

  ' Used in Data(), Read(), Restore()
  Private Shared m_data As New List(Of Object)
  Private Shared m_readIndex As Integer = 0

  ' Used in Close(), ...
  Private Shared m_filenum As New List(Of Integer) ' NOTE: This needs to be evolved more to handle state (number (possibly a dictionary), file path / device info, position, etc.

  Public Enum PutMode
    PSET
    [XOR]
  End Enum

  Public Enum LineMode
    None
    B
    BF
  End Enum

  Private Sub New()
  End Sub

  ' ABS

  Public Shared Function Abs(value As Single) As Single
    Return Math.Abs(value)
  End Function

  Public Shared Function Abs(value As Double) As Double
    Return Math.Abs(value)
  End Function

  Public Shared Function Abs(value As Byte) As Byte
    Return Math.Abs(value)
  End Function

  Public Shared Function Abs(value As SByte) As SByte
    Return Math.Abs(value)
  End Function

  Public Shared Function Abs(value As Short) As Short
    Return Math.Abs(value)
  End Function

  Public Shared Function Abs(value As Integer) As Integer
    Return Math.Abs(value)
  End Function

  Public Shared Function Abs(value As Long) As Long
    Return Math.Abs(value)
  End Function

  Public Shared Function Abs(value As Decimal) As Decimal
    Return Math.Abs(value)
  End Function

  Public Shared Function Abs(value As String) As Integer
    Throw New ArgumentException("Type mismatch")
  End Function

  Public Shared Function Abs(value As Object) As Integer
    Throw New ArgumentException("Type mismatch")
  End Function

  ' AND

  ' ...in VB.NET.

  ' ASC

  Public Shared Function Asc(value As Char) As Integer
    Return Microsoft.VisualBasic.Asc(value)
  End Function

  Public Shared Function Asc(value As String) As Integer
    If String.IsNullOrEmpty(value) Then
      Throw New ArgumentException("Illegal function call")
    Else
      Return Microsoft.VisualBasic.Asc(value)
    End If
  End Function

  Public Shared Function Asc(value As Object) As Integer
    Throw New ArgumentException("Type mismatch")
  End Function

  ' ATN

  Public Shared Function Atn(value As Double) As Double
    Return Math.Atan(value)
  End Function

  Public Shared Function Atn(value As Object) As Integer
    Throw New ArgumentException("Type mismatch")
  End Function

  ' BEEP

  Public Shared Sub Beep()
    Console.Beep()
  End Sub

  ' BLOAD

  Public Shared Sub BLoad(name As String, Optional offset As Integer = 0)
    'TODO: Need to put some thought into how (or if) there's a way to handle this method; for now, throw not implemented.
    Throw New NotImplementedException()
  End Sub

  ' BSAVE

  Public Shared Sub BSave(name As String, offset As Integer, bytes As Integer)
    'TODO: Need to put some thought into how (or if) there's a way to handle this method; for now, throw not implemented.
    Throw New NotImplementedException()
  End Sub

  ' CALL

  ' ...in VB.NET.

  ' CDBL

  ' ...in VB.NET.


  ' CHAIN

  Public Shared Sub Chain(filename As String)
    ' ...not something that can be translated easily.
    Throw New NotImplementedException()
  End Sub

  ' CHDIR

  Public Shared Sub ChDir(directory As String)
    If Not String.IsNullOrEmpty(directory) Then
      If IO.Directory.Exists(directory) Then
        System.IO.Directory.SetCurrentDirectory(directory)
      Else
        Throw New IO.DirectoryNotFoundException("Path not found")
      End If
    Else
      Throw New ArgumentException("Expected: expression")
    End If
  End Sub

  ' CHR$

  Public Shared Function Chr(val As String) As String
    If String.IsNullOrEmpty(val) Then
      Return Chr(0)
    ElseIf IsNumeric(val) Then
      Return Chr(CInt(val))
    Else
      Throw New ArgumentException("Type mismatch")
    End If
  End Function

  Public Shared Function Chr(val As Integer) As String
    Select Case val
      Case 0 To 255
        Return Microsoft.VisualBasic.Chr(val)
      Case Else
        Throw New ArgumentException("Illegal Function call")
    End Select
  End Function

  ' CINT

  ' ... in VB.NET.

  ' CIRCLE (NOTE: Syntax Change)

  Public Shared Sub Circle([step] As Boolean, x As Integer, y As Integer, radius As Integer, Optional color As Integer = -1, Optional start As Double = 0.00, Optional [stop] As Double = 0.00, Optional aspect As Double = 0.00)
    If [step] Then
      ' Calculate x, y as "offset"...
    End If
    Circle(x, y, radius, color, start, [stop], aspect)
  End Sub

  Public Shared Sub Circle(x As Double, y As Double, radius As Double, Optional color As Integer = -1, Optional start As Double = 0.00, Optional [stop] As Double = 0.00, Optional aspect As Double = 0.00)
    Circle(CInt(x), CInt(y), CInt(radius), color, start, [stop], aspect)
  End Sub

  Public Shared Sub Circle(x As Integer, y As Integer, radius As Integer, Optional color As Integer = -1, Optional start As Double = 0.00, Optional [stop] As Double = 0.00, Optional aspect As Double = 0.00)
    Throw New NotImplementedException()
  End Sub

  ' CLEAR

  ' ... not something that can be translated to VB.NET.
  Public Shared Sub Clear(dummy1 As Object, dummy2 As Object, stack As Integer)
    Restore()
    Throw New NotImplementedException()
  End Sub

  ' CLNG

  ' ... in VB.NET.

  ' CLOSE

  Public Shared Sub Close()
    For Each filenum In m_filenum
      'TODO: Close the file.
    Next
    m_filenum.Clear()
  End Sub

  Public Shared Sub Close(ParamArray filenums As Integer())
    For Each filenum In filenums
      If m_filenum.Contains(filenum) Then
        'TODO: Close the file.
        m_filenum.Remove(filenum)
      End If
    Next
  End Sub

  ' CLS

  Public Shared Sub Cls(Optional viewport As Integer? = -1)
    ' TODO: Need to revise this to work with the graphics "screen" as that part of the project takes shape.
    If viewport Is Nothing Then
      viewport = -1
    End If
    Select Case viewport
      Case -1
        ' Clears the currently active viewport.  If neither a graphics viewport nor a text viewport is defined, the statement clears the entire screen.
        Console.Clear()
      Case 0
        ' Clears the entire screen, regardless of the currently active viewport.
        Console.Clear()
      Case 1
        ' Clears the graphics viewport.
        Console.Clear()
      Case 2
        ' Clears the text viewport.
        Console.Clear()
      Case Else
        Throw New ArgumentOutOfRangeException("CLS viewport must be not provided, Nothing, 0, 1, or 2.")
    End Select
  End Sub

  ' COLOR

  Public Shared Sub Color(Optional foreground As Integer = -1, Optional background As Integer = -1, Optional border As Integer = -1)

    'TODO: Need to take into account the current screen mode...

    ' Screen mode 0
    ' Screen Mode 1 - second param is "palette"; 3rd param is unused (invalid).
    ' Screen Mode 7-10 - second param is "background"; 3rd param is unused (invalid).

    If foreground > -1 Then
      Console.ForegroundColor = foreground
    End If

    If background > -1 Then
      Console.BackgroundColor = background
    End If

    If border > -1 Then
      ' Windows console doesn't support the concept of "border".  TODO: or does it - need to verify?
    End If

  End Sub

  ' COMMON

  ' ...not in VB.NET; see CHAIN.

  ' COM ON/OFF/STOP

  ' CONST

  ' ...in VB.NET.

  ' COS

  Public Shared Function Cos(angle As String) As Double
    If IsNumeric(angle) Then
      Return System.Math.Cos(angle)
    Else
      Throw New ArgumentException("Type mismatch")
    End If
  End Function

  Public Shared Function Cos(angle As Double) As Double
    Return System.Math.Cos(angle)
  End Function

  ' CSNG

  ' ...in VB.NET.

  ' CSRLIN

  Public Shared Function CsrLin() As Integer
    'TODO: Need to handle all Screen() modes; for now, just working with text.
    Return Console.CursorTop + 1 ' Range of 1-25 (CGA) , or 1-43 (EGA) or 1-60 (VGA/MCGA)...
  End Function

  ' CVD

  Public Shared Function CVD(value As String) As Double

    If value.Length <> 8 Then
      Throw New ArgumentException()
    End If

    Dim result As Object = CDbl(0)

    Dim hGC = Runtime.InteropServices.GCHandle.Alloc(result, Runtime.InteropServices.GCHandleType.Pinned)
    Try
      Dim ptr = hGC.AddrOfPinnedObject()
      Dim encode() = Text.Encoding.GetEncoding("iso-8859-1").GetBytes(value)
      Runtime.InteropServices.Marshal.Copy(encode, 0, ptr, 8)
    Finally
      hGC.Free()
    End Try

    Return DirectCast(result, Double)

  End Function

  ' CVDMBF

  Public Shared Function CVDMBF(value As String) As Double
    If value.Length <> 8 Then
      Throw New ArgumentException()
    End If
    Throw New NotImplementedException()
  End Function

  ' CVI

  Public Shared Function CVI(value As String) As Short
    If value.Length <> 2 Then
      Throw New ArgumentException()
    End If
    Throw New NotImplementedException()
  End Function

  ' CVL

  Public Shared Function CVL(value As String) As Integer
    If value.Length <> 4 Then
      Throw New ArgumentException()
    End If
    Throw New NotImplementedException()
  End Function

  ' CVS

  Public Shared Function CVS(value As String) As Single
    If value.Length <> 4 Then
      Throw New ArgumentException()
    End If
    Throw New NotImplementedException()
  End Function

  ' CVSMBF

  Public Shared Function CVSMBF(value As String) As Single
    If value.Length <> 4 Then
      Throw New ArgumentException()
    End If
    Throw New NotImplementedException()
  End Function

  ' DATA

  Public Shared Sub Data(ParamArray values As Object())
    For Each value In values
      m_data.Add(value)
    Next
  End Sub

  ' DATE$ (Statement)

  ' ... not something that can be in VB.NET??? (Wondering if this could be some sort of a write-only property???)

  ' DATE$ (Function)

  Public Shared Function QBDate() As String
    Return Now.ToString("MM-dd-yyyy")
  End Function

  ' DECLARE 

  ' ...not used in the same way as QB; general translation is to "just remove them".

  ' DEF FN

  ' ... not something that can be translated easily.

  ' DEF SEG

  ' ... not something that can be translated easily.

  ' DEF type

  ' ... not something that can be translated easily.

  ' DIM

  ' ...in VB.NET.

  ' DO...LOOP

  ' ...in VB.NET.

  ' DOUBLE

  ' ...in VB.NET.

  ' DRAW

  Public Shared Sub Draw(commandString As String)
    'TODO: I have an existing project where I've implemented something very similar to this command (sub-language); 
    '      it was for the Tandy Color Computer version of BASIC, but should be pretty easy to get up to the GW-BASIC/QB version. -- Cory Smith
    Throw New NotImplementedException()
  End Sub

  ' $DYNAMIC

  ' ... not something that can be translated easily.

  ' END

  ' ...in VB.NET.

  ' ENVIRON/ENVIRON$

  Public Shared Function Environ(value As String) As String

    'NOTE: This acts as both the getter and setter; this is handled based on the value parameter.
    '      If contains an = in the value, it's assumed to be a setter.
    '      If no = in the value, it's assumed to be a getter.
    ' 
    '      This allows for both of the following scenarios to be valid:
    '   
    '      a$ = Environ("PATH")
    '      Environ("PATH=C:\DOS")
    '
    ' .NET Docs: https://docs.microsoft.com/en-us/dotnet/api/system.environment.getenvironmentvariable?view=netframework-4.7.2

    If value.Contains("=") Then
      Dim entries = value.Split(";"c)
      For Each entry In entries
        If entry.Contains("=") Then
          Dim nameValue = entry.Split("=")
          If nameValue.Length = 2 Then
            Environment.SetEnvironmentVariable(nameValue(0), nameValue(1))
          Else
            Throw New ArgumentException()
          End If
        Else
          Throw New ArgumentException()
        End If
      Next
      Return Nothing
    Else
      Return Environment.GetEnvironmentVariable(value)
    End If

  End Function

  Public Shared Function Environ(num As Integer) As String
    Dim index = 0
    For Each entry In Environment.GetEnvironmentVariables()
      If index + 1 = num Then
        Return $"{entry.key}={entry.value}"
      End If
      index += 1
    Next
    Return ""
  End Function

  ' EOF

  Public Shared Function Eof(filenum As Integer) As Integer
    Dim result = 0
    If m_filenum.Contains(filenum) Then
      'TODO: Add additional details to the m_filenum "state machine" to provide ability to determine if EOF has been encountered.
      'TODO: Determine if the last "input statement" has reached the end of the file.
      'TODO: If using GET to read from a file, the only way that a -1 is returned is if the GET didn't have enough data to populate the structure.
      Throw New NotImplementedException()
    Else
      Throw New ArgumentException("Bad file name or number")
    End If
    Return result
  End Function

  ' EQV

  ' ...roughly translates to value AndAlso value (alternatively Not value AndAlso Not value) when used as a logical operator; as a mathimatical operator, no easily translation available.

  ' ERASE

  Public Shared Sub [Erase](ParamArray arrays As Object())
    ' With a static array, the ERASE statement merely resets all elements to their default value -- 0 if numeric or null strings ("").  ERASE does not change the dimensions of the static array defined in the DIM statement nore release any memory.
    ' With a dynamic array, however, ERASE deallocates the memory used by the array, thereby freeing the memory for other dynamic arrays.  Before you can reuse an erased array, you must dimension it agin with DIM.  (QBasic offers the REDIM statement as an alternative to erasing and redimensioning a dynamic array.  REDIM deallocates, redimensions, and reinitializes a dynamic array in one step.)
    Throw New NotImplementedException()
  End Sub

  ' ERDEV

  Public Shared Function ErDev() As Short

    ' LSB = MS-DOS error code that reveals the nature of the error.
    ' MSB = "device attribute word", a 2-byte collection of flags that is part of the device-driver program in memory.
    '
    ' To read the separate values:
    ' 
    ' deviceId% = VAL("&H" + LEFT$(HEX$(ERDEV), 2))
    ' errorCode% = VAL("&H" + RIGHT$(HEX$(ERDEV), 2))

    'NOTE: There are two tables in The Waite Group's Microsoft QuickBASIC Bible p356 that can be used to potentially simulate the same information in VB.NET.

    Throw New NotImplementedException()

  End Function

  ' ERDEV$

  Public Shared Function ErDevString() As String

    Return "A:"
    Return "B:"
    Return "C:"
    Return "LPT1    "
    Return "COM1    "

  End Function

  ' ERL

  Public Shared Function Erl() As Integer
    Return Microsoft.VisualBasic.Err.Erl
  End Function

  ' ERR

  Public Shared Function Err() As Integer
    Return Microsoft.VisualBasic.Err.Number
  End Function

  ' ERROR

  Public Shared Sub [Error](errorCode As Integer)
    Microsoft.VisualBasic.Err.Raise(errorCode)
  End Sub

  ' EXIT

  ' ...translates to Exit For, Exit Do, etc.

  ' EXP

  Public Shared Function Exp(expr As Double) As Double
    Return Math.Exp(expr)
  End Function

  ' FIELD

  ' ... Not sure if there is a way to handle FIELD; will have to think on this one a bit more.

  ' FILEATTR

  Public Shared Sub FileAttr(filenum As Integer, attribute As Integer)
    If m_filenum.Contains(filenum) Then
      Select Case attribute
        Case 1 ' Returns information about the mode in which the file was opened.
          ' Returns:
          '  
          '   1 Input
          '   2 Output
          '   4 Random
          '   4 Append
          '  32 Binary
          '
          Throw New NotImplementedException()
        Case 2 ' Returns the file's DOS file handle.
          Throw New NotImplementedException()
        Case Else
          Throw New ArgumentException("attribute")
      End Select
    Else
      Throw New ArgumentException("Bad file name or number")
    End If
  End Sub

  ' FILES

  Public Shared Sub Files(Optional filespec As String = Nothing)
    ' TODO: Need to do some work for the presented output; some thought needs to go into how to have it look like QB but work with long filenames.
    ' NOTE: Looks like more modern systems (Windows 10) don't have 8dot3 names enabled; so attempting to pull ShortPathName is pointless.
    If filespec Is Nothing Then
      filespec = IO.Directory.GetCurrentDirectory()
    End If
    Dim searchPattern As String = "*.*"
    If filespec.Contains("*") OrElse filespec.Contains("?") Then
      'TODO: Need to split out the search pattern from the path.
    End If
    Print(IO.Directory.GetCurrentDirectory) ' FILES displays a 1-line header that contains the current directory even if you specify another pathname in the FILES statement.
    Print("        .   <DIR>")
    Print("        ..  <DIR>")
    For Each directory In IO.Directory.GetDirectories(filespec, searchPattern)
      Print(IO.Path.GetDirectoryName(directory) & " <DIR>")
    Next
    For Each file In IO.Directory.GetFiles(filespec, searchPattern)
      Print(IO.Path.GetFileName(file))
      'Print(IO.Path.GetFileName(GetShortPath(file)))
    Next
    Dim driveName = IO.Path.GetPathRoot(filespec)
    Dim di = New IO.DriveInfo(driveName)
    Print(String.Format("{0} Bytes free", di.AvailableFreeSpace))
  End Sub

  'Private Declare Auto Function GetShortPathName Lib "kernel32" (longPath As String, shortPath As Text.StringBuilder, shortBufferSize As Integer) As Integer

  'Private Shared Function GetShortPath(path As String) As String
  '  Dim result = New Text.StringBuilder(260)
  '  GetShortPathName(path, result, result.Capacity)
  '  Return result.ToString
  'End Function

  ' FIX

  ' ...in VB.NET.  
  'TODO: Not wrapping this one since the VB.NET compiler has an optimization technique when encountering CINT(FIX(value)); if we were to wrap it to 
  '      in an effort to reproduce the total behavior of QB, we would lose this optimzation.  Need to evaluate further.

  ' FOR...NEXT

  ' ...in VB.NET.

  ' FRE

  Public Shared Function Fre(number As Integer) As Integer
    Select Case number
      Case -1 ' Returns the amount of memory available in the heap.
        Throw New NotImplementedException()
      Case -2 ' Returns the amount of available stack space.
        Throw New NotImplementedException()
      Case Else ' Returns the size of the next block of unused string space.
        Throw New NotImplementedException()
    End Select
  End Function

  Public Shared Function Fre(value As String) As Integer
    ' Compacts the unued string space into the largest possible block and then returns the amount of available string space.
    Throw New NotImplementedException()
  End Function

  ' FREEFILE

  Public Shared Function FreeFile() As Integer
    ' Returns the lowest file number that is not associated with an open file.
    If m_filenum.Count >= 15 Then
      ' QB4.5 allows nor more than 15 files to be open at once.
      Throw New InvalidOperationException("Path/file access")
    End If
    For index = 1 To 255
      If Not m_filenum.Contains(index) Then
        Return index
      End If
    Next
    Throw New InvalidOperationException() ' ????? Not sure what would happen if you exceeded 255 open files (which isn't possible in QB45, so have to try to test other versions).
  End Function

  ' FUNCTION

  ' ...in VB.NET.

  ' GET (File I/O)

  Public Shared Sub QBGet(filenum As Integer, Optional position As Integer? = Nothing, Optional ByRef variable As Object = Nothing)

    If m_filenum.Contains(filenum) Then

      If variable Is Nothing Then

        ' GET #1, 1
        ' - Retrieves the first record in a random-access file that contains records defined by a FIELD statement.

        Throw New NotImplementedException()

      Else

        Dim isBinary = False

        If isBinary Then

          ' OPEN "CONFIG.DAT" FOR BINARY AS #1
          ' GET #1, 100, config%
          ' - Opens a binary file, retrieves from the 100th byte position an integer and assigns it to the variable config%.

          If position IsNot Nothing Then
            'TODO: Determine position to read based as a byte-by-byte index; in other words, position * byte.
          End If

          Dim t = variable.GetType
          If t.Equals(GetType(Byte)) Then
          ElseIf t.Equals(GetType(SByte)) Then
          ElseIf t.Equals(GetType(Short)) Then
          ElseIf t.Equals(GetType(UShort)) Then
          ElseIf t.Equals(GetType(Integer)) Then
          ElseIf t.Equals(GetType(UInteger)) Then
          ElseIf t.Equals(GetType(Long)) Then
          ElseIf t.Equals(GetType(ULong)) Then
          ElseIf t.Equals(GetType(Decimal)) Then
          ElseIf t.Equals(GetType(Single)) Then
          ElseIf t.Equals(GetType(Double)) Then
          ElseIf t.Equals(GetType(String)) Then
          Else
            ' Other?
          End If

          Throw New NotImplementedException()

        Else

          ' GET #1, , info
          ' - Retieves the next record (if one exists) in a random-access file and assigns it to the record variable info.

          If position IsNot Nothing Then
            'TODO: Determine position to read based as a size of the structure; in other words, position * Len(structure).
          End If

          Throw New NotImplementedException()

        End If

      End If

    Else
      Throw New ArgumentException("filenum")
    End If

  End Sub

  ' GET (Graphics)

  Public Structure QBCoord
    Public X As Integer
    Public Y As Integer
    Public [Step] As Boolean
  End Structure

  Public Shared Function QBStep(x As Integer, y As Integer) As QBCoord
    Return New QBCoord With {.X = x, .Y = y, .[Step] = True}
  End Function

  Public Shared Sub QBGet(coord1 As Integer(), coord2 As Integer(), buffer As Short())

    ' GET STEP(20, -15)-STEP(xInc, yInc), image&(0, 5)

    Throw New NotImplementedException()

  End Sub

  Public Shared Sub QBGet(coord1 As QBCoord, coord2 As QBCoord, buffer As Short())

    ' GET STEP(20, -15)-STEP(xInc, yInc), image&(0, 5)

    Throw New NotImplementedException()

  End Sub

  Public Shared Sub QBGet(x1 As Double, y1 As Double, x2 As Double, y2 As Double, array As System.Array)
    QBGet(CInt(x1), CInt(y1), CInt(x2), CInt(y2), array)
  End Sub

  Public Shared Sub QBGet(x1 As Integer, y1 As Integer, x2 As Integer, y2 As Integer, array As System.Array)
    Throw New NotImplementedException()
  End Sub

  ' GOSUB...RETURN

  ' ...in VB.NET.

  ' GOTO

  ' ...in VB.NET.

  ' HEX$

  Public Shared Function Hex(num As Object) As String
    Return Microsoft.VisualBasic.Hex(num)
  End Function

  Public Shared Function Hex(num As Byte) As String
    Return Microsoft.VisualBasic.Hex(num)
  End Function

  Public Shared Function Hex(num As SByte) As String
    Return Microsoft.VisualBasic.Hex(num)
  End Function

  Public Shared Function Hex(num As UShort) As String
    Return Microsoft.VisualBasic.Hex(num)
  End Function

  Public Shared Function Hex(num As UInteger) As String
    Return Microsoft.VisualBasic.Hex(num)
  End Function

  Public Shared Function Hex(num As ULong) As String
    Return Microsoft.VisualBasic.Hex(num)
  End Function

  Public Shared Function Hex(num As Short) As String
    Return Microsoft.VisualBasic.Hex(num)
  End Function

  Public Shared Function Hex(num As Integer) As String
    Return Microsoft.VisualBasic.Hex(num)
  End Function

  Public Shared Function Hex(num As Long) As String
    Return Microsoft.VisualBasic.Hex(num)
  End Function

  ' IF

  ' ...in VB.NET.

  ' IMP

  ' ...roughly translates to = Not (Not value Andalso value) when used as an logical operator; ; as a mathimatical operator, no easily translation available.

  ' INKEY$

  Public Shared Function Inkey() As String
    If Console.KeyAvailable Then
      Dim key = Console.ReadKey(True) ' We don't want to "echo".
      'TODO: Need to handle "function keys" which include:
      '      arrow keys, pgup, pgdn, home, end, F1-F12 and any Alt+alpha/numeric key combinations.
      '      This will need to be returned as CHR$(0) + the keys "scan code".
      ' See p312.
      Return key.KeyChar
    Else
      Return ""
    End If
  End Function

  ' INP

  ' INPUT

  Public Shared Sub Input(ParamArray a As String())
    Input(False, Nothing, False, a)
  End Sub

  Public Shared Sub Input(noCr As Boolean, ParamArray a As String())
    Input(noCr, Nothing, False, a)
  End Sub

  Public Shared Sub Input(noCr As Boolean, prompt As String, ParamArray a As String())
    Input(noCr, prompt, False, a)
  End Sub

  Public Shared Sub Input(prompt As String, ParamArray a As String())
    Input(False, prompt, False, a)
  End Sub

  Public Shared Sub Input(noCr As Boolean, prompt As String, includeQuestion As Boolean, ParamArray a As String())

    Do

      If prompt Is Nothing Then
        Console.Write("? ")
      Else
        Console.Write($"{prompt}{If(includeQuestion, "?", "")} ")
      End If

      Dim result = Console.ReadLine() 'TODO: This may need to be replaced with ReadKey as there is support for "editing" the entry with several special key combinations... (See p315)

      Dim split = result.Split(","c)

      If a.Count = split.Count Then
        For index = 0 To a.Count - 1
          a(index) = split(index)
        Next
        Exit Do
      Else
        Console.WriteLine("Redo from start")
      End If

    Loop

    If noCr Then
      'TODO: Need to keep the cursor on the same line after the enter key is pressed.
    End If

  End Sub

  ' INPUT #

  ' INPUT$ (File I/O)

  ' INPUT$ (Keyboard)

  Public Shared Function Input(chars As Integer) As String

    ' The INPUT$ (keyboard) function reads a specified number of characters from the keyboard.  
    ' Usually INPUT$ is used to read from a file; however if no file number is specified, INPUT$ 
    ' reads input from the standard input device, which, by default, is the keyboard.

    Dim result = ""

    Do
      If Console.KeyAvailable Then
        Dim key = Console.ReadKey(True) ' We don't want to "echo".
        If key.Modifiers = ConsoleModifiers.Control AndAlso
           key.Key = ConsoleKey.C Then 'TODO: Documentation specifically states CTRL+BREAK; not sure if that maps to CTRL+C or not.
          Return result 'TODO: Not sure if I should return what we've captured thus far or should an empty string be returned?
        End If
        'TODO: Need to limit which keys are allowed...
        result &= key.KeyChar
      End If
      If result.Length = chars Then
        Exit Do
      End If
    Loop

    Return result

  End Function

  ' INSTR

  Public Shared Function Instr(string1 As String, string2 As String) As Integer
    Return Microsoft.VisualBasic.InStr(string1, string2)
  End Function

  Public Shared Function Instr(start As Integer, string1 As String, string2 As String) As Integer
    Return Microsoft.VisualBasic.InStr(start, string1, string2)
  End Function

  ' INT

  ' ...in VB.NET.

  ' INTEGER

  ' ...in VB.NET.

  ' IOCTL

  ' IOCTL$

  ' KEY

  ' KEY(n) ON/OFF/STOP

  ' KILL

  ' LBOUND

  Public Shared Function LBound(array As System.Array, Optional rank As Integer = 1) As Integer
    Return Microsoft.VisualBasic.LBound(array, rank)
  End Function

  ' LCASE$

  Public Shared Function LCase(value As Char) As Char
    Return Microsoft.VisualBasic.LCase(value)
  End Function

  Public Shared Function LCase(value As String) As String
    Return Microsoft.VisualBasic.LCase(value)
  End Function

  ' LEFT$

  Public Shared Function Left(str As String, length As Integer) As String
    Return Microsoft.VisualBasic.Left(str, length)
  End Function

  ' LEN

  Public Shared Function Len(expression As Boolean) As Integer
    Return Microsoft.VisualBasic.Len(expression)
  End Function

  Public Shared Function Len(expression As Byte) As Integer
    Return Microsoft.VisualBasic.Len(expression)
  End Function
  Public Shared Function Len(expression As Char) As Integer
    Return Microsoft.VisualBasic.Len(expression)
  End Function
  Public Shared Function Len(expression As Date) As Integer
    Return Microsoft.VisualBasic.Len(expression)
  End Function
  Public Shared Function Len(expression As Decimal) As Integer
    Return Microsoft.VisualBasic.Len(expression)
  End Function
  Public Shared Function Len(expression As Double) As Integer
    Return Microsoft.VisualBasic.Len(expression)
  End Function
  Public Shared Function Len(expression As Integer) As Integer
    Return Microsoft.VisualBasic.Len(expression)
  End Function
  Public Shared Function Len(expression As Long) As Integer
    Return Microsoft.VisualBasic.Len(expression)
  End Function
  Public Shared Function Len(expression As Object) As Integer
    Return Microsoft.VisualBasic.Len(expression)
  End Function
  Public Shared Function Len(expression As SByte) As Integer
    Return Microsoft.VisualBasic.Len(expression)
  End Function
  Public Shared Function Len(expression As Short) As Integer
    Return Microsoft.VisualBasic.Len(expression)
  End Function
  Public Shared Function Len(expression As Single) As Integer
    Return Microsoft.VisualBasic.Len(expression)
  End Function
  Public Shared Function Len(expression As String) As Integer
    Return Microsoft.VisualBasic.Len(expression)
  End Function
  Public Shared Function Len(expression As UInteger) As Integer
    Return Microsoft.VisualBasic.Len(expression)
  End Function
  Public Shared Function Len(expression As ULong) As Integer
    Return Microsoft.VisualBasic.Len(expression)
  End Function
  Public Shared Function Len(expression As UShort) As Integer
    Return Microsoft.VisualBasic.Len(expression)
  End Function

  ' LET

  ' ... no real way to translate this.

  ' LINE

  Public Shared Sub Line(coord1 As Integer(), coord2 As Integer(), Optional color As Integer = -1, Optional lm As LineMode = LineMode.None, Optional style As Integer = -1)
    Throw New NotImplementedException()
  End Sub

  Public Shared Sub Line(x1 As Integer, y1 As Integer, x2 As Integer, y2 As Integer, Optional color As Integer = -1, Optional lm As LineMode = LineMode.None, Optional style As Integer = -1)
    Throw New NotImplementedException()
  End Sub

  ' LINE INPUT

  Public Shared Sub LineInput(ByRef var As String)
    ' The LINE INPUT statement accepts all data entered as ASCII text and assigns it to a single 
    ' string variable.  This statement lets the user enter any characters -- even commas and 
    ' question marks -- that are regarded as delimiters by the INPUT statement.
    var = ""
  End Sub

  Public Shared Sub LineInput(prompt As String, ByRef var As String)
    ' The LINE INPUT statement accepts all data entered as ASCII text and assigns it to a single 
    ' string variable.  This statement lets the user enter any characters -- even commas and 
    ' question marks -- that are regarded as delimiters by the INPUT statement.
    var = ""
  End Sub

  Public Shared Sub LineInput(noCr As Boolean, prompt As String, ByRef var As String)
    ' The LINE INPUT statement accepts all data entered as ASCII text and assigns it to a single 
    ' string variable.  This statement lets the user enter any characters -- even commas and 
    ' question marks -- that are regarded as delimiters by the INPUT statement.
    var = ""
  End Sub

  ' LINE INPUT #

  ' LOC

  ' LOCATE

  Public Shared Sub Locate(Optional row As Integer = -1, Optional col As Integer = -1, Optional visible As Integer = -1, Optional start As Integer = -1, Optional [end] As Integer = -1)
    If row = -1 Then
      row = CsrLin()
    End If
    If col = -2 Then
      row = Pos(0)
    End If
    Console.SetCursorPosition(col - 1, row - 1)
    If visible = 0 OrElse visible = 1 Then
      If visible = 0 Then
        Console.CursorVisible = False
      Else
        Console.CursorVisible = True
      End If
    End If
  End Sub

  ' LOCK

  ' LOF

  ' LOG

  ' LONG

  ' ...in VB.NET.

  ' LPOS

  ' LPRINT

  ' LPRINT USING

  ' LSET

  ' LTRIM$

  ' MID$ (Function)

  Public Shared Function Mid(str As String, start As Integer) As String
    Return Microsoft.VisualBasic.Mid(str, start)
  End Function

  Public Shared Function Mid(str As String, start As Integer, length As String) As String
    Return Microsoft.VisualBasic.Mid(str, start, length)
  End Function

  ' MID$ (Statemnt)

  ' ...not something that can easily be added since the VB.NET doesn't support MID$(a$, 7, 5) = "new value"; with that said, here's something that "kind of" translates.

  Public Shared Sub Mid(ByRef str As String, start As Integer, length As String, replacement As String)
    Dim l = If(start - 1 >= 0, Left(str, start - 1), "")
    Dim r = If(start + length - 1 <= Len(str), Right(str, Len(str) - (start + length - 1)), "")
    str = l & Left(replacement, length) & r
  End Sub

  ' MKD$

  Public Shared Function MKD(value As Double) As String

    Dim result(7) As Byte

    Dim hGC = Runtime.InteropServices.GCHandle.Alloc(value, Runtime.InteropServices.GCHandleType.Pinned)
    Try
      Dim ptr = hGC.AddrOfPinnedObject()
      Runtime.InteropServices.Marshal.Copy(ptr, result, 0, 8)
    Finally
      hGC.Free()
    End Try

    Return Text.Encoding.GetEncoding("iso-8859-1").GetString(result)

  End Function

  ' MKDIR

  ' MKDMBF$

  ' MKI$

  ' MKL$

  ' MKS$

  ' MKSMBF$

  ' MOD

  ' ...in VB.NET.

  ' NAME

  ' NOT

  ' ...in VB.NET.

  ' OCT$

  Public Shared Function Oct(num As Object) As String
    Return Microsoft.VisualBasic.Oct(num)
  End Function

  Public Shared Function Oct(num As Byte) As String
    Return Microsoft.VisualBasic.Oct(num)
  End Function

  Public Shared Function Oct(num As SByte) As String
    Return Microsoft.VisualBasic.Oct(num)
  End Function

  Public Shared Function Oct(num As UShort) As String
    Return Microsoft.VisualBasic.Oct(num)
  End Function

  Public Shared Function Oct(num As UInteger) As String
    Return Microsoft.VisualBasic.Oct(num)
  End Function

  Public Shared Function Oct(num As ULong) As String
    Return Microsoft.VisualBasic.Oct(num)
  End Function

  Public Shared Function Oct(num As Short) As String
    Return Microsoft.VisualBasic.Oct(num)
  End Function

  Public Shared Function Oct(num As Integer) As String
    Return Microsoft.VisualBasic.Oct(num)
  End Function

  Public Shared Function Oct(num As Long) As String
    Return Microsoft.VisualBasic.Oct(num)
  End Function

  ' ON COM GOSUB

  ' ...no real way to translate this other than to migrate to "events".  With that said, there might be "something" that can be done to provide similar functionality...

  ' ON ERROR GOTO

  ' ON...GOSUB

  ' ...no real way to translate this other than to change the approach; the closest thing to this is to transition to either IF/THEN/ELSE or SELECT CASE and SUB/FUNCTION.

  ' ON...GOTO

  ' ...no real way to translate this other than to change the approach; the closest thing to this is to transition to either IF/THEN/ELSE or SELECT CASE and SUB/FUNCTION.

  ' ON KEY(n) GOSUB

  ' ...no real way to translate this other than to migrate to "events".  With that said, there might be "something" that can be done to provide similar functionality...

  ' ON PEN GOSUB

  ' ...no real way to translate this other than to migrate to "events".  With that said, there might be "something" that can be done to provide similar functionality...

  ' ON PLAY GOSUB

  ' ...no real way to translate this other than to migrate to "events".  With that said, there might be "something" that can be done to provide similar functionality...

  ' ON STRIG GOSUB

  ' ...no real way to translate this other than to migrate to "events".  With that said, there might be "something" that can be done to provide similar functionality...

  ' ON TIMER GOSUB

  ' ...no real way to translate this other than to migrate to "events".  With that said, there might be "something" that can be done to provide similar functionality...

  ' OPEN

  ' OPEN COM

  ' ...no real way to translate this other than to migrate to "events".  With that said, there might be "something" that can be done to provide similar functionality...

  ' OPTION BASE

  ' ... no real way to translate this to VB.NET.

  ' OR

  ' ...in VB.NET.

  ' OUT

  ' PAINT

  Public Shared Sub Paint([step] As Boolean, x As Integer, y As Integer, Optional color As Integer = -1, Optional border As Integer = -1, Optional background As Integer = -1)
    Throw New NotImplementedException()
  End Sub

  Public Shared Sub Paint(x As Integer, y As Integer, Optional color As Integer = -1, Optional border As Integer = -1, Optional background As Integer = -1)
    Throw New NotImplementedException()
  End Sub

  ' PALETTE, PALETTE USING

  Public Shared Sub Palette(color As Integer, display As Integer)
    Throw New NotImplementedException()
  End Sub

  ' PCOPY

  ' PEEK

  ' PEN

  ' PEN ON/OFF/STOP

  ' PLAY (Function)

  ' PLAY (Statement)

  Public Shared Sub Play(commands As String)
    'TODO: This is a rather complex method to implement... for now, will just "do nothing" as this method will probably end up being the last to be done.
    'Throw New NotImplementedException()
  End Sub

  ' PLAY ON/OFF/STOP

  ' PMAP

  ' POINT

  Public Shared Function Point(x As Integer, y As Integer) As Integer
    Throw New NotImplementedException()
  End Function

  Public Shared Function Point(switch As Integer) As Integer
    Throw New NotImplementedException()
  End Function

  ' POKE

  ' POS

  Public Shared Function Pos(n As Integer)
    ' n is a "dummy" argument; required but never does anything.
    Return Console.CursorLeft + 1 ' 1-40 or 1-80
  End Function

  ' PRESET, PSET

  Public Shared Sub PReset(coord1 As Integer(), Optional attribute As Integer = -1)
    Throw New NotImplementedException()
  End Sub

  Public Shared Sub PReset(x As Integer, y As Integer, Optional attribute As Integer = -1)
    Throw New NotImplementedException()
  End Sub

  Public Shared Sub PSet(coord1 As Integer(), Optional attribute As Integer = -1)
    Throw New NotImplementedException()
  End Sub

  Public Shared Sub PSet(x As Integer, y As Integer, Optional attribute As Integer = -1)
    Throw New NotImplementedException()
  End Sub

  ' PRINT

  Public Shared Sub Print(ParamArray data() As Object)

    Dim index = 0

    Do

      Dim value = data(index)

      Dim peek As String = Nothing
      If data.Length - 1 >= index + 1 Then
        peek = data(index + 1)
      End If

      Dim isSemicolon = False
      Dim isComma = False
      Dim isTab = False

      If peek?.StartsWith(" ") Then
        If peek = SEMICOLON Then
          isSemicolon = True
        ElseIf peek = COMMA Then
          isComma = True
        Else
          ' TAB(col)
          isTab = True
          Dim col = CInt(Right(peek, Len(peek) - 1))
          Locate(, col Mod 80)
        End If
      End If

      If isComma Then
        ' Need to handle "print zones"; each print zone is 14 characters in length).
      End If

      If isTab Then
        ' Don't print anything...
      ElseIf isSemicolon OrElse isComma Then
        Console.Write(value)
      Else
        Console.WriteLine(value)
      End If

      index += 1 ' Move the the next entry...

      If isSemicolon OrElse isComma OrElse isTab Then
        index += 1 ' Move to the next entry (skipping the semicolon or comma).
      End If

      If index > data.Length - 1 Then
        Exit Do
      End If

    Loop

  End Sub

  Public Shared Sub Print(value As String, Optional noCr As Boolean = False)
    'TODO: Add support to TAB(col)...
    If noCr Then
      Console.Write(value)
    Else
      Console.WriteLine(value)
    End If
  End Sub

  ' PRINT #

  ' PRINT # USING

  ' PRINT USING

  Public Shared Sub PrintUsing(template As String, ParamArray a() As String)
    Throw New NotImplementedException()
  End Sub

  ' PUT (File I/O)

  ' PUT (Graphics)

  Public Shared Sub Put(x As Double, y As Double, array As System.Array, Optional action As PutMode = PutMode.XOR)
    Put(CInt(x), CInt(y), array, action)
  End Sub

  Public Shared Sub Put(x As Integer, y As Integer, array As System.Array, Optional action As PutMode = PutMode.XOR)
    Throw New NotImplementedException()
  End Sub

  Public Shared Sub Put(x As Integer, y As Integer, array As System.Array, subscript As Integer, Optional action As PutMode = PutMode.XOR)
    Throw New NotImplementedException()
  End Sub

  ' RANDOMIZE

  ' READ

  Public Shared Sub Read(ByRef var1 As Object)
    If m_readIndex > m_data.Count - 1 Then
      Throw New InvalidOperationException("Out of DATA")
    Else
      Dim value = m_data(m_readIndex)
      m_readIndex += 1
      var1 = value
    End If
  End Sub

  ' REDIM

  ' ...in VB.NET.

  ' REM

  ' ...in VB.NET.

  ' RESTORE

  Public Shared Sub Restore()
    m_readIndex = 0
  End Sub

  Public Shared Sub Restore(index As Integer) ' Not exactly the same, but similar "in spirit".
    m_readIndex = index
  End Sub

  ' RESUME

  ' RIGHT$

  Public Shared Function Right(str As String, length As Integer) As String
    Return Microsoft.VisualBasic.Right(str, length)
  End Function

  ' RMDIR

  ' RND

  ' RSET

  ' RTRIM$

  ' RUN

  ' SCREEN (Function)

  ' SCREEN (Statement)

  Public Shared Sub Screen(Optional mode As Integer = -1, Optional color As Integer = -1, Optional active As Integer = -1, Optional visible As Integer = -1)

    Select Case mode
      Case -1
        ' do nothing...
      Case 0
        ' text mode...
        If m_window IsNot Nothing Then
          m_window.Hide()
          m_window.Dispose()
          m_window = Nothing
        End If
        Console.CursorVisible = False
      Case 1 ' 320x200 (25 rows, 40 columns)
        If m_window Is Nothing Then
          m_window = New System.Windows.Forms.Form With {.Text = Nothing,
                                                         .FormBorderStyle = Windows.Forms.FormBorderStyle.None,
                                                         .Width = 320,
                                                         .Height = 200,
                                                         .BackColor = System.Drawing.Color.FromArgb(255, 0, 0, 0)}
        End If
        m_window.Show()
      Case Else
        Throw New NotImplementedException()
    End Select

  End Sub

  ' SEEK (Function)

  ' SEEK (Statement)

  ' SELECT CASE

  ' ...in VB.NET.

  ' SGN

  ' SHARED

  ' ...Shared keyword is used differently in VB.NET; not something that has an easy translation.

  ' SHELL

  ' SIN

  Public Shared Function Sin(angle As Double) As Double
    Return Math.Sin(angle)
  End Function

  ' SINGLE

  ' ...in VB.NET.

  ' SLEEP

  Public Shared Sub Sleep()

    Do
      If Console.KeyAvailable Then
        Dim junk = Console.ReadKey(True)
        Exit Do
      End If
    Loop

  End Sub

  Public Shared Sub Sleep(time As Integer)

    Dim duration = time * 1000
    Dim elapsed = 0

    Do
      System.Threading.Thread.Sleep(100)
      elapsed += 100
      If elapsed >= duration Then
        Exit Do
      End If
      If Console.KeyAvailable Then
        Dim junk = Console.ReadKey
        Exit Do
      End If
    Loop

  End Sub

  ' SOUND

  ' SPACE$

  Public Shared Function Space(number As Integer) As String
    Return Microsoft.VisualBasic.Space(number)
  End Function

  ' SPC

  Public Shared Sub Spc(col As Integer)
    Console.Write(Space(col))
  End Sub

  ' SQR

  ' STATIC

  ' ...in VB.NET.

  ' $STATIC

  ' ... not something that can easily be translated.

  ' STICK

  ' STOP

  ' ...in VB.NET.

  ' STR$

  Public Shared Function Str(number As Object) As String
    Return Microsoft.VisualBasic.Str(number)
  End Function

  ' STRIG

  ' STRIG ON/OFF/STOP

  ' STRING

  ' ...in VB.NET.

  ' STRING$

  Public Shared Function QBString(num As Integer, code As Integer) As String
    Dim result As String = ""
    For index = 1 To num
      result &= Chr(code)
    Next
    Return result
  End Function

  Public Shared Function QBString(num As Integer, value As String) As String
    Dim result As String = ""
    For index = 1 To num
      result &= value(0)
    Next
    Return result
  End Function

  ' SUB

  ' ...in VB.NET.

  ' SWAP

  Public Shared Sub Swap(ByRef var1 As Object, ByRef var2 As Object)
    Dim temp = var1
    var1 = var2
    var2 = temp
  End Sub

  ' SYSTEM

  ' TAB

  Public Shared Function Tab(col As Integer) As String
    Return $" {col}"
  End Function

  ' TAN

  ' TIME$ (Function and Statement)

  ' TIMER

  ' TIMER ON/OFF/STOP

  ' TROFF, TRON

  ' TYPE...END TYPE

  ' ...Roughly translates to Structure...End Structure.

  ' UBOUND

  Public Shared Function UBound(array As System.Array, Optional rank As Integer = 1) As Integer
    Return Microsoft.VisualBasic.UBound(array, rank)
  End Function

  ' UCASE$

  Public Shared Function UCase(value As Char) As Char
    Return Microsoft.VisualBasic.UCase(value)
  End Function

  Public Shared Function UCase(value As String) As String
    Return Microsoft.VisualBasic.UCase(value)
  End Function

  ' UNLOCK

  ' VAL

  Public Shared Function Val(expression As Char) As Integer
    Return Microsoft.VisualBasic.Val(expression)
  End Function

  Public Shared Function Val(expression As Object) As Integer
    Return Microsoft.VisualBasic.Val(expression)
  End Function

  Public Shared Function Val(expression As String) As Integer
    Return Microsoft.VisualBasic.Val(expression)
  End Function

  ' VARPTR, VARPTR$

  ' VARSEG

  ' VIEW

  ' VIEW PRINT

  Public Shared Sub ViewPrint(top As Integer, bottom As Integer)
    Throw New NotImplementedException()
  End Sub

  ' WAIT

  ' WHILE...WEND

  ' ...in VB.NET as WHILE...END WHILE.

  ' WIDTH (File I/O)

  ' WIDTH (Screen)

  Public Shared Sub Width(Optional cols As Integer = -1, Optional rows As Integer = -1)
    Select Case cols
      Case -1
      Case 40
      Case 80
      Case Else
        Throw New ArgumentOutOfRangeException("Valid cols is 40 or 80.")
    End Select
    Select Case rows
      Case -1
      Case 25
      Case 30
      Case 43
      Case 50
      Case 60
      Case Else
        Throw New ArgumentOutOfRangeException("Valid rows is 25, 30, 43, 50, or 60.")
    End Select
    Cls()
  End Sub

  ' WIDTH LPRINT

  ' WINDOW

  ' WRITE

  Public Shared Sub Write(ParamArray a() As String)
    'TODO: This essentially takes the array and produces a "CSV formatted" output to screen where numeric values are not quoted.
    Throw New NotImplementedException()
  End Sub

  ' WRITE #

  ' XOR

  ' ...in VB.NET.

End Class