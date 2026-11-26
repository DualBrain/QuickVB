# CONTINUE Statement

<!-- keywords: CONTINUE -->

Goes on with the next round of a loop.

## Syntax

`CONTINUE` { `FOR` | `DO` | `WHILE` }

## Comments

The rest of the statements of the loop are skipped for this round.

## Example

```vb
FOR i = 1 TO 5
  IF i = 3 THEN CONTINUE FOR
  PRINT(i)
NEXT
```

Results in:

```text
1
 2
 4
 5
```

## See Also

- [FOR...NEXT](FOR-NEXT), [DO...LOOP](DO-LOOP), [EXIT](EXIT)

## Online

- [Microsoft Learn: Continue statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/continue-statement)
