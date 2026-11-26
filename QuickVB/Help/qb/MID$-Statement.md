# MID$ (Statement)

Replaces a portion of a string with another string.

## Syntax

`MID$`(*string_variable*,*start_offset*[ ,*num_char*])=*string_expression*

## Comments

*string_variable* is the string variable to be modified.

*start_offset* is the position of the first character to be replaced in the string variable.

*num_char* is the number of characters in the string to replace. If you omit this value, `MID$` uses the length of the replacement string.

*string_expression* is any string expression.

## Example

```vb
a$ = "ABCDEF"
MID$(a$, 2, 2) = "bc"
PRINT a$
```

```text
ABcDEF
```

## See Also

- [INSTR](INSTR), [LEFT$](LEFT$), [LEN](LEN), [RIGHT$](RIGHT$)
