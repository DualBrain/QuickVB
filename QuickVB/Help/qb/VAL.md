# VAL

Converts a string representation of a numeric value to the actual numeric value.

## Syntax

`VAL`(*string_expression*)

## Comments

*string_expression* is the string representation of a numeric value.

`VAL` stops at the first character it cannot recognize as part of a number. Valid characters are 0 through 9, the period (.), the minus sign (-) and the plus sign (+).

## Example

```vb
PRINT VAL("33.44")
PRINT VAL("88k")
```

Results in:

```txt
 33.44
 88
```

## See Also

- [STR$](STR$)
