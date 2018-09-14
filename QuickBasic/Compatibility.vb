Public Class Compatibility

  Public Const SEMICOLON As String = ";"
  Public Const COMMA As String = ","

  Private Shared m_screenMode As Integer = 0
  Private Shared m_window As System.Windows.Forms.Form

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

  ' AND

  ' ...in VB.NET.

  ' ASC

  ' ...in VB.NET.

  ' ATN

  Public Shared Function Atn(value As Double) As Double
    Return Math.Atan(value)
  End Function

  ' BEEP

  Public Shared Sub Beep()
    Console.Beep()
  End Sub

  ' BLOAD

  ' BSAVE

  ' CALL

  ' ...in VB.NET.

  ' CDBL

  ' ...in VB.NET.

  ' CHAIN

  ' ...not something that can be translated easily.

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

  Public Shared Function Chr(val As Integer) As String
    Return Microsoft.VisualBasic.Chr(val)
  End Function

  ' CINT

  ' ... in VB.NET.

  ' CIRCLE

  ' CLEAR

  ' CLNG

  ' ... in VB.NET.

  ' CLOSE

  ' CLS

  Public Shared Sub Cls(Optional viewport As Integer? = -1)
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
      ' Windows console doesn't support the concept of "border".
    End If

  End Sub

  ' COMMON

  ' ...not in VB.NET; see CHAIN.

  ' COM ON/OFF/STOP

  ' CONST

  ' ...in VB.NET.

  ' COS

  ' CSNG

  ' ...in VB.NET.

  ' CSRLIN

  Public Shared Function CsrLin() As Integer
    Return Console.CursorTop + 1 ' Range of 1-25 (CGA) , or 1-43 (EGA) or 1-60 (VGA/MCGA)...
  End Function

  ' CVD

  ' CVDMBF

  ' CVI

  ' CVL

  ' CVS

  ' CVSMBF

  ' DATA

  Private Shared m_data As New List(Of Object)
  Private Shared m_readIndex As Integer = 0

  Public Sub Data(ParamArray values As Object())
    For Each value In values
      m_data.Add(value)
    Next
  End Sub

  ' DATE$

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

  ' $DYNAMIC

  ' ... not something that can be translated easily.

  ' END

  ' ...in VB.NET.

  ' ENVIRON

  ' ENVIRON$

  ' EOF

  ' EQV

  ' ...roughly translates to value AndAlso value (alternatively Not value AndAlso Not value) when used as a logical operator; as a mathimatical operator, no easily translation available.

  ' ERASE

  Public Sub [Erase](ParamArray arrays As Object())
    ' With a static array, the ERASE statement merely resets all elements to their default value -- 0 if numeric or null strings ("").  ERASE does not change the dimensions of the static array defined in the DIM statement nore release any memory.
    ' With a dynamic array, however, ERASE deallocates the memory used by the array, thereby freeing the memory for other dynamic arrays.  Before you can reuse an erased array, you must dimension it agin with DIM.  (QBasic offers the REDIM statement as an alternative to erasing and redimensioning a dynamic array.  REDIM deallocates, redimensions, and reinitializes a dynamic array in one step.)
    Throw New NotImplementedException()
  End Sub

  ' ERDEV

  ' ERDEV$

  ' ERL

  ' ERR

  ' ERROR

  ' EXIT

  ' ...translates to Exit For, Exit Do, etc.

  ' EXP

  ' FIELD

  ' FILEATTR

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

  ' FOR...NEXT

  ' ...in VB.NET.

  ' FRE

  ' FREEFILE

  ' FUNCTION

  ' ...in VB.NET.

  ' GET (File I/O)

  ' GET (Graphics)

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
    Return ""
  End Function

  ' INP

  ' INPUT

  Public Shared Sub Input(noCr As Boolean, prompt As String, noNewLine As Boolean, noQuestion As Boolean, ByRef a As String())

  End Sub

  ' INPUT #

  ' INPUT$ (File I/O)

  ' INPUT$ (Keyboard)

  Public Shared Function Input(chars As Integer) As String
    ' The INPUT$ (keyboard) function reads a specified number of characters from the keyboard.  
    ' Usually INPUT$ is used to read from a file; however if no file number is specified, INPUT$ 
    ' reads input from the standard input device, which, by default, is the keyboard.
    Return ""
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

  ' LINE INPUT

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

  ' PALETTE, PALETTE USING

  ' PCOPY

  ' PEEK

  ' PEN

  ' PEN ON/OFF/STOP

  ' PLAY (Function)

  ' PLAY (Statement)

  ' PLAY ON/OFF/STOP

  ' PMAP

  ' POINT

  ' POKE

  ' POS

  Public Shared Function Pos(n As Integer)
    ' n is a "dummy" argument; required but never does anything.
    Return Console.CursorLeft + 1 ' 1-40 or 1-80
  End Function

  ' PRESET, PSET

  ' PRINT

  Public Shared Sub Print(ParamArray data() As String)

    Dim index = 0

    Do

      Dim value = data(index)

      Dim peek As String = Nothing
      If data.Length - 1 >= index + 1 Then
        peek = data(index + 1)
      End If

      Dim isSemicolon = False
      Dim isComma = False

      If peek = SEMICOLON Then
        isSemicolon = True
      ElseIf peek = COMMA Then
        isComma = True
      End If

      If isComma Then
        ' Need to handle "print zones"; each print zone is 14 characters in length).
      End If

      If isSemicolon OrElse isComma Then
        Console.Write(value)
      Else
        Console.WriteLine(value)
      End If

      index += 1 ' Move the the next entry...

      If isSemicolon OrElse isComma Then
        index += 1 ' Move to the next entry (skipping the semicolon or comma).
      End If

      If index > data.Length - 1 Then
        Exit Do
      End If

    Loop

  End Sub

  Public Shared Sub Print(value As String, Optional noCr As Boolean = False)
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

  ' SINGLE

  ' ...in VB.NET.

  ' SLEEP

  Public Shared Sub Sleep()

    Do
      If Console.KeyAvailable Then
        Dim junk = Console.ReadKey
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

  Public Shared Sub Tab(col As Integer)
    Throw New NotImplementedException()
  End Sub

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