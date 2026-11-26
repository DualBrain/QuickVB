# VARPTR$

Returns a string representation of a variable's offset for use in the [PLAY](PLAY) and [DRAW](DRAW) statements.

## Syntax

`VARPTR$`(*string_variable*)

## Comments

*string_variable* is a string variable containing [DRAW](DRAW) or [PLAY](PLAY) commands.

QBasic does not guarantee that a variable will reside in the same memory location throughout the program execution. Use `VARPTR$` immediately before any code that uses the address.

## Example

```vb
scale$ = "CDEFGAB
PLAY "X" + VARPTR$(scale$)
```

## See Also

- [DRAW](DRAW), [PLAY](PLAY)
