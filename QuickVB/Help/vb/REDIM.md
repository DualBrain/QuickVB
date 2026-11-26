# REDIM Statement

<!-- keywords: REDIM PRESERVE -->

Changes the size of an array.

## Syntax

`REDIM` [ `PRESERVE` ] *array*(*upper*)

## Comments

`REDIM` throws the values away; `REDIM PRESERVE` keeps them (only the last dimension can change).

## Example

```vb
DIM a(2) AS INTEGER
REDIM PRESERVE a(9)
```

## See Also

- [DIM](DIM), [ERASE](ERASE)

## Online

- [Microsoft Learn: ReDim statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/redim-statement)
