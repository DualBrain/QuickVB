# GOSUB

Directs execution to continue at a BASIC subroutine.

## Syntax

`GOSUB` *location*

## Comments

*location* is either a line number or label at which execution should continue.

USe a [RETURN](RETURN) statement to end a subroutine.

A subroutine may be called any number of times in a program, and a subroutine may be called from within another subroutine. Such nesting of subroutines is limited only by available memory.

Subroutines can appear anywhere in the program, but must be readily distinguishable from the main program.

To prevent inadvertent entry, precede the subroutine by a [STOP](STOP), [END](END), or [GOTO](GOTO) statement to direct program control around the subroutine.

`GOSUB` is the older method of accessing subroutines. Most new programs use QBasic [SUB](SUB) and [CALL](CALL) statements.

```vb
GOSUB Test
END
Test:
  PRINT "In subroutine Test"
  RETURN
```

## See Also

- [ON...GOSUB](ON...GOSUB), [ON](ON), [RETURN](RETURN), [SUB](SUB)
