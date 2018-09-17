'                         Q B a s i c   G o r i l l a s
'
'                   Copyright (C) Microsoft Corporation 1990
'                     (Converted to VB.NET by Cory Smith)
'
' Your mission is to hit your opponent with the exploding banana
' by varying the angle and power of your throw, taking into account
' wind speed, gravity, and the city skyline.
'
' Speed of this game is determined by the constant SPEEDCONST.  If the
' program is too slow or too fast adjust the "CONST SPEEDCONST = 500" line
' below.  The larger the number the faster the game will go.
'
' To run this game, press Shift+F5.
'
' To exit QBasic, press Alt, F, X.
'
' To get help on a BASIC keyword, move the cursor to the keyword and press
' F1 or click the right mouse button.
'

' 'Set default data type to integer for faster game play
' DEFINT A-Z

' 'Sub Declarations
' DECLARE SUB DoSun (Mouth)
' DECLARE SUB SetScreen ()
' DECLARE SUB EndGame ()
' DECLARE SUB Center (Row, Text$)
' DECLARE SUB Intro ()
' DECLARE SUB SparklePause ()
' DECLARE SUB GetInputs (Player1$, Player2$, NumGames)
' DECLARE SUB PlayGame (Player1$, Player2$, NumGames)
' DECLARE SUB DoExplosion (x#, y#)
' DECLARE SUB MakeCityScape (BCoor() AS ANY)
' DECLARE SUB PlaceGorillas (BCoor() AS ANY)
' DECLARE SUB UpdateScores (Record(), PlayerNum, Results)
' DECLARE SUB DrawGorilla (x, y, arms)
' DECLARE SUB GorillaIntro (Player1$, Player2$)
' DECLARE SUB Rest (t#)
' DECLARE SUB VictoryDance (Player)
' DECLARE SUB ClearGorillas ()
' DECLARE SUB DrawBan (xc#, yc#, r, bc)
' DECLARE FUNCTION Scl (n!)
' DECLARE FUNCTION GetNum# (Row, Col)
' DECLARE FUNCTION DoShot (PlayerNum, x, y)
' DECLARE FUNCTION ExplodeGorilla (x#, y#)
' DECLARE FUNCTION Getn# (Row, Col)
' DECLARE FUNCTION PlotShot (StartX, StartY, Angle#, Velocity, PlayerNum)
' DECLARE FUNCTION CalcDelay! ()

Module Gorilla

  'Make all arrays Dynamic
  '$DYNAMIC

  ' 'User-Defined TYPEs
  ' TYPE XYPoint
  '   XCoor AS INTEGER
  '   YCoor AS INTEGER
  ' END TYPE

  Structure XYPoint
    Public XCoor As Integer
    Public YCoor As Integer
  End Structure

  'Constants
  Const SPEEDCONST As Integer = 500
  'CONST TRUE = -1
  'CONST FALSE = NOT TRUE
  Const HITSELF As Boolean = True 'Integer = 1
  Const BACKATTR As Integer = 0
  Const OBJECTCOLOR As Integer = 1
  Const WINDOWCOLOR As Integer = 14
  Const SUNATTR As Integer = 3
  Const SUNHAPPY As Boolean = False
  Const SUNSHOCK As Boolean = True
  Const RIGHTUP As Integer = 1
  Const LEFTUP As Integer = 2
  Const ARMSDOWN As Integer = 3

  'Global Variables
  Dim GorillaX(2) '(1 To 2)  'Location of the two gorillas
  Dim GorillaY(2) '(1 To 2)
  Dim LastBuilding As Integer

  Dim pi#
  'Dim LBan&(x), RBan&(x), UBan&(x), DBan&(x) 'Graphical picture of banana
  Dim LBan&(0), RBan&(0), UBan&(0), DBan&(0) 'Graphical picture of banana
  Dim GorD&(120)        'Graphical picture of Gorilla arms down
  Dim GorL&(120)        'Gorilla left arm raised
  Dim GorR&(120)        'Gorilla right arm raised

  Dim gravity#
  Dim Wind As Integer

  'Screen Mode Variables
  Dim ScrHeight As Integer
  Dim ScrWidth As Integer
  Dim Mode As Integer
  Dim MaxCol As Integer

  'Screen Color Variables
  Dim ExplosionColor As Integer
  Dim SunColor As Integer
  Dim BackColor As Integer
  Dim SunHit As Integer

  Dim SunHt As Integer
  Dim GHeight As Integer
  Dim MachSpeed As Single

  'DEF FnRan(x) = INT(RND(1) * x) + 1
  Function FnRan(x As Integer) As Integer
    Return Int(Rnd(1) * x) + 1
  End Function

  Sub Main()

    'DEF SEG = 0                         ' Set NumLock to ON
    'KeyFlags = PEEK(1047)
    'If (KeyFlags And 32) = 0 Then
    '  POKE 1047, KeyFlags Or 32
    'End If
    'DEF SEG

    'GOSUB InitVars

    'InitVars:
    pi# = 4 * Atn(1.0#)

    'This is a clever way to pick the best graphics mode available
    On Error GoTo ScreenModeError
    Mode = 9
    Screen(Mode)
    On Error GoTo PaletteError
    If Mode = 9 Then Palette(4, 0)   'Check for 64K EGA
    On Error GoTo 0

    MachSpeed = CalcDelay()

    If Mode = 9 Then
      ScrWidth = 640
      ScrHeight = 350
      GHeight = 25
      'RESTORE(EGABanana)
      Call RestoreEgaBanana()
      ReDim LBan&(8), RBan&(8), UBan&(8), DBan&(8)

      For i = 0 To 8
        Read(LBan&(i))
      Next i

      For i = 0 To 8
        Read(DBan&(i))
      Next i

      For i = 0 To 8
        Read(UBan&(i))
      Next i

      For i = 0 To 8
        Read(RBan&(i))
      Next i

      SunHt = 39

    Else

      ScrWidth = 320
      ScrHeight = 200
      GHeight = 12
      'RESTORE(CGABanana)
      Call RestoreCgaBanana()
      ReDim LBan&(2), RBan&(2), UBan&(2), DBan&(2)
      ReDim GorL&(20), GorD&(20), GorR&(20)

      For i = 0 To 2
        Read(LBan&(i))
      Next i
      For i = 0 To 2
        Read(DBan&(i))
      Next i
      For i = 0 To 2
        Read(UBan&(i))
      Next i
      For i = 0 To 2
        Read(RBan&(i))
      Next i

      MachSpeed = MachSpeed * 1.3
      SunHt = 20

    End If

    Intro()
    GetInputs(Name1$, Name2$, NumGames)
    GorillaIntro(Name1$, Name2$)
    PlayGame(Name1$, Name2$, NumGames)

    'DEF SEG = 0                         ' Restore NumLock state
    'POKE 1047, KeyFlags
    'DEF SEG

    End

    'CGABanana:
    '    'BananaLeft
    '    DATA(327686, -252645316, 60)
    '    'BananaDown
    '    DATA(196618, -1057030081, 49344)
    '    'BananaUp
    '    DATA(196618, -1056980800, 63)
    '    'BananaRight
    '    DATA(327686, 1010580720, 240)

    'EGABanana:
    '    'BananaLeft
    '    DATA(458758, 202116096, 471604224, 943208448, 943208448, 943208448, 471604224, 202116096, 0)
    '    'BananaDown
    '    DATA(262153, -2134835200, -2134802239, -2130771968, -2130738945, 8323072, 8323199, 4063232, 4063294)
    '    'BananaUp
    '    DATA(262153, 4063232, 4063294, 8323072, 8323199, -2130771968, -2130738945, -2134835200, -2134802239)
    '    'BananaRight
    '    DATA(458758, -1061109760, -522133504, 1886416896, 1886416896, 1886416896, -522133504, -1061109760, 0)

    'InitVars:
    '    pi# = 4 * ATN(1.0#)

    '    'This is a clever way to pick the best graphics mode available
    '    On Error GoTo ScreenModeError
    '    Mode = 9
    '    SCREEN(Mode)
    '    On Error GoTo PaletteError
    '    If Mode = 9 Then PALETTE(4, 0)   'Check for 64K EGA
    '    On Error GoTo 0

    '    MachSpeed = CalcDelay()

    '    If Mode = 9 Then
    '      ScrWidth = 640
    '      ScrHeight = 350
    '      GHeight = 25
    '      RESTORE(EGABanana)
    '      ReDim LBan&(8), RBan&(8), UBan&(8), DBan&(8)

    '      For i = 0 To 8
    '        READ(LBan&(i))
    '      Next i

    '      For i = 0 To 8
    '        READ(DBan&(i))
    '      Next i

    '      For i = 0 To 8
    '        READ(UBan&(i))
    '      Next i

    '      For i = 0 To 8
    '        READ(RBan&(i))
    '      Next i

    '      SunHt = 39

    '    Else

    '      ScrWidth = 320
    '      ScrHeight = 200
    '      GHeight = 12
    '      RESTORE(CGABanana)
    '      ReDim LBan&(2), RBan&(2), UBan&(2), DBan&(2)
    '      ReDim GorL&(20), GorD&(20), GorR&(20)

    '      For i = 0 To 2
    '        READ(LBan&(i))
    '      Next i
    '      For i = 0 To 2
    '        READ(DBan&(i))
    '      Next i
    '      For i = 0 To 2
    '        READ(UBan&(i))
    '      Next i
    '      For i = 0 To 2
    '        READ(RBan&(i))
    '      Next i

    '      MachSpeed = MachSpeed * 1.3
    '      SunHt = 20

    '    End If

    '    Return

ScreenModeError:
    If Mode = 1 Then
      Cls()
      Locate(10, 5)
      Print("Sorry, you must have CGA, EGA color, or VGA graphics to play GORILLA.BAS")
      End
    Else
      Mode = 1
      Resume
    End If

PaletteError:
    Mode = 1            '64K EGA cards will run in CGA mode.
    Resume Next

  End Sub

  Sub RestoreCgaBanana()
    Restore()
    'BananaLeft
    Data(327686, -252645316, 60)
    'BananaDown
    Data(196618, -1057030081, 49344)
    'BananaUp
    Data(196618, -1056980800, 63)
    'BananaRight
    Data(327686, 1010580720, 240)
  End Sub

  Sub RestoreEgaBanana()
    Restore()
    'BananaLeft
    Data(458758, 202116096, 471604224, 943208448, 943208448, 943208448, 471604224, 202116096, 0)
    'BananaDown
    Data(262153, -2134835200, -2134802239, -2130771968, -2130738945, 8323072, 8323199, 4063232, 4063294)
    'BananaUp
    Data(262153, 4063232, 4063294, 8323072, 8323199, -2130771968, -2130738945, -2134835200, -2134802239)
    'BananaRight
    Data(458758, -1061109760, -522133504, 1886416896, 1886416896, 1886416896, -522133504, -1061109760, 0)
  End Sub

  'REM $STATIC

  'CalcDelay:
  '  Checks speed of the machine.
  Function CalcDelay!()

    s! = Timer
    Do
      i! = i! + 1
    Loop Until Timer - s! >= 0.5
    CalcDelay! = i!

  End Function

  ' Center:
  '   Centers and prints a text string on a given row
  ' Parameters:
  '   Row - screen row number
  '   Text$ - text to be printed
  '
  Sub Center(Row As Integer, Text$)
    Col = MaxCol \ 2
    LOCATE(Row, Col - (Len(Text$) / 2 + 0.5))
    Print(Text$, SEMICOLON) ';
  End Sub

  ' DoExplosion:
  '   Produces explosion when a shot is fired
  ' Parameters:
  '   X#, Y# - location of explosion
  '
  Sub DoExplosion(x#, y#)

    PLAY("MBO0L32EFGEFDC")
    Radius = ScrHeight / 50
    If Mode = 9 Then Inc# = 0.5 Else Inc# = 0.41
    For c# = 0 To Radius Step Inc#
      CIRCLE(x#, y#, c#, ExplosionColor)
    Next c#
    For c# = Radius To 0 Step (-1 * Inc#)
      CIRCLE(x#, y#, c#, BACKATTR)
      For i = 1 To 100
      Next i
      Rest(0.005)
    Next c#
  End Sub

  ' DoShot:
  '   Controls banana shots by accepting player input and plotting
  '   shot angle
  ' Parameters:
  '   PlayerNum - Player
  '   x, y - Player's gorilla position
  '
  Function DoShot(PlayerNum As Integer, x As Integer, y As Integer) As Boolean

    'Input shot
    If PlayerNum = 1 Then
      LocateCol = 1
    Else
      If Mode = 9 Then
        LocateCol = 66
      Else
        LocateCol = 26
      End If
    End If

    LOCATE(2, LocateCol)
    Print("Angle:", SEMICOLON) ';
    Angle# = GetNum#(2, LocateCol + 7)

    LOCATE(3, LocateCol)
    Print("Velocity:", SEMICOLON) ';
    Velocity = GetNum#(3, LocateCol + 10)

    If PlayerNum = 2 Then
      Angle# = 180 - Angle#
    End If

    'Erase input
    For i = 1 To 4
      LOCATE(i, 1)
      Print(Space(30 \ (80 \ MaxCol)), SEMICOLON) ';
      Locate(i, (50 \ (80 \ MaxCol)))
      Print(Space(30 \ (80 \ MaxCol)), SEMICOLON) ';
    Next

    SunHit = False
    PlayerHit = PlotShot(x, y, Angle#, Velocity, PlayerNum)
    If PlayerHit = 0 Then
      DoShot = False
    Else
      DoShot = True
      If PlayerHit = PlayerNum Then PlayerNum = 3 - PlayerNum
      VictoryDance(PlayerNum)
    End If

  End Function

  ' DoSun:
  '   Draws the sun at the top of the screen.
  ' Parameters:
  '   Mouth - If TRUE draws "O" mouth else draws a smile mouth.
  '
  Sub DoSun(Mouth As Integer)

    'set position of sun
    x = ScrWidth \ 2 : y = Scl(25)

    'clear old sun
    LINE(x - Scl(22), y - Scl(18), x + Scl(22), y + Scl(18), BACKATTR, LineMode.BF)

    'draw new sun:
    'body
    CIRCLE(x, y, Scl(12), SUNATTR)
    PAINT(x, y, SUNATTR)

    'rays
    LINE(x - Scl(20), y, x + Scl(20), y, SUNATTR)
    LINE(x, y - Scl(15), x, y + Scl(15), SUNATTR)

    LINE(x - Scl(15), y - Scl(10), x + Scl(15), y + Scl(10), SUNATTR)
    LINE(x - Scl(15), y + Scl(10), x + Scl(15), y - Scl(10), SUNATTR)

    LINE(x - Scl(8), y - Scl(13), x + Scl(8), y + Scl(13), SUNATTR)
    LINE(x - Scl(8), y + Scl(13), x + Scl(8), y - Scl(13), SUNATTR)

    LINE(x - Scl(18), y - Scl(5), x + Scl(18), y + Scl(5), SUNATTR)
    LINE(x - Scl(18), y + Scl(5), x + Scl(18), y - Scl(5), SUNATTR)

    'mouth
    If Mouth Then  'draw "o" mouth
      CIRCLE(x, y + Scl(5), Scl(2.9), 0)
      PAINT(x, y + Scl(5), 0, 0)
    Else           'draw smile
      CIRCLE(x, y, Scl(8), 0, (210 * pi# / 180), (330 * pi# / 180))
    End If

    'eyes
    CIRCLE(x - 3, y - 2, 1, 0)
    CIRCLE(x + 3, y - 2, 1, 0)
    PSET(x - 3, y - 2, 0)
    PSET(x + 3, y - 2, 0)

  End Sub

  'DrawBan:
  '  Draws the banana
  'Parameters:
  '  xc# - Horizontal Coordinate
  '  yc# - Vertical Coordinate
  '  r - rotation position (0-3). (  \_/  ) /-\
  '  bc - if TRUE then DrawBan draws the banana ELSE it erases the banana
  Sub DrawBan(xc#, yc#, r As Integer, bc As Integer)

    Select Case r
      Case 0
        If bc Then PUT(xc#, yc#, LBan&, PutMode.PSET) Else PUT(xc#, yc#, LBan&, PutMode.Xor)
      Case 1
        If bc Then PUT(xc#, yc#, UBan&, PutMode.PSET) Else PUT(xc#, yc#, UBan&, PutMode.Xor)
      Case 2
        If bc Then PUT(xc#, yc#, DBan&, PutMode.PSET) Else PUT(xc#, yc#, DBan&, PutMode.Xor)
      Case 3
        If bc Then PUT(xc#, yc#, RBan&, PutMode.PSET) Else PUT(xc#, yc#, RBan&, PutMode.Xor)
    End Select

  End Sub

  'DrawGorilla:
  '  Draws the Gorilla in either CGA or EGA mode
  '  and saves the graphics data in an array.
  'Parameters:
  '  x - x coordinate of gorilla
  '  y - y coordinate of the gorilla
  '  arms - either Left up, Right up, or both down
  Sub DrawGorilla(x As Integer, y As Integer, arms As Integer)
    Dim i As Single   ' Local index must be single precision

    'draw head
    LINE(x - Scl(4), y, x + Scl(2.9), y + Scl(6), OBJECTCOLOR, LineMode.BF)
    LINE(x - Scl(5), y + Scl(2), x + Scl(4), y + Scl(4), OBJECTCOLOR, LineMode.BF)

    'draw eyes/brow
    LINE(x - Scl(3), y + Scl(2), x + Scl(2), y + Scl(2), 0)

    'draw nose if ega
    If Mode = 9 Then
      For i = -2 To -1
        PSET(x + i, y + 4, 0)
        PSET(x + i + 3, y + 4, 0)
      Next i
    End If

    'neck
    LINE(x - Scl(3), y + Scl(7), x + Scl(2), y + Scl(7), OBJECTCOLOR)

    'body
    LINE(x - Scl(8), y + Scl(8), x + Scl(6.9), y + Scl(14), OBJECTCOLOR, LineMode.BF)
    LINE(x - Scl(6), y + Scl(15), x + Scl(4.9), y + Scl(20), OBJECTCOLOR, LineMode.BF)

    'legs
    For i = 0 To 4
      CIRCLE(x + Scl(i), y + Scl(25), Scl(10), OBJECTCOLOR, 3 * pi# / 4, 9 * pi# / 8)
      CIRCLE(x + Scl(-6) + Scl(i - 0.1), y + Scl(25), Scl(10), OBJECTCOLOR, 15 * pi# / 8, pi# / 4)
    Next

    'chest
    CIRCLE(x - Scl(4.9), y + Scl(10), Scl(4.9), 0, 3 * pi# / 2, 0)
    CIRCLE(x + Scl(4.9), y + Scl(10), Scl(4.9), 0, pi#, 3 * pi# / 2)

    For i = -5 To -1
      Select Case arms
        Case 1
          'Right arm up
          CIRCLE(x + Scl(i - 0.1), y + Scl(14), Scl(9), OBJECTCOLOR, 3 * pi# / 4, 5 * pi# / 4)
          CIRCLE(x + Scl(4.9) + Scl(i), y + Scl(4), Scl(9), OBJECTCOLOR, 7 * pi# / 4, pi# / 4)
          QBGET(x - Scl(15), y - Scl(1), x + Scl(14), y + Scl(28), GorR&)
        Case 2
          'Left arm up
          CIRCLE(x + Scl(i - 0.1), y + Scl(4), Scl(9), OBJECTCOLOR, 3 * pi# / 4, 5 * pi# / 4)
          CIRCLE(x + Scl(4.9) + Scl(i), y + Scl(14), Scl(9), OBJECTCOLOR, 7 * pi# / 4, pi# / 4)
          QBGET(x - Scl(15), y - Scl(1), x + Scl(14), y + Scl(28), GorL&)
        Case 3
          'Both arms down
          CIRCLE(x + Scl(i - 0.1), y + Scl(14), Scl(9), OBJECTCOLOR, 3 * pi# / 4, 5 * pi# / 4)
          CIRCLE(x + Scl(4.9) + Scl(i), y + Scl(14), Scl(9), OBJECTCOLOR, 7 * pi# / 4, pi# / 4)
          QBGET(x - Scl(15), y - Scl(1), x + Scl(14), y + Scl(28), GorD&)
      End Select
    Next i
  End Sub

  'ExplodeGorilla:
  '  Causes gorilla explosion when a direct hit occurs
  'Parameters:
  '  X#, Y# - shot location
  Function ExplodeGorilla(x#, y#) As Integer
    YAdj = Scl(12)
    XAdj = Scl(5)
    SclX# = ScrWidth / 320
    SclY# = ScrHeight / 200
    If x# < ScrWidth / 2 Then PlayerHit = 1 Else PlayerHit = 2
    PLAY("MBO0L16EFGEFDC")

    For i = 1 To 8 * SclX#
      CIRCLE(GorillaX(PlayerHit) + 3.5 * SclX# + XAdj, GorillaY(PlayerHit) + 7 * SclY# + YAdj, i, ExplosionColor, , , -1.57)
      LINE(GorillaX(PlayerHit) + 7 * SclX#, GorillaY(PlayerHit) + 9 * SclY# - i, GorillaX(PlayerHit), GorillaY(PlayerHit) + 9 * SclY# - i, ExplosionColor)
    Next i

    For i = 1 To 16 * SclX#
      If i < (8 * SclX#) Then CIRCLE(GorillaX(PlayerHit) + 3.5 * SclX# + XAdj, GorillaY(PlayerHit) + 7 * SclY# + YAdj, (8 * SclX# + 1) - i, BACKATTR, , , -1.57)
      CIRCLE(GorillaX(PlayerHit) + 3.5 * SclX# + XAdj, GorillaY(PlayerHit) + YAdj, i, i Mod 2 + 1, , , -1.57)
    Next i

    For i = 24 * SclX# To 1 Step -1
      CIRCLE(GorillaX(PlayerHit) + 3.5 * SclX# + XAdj, GorillaY(PlayerHit) + YAdj, i, BACKATTR, , , -1.57)
      For Count = 1 To 200
      Next
    Next i

    ExplodeGorilla = PlayerHit
  End Function

  'GetInputs:
  '  Gets user inputs at beginning of game
  'Parameters:
  '  Player1$, Player2$ - player names
  '  NumGames - number of games to play
  Sub GetInputs(Player1$, Player2$, NumGames As Integer)
    COLOR(7, 0)
    CLS

    LOCATE(8, 15)
    LineInput("Name of Player 1 (Default = 'Player 1'): ", Player1$)
    If Player1$ = "" Then
      Player1$ = "Player 1"
    Else
      Player1$ = Left(Player1$, 10)
    End If

    LOCATE(10, 15)
    LineInput("Name of Player 2 (Default = 'Player 2'): ", Player2$)
    If Player2$ = "" Then
      Player2$ = "Player 2"
    Else
      Player2$ = Left(Player2$, 10)
    End If

    Do
      Locate(12, 56) : Print(Space(25), SEMICOLON) ';
      Locate(12, 13)
      Input("Play to how many total points (Default = 3)", game$)
      NumGames = Val(Left(game$, 2))
    Loop Until NumGames > 0 And Len(game$) < 3 Or Len(game$) = 0
    If NumGames = 0 Then NumGames = 3

    Do
      Locate(14, 53) : Print(Space(28), SEMICOLON) ';
      Locate(14, 17)
      Input("Gravity in Meters/Sec (Earth = 9.8)", grav$)
      gravity# = Val(grav$)
    Loop Until gravity# > 0 Or Len(grav$) = 0
    If gravity# = 0 Then gravity# = 9.8
  End Sub

  'GetNum:
  '  Gets valid numeric input from user
  'Parameters:
  '  Row, Col - location to echo input
  Function GetNum#(Row As Integer, Col As Integer)
    Result$ = ""
    Done = False
    While INKEY$ <> "" : End While   'Clear keyboard buffer

    Do While Not Done

      LOCATE(Row, Col)
      Print(Result$, SEMICOLON, Chr(95), SEMICOLON, "    ", SEMICOLON) ';

      Kbd$ = INKEY$
      Select Case Kbd$
        Case "0" To "9"
          Result$ = Result$ + Kbd$
        Case "."
          If InStr(Result$, ".") = 0 Then
            Result$ = Result$ + Kbd$
          End If
        Case Chr(13)
          If Val(Result$) > 360 Then
            Result$ = ""
          Else
            Done = True
          End If
        Case Chr(8)
          If Len(Result$) > 0 Then
            Result$ = Left(Result$, Len(Result$) - 1)
          End If
        Case Else
          If Len(Kbd$) > 0 Then
            Beep()
          End If
      End Select
    Loop

    LOCATE(Row, Col)
    Print(Result$, SEMICOLON, " ", SEMICOLON) ';

    GetNum# = Val(Result$)
  End Function

  'GorillaIntro:
  '  Displays gorillas on screen for the first time
  '  allows the graphical data to be put into an array
  'Parameters:
  '  Player1$, Player2$ - The names of the players
  '
  Sub GorillaIntro(Player1$, Player2$)
    LOCATE(16, 34) : Print("--------------")
    LOCATE(18, 34) : Print("V = View Intro")
    LOCATE(19, 34) : Print("P = Play Game")
    LOCATE(21, 35) : Print("Your Choice?")

    Do While Character$ = ""
      Character$ = INKEY$
    Loop

    If Mode = 1 Then
      x = 125
      y = 100
    Else
      x = 278
      y = 175
    End If

    SCREEN(Mode)
    SetScreen()

    If Mode = 1 Then Center(5, "Please wait while gorillas are drawn.")

    VIEWPRINT(9, 24)

    If Mode = 9 Then PALETTE(OBJECTCOLOR, BackColor)

    DrawGorilla(x, y, ARMSDOWN)
    CLS(2)
    DrawGorilla(x, y, LEFTUP)
    CLS(2)
    DrawGorilla(x, y, RIGHTUP)
    CLS(2)

    VIEWPRINT(1, 25)
    If Mode = 9 Then PALETTE(OBJECTCOLOR, 46)

    If UCase(Character$) = "V" Then
      Center(2, "Q B A S I C   G O R I L L A S")
      Center(5, "             STARRING:               ")
      P$ = Player1$ + " AND " + Player2$
      Center(7, P$)

      PUT(x - 13, y, GorD&, PutMode.PSET)
      PUT(x + 47, y, GorD&, PutMode.PSET)
      Rest(1)

      PUT(x - 13, y, GorL&, PutMode.PSET)
      PUT(x + 47, y, GorR&, PutMode.PSET)
      PLAY("t120o1l16b9n0baan0bn0bn0baaan0b9n0baan0b")
      Rest(0.3)

      PUT(x - 13, y, GorR&, PutMode.PSET)
      PUT(x + 47, y, GorL&, PutMode.PSET)
      PLAY("o2l16e-9n0e-d-d-n0e-n0e-n0e-d-d-d-n0e-9n0e-d-d-n0e-")
      Rest(0.3)

      PUT(x - 13, y, GorL&, PutMode.PSET)
      PUT(x + 47, y, GorR&, PutMode.PSET)
      PLAY("o2l16g-9n0g-een0g-n0g-n0g-eeen0g-9n0g-een0g-")
      Rest(0.3)

      PUT(x - 13, y, GorR&, PutMode.PSET)
      PUT(x + 47, y, GorL&, PutMode.PSET)
      PLAY("o2l16b9n0baan0g-n0g-n0g-eeen0o1b9n0baan0b")
      Rest(0.3)

      For i = 1 To 4
        PUT(x - 13, y, GorL&, PutMode.PSET)
        PUT(x + 47, y, GorR&, PutMode.PSET)
        PLAY("T160O0L32EFGEFDC")
        Rest(0.1)
        PUT(x - 13, y, GorR&, PutMode.PSET)
        PUT(x + 47, y, GorL&, PutMode.PSET)
        PLAY("T160O0L32EFGEFDC")
        Rest(0.1)
      Next
    End If
  End Sub

  'Intro:
  '  Displays game introduction
  Sub Intro()

    SCREEN(0)
    WIDTH(80, 25)
    MaxCol = 80
    COLOR(15, 0)
    CLS

    Center(4, "Q B a s i c    G O R I L L A S")
    COLOR(7)
    Center(6, "Copyright (C) Microsoft Corporation 1990")
    Center(8, "(Converted to VB.NET by Cory Smith)")
    Center(10, "Your mission is to hit your opponent with the exploding")
    Center(11, "banana by varying the angle and power of your throw, taking")
    Center(12, "into account wind speed, gravity, and the city skyline.")
    Center(13, "The wind speed is shown by a directional arrow at the bottom")
    Center(12, "of the playing field, its length relative to its strength.")
    Center(24, "Press any key to continue")

    PLAY("MBT160O1L8CDEDCDL4ECC")
    SparklePause()
    If Mode = 1 Then MaxCol = 40
  End Sub

  'MakeCityScape:
  '  Creates random skyline for game
  'Parameters:
  '  BCoor() - a user-defined type array which stores the coordinates of
  '  the upper left corner of each building.
  Sub MakeCityScape(BCoor() As XYPoint)

    x = 2

    'Set the sloping trend of the city scape. NewHt is new building height
    Slope = FnRan(6)
    Select Case Slope
      Case 1 : NewHt = 15                 'Upward slope
      Case 2 : NewHt = 130                'Downward slope
      Case 3 To 5 : NewHt = 15            '"V" slope - most common
      Case 6 : NewHt = 130                'Inverted "V" slope
    End Select

    If Mode = 9 Then
      BottomLine = 335                   'Bottom of building
      HtInc = 10                         'Increase value for new height
      DefBWidth = 37                     'Default building height
      RandomHeight = 120                 'Random height difference
      WWidth = 3                         'Window width
      WHeight = 6                        'Window height
      WDifV = 15                         'Counter for window spacing - vertical
      WDifh = 10                         'Counter for window spacing - horizontal
    Else
      BottomLine = 190
      HtInc = 6
      NewHt = NewHt * 20 \ 35            'Adjust for CGA
      DefBWidth = 18
      RandomHeight = 54
      WWidth = 1
      WHeight = 2
      WDifV = 5
      WDifh = 4
    End If

    CurBuilding = 1
    Do

      Select Case Slope
        Case 1
          NewHt = NewHt + HtInc
        Case 2
          NewHt = NewHt - HtInc
        Case 3 To 5
          If x > ScrWidth \ 2 Then
            NewHt = NewHt - 2 * HtInc
          Else
            NewHt = NewHt + 2 * HtInc
          End If
        Case 4
          If x > ScrWidth \ 2 Then
            NewHt = NewHt + 2 * HtInc
          Else
            NewHt = NewHt - 2 * HtInc
          End If
      End Select

      'Set width of building and check to see if it would go off the screen
      BWidth = FnRan(DefBWidth) + DefBWidth
      If x + BWidth > ScrWidth Then BWidth = ScrWidth - x - 2

      'Set height of building and check to see if it goes below screen
      BHeight = FnRan(RandomHeight) + NewHt
      If BHeight < HtInc Then BHeight = HtInc

      'Check to see if Building is too high
      If BottomLine - BHeight <= MaxHeight + GHeight Then BHeight = MaxHeight + GHeight - 5

      'Set the coordinates of the building into the array
      BCoor(CurBuilding).XCoor = x
      BCoor(CurBuilding).YCoor = BottomLine - BHeight

      If Mode = 9 Then BuildingColor = FnRan(3) + 4 Else BuildingColor = 2

      'Draw the building, outline first, then filled
      LINE(x - 1, BottomLine + 1, x + BWidth + 1, BottomLine - BHeight - 1, BACKGROUND, LineMode.B)
      LINE(x, BottomLine, x + BWidth, BottomLine - BHeight, BuildingColor, LineMode.BF)

      'Draw the windows
      c = x + 3
      Do
        For i = BHeight - 3 To 7 Step -WDifV
          If Mode <> 9 Then
            WinColr = (FnRan(2) - 2) * -3
          ElseIf FnRan(4) = 1 Then
            WinColr = 8
          Else
            WinColr = WINDOWCOLOR
          End If
          LINE(c, BottomLine - i, c + WWidth, BottomLine - i + WHeight, WinColr, LineMode.BF)
        Next
        c = c + WDifh
      Loop Until c >= x + BWidth - 3

      x = x + BWidth + 2

      CurBuilding = CurBuilding + 1

    Loop Until x > ScrWidth - HtInc

    LastBuilding = CurBuilding - 1

    'Set Wind speed
    Wind = FnRan(10) - 5
    If FnRan(3) = 1 Then
      If Wind > 0 Then
        Wind = Wind + FnRan(10)
      Else
        Wind = Wind - FnRan(10)
      End If
    End If

    'Draw Wind speed arrow
    If Wind <> 0 Then
      WindLine = Wind * 3 * (ScrWidth \ 320)
      LINE(ScrWidth \ 2, ScrHeight - 5, ScrWidth \ 2 + WindLine, ScrHeight - 5, ExplosionColor)
      If Wind > 0 Then ArrowDir = -2 Else ArrowDir = 2
      LINE(ScrWidth / 2 + WindLine, ScrHeight - 5, ScrWidth / 2 + WindLine + ArrowDir, ScrHeight - 5 - 2, ExplosionColor)
      LINE(ScrWidth / 2 + WindLine, ScrHeight - 5, ScrWidth / 2 + WindLine + ArrowDir, ScrHeight - 5 + 2, ExplosionColor)
    End If
  End Sub

  'PlaceGorillas:
  '  PUTs the Gorillas on top of the buildings.  Must have drawn
  '  Gorillas first.
  'Parameters:
  '  BCoor() - user-defined TYPE array which stores upper left coordinates
  '  of each building.
  Sub PlaceGorillas(BCoor() As XYPoint)

    If Mode = 9 Then
      XAdj = 14
      YAdj = 30
    Else
      XAdj = 7
      YAdj = 16
    End If
    SclX# = ScrWidth / 320
    SclY# = ScrHeight / 200

    'Place gorillas on second or third building from edge
    For i = 1 To 2
      If i = 1 Then BNum = FnRan(2) + 1 Else BNum = LastBuilding - FnRan(2)

      BWidth = BCoor(BNum + 1).XCoor - BCoor(BNum).XCoor
      GorillaX(i) = BCoor(BNum).XCoor + BWidth / 2 - XAdj
      GorillaY(i) = BCoor(BNum).YCoor - YAdj
      PUT(GorillaX(i), GorillaY(i), GorD&, PutMode.PSET)
    Next i

  End Sub

  'PlayGame:
  '  Main game play routine
  'Parameters:
  '  Player1$, Player2$ - player names
  '  NumGames - number of games to play
  Sub PlayGame(Player1$, Player2$, NumGames As Integer)
    Dim BCoor(0 To 30) As XYPoint
    Dim TotalWins(2) As Integer '(1 To 2)

    J = 1

    For i = 1 To NumGames

      CLS
      Randomize(Timer)
      'Call MakeCityScape(BCoor())
      Call MakeCityScape(BCoor)
      'Call PlaceGorillas(BCoor())
      Call PlaceGorillas(BCoor)
      DoSun(SUNHAPPY)
      Hit = False
      Do While Hit = False
        J = 1 - J
        LOCATE(1, 1)
        Print(Player1$)
        LOCATE(1, (MaxCol - 1 - Len(Player2$)))
        Print(Player2$)
        Center(23, LTrim(Str(TotalWins(1))) + ">Score<" + LTrim(Str(TotalWins(2))))
        Tosser = J + 1 : Tossee = 3 - J

        'Plot the shot.  Hit is true if Gorilla gets hit.
        Hit = DoShot(Tosser, GorillaX(Tosser), GorillaY(Tosser))

        'Reset the sun, if it got hit
        If SunHit Then DoSun(SUNHAPPY)

        'If Hit = True Then Call UpdateScores(TotalWins(), Tosser, Hit)
        If Hit = True Then Call UpdateScores(TotalWins, Tosser, Hit)
      Loop
      SLEEP(1)
    Next i

    SCREEN(0)
    WIDTH(80, 25)
    COLOR(7, 0)
    MaxCol = 80
    CLS

    Center(8, "GAME OVER!")
    Center(10, "Score:")
    LOCATE(11, 30) : Print(Player1$, SEMICOLON, TAB(50), SEMICOLON, TotalWins(1))
    LOCATE(12, 30) : Print(Player2$, SEMICOLON, TAB(50), SEMICOLON, TotalWins(2))
    Center(24, "Press any key to continue")
    SparklePause()
    COLOR(7, 0)
    CLS
  End Sub

  'PlayGame:
  '  Plots banana shot across the screen
  'Parameters:
  '  StartX, StartY - starting shot location
  '  Angle - shot angle
  '  Velocity - shot velocity
  '  PlayerNum - the banana thrower
  Function PlotShot(StartX As Integer, StartY As Integer, Angle#, Velocity As Integer, PlayerNum As Integer) As Integer

    Angle# = Angle# / 180 * pi#  'Convert degree angle to radians
    Radius = Mode Mod 7

    InitXVel# = COS(Angle#) * Velocity
    InitYVel# = SIN(Angle#) * Velocity

    oldx# = StartX
    oldy# = StartY

    'draw gorilla toss
    If PlayerNum = 1 Then
      PUT(StartX, StartY, GorL&, PutMode.PSET)
    Else
      PUT(StartX, StartY, GorR&, PutMode.PSET)
    End If

    'throw sound
    PLAY("MBo0L32A-L64CL16BL64A+")
    Rest(0.1)

    'redraw gorilla
    PUT(StartX, StartY, GorD&, PutMode.PSET)

    adjust = Scl(4)                   'For scaling CGA

    xedge = Scl(9) * (2 - PlayerNum)  'Find leading edge of banana for check

    Impact = False
    ShotInSun = False
    OnScreen = True
    PlayerHit = 0
    NeedErase = False

    StartXPos = StartX
    StartYPos = StartY - adjust - 3

    If PlayerNum = 2 Then
      StartXPos = StartXPos + Scl(25)
      direction = Scl(4)
    Else
      direction = Scl(-4)
    End If

    If Velocity < 2 Then              'Shot too slow - hit self
      x# = StartX
      y# = StartY
      pointval = OBJECTCOLOR
    End If

    Do While (Not Impact) And OnScreen

      Rest(0.02)

      'Erase old banana, if necessary
      If NeedErase Then
        NeedErase = False
        Call DrawBan(oldx#, oldy#, oldrot, False)
      End If

      x# = StartXPos + (InitXVel# * t#) + (0.5 * (Wind / 5) * t# ^ 2)
      y# = StartYPos + ((-1 * (InitYVel# * t#)) + (0.5 * gravity# * t# ^ 2)) * (ScrHeight / 350)

      If (x# >= ScrWidth - Scl(10)) Or (x# <= 3) Or (y# >= ScrHeight - 3) Then
        OnScreen = False
      End If


      If OnScreen And y# > 0 Then

        'check it
        LookY = 0
        LookX = Scl(8 * (2 - PlayerNum))
        Do
          pointval = POINT(x# + LookX, y# + LookY)
          If pointval = 0 Then
            Impact = False
            If ShotInSun = True Then
              If ABS(ScrWidth \ 2 - x#) > Scl(20) Or y# > SunHt Then ShotInSun = False
            End If
          ElseIf pointval = SUNATTR And y# < SunHt Then
            If Not SunHit Then DoSun(SUNSHOCK)
            SunHit = True
            ShotInSun = True
          Else
            Impact = True
          End If
          LookX = LookX + direction
          LookY = LookY + Scl(6)
        Loop Until Impact Or LookX <> Scl(4)

        If Not ShotInSun And Not Impact Then
          'plot it
          rot = (t# * 10) Mod 4
          Call DrawBan(x#, y#, rot, True)
          NeedErase = True
        End If

        oldx# = x#
        oldy# = y#
        oldrot = rot

      End If


      t# = t# + 0.1

    Loop

    If pointval <> OBJECTCOLOR And Impact Then
      Call DoExplosion(x# + adjust, y# + adjust)
    ElseIf pointval = OBJECTCOLOR Then
      PlayerHit = ExplodeGorilla(x#, y#)
    End If

    PlotShot = PlayerHit

  End Function

  'Rest:
  '  pauses the program
  Sub Rest(t#)
    s# = Timer
    t2# = MachSpeed * t# / SPEEDCONST
    Do
    Loop Until Timer - s# > t2#
  End Sub

  'Scl:
  '  Pass the number in to scaling for cga.  If the number is a decimal, then we
  '  want to scale down for cga or scale up for ega.  This allows a full range
  '  of numbers to be generated for scaling.
  '  (i.e. for 3 to get scaled to 1, pass in 2.9)
  Function Scl(n!) As Integer

    If n! <> Int(n!) Then
      If Mode = 1 Then n! = n! - 1
    End If
    If Mode = 1 Then
      Scl = CInt(n! / 2 + 0.1)
    Else
      Scl = CInt(n!)
    End If

  End Function

  'SetScreen:
  '  Sets the appropriate color statements
  Sub SetScreen()

    If Mode = 9 Then
      ExplosionColor = 2
      BackColor = 1
      PALETTE(0, 1)
      PALETTE(1, 46)
      PALETTE(2, 44)
      PALETTE(3, 54)
      PALETTE(5, 7)
      PALETTE(6, 4)
      PALETTE(7, 3)
      PALETTE(9, 63)       'Display Color
    Else
      ExplosionColor = 2
      BackColor = 0
      COLOR(BackColor, 2)

    End If

  End Sub

  'SparklePause:
  '  Creates flashing border for intro and game over screens
  Sub SparklePause()

    COLOR(4, 0)
    A$ = "*    *    *    *    *    *    *    *    *    *    *    *    *    *    *    *    *    "
    While INKEY$ <> "" : End While 'Clear keyboard buffer

    While INKEY$ = ""
      For i = 1 To 5
        LOCATE(1, 1)                             'print horizontal sparkles
        Print(Mid(A$, i, 80), SEMICOLON) ';
        Locate(22, 1)
        Print(Mid(A$, 6 - i, 80), SEMICOLON) ';

        For b = 2 To 21                         'Print Vertical sparkles
          c = (i + b) Mod 5
          If c = 1 Then
            LOCATE(b, 80)
            Print("*", SEMICOLON) ';
            Locate(23 - b, 1)
            Print("*", SEMICOLON) ';
          Else
            LOCATE(b, 80)
            Print(" ", SEMICOLON) ';
            Locate(23 - b, 1)
            Print(" ", SEMICOLON) ';
          End If
        Next b
      Next i
    End While
  End Sub

  'UpdateScores:
  '  Updates players' scores
  'Parameters:
  '  Record - players' scores
  '  PlayerNum - player
  '  Results - results of player's shot
  Sub UpdateScores(Record() As Integer, PlayerNum As Integer, Results As Boolean)
    If Results = HITSELF Then
      Record(ABS(PlayerNum - 3)) = Record(ABS(PlayerNum - 3)) + 1
    Else
      Record(PlayerNum) = Record(PlayerNum) + 1
    End If
  End Sub

  'VictoryDance:
  '  gorilla dances after he has eliminated his opponent
  'Parameters:
  '  Player - which gorilla is dancing
  Sub VictoryDance(Player As Integer)
    For i# = 1 To 4
      PUT(GorillaX(Player), GorillaY(Player), GorL&, PutMode.PSET)
      PLAY("MFO0L32EFGEFDC")
      Rest(0.2)
      PUT(GorillaX(Player), GorillaY(Player), GorR&, PutMode.PSET)
      PLAY("MFO0L32EFGEFDC")
      Rest(0.2)
    Next
  End Sub

End Module