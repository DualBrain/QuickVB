# SPC

Skips the specified number of spaces in a [PRINT](PRINT) or [LPRINT](PRINT) statement.

## Syntax

`SPC`(*num_spaces*)

## Comments

*num_spaces* is an integer value (from 0 through 32,767).

A semicolon is assumed to follow the `SPC(n)` command.

`SPC` may only be used with [PRINT](PRINT), [LPRINT](LPRINT) and [PRINT#](PRINT-FILE) statements (see the [SPACE$](SPACE$) function).

## Example

```vb
FOR i = 0 TO 5
  PRINT SPC(i); i
NEXT
```

## See Also

- [SPACE$](SPACE$), [TAB](TAB)
