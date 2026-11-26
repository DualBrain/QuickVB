# MKSMBF$

Converts a single-precision value stored in IEEE format to an 4-byte string containing the value in Microsoft binary format for output to a random-access file by [PUT#](PUT-FILE).

## Syntax

`MKSMBF$`(*numeric_expression*)

## Comments

*numeric_expression* is a single-precision numeric expression.

[CVDMBF](CVDMBF), [CVSMBF](CVSMBF), [MKDMBF$](MKDMBF$) and [MKSMBF$](MKSMBF$) are useful for maintaining data files created with earlier versions of BASIC.

## Example

```vb
TYPE EmpRec 
  ename AS STRING * 20
  salary AS STRING * 4
END TYPE

DIM employee AS EmpRec
OPEN "SALARY.DAT" FOR RANDOM AS #1 LEN=LEN(employee)
employee.ename = "Jones"
employee.salary = MKSMBF$(65000)
PUT #1, 1, employee
CLOSE #1
```

## See Also

- [CVDMBF$](CVDMBF$), [CVSMBF$](CVSMBF$), [MKDMBF$](MKDMBF$)
