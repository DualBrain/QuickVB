# RIGHT$

Returns the specified number of characters from the rightmost characters in a string.

## Syntax

`RIGHT$`(*string_expression*,*num_char*)

## Comments

*string_expression* is any string expression.

*num_char* is the number of characters to return. If *num_char* exceeds the length of the string, `RIGHT$` returns the entire string.

## Example

```vb
A$ = "ABCDEFGHIJ"
FOR I = 1 TO 10
  PRINT RIGHT$(A$, I)
NEXT I

```

## See Also

- [INSTR](INSTR), [LEFT$](LEFT$), [LEN](LEN), [MID$](MID$)
