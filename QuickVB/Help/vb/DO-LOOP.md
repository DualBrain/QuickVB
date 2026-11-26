# DO...LOOP Statement

<!-- keywords: DO LOOP UNTIL -->

Repeats a block of statements while a condition is true, or until it becomes true.

## Syntax

`DO` [ { `WHILE` | `UNTIL` } *condition* ]
  [ *statements* ]
`LOOP`

`DO`
  [ *statements* ]
`LOOP` [ { `WHILE` | `UNTIL` } *condition* ]

## Comments

With the condition after `DO` the statements may never run; with it after `LOOP` they run at least once.

`WHILE` repeats as long as the condition is True; `UNTIL` repeats as long as it is False.

`EXIT DO` leaves the loop; `CONTINUE DO` starts the next round.

## Example

```vb
DIM n AS INTEGER = 0
DO UNTIL n >= 3
  n += 1
  PRINT(n)
LOOP
```

Results in:

```text
1
 2
 3
```

## See Also

- [WHILE...END WHILE](WHILE-END-WHILE), [FOR...NEXT](FOR-NEXT), [EXIT](EXIT)

## Online

- [Microsoft Learn: Do...Loop statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/do-loop-statement)
