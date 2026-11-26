# END

Ends a QBasic program.

## Syntax

`END`

## Comments

`END` by itself terminates your program and closes all files.

The `END` statement is also used with [DEF FN](DEF-FN), [FUNCTION](FUNCTION), [IF](IF), [SELECT](SELECT), [SUB](SUB) and [TYPE](TYPE). Those forms of `END` are discussed in relation to the appropriate statements.

`END` statements may be placed anywhere in the program to terminate execution.

An `END` statement at the end of a program is optional.

## Example

```vb
FOR i = 1 TO 10
  PRINT i
NEXT
END
```

## See Also

* [DEF FN](DEF-FN), [FUNCTION](FUNCTION), [IF](IF), [SELECT](SELECT), [SUB](SUB) and [TYPE](TYPE)
