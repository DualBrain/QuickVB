# VARPTR

Returns a variable's offset in memory.

## Syntax

`VARPTR`(*variable*)

## Comments

*variable* is the name of any variable in your program.

QBasic does not guarantee that a variable will reside in the same memory location throughout the program execution. Use `VARPTR` immediately before any code that uses the offset value.

## Examples

See [CALL ABSOLUTE](CALL-ABSOLUTE).

## See Also

- [DEF SEG](DEF-SEG), [VARSEG](VARSEG)
