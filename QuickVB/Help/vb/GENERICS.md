# OF (Generics)

<!-- keywords: OF, GENERIC -->

Gives a type or procedure a type to work with, filled in where it is used.

## Syntax

*name*(`OF` *type*)
`CLASS` *name*(`OF` *T*)
`FUNCTION` *name*(`OF` *T*)(*parameters*) `AS` *T*

## Comments

`List(OF STRING)` is a list of strings; `Dictionary(OF STRING, INTEGER)` maps strings to integers. A `WHERE` clause limits what the type can be.

## Example

```vb
DIM ages AS NEW System.Collections.Generic.Dictionary(OF STRING, INTEGER)
ages("Ann") = 31
```

## See Also

- [CLASS](CLASS), [Data Types](DATA-TYPES)

## Online

- [Microsoft Learn: Generic types](https://learn.microsoft.com/dotnet/visual-basic/language-reference/data-types/generic-types-in-visual-basic)
