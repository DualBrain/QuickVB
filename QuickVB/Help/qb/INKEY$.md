# INKEY$

Reads a character from the keyboard.

## Syntax

v$=`INKEY$`

## Comments

`INKEY$` returns a null string if no character is present, a 1-byte string for standard keys and a 2-byte string for extended keys.

For extended keys, the first character is a null character (ASCII 0) and the second is the keyboard scan code.

`INKEY$` does not echo the character to the screen.

If several characters are pending, only the first is returned.

Two character strings are used to return the extended codes described in [Appendix C](scancodes) of the BASIC User's Guide.

All characters except the following are passed to the program:

CTRL-BREAK
CTRL-NUM LOCK
CTRL-ALT-DEL
CTRL-PRTSCR
PRTSCR

## Example

```vb
PRINT "Press a series of keys - F10 to stop"
DO
  DO
    k$ = INKEY$
  LOOP WHILE k$ = ""
  IF LEN(k$) = 1 THEN
    PRINT "Letter", k$
  ELSE
    PRINT "Scan code", ASC(MID$(k$, 2, 1))
  END IF
LOOP UNTIL MID$(k$, 2, 1) = CHR$(68) ' F10 scan code
```

## See Also

- [ASC](ASC), [CHR$](CHR$), [LEN](LEN)
