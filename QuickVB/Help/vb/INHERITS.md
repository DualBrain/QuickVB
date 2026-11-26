# INHERITS Statement

<!-- keywords: INHERITS, MUSTINHERIT, NOTINHERITABLE, MYBASE -->

Makes a class start from another one, with all its members.

## Syntax

`CLASS` *name*
  `INHERITS` *baseclass*
  [ *members* ]
`END CLASS`

## Comments

`MUSTINHERIT` on a class means it is only a base for others; `NOTINHERITABLE` means nothing can inherit it. `MYBASE` reaches the base class from inside.

## Example

```vb
CLASS Animal
  PUBLIC OVERRIDABLE SUB Speak()
    PRINT("...")
  END SUB
END CLASS

CLASS Dog
  INHERITS Animal
  PUBLIC OVERRIDES SUB Speak()
    PRINT("Woof")
  END SUB
END CLASS
```

## See Also

- [OVERRIDES](OVERRIDES), [CLASS](CLASS), [ME and MYBASE](ME)

## Online

- [Microsoft Learn: Inherits statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/inherits-statement)
