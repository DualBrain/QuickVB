# STRUCTURE Statement

<!-- keywords: STRUCTURE -->

Declares a record: several named values kept together (QuickBASIC's TYPE).

## Syntax

`STRUCTURE` *name*
  `PUBLIC` *field* `AS` *type*
`END STRUCTURE`

## Comments

`TYPE...END TYPE` of QuickBASIC becomes `STRUCTURE...END STRUCTURE`; every field says `PUBLIC` to be used from outside.

## Example

```vb
STRUCTURE Point
  PUBLIC X AS INTEGER
  PUBLIC Y AS INTEGER
END STRUCTURE
```

## See Also

- [CLASS](CLASS), [ENUM](ENUM), [DIM](DIM)

## Online

- [Microsoft Learn: Structure statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/structure-statement)
