# DELEGATE Statement

<!-- keywords: DELEGATE, ADDRESSOF -->

Declares a type for a procedure, so a procedure can be kept in a variable or handed to another procedure.

## Syntax

`DELEGATE` `SUB` | `FUNCTION` *name*(*parameters*) [ `AS` *type* ]
`ADDRESSOF` *procedure*

## Comments

`ADDRESSOF` gives the procedure as a value of a delegate type.

## Example

```vb
DELEGATE FUNCTION Twice(n AS INTEGER) AS INTEGER

FUNCTION Double2(n AS INTEGER) AS INTEGER
  RETURN n * 2
END FUNCTION

DIM f AS Twice = ADDRESSOF Double2
PRINT(f(4))
```

## See Also

- [LAMBDA](LAMBDA), [EVENT](EVENT), [FUNCTION](FUNCTION)

## Online

- [Microsoft Learn: Delegate statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/delegate-statement)
