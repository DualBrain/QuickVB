# MKD$

Convert a double-precision value to an 8-byte string for output to a random-access file by [PUT#](PUT-FILE).

## Syntax

`MKD$`(*numeric_expression*)

## Comments

*numeric_expression* is a double-precision numeric expression.

## Example

```vb
OPEN "TEST.DAT" FOR RANDOM AS #1 LEN=8
FIELD #1, 8 AS sal$
salary = 60000
sal$ = MKD$(salary)
PUT #1
CLOSE #1
```

## See Also

- [CVD](CVD), [CVDMBF$](CVDMBF$), [CVI](CVI), [CVL](CVL), [CVS](CVS), [CVSMBF$](CVSMBF$), [MKDMBF$](MKDMBF$), [MKI$](MKI$), [MKL$](MKL$), [MKS$](MKS$), [MKSMBF$](MKSMBF$)
