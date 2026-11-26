# CONST Statement

<!-- keywords: CONST -->

Declares a constant: a name for a value that never changes.

## Syntax

`CONST` *name* [ `AS` *type* ] = *expression*

## Comments

The value is worked out when the program is compiled, so it can only use literals and other constants.

## Example

```vb
CONST pi AS DOUBLE = 3.14159265
PRINT(pi * 2)
```

## See Also

- [DIM](DIM)

## Online

- [Microsoft Learn: Const statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/const-statement)
