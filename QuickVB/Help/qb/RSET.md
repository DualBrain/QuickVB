# RSET

Moves data into a random-access file buffer or right-justifies the value of a string variable.

## Syntax

`RSET` *string_variable* = *string_expression*

## Comments

*string_variable* is either a random-access file variable or a string variable.

For random-access file variables, `RSET` assigns a record variable of one type to another.

For string variables, `RESET` right-justifies the string.

## Example

```vb
DIM n AS STRING * 10
PRINT "ABCDE"
RSET n = "ABCDE"
PRINT n
```

Resulting in:

```txt
ABCDE
     ABCDE
```

## See Also

- [FIELD](FIELD), [LSET](LSET)
