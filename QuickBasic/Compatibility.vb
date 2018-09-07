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

  ' CDBL

  ' ... in VB.NET.

  ' CHAIN

  ' CHDIR

  ' CHR$

  ' ... in VB.NET.

  ' CINT

  ' ... in VB.NET.

  ' CIRCLE

  ' CLEAR

  ' CLNG

  ' ... in VB.NET.

  ' CLOSE

  ' CLS

  Public Shared Sub Cls()
    Console.Clear()
  End Sub

  Public Shared Sub Cls(viewport As Integer)
    Select Case viewport
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
        Throw New ArgumentOutOfRangeException("CLS viewport must be 0, 1, or 2.")
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

  ' DATE$

  ' DECLARE 

  ' DEF FN

  ' DEF SEG

  ' DEF type

  ' DIM

  ' ...in VB.NET.

  ' DO...LOOP

  ' ...in VB.NET.

  ' DOUBLE

  ' ...in VB.NET.

  ' DRAW

  ' $DYNAMIC

  ' END

  ' ...in VB.NET.

  ' ENVIRON

  ' ENVIRON$

  ' EOF

  ' EQV

  ' ERASE

  ' ERDEV

  ' ERDEV$

  ' ERL

  ' ERR

  ' ERROR

  ' EXIT

  ' EXP

  ' FIELD

  ' FILEATTR

  ' FILES

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

  ' IF

  ' ...in VB.NET.

  ' IMP

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

  ' LCASE$

  ' LEFT$

  ' LEN

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

  ' MID$ (Statemnt)

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

  ' ON COM GOSUB

  ' ON ERROR GOTO

  ' ON...GOSUB

  ' ON...GOTO

  ' ON KEY(n) GOSUB

  ' ON PEN GOSUB

  ' ON PLAY GOSUB

  ' ON STRIG GOSUB

  ' ON TIMER GOSUB

  ' OPEN

  ' OPEN COM

  ' OPTION BASE

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

  ' REDIM

  ' ...in VB.NET.

  ' REM

  ' ...in VB.NET.

  ' RESTORE

  ' RESUME

  ' RIGHT$

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
          m_window = New System.Windows.Forms.Form()
          m_window.Text = Nothing
          m_window.FormBorderStyle = Windows.Forms.FormBorderStyle.None
          m_window.Width = 320
          m_window.Height = 200
          m_window.BackColor = System.Drawing.Color.FromArgb(255, 0, 0, 0)
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

  ' SPC

  Public Shared Sub Spc(col As Integer)
    Console.Write(Space(col))
  End Sub

  ' SQR

  ' STATIC

  ' $STATIC

  ' STICK

  ' STOP

  ' ...in VB.NET.

  ' STR$

  ' STRIG

  ' STRIG ON/OFF/STOP

  ' STRING

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

  ' UBOUND

  ' UCASE$

  ' UNLOCK

  ' VAL

  ' VARPTR, VARPTR$

  ' VARSEG

  ' VIEW

  ' VIEW PRINT

  Public Shared Sub ViewPrint(top As Integer, bottom As Integer)
    Throw New NotImplementedException()
  End Sub

  ' WAIT

  ' WHILE...WEND

  ' ...in VB.NET.

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

End Class