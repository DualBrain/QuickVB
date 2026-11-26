# DIM Statement

<!-- keywords: DIM AS -->

Declares variables and arrays and gives them a type.

## Syntax

`DIM` *variable* [ `AS` *type* ] [ = *value* ]
`DIM` *array*(*upper*) [ `AS` *type* ]
`DIM` *array*(*rows*, *columns*) [ `AS` *type* ]

## Comments

With `OPTION EXPLICIT` on, every variable has to be declared. `OPTION INFER` lets the type come from the value: `DIM n = 5` is an `INTEGER`.

An array declared with `(upper)` has `upper + 1` elements, numbered from 0 to *upper*. See `REDIM` to change the size of an array.

## Example

```vb
DIM count AS INTEGER = 10
DIM name AS STRING = "QuickVB"
DIM scores(4) AS INTEGER
scores(0) = 100
PRINT(count, name, scores(0))
```

## See Also

- [CONST](CONST), [REDIM](REDIM), [ERASE](ERASE), [Data Types](DATA-TYPES)

## Online

- [Microsoft Learn: Dim statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/dim-statement)
