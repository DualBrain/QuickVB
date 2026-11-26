# MID$

Returns a substring of a string expression that begins at the specified offset location.

## Syntax

`MID$`(*string_expression*,*start_offset*[ ,*length*])

## Comments

*string_expression* is any string expression.

*start_offset* is the position of the first character of the substring.

*length* is the number of characters in the substring. If you omit *length*, `MID$` returns all characters from *start_offset* through the end of the string.

## Example

```vb
a$ = "ABCDEFGHI"
PRINT MID$(a$, 1, 5)
PRINT MID$(a$, 6)
```

Results in:

```text
ABCDE
FGHI
```

## See Also

- [INSTR](INSTR), [LEFT$](LEFT$), [LEN](LEN), [RIGHT$](RIGHT$)
