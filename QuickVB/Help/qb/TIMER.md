# TIMER

Returns the number of seconds since midnight (function) or enables/disables timer vent trapping (statements).

## Syntax

v=`TIMER`

`TIMER ON`

`TIMER OFF`

`TIMER STOP`

## Comments

You can use `TIMER` with the [RANDOMIZE](RANDOMIZE) statement to seed the random number generator.

`TIMER ON` enables timer event trapping.

`TIMER OFF` disables timer event trapping. Timer events that occur are ignored.

`TIMER STOP` temporarily suspends timer event trapping. Events that occur are processed after trapping is enabled.

## Example

```vb
RANDOMIZE TIMER
```

```vb
ON TIMER(10) GOSUB Handler
TIMER ON
DO
LOOP UNTIL INKEY$ <> "" 
END

Handler:
  PRINT TIMES
  RETURN
```

## See Also

- [ON...GOSUB](ON...GOSUB)
