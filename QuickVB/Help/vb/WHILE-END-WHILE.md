# WHILE...END WHILE Statement

<!-- keywords: WEND -->

Repeats a block of statements as long as a condition is true.

## Syntax

`WHILE` *condition*
  [ *statements* ]
`END WHILE`

## Comments

The condition is tested first, so the statements may never run. QuickBASIC ended this loop with `WEND`; Visual Basic uses `END WHILE`.

`EXIT WHILE` leaves the loop; `CONTINUE WHILE` starts the next round.

## Example

```vb
DIM n AS INTEGER = 0
WHILE n < 3
  n += 1
  PRINT(n)
END WHILE
```

## See Also

- [DO...LOOP](DO-LOOP), [FOR...NEXT](FOR-NEXT)

## Online

- [Microsoft Learn: While...End While statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/while-end-while-statement)
