# INTERFACE Statement

<!-- keywords: INTERFACE, IMPLEMENTS -->

Declares what a class must provide, without saying how.

## Syntax

`INTERFACE` *name*
  [ *member declarations* ]
`END INTERFACE`

## Comments

A class says `IMPLEMENTS` *name* and then has to supply every member. A variable of the interface type can hold any such class.

## Example

```vb
INTERFACE IShape
  FUNCTION Area() AS DOUBLE
END INTERFACE

CLASS Square
  IMPLEMENTS IShape
  PUBLIC Side AS DOUBLE
  PUBLIC FUNCTION Area() AS DOUBLE IMPLEMENTS IShape.Area
    RETURN Side * Side
  END FUNCTION
END CLASS
```

## See Also

- [CLASS](CLASS), [INHERITS](INHERITS)

## Online

- [Microsoft Learn: Interface statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/interface-statement)
