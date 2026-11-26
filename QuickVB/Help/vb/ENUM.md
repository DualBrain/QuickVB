# ENUM Statement

<!-- keywords: ENUM -->

Declares a set of named whole-number constants.

## Syntax

`ENUM` *name*
  *member* [ = *value* ]
`END ENUM`

## Comments

Members count up from 0 unless a value is given.

## Example

```vb
ENUM Color
  Red
  Green = 5
  Blue
END ENUM
```

## See Also

- [CONST](CONST)

## Online

- [Microsoft Learn: Enum statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/enum-statement)
