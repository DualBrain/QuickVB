# PROPERTY Statement

<!-- keywords: PROPERTY, WRITEONLY -->

Declares a value of an object that is read or set like a variable but runs code.

## Syntax

[ `READONLY` | `WRITEONLY` ] `PROPERTY` *name* `AS` *type*
  `GET`
    `RETURN` *value*
  `END GET`
  `SET`(*value* `AS` *type*)
    [ *statements* ]
  `END SET`
`END PROPERTY`

## Comments

A property needs no `GET` and `SET` blocks when it only keeps a value: `PUBLIC PROPERTY Name AS STRING` does it all.

## Example

```vb
CLASS Person
  PUBLIC PROPERTY Name AS STRING
  PUBLIC READONLY PROPERTY Shout AS STRING
    GET
      RETURN Name.ToUpper()
    END GET
  END PROPERTY
END CLASS
```

## See Also

- [CLASS](CLASS), [STRUCTURE](STRUCTURE), [FUNCTION](FUNCTION)

## Online

- [Microsoft Learn: Property statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/property-statement)
