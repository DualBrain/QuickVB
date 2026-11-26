# FUNCTION Statement

<!-- keywords: FUNCTION -->

Declares a procedure that returns a value.

## Syntax

`FUNCTION` *name* [ ( *parameters* ) ] `AS` *type*
  [ *statements* ]
`END FUNCTION`

## Comments

The value is given back with `RETURN` *value*. `EXIT FUNCTION` leaves early; a `FUNCTION` that ends without `RETURN` gives back the default of its type.

## Example

```vb
FUNCTION Square(n AS INTEGER) AS INTEGER
  RETURN n * n
END FUNCTION
```

## See Also

- [SUB](SUB), [RETURN](RETURN), [CALL](CALL)

## Online

- [Microsoft Learn: Function statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/function-statement)
