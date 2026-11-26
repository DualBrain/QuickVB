# INPUT # (File I/O)

Reads data from a sequential file.

## Syntax

`INPUT`#*file_number*, *variables*

## Comments

*file_number* is the number assigned to the file in its [OPEN](OPEN) statement.

*variables* is the list of variables in which to store data from the file.

## Example

```vb
OPEN "SALARY.DAT" FOR INPUT AS #1
DO WHILE NOT EOF(1)
  INPUT #1, ename$, salary
  PRINT ename$, salary
LOOP
CLOSE #1
```

## See Also

- [INPUT$](INPUT$), [LINE INPUT](LINE-INPUT)
