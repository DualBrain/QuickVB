# FOR...NEXT Statement

<!-- keywords: FOR TO STEP NEXT -->

Repeats a block of statements a specified number of times.

## Syntax

`FOR` *counter* [ `AS` *type* ] = *start* `TO` *end* [ `STEP` *increment* ]
  [ *statements* ]
`NEXT` [ *counter* ]

## Comments

*counter* is the variable that counts. It starts at *start* and is changed by *increment* (1 when `STEP` is left out) each time round, until it passes *end*.

`EXIT FOR` leaves the loop; `CONTINUE FOR` goes on with the next count.

A loop that counts down uses a negative `STEP`. The statements do not run at all when *start* is already past *end*.

## Example

```vb
FOR i = 1 TO 10 STEP 3
  PRINT(i)
NEXT i
```

Results in:

```text
1
 4
 7
 10
```

## See Also

- [FOR EACH...NEXT](FOR-EACH-NEXT), [DO...LOOP](DO-LOOP), [EXIT](EXIT)

## Online

- [Microsoft Learn: For...Next statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/for-next-statement)
