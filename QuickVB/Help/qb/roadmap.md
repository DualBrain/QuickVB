# Roadmap

Future updates for planned releases will be maintained here as the project progresses.

## 0.0.1-beta

The following has been implemented (to one degree or another):

- allow *line numbers*
- support *labels*
- case-insensitive
- [LET](LET) is optional; however, variable types are currently *inferred* based on initial expression type (determined)
- `+ - * / \ ^ () = < > >= <= <>`
- Operator Precedence ([IMP](IMP), [EQV](EQV), [XOR](XOR), [OR](OR), [AND](AND), [NOT](NOT), `= > >= < <= <>`, `+ - binary`, [MOD](MOD), `\`, `* /`, `+ - unary`, `^`)

While the following still needs to be implemented:

- add support for arrays
- add support for numeric suffix types (beyond string `$`)
- add support for different numeric types; currently only supports INT32

The keywords below have been implemented if presented as a link to the documentation; otherwise, still needs to be handled.

### Variables and Types

[CONST](CONST), DEFDBL, DEFINT, DEFLNG, DEFSNG, DEFSTR, DOUBLE, [HEX$](HEX$), INTEGER, [LET](LET), LONG, [OCT$](OCT$), SINGLE, STRING, [SWAP](SWAP), TYPE...END TYPE

### Flow Control

[DO...LOOP](DO...LOOP), EXIT, [FOR...NEXT](FOR...NEXT), [GOTO](GOTO), ON...GOSUB, ON...GOTO, [WHILE...WEND](WHILE...WEND)

### Decisions and Operators

[AND](AND), [EQV](EQV), [IF](IF), [IMP](IMP), [MOD](MOD), [NOT](NOT), [OR](OR), SELECT CASE, [XOR](XOR)

### Procedures

CALL, CHAIN, COMMON, DECLARE, [DEF FN](DEF-FN), [FUNCTION](FUNCTION), [GOSUB...RETURN](GOSUB...RETURN), SHARED, STATIC, SUB

### Strings

[ASC](ASC), [CHR$](CHR$), [INSTR](INSTR), [LCASE$](LCASE$), [LEFT$](LEFT$), [LEN](LEN), [MID$](MID$), [MID$ (Statement)](MID$-Statement), [RIGHT$](RIGHT$), [SPACE$](SPACE$), [STR$](STR$), [STRING$](STRING$), [UCASE$](UCASE$), [VAL](VAL)

### Array and Data

DATA, DIM, $DYNAMIC, ERASE, LBOUND, [OPTION BASE](OPTION-BASE), READ, REDIM, RESTORE, $STATIC, UBOUND

### Math

[ABS](ABS), [ATN](ATN), [CDBL](CDBL), [CINT](CINT), [CLNG](CLNG), [COS](COS), [CSNG](CSNG), [EXP](EXP), [FIX](FIX), [INT](INT), [LOG](LOG), RANDOMIZE, [RND](RND), [SGN](SGN), [SIN](SIN), [SQR](SQR), [TAN](TAN)

### Simple I/O

[CLS](CLS), CSRLIN, INKEY$, INPUT, INPUT$, LINE INPUT, [LOCATE](LOCATE), POS, [PRINT](PRINT), PRINT USING, [SPC](SPC), [TAB](TAB), VIEW PRINT, WIDTH, WRITE

### Error Trapping

ERDEV, ERDEV$, ERL, ERR, ERROR, ON ERROR GOTO, RESUME

### Timing, Date and Time

DATE$, ON TIMER GOSUB, SLEEP, TIME$, TIMER, TIMER ON/OFF/STOP

### Multimedia - Graphics

[CIRCLE](CIRCLE), [COLOR](COLOR), DRAW, GET, LINE, PAINT, PALETTE, PCOPY, PMAP, POINT, [PRESET](PRESET), [PSET](PSET), PUT, [SCREEN](SCREEN), VIEW, WINDOW

### Multimedia - Sound

BEEP, ON PLAY GOSUB, PLAY, PLAY (Statement), PLAY ON/OFF/STOP, SOUND

### Multimedia - Light Pen and Joystick

ON PEN GOSUB, ON STRIG GOSUB, PEN, PEN ON/OFF/STOP, STICK, STRIG, STRIG ON/OFF/STOP

### Devices - Keyboard

KEY, KEY(n) ON/OFF/STOP, ON KEY(n) GOSUB

### Devices - Printer

LPOS, LPRINT, LPRINT USING, WIDTH LPRINT

### Devices - Communications Port

COM ON/OFF/STOP
ON COM GOSUB
OPEN COM

### Devices - Files

CLOSE, CVD, CVDMBF, CVI, CVL, CVS, CVSMBF, EOF, FIELD, FILEATTR, FILES, FREEFILE, GET#, INPUT#, INPUT$, LINE INPUT #, LOC, LOCK, LOF, LSET, [LTRIM$](LTRIM$), MKD$, MKDMBF$, MKI$, MKL$, MKS$, MKSMBF$, NAME, OPEN, PRINT #, PRINT # USING, PUT#, RESET, RSET, [RTRIM$](RTRIM$), SEEK, SEEK (Statement), UNLOCK, WIDTH, WRITE#

### Development - DOS and Program Management

[CHDIR](CHDIR), [CLEAR](CLEAR), [END](END), ENVIRON, ENVIRON$, IOCTL, IOCTL$, [KILL](KILL), [MKDIR](MKDIR), [REM](REM), [RMDIR](RMDIR), [RUN](RUN), SHELL, [STOP](STOP), [SYSTEM](SYSTEM)

### Development - Port and Memory

BLOAD, BSAVE, FRE, INP, OUT, PEEK, POKE, WAIT

### Development - QBasic and Machine Language

CALL ABSOLUTE, DEF SEG, VARPTR, VARPTR$, VARSEG

### Development - Debugging

TROFF, TRON

---

### A

[ABS](ABS), ABSOLUTE, ACCESS, [AND](AND), ANY, APPEND, AS, [ASC](ASC), [ATN](ATN)

### B

[BASE](BASE), BEEP, BINARY, BLOAD, [Boolean Operators](boolean), BSAVE

### C

CALL, CALL ABSOLUTE, CASE, [CDBL](CDBL), CHAIN, [CHDIR](CHDIR), [CHR$](CHR$), [CINT](CINT), [CIRCLE](CIRCLE), [CLEAR](CLEAR), [CLNG](CLNG), CLOSE, [CLS](CLS), [COLOR](COLOR), COM(n), COMMON, [CONST](CONST), [COS](COS), [CSNG](CSNG), CSRLIN, CVD, CVDMBF, CVI, CVL, CVS, CVSMBF

### D

DATA, DATE$, DECLARE, [DEF FN](DEF-FN), DEF SEG, DEFDBL, DEFINT, DEFLNG, DEFSNG, DEFSTR, DIM, [DO...LOOP](DO...LOOP), [DO UNTIL](DO-UNTIL), [DO WHILE](DO-WHILE), DOUBLE, DRAW

### E

ELSE, ELSEIF, [END](END), END IF, ENVIRON, ENVIRON$, EOF, [EQV](EQV), ERASE, ERDEV, ERDEV$, ERL, ERR, ERROR, EXIT, [EXP](EXP)

### F

FIELD, FILEATTR, FILES, [FIX](FIX), [FOR...NEXT](FOR...NEXT), FRE, FREEFILE, [FUNCTION](FUNCTION)

### G

GET, GET#, [GOSUB](GOSUB), [GOTO](GOTO)

### H

[HEX$](HEX$)

### I

[IF...THEN](IF...THEN), [IMP](IMP), INKEY$, INP, [INPUT](INPUT), INPUT#, INPUT$, [INSTR](INSTR), [INT](INT), INTEGER, IOCTL, IOCTL$, IS

### K

KEY, KEY(n), [KILL](KILL)

### L

LBOUND, [LCASE$](LCASE$), [LEFT$](LEFT$), [LEN](LEN), [LET](LET), LINE, LINE INPUT, LINE INPUT#, LOC, [LOCATE](LOCATE), LOCK, LOF, [LOG](LOG), LONG, LOOP, LPOS, LPRINT, LPRINT USING, LSET, [LTRIM$](LTRIM$)

### M

[MID$](MID$), [MID$ (Statement)](MID$-Statement), MKD$, [MKDIR](MKDIR), MKDMBF$, MKI$, MKL$, MKS$, MKSMBF$, [MOD](MOD)

### N

[NAME](NAME), [NEXT](NEXT), [NOT](NOT)

### O

[OCT$](OCT$), OFF, ON, ON COM(n), ON ERROR GOTO, ON KEY(n), ON PEN, ON PLAY(n), ON STRIG(n), ON TIMER(n), ON...GOSUB, ON...GOTO, [OPEN], OPEN "COM(n), [OPTION BASE](OPTION-BASE), [OR](OR), OUT, OUTPUT

### P

PAINT, PALETTE, PALETTE USING, PCOPY, PEEK, PEN, PLAY, PLAY(n), PMAP, POINT, POKE, POS, [PRESET](PRESET), [PRINT](PRINT), PRINT USING, PRINT#, PRINT# USING, [PSET](PSET), PUT, PUT#

### R

RANDOM, RANDOMIZE, READ, REDIM, [REM](REM), RESET, RESTORE, RESUME, [RETURN](RETURN), [RIGHT$](RIGHT$), [RMDIR](RMDIR), [RND](RND), RSET, [RTRIM$](RTRIM$), [RUN](RUN)

### S

[SCREEN](SCREEN), SCREEN Function, SEEK, SEEK (Statement), SELECT, [SGN](SGN), SHARED, SHELL, [SIN](SIN), SINGLE, SLEEP, SOUND, [SPACE$](SPACE$), [SPC](SPC), [SQR](SQR), STATIC, [STEP](STEP), STICK, [STOP](STOP), [STR$](STR$), STRIG, STRIG(n), STRING, [STRING$](STRING$), SUB, [SWAP](SWAP), [SYSTEM](SYSTEM)

### T

[TAB](TAB), [TAN](TAN), [THEN](THEN), TIME$, TIMER, [TO](TO), TRON, TROFF, TYPE

### U

UBOUND, [UCASE$](UCASE$), UNLOCK, UNTIL, USING

### V

[VAL](VAL), VARPTR, VARPTR$, VARSEG, VIEW, VIEW PRINT

### W

WAIT, [WHILE...WEND](WHILE...WEND), WIDTH, WINDOW, WRITE

### X

[XOR](XOR)
