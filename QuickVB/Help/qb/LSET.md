# LSET

Moves data into a random-access file buffer, assigns a variable of one record type to a variable of a different record type or left-justifies the value of a string variable.

## Syntax

`LSET` *string_variable*=*string_expression*

`LSET` *record_variable1* = *record_variable2*

## Comments

*string_variable* is either a random-access file field or a string variable.

*string_expression* is any string expression.

*record_variable1* and *record_variable2* are user-defined record variables.

## Example

```vb
salary = 66000
LSET e$ = MKS$(salary)
PUT #1
```

## See Also

- [RSET](RSET)
