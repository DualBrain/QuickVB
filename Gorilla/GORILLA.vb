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
Imports QB.Graphics

Module Program

  Structure XYPoint
    Implements IFileRecord
    Public XCoor As Short
    Public YCoor As Short
    Public Sub WriteTo(writer As RecordWriter) Implements IFileRecord.WriteTo
      writer.Write(XCoor)
      writer.Write(YCoor)
    End Sub
    Public Sub ReadFrom(reader As RecordReader) Implements IFileRecord.ReadFrom
      XCoor = reader.ReadInt16()
      YCoor = reader.ReadInt16()
    End Sub
  End Structure
  
  Const SPEEDCONST As Short = 500
  Const [TRUE] As Short = -1
  Const [FALSE] As Short = Not [TRUE]
  Const HITSELF As Short = 1
  Const BACKATTR As Short = 0
  Const OBJECTCOLOR As Short = 1
  Const WINDOWCOLOR As Short = 14
  Const SUNATTR As Short = 3
  Const SUNHAPPY As Short = [FALSE]
  Const SUNSHOCK As Short = [TRUE]
  Const RIGHTUP As Short = 1
  Const LEFTUP As Short = 2
  Const ARMSDOWN As Short = 3
  Dim GorillaX(2) As Short
  Dim GorillaY(2) As Short
  Dim LastBuilding As Short
  Dim pi#
  Dim LBan%(x), RBan%(x), UBan%(x), DBan%(x)
  Dim GorD%(120)
  Dim GorL%(120)
  Dim GorR%(120)
  Dim gravity#
  Dim Wind As Short
  Dim ScrHeight As Short
  Dim ScrWidth As Short
  Dim Mode As Short
  Dim MaxCol As Short
  Dim ExplosionColor As Short
  Dim SunColor As Short
  Dim BackColor As Short
  Dim SunHit As Short
  Dim SunHt As Short
  Dim GHeight As Short
  Dim MachSpeed As Single
  Dim x As Short
  Dim KeyFlags As Short
  Dim Name1$ = ""
  Dim Name2$ = ""
  Dim NumGames As Short
  Dim i As Short

  Public Sub Main()
    QB.Graphics.WaitForKeyAtExit = True
    DATA_LABEL("CGABanana")
    DATA(327686, -252645316, 60)
    DATA(196618, -1057030081, 49344)
    DATA(196618, -1056980800, 63)
    DATA(327686, 1010580720, 240)
    DATA_LABEL("EGABanana")
    DATA(458758, 202116096, 471604224, 943208448, 943208448, 943208448, 471604224, 202116096, 0)
    DATA(262153, -2134835200, -2134802239, -2130771968, -2130738945, 8323072, 8323199, 4063232, 4063294)
    DATA(262153, 4063232, 4063294, 8323072, 8323199, -2130771968, -2130738945, -2134835200, -2134802239)
    DATA(458758, -1061109760, -522133504, 1886416896, 1886416896, 1886416896, -522133504, -1061109760, 0)
    
    '                         Q B a s i c   G o r i l l a s
    '
    '                   Copyright (C) Microsoft Corporation 1990
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
    
    'Set default data type to integer for faster game play
    ' DEFINT A-Z
    
    'Sub Declarations
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
    
    'Make all arrays Dynamic
    '$DYNAMIC
    
    'User-Defined TYPEs
    ' TYPE XYPoint (see the Structure at the top)
    
    'Constants
    ' CONST SPEEDCONST = 500
    ' CONST TRUE = -1
    ' CONST FALSE = NOT TRUE
    ' CONST HITSELF = 1
    ' CONST BACKATTR = 0
    ' CONST OBJECTCOLOR = 1
    ' CONST WINDOWCOLOR = 14
    ' CONST SUNATTR = 3
    ' CONST SUNHAPPY = FALSE
    ' CONST SUNSHOCK = TRUE
    ' CONST RIGHTUP = 1
    ' CONST LEFTUP = 2
    ' CONST ARMSDOWN = 3
    
    'Global Variables
    ' DIM SHARED GorillaX(1 TO 2)  'Location of the two gorillas
    ' DIM SHARED GorillaY(1 TO 2)
    ' DIM SHARED LastBuilding
    
    ' DIM SHARED pi#
    ' DIM SHARED LBan&(x), RBan&(x), UBan&(x), DBan&(x) 'Graphical picture of banana
    ' DIM SHARED GorD&(120)        'Graphical picture of Gorilla arms down
    ' DIM SHARED GorL&(120)        'Gorilla left arm raised
    ' DIM SHARED GorR&(120)        'Gorilla right arm raised
    
    ' DIM SHARED gravity#
    ' DIM SHARED Wind
    
    'Screen Mode Variables
    ' DIM SHARED ScrHeight
    ' DIM SHARED ScrWidth
    ' DIM SHARED Mode
    ' DIM SHARED MaxCol
    
    'Screen Color Variables
    ' DIM SHARED ExplosionColor
    ' DIM SHARED SunColor
    ' DIM SHARED BackColor
    ' DIM SHARED SunHit
    
    ' DIM SHARED SunHt
    ' DIM SHARED GHeight
    ' DIM SHARED MachSpeed AS SINGLE
      ' DEF FnRan (x) = INT(RND(1) * x) + 1
      DEF_SEG(0)  ' Set NumLock to ON
      KeyFlags = CShort(PEEK(1047))
      If (KeyFlags And 32) = 0 Then
        POKE(1047, KeyFlags Or 32)
      End If
      DEF_SEG()
    
      InitVars()
      Intro()
      GetInputs(Name1$, Name2$, NumGames)
      GorillaIntro(Name1$, Name2$)
      PlayGame(Name1$, Name2$, NumGames)
    
      DEF_SEG(0)  ' Restore NumLock state
      POKE(1047, KeyFlags)
      DEF_SEG()
    End
    
    
    CGABanana:
      'BananaLeft
      ' DATA 327686, -252645316, 60   (DATA is listed at the start of the program)
      'BananaDown
      ' DATA 196618, -1057030081, 49344   (DATA is listed at the start of the program)
      'BananaUp
      ' DATA 196618, -1056980800, 63   (DATA is listed at the start of the program)
      'BananaRight
      ' DATA 327686,  1010580720, 240   (DATA is listed at the start of the program)
    
    EGABanana:
      'BananaLeft
      ' DATA 458758,202116096,471604224,943208448,943208448,943208448,471604224,202116096,0   (DATA is listed at the start of the program)
      'BananaDown
      ' DATA 262153, -2134835200, -2134802239, -2130771968, -2130738945,8323072, 8323199, 4063232, 4063294   (DATA is listed at the start of the program)
      'BananaUp
      ' DATA 262153, 4063232, 4063294, 8323072, 8323199, -2130771968, -2130738945, -2134835200,-2134802239   (DATA is listed at the start of the program)
      'BananaRight
      ' DATA 458758, -1061109760, -522133504, 1886416896, 1886416896, 1886416896,-522133504,-1061109760,0   (DATA is listed at the start of the program)
    
    
    ScreenModeError:
      If Mode = 1 Then
        CLS()
        LOCATE(10, 5)
        PRINT("Sorry, you must have CGA, EGA color, or VGA graphics to play GORILLA.BAS")
        End
      Else
        Mode = 1
        Resume
      End If
    
    PaletteError:
      Mode = 1  '64K EGA cards will run in CGA mode.
      Resume Next
    
  End Sub

  
  Function FnRan(ByVal x_Int As Short) As Short
    Return CShort(Int(Rnd(1) * x_Int) + 1)
  End Function
  
  Sub InitVars()
    pi# = 4 * Atan(1#)
  
    'This is a clever way to pick the best graphics mode available
    On Error GoTo ScreenModeError
    Mode = 9
    SCREEN(Mode)
    On Error GoTo PaletteError
    If Mode = 9 Then PALETTE(4, 0)  'Check for 64K EGA
    On Error GoTo 0
  
    MachSpeed = CalcDelay!()
  
    If Mode = 9 Then
      ScrWidth = 640
      ScrHeight = 350
      GHeight = 25
      RESTORE("EGABanana")
      ReDim LBan%(8), RBan%(8), UBan%(8), DBan%(8)
  
      For i = 0 To 8
        READ(LBan%(i))
      Next i
  
      For i = 0 To 8
        READ(DBan%(i))
      Next i
  
      For i = 0 To 8
        READ(UBan%(i))
      Next i
  
      For i = 0 To 8
        READ(RBan%(i))
      Next i
  
      SunHt = 39
  
    Else
  
      ScrWidth = 320
      ScrHeight = 200
      GHeight = 12
      RESTORE("CGABanana")
      ReDim LBan%(2), RBan%(2), UBan%(2), DBan%(2)
      ReDim GorL%(20), GorD%(20), GorR%(20)
  
      For i = 0 To 2
        READ(LBan%(i))
      Next i
      For i = 0 To 2
        READ(DBan%(i))
      Next i
      For i = 0 To 2
        READ(UBan%(i))
      Next i
      For i = 0 To 2
        READ(RBan%(i))
      Next i
  
      MachSpeed = MachSpeed * 1.3!
      SunHt = 20
    End If
  Return
    Exit Sub
  ScreenModeError:
    If Mode = 1 Then
      CLS()
      LOCATE(10, 5)
      PRINT("Sorry, you must have CGA, EGA color, or VGA graphics to play GORILLA.BAS")
      End
    Else
      Mode = 1
      Resume
    End If
  PaletteError:
    Mode = 1  '64K EGA cards will run in CGA mode.
    Resume Next
  End Sub
  
  
  ' $STATIC
  'CalcDelay:
  '  Checks speed of the machine.
  Function CalcDelay!()
    Dim i!
  
    Dim s! = CSng(Timer)
    Do
      i! = i! + 1
    EMPTYLOOP() : Loop Until (Timer - s!) >= .5!
    CalcDelay! = i!
  
  End Function
  
  
  ' Center:
  '   Centers and prints a text string on a given row
  ' Parameters:
  '   Row - screen row number
  '   Text$ - text to be printed
  '
  Sub Center(ByRef Row As Short, ByRef Text$)
    Dim Col As Short = CShort(MaxCol \ 2)
    LOCATE(Row, CInt(Col - ((Len(Text$) / 2) + .5!)))
    PRINT(Text$, True)
  End Sub
  
  
  ' DoExplosion:
  '   Produces explosion when a shot is fired
  ' Parameters:
  '   X#, Y# - location of explosion
  '
  Sub DoExplosion(ByRef x_Dbl#, ByRef y#)
    Dim Inc#
  
    PLAY("MBO0L32EFGEFDC")
    Dim Radius As Short = CShort(ScrHeight / 50)
    If Mode = 9 Then Inc# = .5! Else Inc# = .41!
    For c# = 0 To Radius Step Inc#
      CIRCLE(x_Dbl#, y#, c#, ExplosionColor)
    Next c#
    For c# = Radius To 0 Step ((-1) * Inc#)
      CIRCLE(x_Dbl#, y#, c#, BACKATTR)
      For i = 1S To 100S : EMPTYLOOP()
      Next i
      Rest(.005!)
    Next c#
  End Sub
  
  
  ' DoShot:
  '   Controls banana shots by accepting player input and plotting
  '   shot angle
  ' Parameters:
  '   PlayerNum - Player
  '   x, y - Player's gorilla position
  '
  Function DoShot(ByRef PlayerNum As Short, ByRef x_Int As Short, ByRef y As Short) As Short
    Dim LocateCol As Short
  
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
    PRINT("Angle:", True)
    Dim Angle# = GetNum#(2, CShort(LocateCol + 7))
  
    LOCATE(3, LocateCol)
    PRINT("Velocity:", True)
    Dim Velocity As Short = CShort(GetNum#(3, CShort(LocateCol + 10)))
  
    If PlayerNum = 2 Then
      Angle# = 180 - Angle#
    End If
  
    'Erase input
    For i = 1S To 4S
      LOCATE(i, 1)
      PRINT(Space$(30 \ (80 \ MaxCol)), True)
      LOCATE(i, (50 \ (80 \ MaxCol)))
      PRINT(Space$(30 \ (80 \ MaxCol)), True)
    Next
  
    SunHit = [FALSE]
    Dim PlayerHit As Short = PlotShot(x_Int, y, Angle#, Velocity, PlayerNum)
    If PlayerHit = 0 Then
      DoShot = [FALSE]
    Else
      DoShot = [TRUE]
      If PlayerHit = PlayerNum Then PlayerNum = CShort(3 - PlayerNum)
      VictoryDance(PlayerNum)
    End If
  
  End Function
  
  
  ' DoSun:
  '   Draws the sun at the top of the screen.
  ' Parameters:
  '   Mouth - If TRUE draws "O" mouth else draws a smile mouth.
  '
  Sub DoSun(ByRef Mouth As Short)
    Dim y As Short
  
    'set position of sun
    x = CShort(ScrWidth \ 2) : y = Scl(25)
  
    'clear old sun
    LINE(x - Scl(22), y - Scl(18), x + Scl(22), y + Scl(18), BACKATTR, LineOption.BF)
  
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
    If Mouth <> 0 Then  'draw "o" mouth
      CIRCLE(x, y + Scl(5), Scl(2.9!), 0)
      PAINT(x, y + Scl(5), 0, 0)
    Else  'draw smile
      CIRCLE(x, y, Scl(8), 0, ((210 * pi#) / 180), ((330 * pi#) / 180))
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
  Sub DrawBan(ByRef xc#, ByRef yc#, ByRef r As Short, ByRef bc As Short)
  
  Select Case r
    Case 0
      If bc <> 0 Then PUT(xc#, yc#, LBan%, PutOption.PUT_PSET) Else PUT(xc#, yc#, LBan%, PutOption.PUT_XOR)
    Case 1
      If bc <> 0 Then PUT(xc#, yc#, UBan%, PutOption.PUT_PSET) Else PUT(xc#, yc#, UBan%, PutOption.PUT_XOR)
    Case 2
      If bc <> 0 Then PUT(xc#, yc#, DBan%, PutOption.PUT_PSET) Else PUT(xc#, yc#, DBan%, PutOption.PUT_XOR)
    Case 3
      If bc <> 0 Then PUT(xc#, yc#, RBan%, PutOption.PUT_PSET) Else PUT(xc#, yc#, RBan%, PutOption.PUT_XOR)
  End Select
  
  End Sub
  
  
  'DrawGorilla:
  '  Draws the Gorilla in either CGA or EGA mode
  '  and saves the graphics data in an array.
  'Parameters:
  '  x - x coordinate of gorilla
  '  y - y coordinate of the gorilla
  '  arms - either Left up, Right up, or both down
  Sub DrawGorilla(ByRef x_Int As Short, ByRef y As Short, ByRef arms As Short)
    Dim i As Single  ' Local index must be single precision
  
    'draw head
    LINE(x_Int - Scl(4), y, x_Int + Scl(2.9!), y + Scl(6), OBJECTCOLOR, LineOption.BF)
    LINE(x_Int - Scl(5), y + Scl(2), x_Int + Scl(4), y + Scl(4), OBJECTCOLOR, LineOption.BF)
  
    'draw eyes/brow
    LINE(x_Int - Scl(3), y + Scl(2), x_Int + Scl(2), y + Scl(2), 0)
  
    'draw nose if ega
    If Mode = 9 Then
      For i = -2 To -1
        PSET(x_Int + i, y + 4, 0)
        PSET((x_Int + i) + 3, y + 4, 0)
      Next i
    End If
  
    'neck
    LINE(x_Int - Scl(3), y + Scl(7), x_Int + Scl(2), y + Scl(7), OBJECTCOLOR)
  
    'body
    LINE(x_Int - Scl(8), y + Scl(8), x_Int + Scl(6.9!), y + Scl(14), OBJECTCOLOR, LineOption.BF)
    LINE(x_Int - Scl(6), y + Scl(15), x_Int + Scl(4.9!), y + Scl(20), OBJECTCOLOR, LineOption.BF)
  
    'legs
    For i = 0 To 4
      CIRCLE(x_Int + Scl(i), y + Scl(25), Scl(10), OBJECTCOLOR, (3 * pi#) / 4, (9 * pi#) / 8)
      CIRCLE((x_Int + Scl(-6)) + Scl(i - .1!), y + Scl(25), Scl(10), OBJECTCOLOR, (15 * pi#) / 8, pi# / 4)
    Next
  
    'chest
    CIRCLE(x_Int - Scl(4.9!), y + Scl(10), Scl(4.9!), 0, (3 * pi#) / 2, 0)
    CIRCLE(x_Int + Scl(4.9!), y + Scl(10), Scl(4.9!), 0, pi#, (3 * pi#) / 2)
  
    For i = -5 To -1
      Select Case arms
        Case 1
          'Right arm up
          CIRCLE(x_Int + Scl(i - .1!), y + Scl(14), Scl(9), OBJECTCOLOR, (3 * pi#) / 4, (5 * pi#) / 4)
          CIRCLE((x_Int + Scl(4.9!)) + Scl(i), y + Scl(4), Scl(9), OBJECTCOLOR, (7 * pi#) / 4, pi# / 4)
          [GET](x_Int - Scl(15), y - Scl(1), x_Int + Scl(14), y + Scl(28), GorR%)
        Case 2
          'Left arm up
          CIRCLE(x_Int + Scl(i - .1!), y + Scl(4), Scl(9), OBJECTCOLOR, (3 * pi#) / 4, (5 * pi#) / 4)
          CIRCLE((x_Int + Scl(4.9!)) + Scl(i), y + Scl(14), Scl(9), OBJECTCOLOR, (7 * pi#) / 4, pi# / 4)
          [GET](x_Int - Scl(15), y - Scl(1), x_Int + Scl(14), y + Scl(28), GorL%)
        Case 3
          'Both arms down
          CIRCLE(x_Int + Scl(i - .1!), y + Scl(14), Scl(9), OBJECTCOLOR, (3 * pi#) / 4, (5 * pi#) / 4)
          CIRCLE((x_Int + Scl(4.9!)) + Scl(i), y + Scl(14), Scl(9), OBJECTCOLOR, (7 * pi#) / 4, pi# / 4)
          [GET](x_Int - Scl(15), y - Scl(1), x_Int + Scl(14), y + Scl(28), GorD%)
      End Select
    Next i
  End Sub
  
  
  'ExplodeGorilla:
  '  Causes gorilla explosion when a direct hit occurs
  'Parameters:
  '  X#, Y# - shot location
  Function ExplodeGorilla(ByRef x_Dbl#, ByRef y#) As Short
    Dim PlayerHit As Short
    Dim YAdj As Short = Scl(12)
    Dim XAdj As Short = Scl(5)
    Dim SclX# = ScrWidth / 320
    Dim SclY# = ScrHeight / 200
    If x_Dbl# < (ScrWidth / 2) Then PlayerHit = 1 Else PlayerHit = 2
    PLAY("MBO0L16EFGEFDC")
  
    For i As Short = 1 To CShort(8 * SclX#)
      CIRCLE((GorillaX(PlayerHit) + (3.5! * SclX#)) + XAdj, (GorillaY(PlayerHit) + (7 * SclY#)) + YAdj, i, ExplosionColor, Nothing, Nothing, -1.57!)
      LINE(GorillaX(PlayerHit) + (7 * SclX#), (GorillaY(PlayerHit) + (9 * SclY#)) - i, GorillaX(PlayerHit), (GorillaY(PlayerHit) + (9 * SclY#)) - i, ExplosionColor)
    Next i
  
    For i As Short = 1 To CShort(16 * SclX#)
      If i < (8 * SclX#) Then CIRCLE((GorillaX(PlayerHit) + (3.5! * SclX#)) + XAdj, (GorillaY(PlayerHit) + (7 * SclY#)) + YAdj, ((8 * SclX#) + 1) - i, BACKATTR, Nothing, Nothing, -1.57!)
      CIRCLE((GorillaX(PlayerHit) + (3.5! * SclX#)) + XAdj, GorillaY(PlayerHit) + YAdj, i, (i Mod 2) + 1, Nothing, Nothing, -1.57!)
    Next i
  
    For i As Short = CShort(24 * SclX#) To 1 Step -1
      CIRCLE((GorillaX(PlayerHit) + (3.5! * SclX#)) + XAdj, GorillaY(PlayerHit) + YAdj, i, BACKATTR, Nothing, Nothing, -1.57!)
      For Count = 1S To 200S : EMPTYLOOP()
      Next
    Next i
  
    ExplodeGorilla = PlayerHit
  End Function
  
  
  'GetInputs:
  '  Gets user inputs at beginning of game
  'Parameters:
  '  Player1$, Player2$ - player names
  '  NumGames - number of games to play
  Sub GetInputs(ByRef Player1$, ByRef Player2$, ByRef NumGames As Short)
    Dim game$ = ""
    Dim grav$ = ""
    COLOR(7, 0)
    CLS()
  
    LOCATE(8, 15)
    LINE_INPUT("Name of Player 1 (Default = 'Player 1'): ", Player1$)
    If Player1$ = "" Then
      Player1$ = "Player 1"
    Else
      Player1$ = Left$(Player1$, 10)
    End If
  
    LOCATE(10, 15)
    LINE_INPUT("Name of Player 2 (Default = 'Player 2'): ", Player2$)
    If Player2$ = "" Then
      Player2$ = "Player 2"
    Else
      Player2$ = Left$(Player2$, 10)
    End If
  
    Do
      LOCATE(12, 56) : PRINT(Space$(25), True)
      LOCATE(12, 13)
      INPUT("Play to how many total points (Default = 3)", game$)
      NumGames = CShort(Val(Left$(game$, 2)))
    Loop Until ((NumGames > 0) And (Len(game$) < 3)) Or (Len(game$) = 0)
    If NumGames = 0 Then NumGames = 3
  
    Do
      LOCATE(14, 53) : PRINT(Space$(28), True)
      LOCATE(14, 17)
      INPUT("Gravity in Meters/Sec (Earth = 9.8)", grav$)
      gravity# = Val(grav$)
    Loop Until (gravity# > 0) Or (Len(grav$) = 0)
    If gravity# = 0 Then gravity# = 9.8!
  End Sub
  
  
  'GetNum:
  '  Gets valid numeric input from user
  'Parameters:
  '  Row, Col - location to echo input
  Function GetNum#(ByRef Row As Short, ByRef Col As Short)
    Dim Kbd$ = ""
    Dim Result$ = ""
    Dim Done As Short = [FALSE]
    While INKEY$() <> "" : End While  'Clear keyboard buffer
  
    Do While (Not Done) <> 0
  
      LOCATE(Row, Col)
      PRINT(Result$ & QBChr(95) & "    ", True)
  
      Kbd$ = INKEY$()
      Select Case Kbd$
        Case "0" To "9"
          Result$ = Result$ + Kbd$
        Case "."
          If InStr(Result$, ".") = 0 Then
            Result$ = Result$ + Kbd$
          End If
        Case QBChr(13)
          If Val(Result$) > 360 Then
            Result$ = ""
          Else
            Done = [TRUE]
          End If
        Case QBChr(8)
          If Len(Result$) > 0 Then
            Result$ = Left$(Result$, Len(Result$) - 1)
          End If
        Case Else
          If Len(Kbd$) > 0 Then
            BEEP()
          End If
        End Select
    Loop
  
    LOCATE(Row, Col)
    PRINT(Result$ & " ", True)
  
    GetNum# = Val(Result$)
  End Function
  
  
  'GorillaIntro:
  '  Displays gorillas on screen for the first time
  '  allows the graphical data to be put into an array
  'Parameters:
  '  Player1$, Player2$ - The names of the players
  '
  Sub GorillaIntro(ByRef Player1$, ByRef Player2$)
    Dim Char_$ = ""
    Dim y As Short
    Dim P$ = ""
    LOCATE(16, 34) : PRINT("--------------")
    LOCATE(18, 34) : PRINT("V = View Intro")
    LOCATE(19, 34) : PRINT("P = Play Game")
    LOCATE(21, 35) : PRINT("Your Choice?")
  
    Do While Char_$ = ""
      Char_$ = INKEY$()
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
  
    VIEW_PRINT(9, 24)
  
    If Mode = 9 Then PALETTE(OBJECTCOLOR, BackColor)
  
    DrawGorilla(x, y, ARMSDOWN)
    CLS(2)
    DrawGorilla(x, y, LEFTUP)
    CLS(2)
    DrawGorilla(x, y, RIGHTUP)
    CLS(2)
  
    VIEW_PRINT(1, 25)
    If Mode = 9 Then PALETTE(OBJECTCOLOR, 46)
  
    If UCase$(Char_$) = "V" Then
      Center(2, "Q B A S I C   G O R I L L A S")
      Center(5, "             STARRING:               ")
      P$ = (Player1$ + " AND ") + Player2$
      Center(7, P$)
  
      PUT(x - 13, y, GorD%, PutOption.PUT_PSET)
      PUT(x + 47, y, GorD%, PutOption.PUT_PSET)
      Rest(1)
  
      PUT(x - 13, y, GorL%, PutOption.PUT_PSET)
      PUT(x + 47, y, GorR%, PutOption.PUT_PSET)
      PLAY("t120o1l16b9n0baan0bn0bn0baaan0b9n0baan0b")
      Rest(.3!)
  
      PUT(x - 13, y, GorR%, PutOption.PUT_PSET)
      PUT(x + 47, y, GorL%, PutOption.PUT_PSET)
      PLAY("o2l16e-9n0e-d-d-n0e-n0e-n0e-d-d-d-n0e-9n0e-d-d-n0e-")
      Rest(.3!)
  
      PUT(x - 13, y, GorL%, PutOption.PUT_PSET)
      PUT(x + 47, y, GorR%, PutOption.PUT_PSET)
      PLAY("o2l16g-9n0g-een0g-n0g-n0g-eeen0g-9n0g-een0g-")
      Rest(.3!)
  
      PUT(x - 13, y, GorR%, PutOption.PUT_PSET)
      PUT(x + 47, y, GorL%, PutOption.PUT_PSET)
      PLAY("o2l16b9n0baan0g-n0g-n0g-eeen0o1b9n0baan0b")
      Rest(.3!)
  
      For i = 1S To 4S
        PUT(x - 13, y, GorL%, PutOption.PUT_PSET)
        PUT(x + 47, y, GorR%, PutOption.PUT_PSET)
        PLAY("T160O0L32EFGEFDC")
        Rest(.1!)
        PUT(x - 13, y, GorR%, PutOption.PUT_PSET)
        PUT(x + 47, y, GorL%, PutOption.PUT_PSET)
        PLAY("T160O0L32EFGEFDC")
        Rest(.1!)
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
    CLS()
  
    Center(4, "Q B a s i c    G O R I L L A S")
    COLOR(7)
    Center(6, "Copyright (C) Microsoft Corporation 1990")
    Center(8, "Your mission is to hit your opponent with the exploding")
    Center(9, "banana by varying the angle and power of your throw, taking")
    Center(10, "into account wind speed, gravity, and the city skyline.")
    Center(11, "The wind speed is shown by a directional arrow at the bottom")
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
  Sub MakeCityScape(ByRef BCoor() As XYPoint)
    Dim NewHt As Short
    Dim BottomLine As Short
    Dim HtInc As Short
    Dim DefBWidth As Short
    Dim RandomHeight As Short
    Dim WWidth As Short
    Dim WHeight As Short
    Dim WDifV As Short
    Dim WDifh As Short
    Dim BWidth As Short
    Dim BHeight As Short
    Dim MaxHeight As Short
    Dim BuildingColor As Short
    Dim BACKGROUND As Short
    Dim c As Short
    Dim WinColr As Short
    Dim WindLine As Short
    Dim ArrowDir As Short
  
    x = 2
  
    'Set the sloping trend of the city scape. NewHt is new building height
    Dim Slope As Short = FnRan(6)
    Select Case Slope
      Case 1 : NewHt = 15  'Upward slope
      Case 2 : NewHt = 130  'Downward slope
      Case 3 To 5 : NewHt = 15  '"V" slope - most common
      Case 6 : NewHt = 130  'Inverted "V" slope
    End Select
  
    If Mode = 9 Then
      BottomLine = 335  'Bottom of building
      HtInc = 10  'Increase value for new height
      DefBWidth = 37  'Default building height
      RandomHeight = 120  'Random height difference
      WWidth = 3  'Window width
      WHeight = 6  'Window height
      WDifV = 15  'Counter for window spacing - vertical
      WDifh = 10  'Counter for window spacing - horizontal
    Else
      BottomLine = 190
      HtInc = 6
      NewHt = CShort((NewHt * 20) \ 35)  'Adjust for CGA
      DefBWidth = 18
      RandomHeight = 54
      WWidth = 1
      WHeight = 2
      WDifV = 5
      WDifh = 4
    End If
  
    Dim CurBuilding = 1S
    Do
  
      Select Case Slope
        Case 1
          NewHt = NewHt + HtInc
        Case 2
          NewHt = NewHt - HtInc
        Case 3 To 5
          If x > (ScrWidth \ 2) Then
            NewHt = CShort(NewHt - (2 * HtInc))
          Else
            NewHt = CShort(NewHt + (2 * HtInc))
          End If
        Case 4
          If x > (ScrWidth \ 2) Then
            NewHt = CShort(NewHt + (2 * HtInc))
          Else
            NewHt = CShort(NewHt - (2 * HtInc))
          End If
      End Select
  
      'Set width of building and check to see if it would go off the screen
      BWidth = FnRan(DefBWidth) + DefBWidth
      If (x + BWidth) > ScrWidth Then BWidth = CShort((ScrWidth - x) - 2)
  
      'Set height of building and check to see if it goes below screen
      BHeight = FnRan(RandomHeight) + NewHt
      If BHeight < HtInc Then BHeight = HtInc
  
      'Check to see if Building is too high
      If (BottomLine - BHeight) <= (MaxHeight + GHeight) Then BHeight = CShort((MaxHeight + GHeight) - 5)
  
      'Set the coordinates of the building into the array
      BCoor(CurBuilding).XCoor = x
      BCoor(CurBuilding).YCoor = BottomLine - BHeight
  
      If Mode = 9 Then BuildingColor = CShort(FnRan(3) + 4) Else BuildingColor = 2
  
      'Draw the building, outline first, then filled
      LINE(x - 1, BottomLine + 1, (x + BWidth) + 1, (BottomLine - BHeight) - 1, BACKGROUND, LineOption.B)
      LINE(x, BottomLine, x + BWidth, BottomLine - BHeight, BuildingColor, LineOption.BF)
  
      'Draw the windows
      c = CShort(x + 3)
      Do
        For i As Short = CShort(BHeight - 3) To 7 Step -WDifV
          If Mode <> 9 Then
            WinColr = CShort((FnRan(2) - 2) * (-3))
          ElseIf FnRan(4) = 1 Then
            WinColr = 8
          Else
            WinColr = WINDOWCOLOR
          End If
          LINE(c, BottomLine - i, c + WWidth, (BottomLine - i) + WHeight, WinColr, LineOption.BF)
        Next
        c = c + WDifh
      Loop Until c >= ((x + BWidth) - 3)
  
      x = CShort((x + BWidth) + 2)
  
      CurBuilding = CShort(CurBuilding + 1)
  
    Loop Until x > (ScrWidth - HtInc)
  
    LastBuilding = CShort(CurBuilding - 1)
  
    'Set Wind speed
    Wind = CShort(FnRan(10) - 5)
    If FnRan(3) = 1 Then
      If Wind > 0 Then
        Wind = Wind + FnRan(10)
      Else
        Wind = Wind - FnRan(10)
      End If
    End If
  
    'Draw Wind speed arrow
    If Wind <> 0 Then
      WindLine = CShort((Wind * 3) * (ScrWidth \ 320))
      LINE(ScrWidth \ 2, ScrHeight - 5, (ScrWidth \ 2) + WindLine, ScrHeight - 5, ExplosionColor)
      If Wind > 0 Then ArrowDir = -2 Else ArrowDir = 2
      LINE((ScrWidth / 2) + WindLine, ScrHeight - 5, ((ScrWidth / 2) + WindLine) + ArrowDir, (ScrHeight - 5) - 2, ExplosionColor)
      LINE((ScrWidth / 2) + WindLine, ScrHeight - 5, ((ScrWidth / 2) + WindLine) + ArrowDir, (ScrHeight - 5) + 2, ExplosionColor)
    End If
  End Sub
  
  
  'PlaceGorillas:
  '  PUTs the Gorillas on top of the buildings.  Must have drawn
  '  Gorillas first.
  'Parameters:
  '  BCoor() - user-defined TYPE array which stores upper left coordinates
  '  of each building.
  Sub PlaceGorillas(ByRef BCoor() As XYPoint)
    Dim XAdj As Short
    Dim YAdj As Short
    Dim BNum As Short
    Dim BWidth As Short
  
    If Mode = 9 Then
      XAdj = 14
      YAdj = 30
    Else
      XAdj = 7
      YAdj = 16
    End If
    Dim SclX# = ScrWidth / 320
    Dim SclY# = ScrHeight / 200
  
    'Place gorillas on second or third building from edge
    For i = 1S To 2S
      If i = 1 Then BNum = CShort(FnRan(2) + 1) Else BNum = LastBuilding - FnRan(2)
  
      BWidth = BCoor(BNum + 1).XCoor - BCoor(BNum).XCoor
      GorillaX(i) = CShort((BCoor(BNum).XCoor + (BWidth / 2)) - XAdj)
      GorillaY(i) = BCoor(BNum).YCoor - YAdj
      PUT(GorillaX(i), GorillaY(i), GorD%, PutOption.PUT_PSET)
    Next i
  
  End Sub
  
  
  'PlayGame:
  '  Main game play routine
  'Parameters:
  '  Player1$, Player2$ - player names
  '  NumGames - number of games to play
  Sub PlayGame(ByRef Player1$, ByRef Player2$, ByRef NumGames As Short)
    Dim Hit As Short
    Dim Tosser As Short
    Dim Tossee As Short
    Dim BCoor(30) As XYPoint
    Dim TotalWins(2) As Short
  
    Dim J = 1S
  
    For i As Short = 1 To NumGames
  
      CLS()
      RANDOMIZE(CInt((Timer)))
      MakeCityScape(BCoor)
      PlaceGorillas(BCoor)
      DoSun(SUNHAPPY)
      Hit = [FALSE]
      Do While Hit = [FALSE]
        J = CShort(1 - J)
        LOCATE(1, 1)
        PRINT(Player1$)
        LOCATE(1, ((MaxCol - 1) - Len(Player2$)))
        PRINT(Player2$)
        Center(23, (LTrim$(Str$(TotalWins(1))) + ">Score<") + LTrim$(Str$(TotalWins(2))))
        Tosser = CShort(J + 1) : Tossee = CShort(3 - J)
  
        'Plot the shot.  Hit is true if Gorilla gets hit.
        Hit = DoShot(Tosser, GorillaX(Tosser), GorillaY(Tosser))
  
        'Reset the sun, if it got hit
        If SunHit <> 0 Then DoSun(SUNHAPPY)
  
        If Hit = [TRUE] Then UpdateScores(TotalWins, Tosser, Hit)
      Loop
      SLEEP(1)
    Next i
  
    SCREEN(0)
    WIDTH(80, 25)
    COLOR(7, 0)
    MaxCol = 80
    CLS()
  
    Center(8, "GAME OVER!")
    Center(10, "Score:")
    LOCATE(11, 30) : PRINT(Player1$ & TAB(50) & Str$(TotalWins(1)) & " ")
    LOCATE(12, 30) : PRINT(Player2$ & TAB(50) & Str$(TotalWins(2)) & " ")
    Center(24, "Press any key to continue")
    SparklePause()
    COLOR(7, 0)
    CLS()
  End Sub
  
  
  'PlayGame:
  '  Plots banana shot across the screen
  'Parameters:
  '  StartX, StartY - starting shot location
  '  Angle - shot angle
  '  Velocity - shot velocity
  '  PlayerNum - the banana thrower
  Function PlotShot(ByRef StartX As Short, ByRef StartY As Short, ByRef Angle#, ByRef Velocity As Short, ByRef PlayerNum As Short) As Short
    Dim direction As Short
    Dim x_Dbl#
    Dim y#
    Dim pointval As Short
    Dim oldrot As Short
    Dim t#
    Dim LookY As Short
    Dim LookX As Short
    Dim rot As Short
  
    Angle# = (Angle# / 180) * pi#  'Convert degree angle to radians
    Dim Radius As Short = CShort(Mode Mod 7)
  
    Dim InitXVel# = Cos(Angle#) * Velocity
    Dim InitYVel# = Sin(Angle#) * Velocity
  
    Dim oldx# = StartX
    Dim oldy# = StartY
  
    'draw gorilla toss
    If PlayerNum = 1 Then
      PUT(StartX, StartY, GorL%, PutOption.PUT_PSET)
    Else
      PUT(StartX, StartY, GorR%, PutOption.PUT_PSET)
    End If
  
    'throw sound
    PLAY("MBo0L32A-L64CL16BL64A+")
    Rest(.1!)
  
    'redraw gorilla
    PUT(StartX, StartY, GorD%, PutOption.PUT_PSET)
  
    Dim adjust As Short = Scl(4)  'For scaling CGA
  
    Dim xedge As Short = CShort(Scl(9) * (2 - PlayerNum))  'Find leading edge of banana for check
  
    Dim Impact As Short = [FALSE]
    Dim ShotInSun As Short = [FALSE]
    Dim OnScreen As Short = [TRUE]
    Dim PlayerHit = 0S
    Dim NeedErase As Short = [FALSE]
  
    Dim StartXPos As Short = StartX
    Dim StartYPos As Short = CShort((StartY - adjust) - 3)
  
    If PlayerNum = 2 Then
      StartXPos = StartXPos + Scl(25)
      direction = Scl(4)
    Else
      direction = Scl(-4)
    End If
  
    If Velocity < 2 Then  'Shot too slow - hit self
      x_Dbl# = StartX
      y# = StartY
      pointval = OBJECTCOLOR
    End If
  
    Do While ((Not Impact) And OnScreen) <> 0
  
    Rest(.02!)
  
    'Erase old banana, if necessary
    If NeedErase <> 0 Then
      NeedErase = [FALSE]
      DrawBan(oldx#, oldy#, oldrot, [FALSE])
    End If
  
    x_Dbl# = (StartXPos + (InitXVel# * t#)) + ((.5! * (Wind / 5)) * (t# ^ 2))
    y# = StartYPos + ((((-1) * (InitYVel# * t#)) + ((.5! * gravity#) * (t# ^ 2))) * (ScrHeight / 350))
  
    If ((x_Dbl# >= (ScrWidth - Scl(10))) Or (x_Dbl# <= 3)) Or (y# >= (ScrHeight - 3)) Then
      OnScreen = [FALSE]
    End If
  
  
    If (OnScreen And CShort(y# > 0)) <> 0 Then
  
      'check it
      LookY = 0
      LookX = Scl(8 * (2 - PlayerNum))
      Do
        pointval = CShort(POINT(x_Dbl# + LookX, y# + LookY))
        If pointval = 0 Then
          Impact = [FALSE]
          If ShotInSun = [TRUE] Then
            If (Abs((ScrWidth \ 2) - x_Dbl#) > Scl(20)) Or (y# > SunHt) Then ShotInSun = [FALSE]
          End If
        ElseIf (pointval = SUNATTR) And (y# < SunHt) Then
          If (Not SunHit) <> 0 Then DoSun(SUNSHOCK)
          SunHit = [TRUE]
          ShotInSun = [TRUE]
        Else
          Impact = [TRUE]
        End If
        LookX = LookX + direction
        LookY = LookY + Scl(6)
      Loop Until (Impact Or CShort(LookX <> Scl(4))) <> 0
  
      If ((Not ShotInSun) And (Not Impact)) <> 0 Then
        'plot it
        rot = CShort(CInt((t# * 10)) Mod 4)
        DrawBan(x_Dbl#, y#, rot, [TRUE])
        NeedErase = [TRUE]
      End If
  
      oldx# = x_Dbl#
      oldy# = y#
      oldrot = rot
  
    End If
  
  
    t# = t# + .1!
  
    Loop
  
    If (CShort(pointval <> OBJECTCOLOR) And Impact) <> 0 Then
      DoExplosion(x_Dbl# + adjust, y# + adjust)
    ElseIf pointval = OBJECTCOLOR Then
      PlayerHit = ExplodeGorilla(x_Dbl#, y#)
    End If
  
    PlotShot = PlayerHit
  
  End Function
  
  
  'Rest:
  '  pauses the program
  Sub Rest(ByRef t#)
    Dim s# = Timer
    Dim t2# = (MachSpeed * t#) / SPEEDCONST
    Do
    EMPTYLOOP() : Loop Until (Timer - s#) > t2#
  End Sub
  
  
  'Scl:
  '  Pass the number in to scaling for cga.  If the number is a decimal, then we
  '  want to scale down for cga or scale up for ega.  This allows a full range
  '  of numbers to be generated for scaling.
  '  (i.e. for 3 to get scaled to 1, pass in 2.9)
  Function Scl(ByRef n!) As Short
  
    If n! <> Int(n!) Then
        If Mode = 1 Then n! = n! - 1
    End If
    If Mode = 1 Then
        Scl = CShort((n! / 2) + .1!)
    Else
        Scl = CShort(n!)
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
      PALETTE(9, 63)  'Display Color
    Else
      ExplosionColor = 2
      BackColor = 0
      COLOR(BackColor, 2)
  
    End If
  
  End Sub
  
  
  'SparklePause:
  '  Creates flashing border for intro and game over screens
  Sub SparklePause()
    Dim A_Int As Short
    Dim c As Short
  
    COLOR(4, 0)
    Dim A$ = "*    *    *    *    *    *    *    *    *    *    *    *    *    *    *    *    *    "
    While INKEY$() <> "" : End While  'Clear keyboard buffer
  
    While INKEY$() = ""
      For A_Int = 1 To 5
        LOCATE(1, 1)  'print horizontal sparkles
        PRINT(Mid$(A$, A_Int, 80), True)
        LOCATE(22, 1)
        PRINT(Mid$(A$, 6 - A_Int, 80), True)
  
        For b = 2S To 21S  'Print Vertical sparkles
          c = CShort((A_Int + b) Mod 5)
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
      Next A_Int
    End While
  End Sub
  
  
  'UpdateScores:
  '  Updates players' scores
  'Parameters:
  '  Record - players' scores
  '  PlayerNum - player
  '  Results - results of player's shot
  Sub UpdateScores(ByRef Record() As Short, ByRef PlayerNum As Short, ByRef Results As Short)
    If Results = HITSELF Then
      Record(Abs(PlayerNum - 3)) = CShort(Record(Abs(PlayerNum - 3)) + 1)
    Else
      Record(PlayerNum) = CShort(Record(PlayerNum) + 1)
    End If
  End Sub
  
  
  'VictoryDance:
  '  gorilla dances after he has eliminated his opponent
  'Parameters:
  '  Player - which gorilla is dancing
  Sub VictoryDance(ByRef Player As Short)
  
    For i# = 1 To 4
      PUT(GorillaX(Player), GorillaY(Player), GorL%, PutOption.PUT_PSET)
      PLAY("MFO0L32EFGEFDC")
      Rest(.2!)
      PUT(GorillaX(Player), GorillaY(Player), GorR%, PutOption.PUT_PSET)
      PLAY("MFO0L32EFGEFDC")
      Rest(.2!)
    Next
  End Sub
  
End Module
