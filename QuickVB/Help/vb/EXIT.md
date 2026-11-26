# EXIT Statement

<!-- keywords: EXIT -->

Leaves a procedure or a block early.

## Syntax

`EXIT` { `SUB` | `FUNCTION` | `FOR` | `DO` | `WHILE` | `SELECT` | `TRY` }

## Comments

The program goes on with the statement after the block that was left.

## Example

```vb
FOR i = 1 TO 100
  IF i * i > 50 THEN EXIT FOR
NEXT
```

## See Also

- [FOR...NEXT](FOR-NEXT), [DO...LOOP](DO-LOOP), [SUB](SUB), [FUNCTION](FUNCTION)

## Online

- [Microsoft Learn: Exit statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/exit-statement)
