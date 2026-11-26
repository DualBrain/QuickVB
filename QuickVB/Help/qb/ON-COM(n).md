# ON COM(n)

Specify the first line of an event-trapping subroutine.

## Syntax

`ON COM`(*n*) `GOSUB` *location*

`ON KEY`(*n*) `GOSUB` *location*

`ON PEN GOSUB` *location*

`ON PLAY`(*queuesize*) `GOSUB` *location*

`ON STRIG`(*n*) `GOSUB` *location*

`ON TIMER`(*n*) `GOSUB` *location*

## Comments

The `ON COM(n) GOSUB` statement branches to the subroutine whenever characters are received at the specified serial port.

The `ON KEY(n) GOSUB` statement branches to the subroutine whenever the key associated with the specified number is pressed.

The `ON PEN GOSUB` statement branches to the subroutine whenever the light pen is activated.

The `ON PLAY(queuesize) GOSUB` statement branches to the subroutine whenever the number of notes in the music buffer goes from *queuesize* to *queuesize* - 1. The variable *queuesize* must be in the range 1 through 32.

The `ON STRIG(n) GOSUB` statement branches to the subroutine whenever the specified joystick button is pressed. See [STRIG](STRIG) statement for valid values for *n*.

The `ON TIMER(n) GOSUB` statement branches to the subroutine whenever the specified number of seconds have passed. The number of seconds must be in the range 1 through 86,400.

These statements do not enable event trapping; they only associate a subroutine with an event.

The syntax shown sets up an event trap line number for the specified event. A *location* of 0 disables trapping for this event.

Once trap *locations* have been set, event trapping itself can be controlled with the following syntax lines:

`event specifier ON`

When an event is ON, and a nonzero line number is specified for the trap, then every time BASIC starts a new statement, it checks to see if the specified event has occurred. If it has, BASIC performs a [GOSUB](GOSUB) to the line specified in the `ON` statement.

`event specifier OFF`

When an event is `OFF`, no trapping occurs and the event is not remembered, even if it occurs.

`event specifier STOP`

When an event is stopped, no trapping can occur, but if the event happens, it is remembered so an immediate trap occurs when an event specifier `ON` is executed.

When a trap is made for a particular event, the trap automatically causes a stop on that event, so recursive traps can never take place.

The return from the trap routine automatically does an `ON` unless an explicit `OFF` has been performed inside the trap routine.

When an error trap takes place, this automatically disables all trapping.

Trapping will never take place when BASIC is not executing a program.

The following are valid values for *event specifier*:

`COM(n)`

*n* is the number of the COM channel (1 or 2).

`KEY(n)`

*n* is a function key number 1-20. 1 through 10 are the function keys F1 through F10. 11 through 14 are the cursor control keys as follows:

* 11 = CURSOR UP
* 12 = CURSOR LEFT
* 13 = CURSOR RIGHT
* 14 = CURSOR DOWN
* 15-20 are user-defined keys.

`PEN`

Since there is only one pen, no number is given.

`PLAY(n)`

*n* is an integer expression in the range of 1-32. Values outside this range result in "Illegal function call" errors.

`STRIG(n)`
*n* is 0, 2, 4 or 6. (0=trigger A1; 4=trigger A2; 2=trigger B1; 6=trigger B2).

`TIMER(n)`

*n* is a numeric expression within the range of 1 to 86,400. A value outside of this range results in an "Illegal function call" error.

`RETURN line number`

This optional form of [RETURN](RETURN) is primarily intended for use with event trapping. The event-trapping routine may want to go back into the BASIC program at a fixed line number while still eliminating the [GOSUB](GOSUB) entry that the trap created.

Use of the nonlocal [RETURN](RETURN) must be done with care. Any other [GOSUB](GOSUB), [WHILE](WHILE), or [FOR](FOR) that was active at the time of the trap remains active.

If the trap comes out of a subroutine, any attempt to continue loops outside the subroutine results in a "NEXT without FOR" error.

## Special Notes about Each Type of Trap

### COM Trapping

Typically, the COM trap routine will read an entire message from the COM port before returning.

It is recommended that you not use the COM trap for single character messages, since at high baud rates the overhead of trapping and reading for each individual character may allow the interrupt buffer for COM to overflow.

### KEY Trapping

Trappable keys 15 to 20 are defined by the following statement:

`KEY(n),CHR$[hex code]+CHR$[scan code]`

*n* is an integer expression within the range of 15 to 20 defining the key to be trapped.

*hex code* is the mask for the latched key: (CAPS LOCK, NUM LOCK, ALT, CTRL, LEFT SHIFT, RIGHT SHIFT)

*scan code* is the number identifying one of the 83 keys to trap. Refer to Appendix H in the BASIC User's Guide for key scan codes.

The appropriate bit in hex code must be set in order to trap a key that is shifted, control-shifted, or alt-shifted. hex code values are as follows:

| Mask | Hex code | Indicates that |
| ---- | -------- | -------------- |
| EXTENDED | &H80 | Key is extended |
| CAPS LOCK | &H40 | CAPS LOCK is active |
| NUM LOCK | &H20 | NUM LOCK is active |
| ALT | &H08 | The ALT key is pressed |
| CTRL | &H04 | The CTRL key is pressed |
| LEFT SHIFT | &H02 | The left SHIFT key is pressed |
| RIGHT SHIFT | &H01 | The right SHIFT key is pressed |

For trapping shifted keys, you may use the value &H01, &H02, or &H03. The left and right SHIFT keys are coupled when &H03 is used.

Refer to the [KEY(n)](KEY(n)) statement for more information.

No type of trapping is activated when BASIC is in direct mode. Function keys resume their standard expansion meaning during input.

A key that causes a trap is not available for examination with the [INPUT](INPUT) or [INKEY$](INKEY$) statement, so the trap routine for each key must be different if a different function is desired.

If CTL-PRTSCR is trapped, the line printer echo toggle is processed first. Defining CTL-PRTSCR as a key trap does not prevent characters from being echoed to the printer if CTL-PRTSCR is pressed.

Function keys 1 through 14 are predefined. Therefore, setting scan codes 59-68, 72, 75, 77, or 80 has no effect.

### PLAY Trapping

A `PLAY` event trap is issued only when playing background music ([PLAY](PLAY)`"MB..`). `PLAY` event music traps are not issued when running in MUSIC foreground (default case, or [PLAY](PLAY)`"MF..`).

Choose conservative values for *n*. An `ON PLAY(32)`.. statement will cause event traps so often that there will be little time to execute the rest of your program.

The `ON PLAY(n)` statement causes an event trap when the background music queue goes from *n* to *n-1* notes.

### STRIG Trapping

Using `STRIG(n) ON` activates the interrupt routine that checks the trigger status. Down-strokes that cause trapping will not set `STRIG(0)`, `STRIG(2)`, `STRIG(4)`, or `STRIG(6)` functions.

### TIMER Trapping

An `ON TIMER(n)` event trapping statement is used with applications needing an internal timer. The trap occurs when *n* seconds have elapsed since the `TIMER ON` statement.

## Examples

This is a very simple terminal program.

```vb
10 REM "ON COM(n)" EXAMPLE
20 OPEN "COM1:9600, O, 7" AS #1
30 ON COM(1) GOSUB 80
40 COM(1) ON
50 REM TRANSMIT CHARACTERS FROM KEYBOARD
60 A$=INKEY$: IF A$=""THEN 50
70 PRINT #1, A$;: GOTO 50
80 REM DISPLAY RECEIVE CHARACTERS
90 ALL=LOC(1): IF ALL<1 THEN RETURN
100 B$=INPUT$(ALL, #1): PRINT B$;:RETURN
```

Prevents a CTRL-BREAK or system reset during a program.

```vb
10 KEY 15, CHR$(4)+CHR$(70) REM Trap ^BREAK
20 KEY 16, CHR$(12)+CHR$(83) REM Trap system reset
30 ON KEY(15) GOSUB 1000
40 ON KEY(16) GOSUB 2000
50 KEY(15) ON
60 KEY(16) ON
.
.
.
1000 PRINT "I'm sorry, I can't let you do that"
1010 RETURN
2000 ATTEMPS=ATTEMPS+1
2010 ON ATTEMPS GOTO 2100, 2200, 2300, 2400, 2500
2100 PRINT "Mary had a little lamb": RETURN
2200 PRINT "Its fleece was white as snow": RETURN
2300 PRINT "And everywhere that Mary went": RETURN
2400 PRINT "The lamb was sure to go": RETURN
2500 KEY(16) OFF REM If they hit us once more...
2510 RETURN REM then BASIC dies...
```

Displays the time of day on line 1 every minute.

```vb
10 ON TIMER(60) GOSUB 10000
20 TIMER ON
.
.
.
10000 OLDROW=CSRLIN REM Saves the current row
10010 OLDCOL=POS(0) REM Saves the current column
10020 LOCATE 1, 1: PRINT TIME$
10030 LOCATE OLDROW, OLDCOL REM Restores row and column
10040 RETURN
```
