# RETURN

Returns control from a subroutine to the calling procedure.

## Syntax

`RETURN` [ *location*]

## Comments

*location* is the line number or label at which execution should continue. If you omit *location*, execution continues at the line following the [GOSUB](GOSUB) statement or, for event handling, at the line at which the event occurred.

## Example

```vb
GOSUB One
GOSUB Two
END

One:
  PRINT "In One"
  RETURN

Two:
  PRINT "In Two"
  RETURN
```

## See Also

- [GOSUB](GOSUB), [ON...GOSUB](ON...GOSUB)
