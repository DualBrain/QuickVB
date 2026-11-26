# LEN

Returns the number of characters in a string or the number of bytes used to store a variable.

## Syntax

`LEN`(*string_expression*)

`LEN`(*variable*)

## Comments

*string_expression* is any string expression.

*variable* is any variable of a type other than [STRING](STRING).

## Example

```vb
DIM x as INTEGER, y AS LONG
a$ = "13 CHARACTERS"
PRINT a$, LEN(a$)
PRINT "Integer"; LEN(x), "Long"; LEN(y)
```

Results in:

```text
13 CHARACTERS   13
Integer 2       Long 4
```

## See Also

- [INSTR](INSTR), [LEFT$](LEFT$), [MID$](MID$), [RIGHT$](RIGHT$)
