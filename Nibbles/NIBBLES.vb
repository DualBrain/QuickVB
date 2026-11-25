' Converted from QBasic by qb2vb.

Imports System.Math
Imports QB
Imports QB.Core
Imports QB.Development
Imports QB.Devices
Imports QB.Events
Imports QB.File
Imports QB.Multimedia
Imports QB.Printer
Imports QB.Video

Module Program

  Structure snakeBody
    Implements IFileRecord
    Public row As Short
    Public col As Short
    Public Sub WriteTo(writer As RecordWriter) Implements IFileRecord.WriteTo
      writer.Write(row)
      writer.Write(col)
    End Sub
    Public Sub ReadFrom(reader As RecordReader) Implements IFileRecord.ReadFrom
      row = reader.ReadInt16()
      col = reader.ReadInt16()
    End Sub
  End Structure
  
  Structure snaketype
    Implements IFileRecord
    Public head As Short
    Public length As Short
    Public row As Short
    Public col As Short
    Public direction As Short
    Public lives As Short
    Public score As Short
    Public scolor As Short
    Public alive As Short
    Public Sub WriteTo(writer As RecordWriter) Implements IFileRecord.WriteTo
      writer.Write(head)
      writer.Write(length)
      writer.Write(row)
      writer.Write(col)
      writer.Write(direction)
      writer.Write(lives)
      writer.Write(score)
      writer.Write(scolor)
      writer.Write(alive)
    End Sub
    Public Sub ReadFrom(reader As RecordReader) Implements IFileRecord.ReadFrom
      head = reader.ReadInt16()
      length = reader.ReadInt16()
      row = reader.ReadInt16()
      col = reader.ReadInt16()
      direction = reader.ReadInt16()
      lives = reader.ReadInt16()
      score = reader.ReadInt16()
      scolor = reader.ReadInt16()
      alive = reader.ReadInt16()
    End Sub
  End Structure
  
  Structure arenaType
    Implements IFileRecord
    Public realRow As Short
    Public acolor As Short
    Public sister As Short
    Public Sub WriteTo(writer As RecordWriter) Implements IFileRecord.WriteTo
      writer.Write(realRow)
      writer.Write(acolor)
      writer.Write(sister)
    End Sub
    Public Sub ReadFrom(reader As RecordReader) Implements IFileRecord.ReadFrom
      realRow = reader.ReadInt16()
      acolor = reader.ReadInt16()
      sister = reader.ReadInt16()
    End Sub
  End Structure
  
  Const [TRUE] As Short = -1
  Const [FALSE] As Short = Not [TRUE]
  Const MAXSNAKELENGTH As Short = 1000
  Const STARTOVER As Short = 1
  Const SAMELEVEL As Short = 2
  Const NEXTLEVEL As Short = 3
  Dim arena(50, 80) As arenaType
  Dim curLevel As Short, colorTable(10) As Short
  Dim NumPlayers As Short
  Dim speed As Short
  Dim diff$ = ""
  Dim monitor$ = ""
  Dim KeyFlags As Short
  Dim a As Short

  Public Sub Main()
    QB.Video.WaitForKeyAtExit = True
#If QB_WINDOW Then
    QB.SdlBackend.Use()
#End If
    DATA_LABEL("mono")
    DATA(15, 7, 7, 0, 15, 0)
    DATA_LABEL("normal")
    DATA(14, 13, 12, 1, 15, 4)
    
    '
    '                         Q B a s i c   N i b b l e s
    '
    '                   Copyright (C) Microsoft Corporation 1990
    '
    ' Nibbles is a game for one or two players.  Navigate your snakes
    ' around the game board trying to eat up numbers while avoiding
    ' running into walls or other snakes.  The more numbers you eat up,
    ' the more points you gain and the longer your snake becomes.
    '
    ' To run this game, press Shift+F5.
    '
    ' To exit QBasic, press Alt, F, X.
    '
    ' To get help on a BASIC keyword, move the cursor to the keyword and press
    ' F1 or click the right mouse button.
    '
    
    'Set default data type to integer for faster game play
    ' DEFINT A-Z
    
    'User-defined TYPEs
    ' TYPE snakeBody (see the Structure at the top)
    
    'This type defines the player's snake
    ' TYPE snaketype (see the Structure at the top)
    
    'This type is used to represent the playing screen in memory
    'It is used to simulate graphics in text mode, and has some interesting,
    'and slightly advanced methods to increasing the speed of operation.
    'Instead of the normal 80x25 text graphics using chr$(219) "█", we will be
    'using chr$(220)"▄" and chr$(223) "▀" and chr$(219) "█" to mimic an 80x50
    'pixel screen.
    'Check out sub-programs SET and POINTISTHERE to see how this is implemented
    'feel free to copy these (as well as arenaType and the DIM ARENA stmt and the
    'initialization code in the DrawScreen subprogram) and use them in your own
    'programs
    ' TYPE arenaType (see the Structure at the top)
    
    'Sub Declarations
    ' DECLARE SUB SpacePause (text$)
    ' DECLARE SUB PrintScore (NumPlayers%, score1%, score2%, lives1%, lives2%)
    ' DECLARE SUB Intro ()
    ' DECLARE SUB GetInputs (NumPlayers, speed, diff$, monitor$)
    ' DECLARE SUB DrawScreen ()
    ' DECLARE SUB PlayNibbles (NumPlayers, speed, diff$)
    ' DECLARE SUB Set (row, col, acolor)
    ' DECLARE SUB Center (row, text$)
    ' DECLARE SUB DoIntro ()
    ' DECLARE SUB Initialize ()
    ' DECLARE SUB SparklePause ()
    ' DECLARE SUB Level (WhatToDO, sammy() AS snaketype)
    ' DECLARE SUB InitColors ()
    ' DECLARE SUB EraseSnake (snake() AS ANY, snakeBod() AS ANY, snakeNum%)
    ' DECLARE FUNCTION StillWantsToPlay ()
    ' DECLARE FUNCTION PointIsThere (row, col, backColor)
    
    'Constants
    ' CONST TRUE = -1
    ' CONST FALSE = NOT TRUE
    ' CONST MAXSNAKELENGTH = 1000
    ' CONST STARTOVER = 1             ' Parameters to 'Level' SUB
    ' CONST SAMELEVEL = 2
    ' CONST NEXTLEVEL = 3
    
    'Global Variables
    ' DIM SHARED arena(1 TO 50, 1 TO 80) AS arenaType
    ' DIM SHARED curLevel, colorTable(10)
    
        RANDOMIZE(CInt(Timer))
        ClearKeyLocks()
        Intro()
        GetInputs(NumPlayers, speed, diff$, monitor$)
        SetColors()
        DrawScreen()
    
        Do
          PlayNibbles(NumPlayers, speed, diff$)
        Loop While StillWantsToPlay() <> 0
    
        RestoreKeyLocks()
        COLOR(15, 0)
        CLS()
    End
    
    
    
    
               'snake1     snake2   Walls  Background  Dialogs-Fore  Back
    ' mono:   DATA 15,         7,       7,     0,          15,            0   (DATA is listed at the start of the program)
    ' normal: DATA 14,         13,      12,    1,          15,            4   (DATA is listed at the start of the program)
    End
    
  End Sub

  Sub ClearKeyLocks()
      DEF_SEG(0)  ' Turn off CapLock, NumLock and ScrollLock
      KeyFlags = CShort(PEEK(1047))
      POKE(1047, 0)
      DEF_SEG()
      Return
  End Sub
  
  Sub RestoreKeyLocks()
      DEF_SEG(0)  ' Restore CapLock, NumLock and ScrollLock states
      POKE(1047, KeyFlags)
      DEF_SEG()
      Return
  End Sub
  
  Sub SetColors()
      If monitor$ = "M" Then
          RESTORE("mono")
      Else
          RESTORE("normal")
      End If
  
      For a = 1 To 6
          READ(colorTable(a))
      Next a
      Return
  End Sub
  
  
  'Center:
  '  Centers text on given row
  Sub Center(ByRef row As Short, ByRef text$)
      LOCATE(row, CInt(41 - (Len(text$) / 2)))
      PRINT(text$, True)
  End Sub
  
  
  'DrawScreen:
  '  Draws playing field
  Sub DrawScreen()
  
      'initialize screen
      VIEW_PRINT()
      COLOR(colorTable(1), colorTable(4))
      CLS()
  
      'Print title & message
      Center(1, "Nibbles!")
      Center(11, "Initializing Playing Field...")
  
      'Initialize arena array
      For row = 1S To 50S
          For col = 1S To 80S
              arena(row, col).realRow = CShort(Int((row + 1) / 2))
              arena(row, col).sister = CShort(((row Mod 2) * 2) - 1)
          Next col
      Next row
  End Sub
  
  
  'EraseSnake:
  '  Erases snake to facilitate moving through playing field
  Sub EraseSnake(ByRef snake() As snaketype, ByRef snakeBod(,) As snakeBody, ByRef snakeNum As Short)
    Dim tail As Short
  
      For c = 0S To 9S
          For b As Short = snake(snakeNum).length - c To 0 Step -10
              tail = ((snake(snakeNum).head + MAXSNAKELENGTH) - b) Mod MAXSNAKELENGTH
              [Set](snakeBod(tail, snakeNum).row, snakeBod(tail, snakeNum).col, colorTable(4))
          Next b
      Next c
  
  End Sub
  
  
  'GetInputs:
  '  Gets player inputs
  Sub GetInputs(ByRef NumPlayers As Short, ByRef speed As Short, ByRef diff$, ByRef monitor$)
    Dim num$ = ""
    Dim gamespeed$ = ""
  
      COLOR(7, 0)
      CLS()
  
      Do
          LOCATE(5, 47) : PRINT(Space$(34), True)
          LOCATE(5, 20)
          INPUT("How many players (1 or 2)", num$)
      Loop Until (Val(num$) = 1) Or (Val(num$) = 2)
      NumPlayers = CShort(Val(num$))
  
      LOCATE(8, 21) : PRINT("Skill level (1 to 100)")
      LOCATE(9, 22) : PRINT("1   = Novice")
      LOCATE(10, 22) : PRINT("90  = Expert")
      LOCATE(11, 22) : PRINT("100 = Twiddle Fingers")
      LOCATE(12, 15) : PRINT("(Computer speed may affect your skill level)")
      Do
          LOCATE(8, 44) : PRINT(Space$(35), True)
          LOCATE(8, 43)
          INPUT(gamespeed$)
      Loop Until (Val(gamespeed$) >= 1) And (Val(gamespeed$) <= 100)
      speed = CShort(Val(gamespeed$))
  
      speed = CShort(((100 - speed) * 2) + 1)
  
      Do
          LOCATE(15, 56) : PRINT(Space$(25), True)
          LOCATE(15, 15)
          INPUT("Increase game speed during play (Y or N)", diff$)
          diff$ = UCase$(diff$)
      Loop Until (diff$ = "Y") Or (diff$ = "N")
  
      Do
          LOCATE(17, 46) : PRINT(Space$(34), True)
          LOCATE(17, 17)
          INPUT("Monochrome or color monitor (M or C)", monitor$)
          monitor$ = UCase$(monitor$)
      Loop Until (monitor$ = "M") Or (monitor$ = "C")
  
      Dim startTime# = Timer  ' Calculate speed of system
      For i# = 1 To 1000 : EMPTYLOOP() : Next i#  ' and do some compensation
      Dim stopTime# = Timer
      speed = CShort((speed * .5!) / (stopTime# - startTime#))
  
  End Sub
  
  
  'InitColors:
  'Initializes playing field colors
  Sub InitColors()
  
      For row = 1S To 50S
          For col = 1S To 80S
              arena(row, col).acolor = colorTable(4)
          Next col
      Next row
  
      CLS()
  
      'Set (turn on) pixels for screen border
      For col = 1S To 80S
          [Set](3, col, colorTable(3))
          [Set](50, col, colorTable(3))
      Next col
  
      For row = 4S To 49S
          [Set](row, 1, colorTable(3))
          [Set](row, 80, colorTable(3))
      Next row
  
  End Sub
  
  
  'Intro:
  '  Displays game introduction
  Sub Intro()
      SCREEN(0)
      WIDTH(80, 25)
      COLOR(15, 0)
      CLS()
  
      Center(4, "Q B a s i c   N i b b l e s")
      COLOR(7)
      Center(6, "Copyright (C) Microsoft Corporation 1990")
      Center(8, "Nibbles is a game for one or two players.  Navigate your snakes")
      Center(9, "around the game board trying to eat up numbers while avoiding")
      Center(10, "running into walls or other snakes.  The more numbers you eat up,")
      Center(11, "the more points you gain and the longer your snake becomes.")
      Center(13, " Game Controls ")
      Center(15, "  General             Player 1               Player 2    ")
      Center(16, "                        (Up)                   (Up)      ")
      Center(17, ("P - Pause                " + QBChr(24)) + "                      W       ")
      Center(18, ((("                     (Left) " + QBChr(27)) + "   ") + QBChr(26)) + " (Right)   (Left) A   D (Right)  ")
      Center(19, ("                         " + QBChr(25)) + "                      S       ")
      Center(20, "                       (Down)                 (Down)     ")
      Center(24, "Press any key to continue")
  
      PLAY("MBT160O1L8CDEDCDL4ECC")
      SparklePause()
  
  End Sub
  
  
  'Level:
  'Sets game level
  Sub Level(ByRef WhatToDO As Short, ByRef sammy() As snaketype)
    Static i As Short
  
      Select Case (WhatToDO)
  
      Case STARTOVER
          curLevel = 1
      Case NEXTLEVEL
          curLevel = CShort(curLevel + 1)
      End Select
  
      sammy(1).head = 1  'Initialize Snakes
      sammy(1).length = 2
      sammy(1).alive = [TRUE]
      sammy(2).head = 1
      sammy(2).length = 2
      sammy(2).alive = [TRUE]
  
      InitColors()
  
      Select Case curLevel
      Case 1
          sammy(1).row = 25 : sammy(2).row = 25
          sammy(1).col = 50 : sammy(2).col = 30
          sammy(1).direction = 4 : sammy(2).direction = 3
  
  
      Case 2
          For i = 20 To 60
              [Set](25, i, colorTable(3))
          Next i
          sammy(1).row = 7 : sammy(2).row = 43
          sammy(1).col = 60 : sammy(2).col = 20
          sammy(1).direction = 3 : sammy(2).direction = 4
  
      Case 3
          For i = 10 To 40
              [Set](i, 20, colorTable(3))
              [Set](i, 60, colorTable(3))
          Next i
          sammy(1).row = 25 : sammy(2).row = 25
          sammy(1).col = 50 : sammy(2).col = 30
          sammy(1).direction = 1 : sammy(2).direction = 2
  
      Case 4
          For i = 4 To 30
              [Set](i, 20, colorTable(3))
              [Set](CShort(53 - i), 60, colorTable(3))
          Next i
          For i = 2 To 40
              [Set](38, i, colorTable(3))
              [Set](15, CShort(81 - i), colorTable(3))
          Next i
          sammy(1).row = 7 : sammy(2).row = 43
          sammy(1).col = 60 : sammy(2).col = 20
          sammy(1).direction = 3 : sammy(2).direction = 4
  
      Case 5
          For i = 13 To 39
              [Set](i, 21, colorTable(3))
              [Set](i, 59, colorTable(3))
          Next i
          For i = 23 To 57
              [Set](11, i, colorTable(3))
              [Set](41, i, colorTable(3))
          Next i
          sammy(1).row = 25 : sammy(2).row = 25
          sammy(1).col = 50 : sammy(2).col = 30
          sammy(1).direction = 1 : sammy(2).direction = 2
  
      Case 6
          For i = 4 To 49
              If (i > 30) Or (i < 23) Then
                  [Set](i, 10, colorTable(3))
                  [Set](i, 20, colorTable(3))
                  [Set](i, 30, colorTable(3))
                  [Set](i, 40, colorTable(3))
                  [Set](i, 50, colorTable(3))
                  [Set](i, 60, colorTable(3))
                  [Set](i, 70, colorTable(3))
              End If
          Next i
          sammy(1).row = 7 : sammy(2).row = 43
          sammy(1).col = 65 : sammy(2).col = 15
          sammy(1).direction = 2 : sammy(2).direction = 1
  
      Case 7
          For i = 4 To 49 Step 2
              [Set](i, 40, colorTable(3))
          Next i
          sammy(1).row = 7 : sammy(2).row = 43
          sammy(1).col = 65 : sammy(2).col = 15
          sammy(1).direction = 2 : sammy(2).direction = 1
  
      Case 8
          For i = 4 To 40
              [Set](i, 10, colorTable(3))
              [Set](CShort(53 - i), 20, colorTable(3))
              [Set](i, 30, colorTable(3))
              [Set](CShort(53 - i), 40, colorTable(3))
              [Set](i, 50, colorTable(3))
              [Set](CShort(53 - i), 60, colorTable(3))
              [Set](i, 70, colorTable(3))
          Next i
          sammy(1).row = 7 : sammy(2).row = 43
          sammy(1).col = 65 : sammy(2).col = 15
          sammy(1).direction = 2 : sammy(2).direction = 1
  
      Case 9
          For i = 6 To 47
              [Set](i, i, colorTable(3))
              [Set](i, CShort(i + 28), colorTable(3))
          Next i
          sammy(1).row = 40 : sammy(2).row = 15
          sammy(1).col = 75 : sammy(2).col = 5
          sammy(1).direction = 1 : sammy(2).direction = 2
  
      Case Else
          For i = 4 To 49 Step 2
              [Set](i, 10, colorTable(3))
              [Set](CShort(i + 1), 20, colorTable(3))
              [Set](i, 30, colorTable(3))
              [Set](CShort(i + 1), 40, colorTable(3))
              [Set](i, 50, colorTable(3))
              [Set](CShort(i + 1), 60, colorTable(3))
              [Set](i, 70, colorTable(3))
          Next i
          sammy(1).row = 7 : sammy(2).row = 43
          sammy(1).col = 65 : sammy(2).col = 15
          sammy(1).direction = 2 : sammy(2).direction = 1
  
      End Select
  End Sub
  
  
  'PlayNibbles:
  '  Main routine that controls game play
  Sub PlayNibbles(ByRef NumPlayers As Short, ByRef speed As Short, ByRef diff$)
    Dim startRow1 As Short
    Dim startCol1 As Short
    Dim startRow2 As Short
    Dim startCol2 As Short
    Dim number As Short
    Dim nonum As Short
    Dim playerDied As Short
    Dim numberRow As Short
    Dim NumberCol As Short
    Dim sisterRow As Short
    Dim count As Short
    Dim kbd$ = ""
    Dim tail As Short
  
      'Initialize Snakes
      Dim sammyBody(MAXSNAKELENGTH - 1, 2) As snakeBody
      Dim sammy(2) As snaketype
      sammy(1).lives = 5
      sammy(1).score = 0
      sammy(1).scolor = colorTable(1)
      sammy(2).lives = 5
      sammy(2).score = 0
      sammy(2).scolor = colorTable(2)
  
      Level(STARTOVER, sammy)
      startRow1 = sammy(1).row : startCol1 = sammy(1).col
      startRow2 = sammy(2).row : startCol2 = sammy(2).col
  
      Dim curSpeed As Short = speed
  
      'play Nibbles until finished
  
      SpacePause(("     Level" + Str$(curLevel)) + ",  Push Space")
      Dim gameOver As Short = [FALSE]
      Do
          If NumPlayers = 1 Then
              sammy(2).row = 0
          End If
  
          number = 1  'Current number that snakes are trying to run into
          nonum = [TRUE]  'nonum = TRUE if a number is not on the screen
  
          playerDied = [FALSE]
          PrintScore(NumPlayers, sammy(1).score, sammy(2).score, sammy(1).lives, sammy(2).lives)
          PLAY("T160O1>L20CDEDCDL10ECC")
  
          Do
              'Print number if no number exists
              If nonum = [TRUE] Then
                  Do
                      numberRow = CShort(Int((Rnd(1) * 47) + 3))
                      NumberCol = CShort(Int((Rnd(1) * 78) + 2))
                      sisterRow = numberRow + arena(numberRow, NumberCol).sister
                  Loop Until ((Not PointIsThere(numberRow, NumberCol, colorTable(4))) And (Not PointIsThere(sisterRow, NumberCol, colorTable(4)))) <> 0
                  numberRow = arena(numberRow, NumberCol).realRow
                  nonum = [FALSE]
                  COLOR(colorTable(1), colorTable(4))
                  LOCATE(numberRow, NumberCol)
                  PRINT(Right$(Str$(number), 1), True)
                  count = 0
              End If
  
              'Delay game
              For a# = 1 To curSpeed : EMPTYLOOP() : Next a#
  
              'Get keyboard input & Change direction accordingly
              kbd$ = INKEY$()
              Select Case kbd$
                  Case "w", "W" : If sammy(2).direction <> 2 Then sammy(2).direction = 1
                  Case "s", "S" : If sammy(2).direction <> 1 Then sammy(2).direction = 2
                  Case "a", "A" : If sammy(2).direction <> 4 Then sammy(2).direction = 3
                  Case "d", "D" : If sammy(2).direction <> 3 Then sammy(2).direction = 4
                  Case QBChr(0) + "H" : If sammy(1).direction <> 2 Then sammy(1).direction = 1
                  Case QBChr(0) + "P" : If sammy(1).direction <> 1 Then sammy(1).direction = 2
                  Case QBChr(0) + "K" : If sammy(1).direction <> 4 Then sammy(1).direction = 3
                  Case QBChr(0) + "M" : If sammy(1).direction <> 3 Then sammy(1).direction = 4
                  Case "p", "P" : SpacePause(" Game Paused ... Push Space  ")
                  Case Else
              End Select
  
              For a_Int As Short = 1 To NumPlayers
                  'Move Snake
                  Select Case sammy(a_Int).direction
                      Case 1 : sammy(a_Int).row = CShort(sammy(a_Int).row - 1)
                      Case 2 : sammy(a_Int).row = CShort(sammy(a_Int).row + 1)
                      Case 3 : sammy(a_Int).col = CShort(sammy(a_Int).col - 1)
                      Case 4 : sammy(a_Int).col = CShort(sammy(a_Int).col + 1)
                  End Select
  
                  'If snake hits number, respond accordingly
                  If (numberRow = Int((sammy(a_Int).row + 1) / 2)) And (NumberCol = sammy(a_Int).col) Then
                      PLAY("MBO0L16>CCCE")
                      If sammy(a_Int).length < (MAXSNAKELENGTH - 30) Then
                          sammy(a_Int).length = CShort(sammy(a_Int).length + (number * 4))
                      End If
                      sammy(a_Int).score = sammy(a_Int).score + number
                      PrintScore(NumPlayers, sammy(1).score, sammy(2).score, sammy(1).lives, sammy(2).lives)
                      number = CShort(number + 1)
                      If number = 10 Then
                          EraseSnake(sammy, sammyBody, 1)
                          EraseSnake(sammy, sammyBody, 2)
                          LOCATE(numberRow, NumberCol) : PRINT(" ")
                          Level(NEXTLEVEL, sammy)
                          PrintScore(NumPlayers, sammy(1).score, sammy(2).score, sammy(1).lives, sammy(2).lives)
                          SpacePause(("     Level" + Str$(curLevel)) + ",  Push Space")
                          If NumPlayers = 1 Then sammy(2).row = 0
                          number = 1
                          If diff$ = "P" Then speed = CShort(speed - 10) : curSpeed = speed
                      End If
                      nonum = [TRUE]
                      If curSpeed < 1 Then curSpeed = 1
                  End If
              Next a_Int
  
              For a_Int As Short = 1 To NumPlayers
                  'If player runs into any point, or the head of the other snake, it dies.
                  If (PointIsThere(sammy(a_Int).row, sammy(a_Int).col, colorTable(4)) Or CShort(((sammy(1).row = sammy(2).row) And (sammy(1).col = sammy(2).col)))) <> 0 Then
                      PLAY("MBO0L32EFGEFDC")
                      COLOR(-1, colorTable(4))
                      LOCATE(numberRow, NumberCol)
                      PRINT(" ")
  
                      playerDied = [TRUE]
                      sammy(a_Int).alive = [FALSE]
                      sammy(a_Int).lives = CShort(sammy(a_Int).lives - 1)
  
                  'Otherwise, move the snake, and erase the tail
                  Else
                      sammy(a_Int).head = CShort((sammy(a_Int).head + 1) Mod MAXSNAKELENGTH)
                      sammyBody(sammy(a_Int).head, a_Int).row = sammy(a_Int).row
                      sammyBody(sammy(a_Int).head, a_Int).col = sammy(a_Int).col
                      tail = ((sammy(a_Int).head + MAXSNAKELENGTH) - sammy(a_Int).length) Mod MAXSNAKELENGTH
                      [Set](sammyBody(tail, a_Int).row, sammyBody(tail, a_Int).col, colorTable(4))
                      sammyBody(tail, a_Int).row = 0
                      [Set](sammy(a_Int).row, sammy(a_Int).col, sammy(a_Int).scolor)
                  End If
              Next a_Int
  
          Loop Until playerDied <> 0
  
          curSpeed = speed  ' reset speed to initial value
  
          For a_Int As Short = 1 To NumPlayers
              EraseSnake(sammy, sammyBody, a_Int)
  
              'If dead, then erase snake in really cool way
              If sammy(a_Int).alive = [FALSE] Then
                  'Update score
                  sammy(a_Int).score = CShort(sammy(a_Int).score - 10)
                  PrintScore(NumPlayers, sammy(1).score, sammy(2).score, sammy(1).lives, sammy(2).lives)
  
                  If a_Int = 1 Then
                      SpacePause(" Sammy Dies! Push Space! --->")
                  Else
                      SpacePause(" <---- Jake Dies! Push Space ")
                  End If
              End If
          Next a_Int
  
          Level(SAMELEVEL, sammy)
          PrintScore(NumPlayers, sammy(1).score, sammy(2).score, sammy(1).lives, sammy(2).lives)
  
      'Play next round, until either of snake's lives have run out.
      Loop Until (sammy(1).lives = 0) Or (sammy(2).lives = 0)
  
  End Sub
  
  
  'PointIsThere:
  '  Checks the global  arena array to see if the boolean flag is set
  Function PointIsThere(ByRef row As Short, ByRef col As Short, ByRef acolor As Short) As Short
      If row <> 0 Then
          If arena(row, col).acolor <> acolor Then
              PointIsThere = [TRUE]
          Else
              PointIsThere = [FALSE]
          End If
      End If
  End Function
  
  
  'PrintScore:
  '  Prints players scores and number of lives remaining
  Sub PrintScore(ByRef NumPlayers As Short, ByRef score1 As Short, ByRef score2 As Short, ByRef lives1 As Short, ByRef lives2 As Short)
      COLOR(15, colorTable(4))
  
      If NumPlayers = 2 Then
          LOCATE(1, 1)
          PRINT_USING("#,###,#00  Lives: #  <--JAKE", score2, lives2)
      End If
  
      LOCATE(1, 49)
      PRINT_USING("SAMMY-->  Lives: #     #,###,#00", lives1, score1)
  End Sub
  
  
  'Set:
  '  Sets row and column on playing field to given color to facilitate moving
  '  of snakes around the field.
  Sub [Set](ByRef row As Short, ByRef col As Short, ByRef acolor As Short)
    Dim realRow As Short
    Dim topFlag As Short
    Dim sisterRow As Short
    Dim sisterColor As Short
      If row <> 0 Then
          arena(row, col).acolor = acolor  'assign color to arena
          realRow = arena(row, col).realRow  'Get real row of pixel
          topFlag = CShort(arena(row, col).sister + (1 / 2))  'Deduce whether pixel
                                                      'is on top▀, or bottom▄
          sisterRow = row + arena(row, col).sister  'Get arena row of sister
          sisterColor = arena(sisterRow, col).acolor  'Determine sister's color
  
          LOCATE(realRow, col)
  
          If acolor = sisterColor Then  'If both points are same
              COLOR(acolor, acolor)  'Print chr$(219) "█"
              PRINT(QBChr(219), True)
          Else
              If topFlag <> 0 Then  'Since you cannot have
                  If acolor > 7 Then  'bright backgrounds
                      COLOR(acolor, sisterColor)  'determine best combo
                      PRINT(QBChr(223), True)  'to use.
                  Else
                      COLOR(sisterColor, acolor)
                      PRINT(QBChr(220), True)
                  End If
              Else
                  If acolor > 7 Then
                      COLOR(acolor, sisterColor)
                      PRINT(QBChr(220), True)
                  Else
                      COLOR(sisterColor, acolor)
                      PRINT(QBChr(223), True)
                  End If
              End If
          End If
      End If
  End Sub
  
  
  'SpacePause:
  '  Pauses game play and waits for space bar to be pressed before continuing
  Sub SpacePause(ByRef text$)
  
      COLOR(colorTable(5), colorTable(6))
      Center(11, "█▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀█")
      Center(12, ("█ " + Left$(text$ + Space$(29), 29)) + " █")
      Center(13, "█▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄█")
      While INKEY$() <> "" : End While
      While INKEY$() <> " " : End While
      COLOR(15, colorTable(4))
  
      For i = 21S To 26S  ' Restore the screen background
          For j = 24S To 56S
              [Set](i, j, arena(i, j).acolor)
          Next j
      Next i
  
  End Sub
  
  
  'SparklePause:
  '  Creates flashing border for intro screen
  Sub SparklePause()
    Dim a_Int As Short
    Dim c As Short
  
      COLOR(4, 0)
      Dim a$ = "*    *    *    *    *    *    *    *    *    *    *    *    *    *    *    *    *    "
      While INKEY$() <> "" : End While  'Clear keyboard buffer
  
      While INKEY$() = ""
          For a_Int = 1 To 5
              LOCATE(1, 1)  'print horizontal sparkles
              PRINT(Mid$(a$, a_Int, 80), True)
              LOCATE(22, 1)
              PRINT(Mid$(a$, 6 - a_Int, 80), True)
  
              For b = 2S To 21S  'Print Vertical sparkles
                  c = CShort((a_Int + b) Mod 5)
                  If c = 1 Then
                      LOCATE(b, 80)
                      PRINT("*", True)
                      LOCATE(23 - b, 1)
                      PRINT("*", True)
                  Else
                      LOCATE(b, 80)
                      PRINT(" ", True)
                      LOCATE(23 - b, 1)
                      PRINT(" ", True)
                  End If
              Next b
          Next a_Int
      End While
  
  End Sub
  
  
  'StillWantsToPlay:
  '  Determines if users want to play game again.
  Function StillWantsToPlay() As Short
    Dim kbd$ = ""
  
      COLOR(colorTable(5), colorTable(6))
      Center(10, "█▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀█")
      Center(11, "█       G A M E   O V E R       █")
      Center(12, "█                               █")
      Center(13, "█      Play Again?   (Y/N)      █")
      Center(14, "█▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄█")
  
      While INKEY$() <> "" : End While
      Do
          kbd$ = UCase$(INKEY$())
      Loop Until (kbd$ = "Y") Or (kbd$ = "N")
  
      COLOR(15, colorTable(4))
      Center(10, "                                 ")
      Center(11, "                                 ")
      Center(12, "                                 ")
      Center(13, "                                 ")
      Center(14, "                                 ")
  
      If kbd$ = "Y" Then
          StillWantsToPlay = [TRUE]
      Else
          StillWantsToPlay = [FALSE]
          COLOR(7, 0)
          CLS()
      End If
  
  End Function
  
End Module
