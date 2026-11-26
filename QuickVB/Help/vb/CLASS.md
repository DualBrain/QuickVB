# CLASS Statement

<!-- keywords: CLASS -->

Declares a class: a type that has data and procedures of its own.

## Syntax

`CLASS` *name*
  [ *members* ]
`END CLASS`

## Comments

A class is the model of an object. `DIM p AS NEW Person` makes one. Classes are .NET's way to group data with the code that works on it.

## Example

```vb
CLASS Person
  PUBLIC Name AS STRING
END CLASS
```

## See Also

- [STRUCTURE](STRUCTURE), [MODULE](MODULE)

## Online

- [Microsoft Learn: Class statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/class-statement)
