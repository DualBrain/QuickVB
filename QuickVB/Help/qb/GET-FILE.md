# GET (File/IO)

Reads a record from a random-access or binary disk file.

## Syntax

`GET` [ # ] *file_number* [ , [ *record_number* ] [ , *variable* ]]

## Comments

*file_number* is the number assigned to the file in its [OPEN](OPEN) statement.

*record_number* is the number of the record desired, from 1 through 2,147,483,647. If you omit a record number, `GET` reads the next record.

*variable* is the name of the variable into which `GET` enters the data. Usually a user-defined record variable is used.

## Example

```vb
TYPE SalaryRecord
  ename AS STRING * 20
  salary AS SINGLE
END TYPE

DIM employee AS SalaryRecord

OPEN "SALARY.DAT" FOR RANDOM AS #1 LEN = LEN(employee)
GET #1, 1, employee
PRINT employee.ename, employee.salary
CLOSE #1
```

## See Also

- [CVD](CVD), [CVI](CVI), [CVL](CVL), [CVS](CVS), [FIELD](FIELD), [INPUT](INPUT), [LINE INPUT](LINE-INPUT), [LSET](LSET), [MKD$](MKD$), [MKI$](MKI$), [MKL$](MKL$), [MKS$](MKS$), [PUT#](PUT-FILE), [RSET](RSET)
