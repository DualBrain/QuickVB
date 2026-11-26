# RTRIM$

Removes trailing spaces from a string expression.

## Syntax

`RTRIM$`(*string_expression*)

## Comments

*string_expression* is any string expression.

## Example

```vb
a$ = "AAAA    "
b$ = "BBBB"
PRINT RTRIM$(a$); b$
```

Resulting in:

```txt
AAAABBBB
```

## See Also

- [LTRIM$](LTRIM$)
