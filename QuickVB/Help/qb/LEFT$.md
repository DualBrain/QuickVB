# LEFT$

Returns the specified number of characters beginning from the leftmost character of a string.

## Syntax

`LEFT$`(*string_expression*,*num_char*)

## Comments

*string_expression* is any string expression.

*num_char* is the number of characters to extract from the string. It must be in the range 0 through 32,767.

## Example

```vb
s$ = "TEST STRING"
FOR i = 1 TO LEN(s$)
  PRINT LEFT$(s$, i)
NEXT i
```

## See Also

- [INSTR](INSTR), [LEN](LEN), [MID$](MID$), [RIGHT$](RIGHT$)
