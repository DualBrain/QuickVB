# FIELD

Allocates space for variables in a random-access file buffer.

## Syntax

`FIELD` [ # ] *file_number*, *width* `AS` *str_variable*...

## Comments

*file_number* is the number assigned to the file in its [OPEN](OPEN) statement.

*width* is the number of characters in the field.

*str_variable* is the name of a string variable to be used when reading from or writing to the file.

A variable name that appears in a `FIELD` statement should not appear in an [INPUT](INPUT) statement or on the left side of an assignment operator. If it does, the variable will no longer reference the random-access file buffer.

Using record variables is usually more convenient than using the `FIELD` statement.

## Example

```vb
OPEN "RANDOM.DAT" FOR RANDOM AS #1 LEN=80
FIELD #1, 76 AS name$, 4 AS salary$
```

## Note

Do not use a fielded variable name in an [INPUT](INPUT) or [LET](LET) statement. Once a variable name is fielded, it points to the correct place in the random file buffer. If a subsequent [INPUT](INPUT) or [LET](LET) statement with that variable name is executed, the variable's pointer is moved to string space (see [LSET](LSET)/[RSET](RSET) and [GET](GET) statements).

## See Also

- [GET](GET), [LSET](LSET), [OPEN](OPEN), [PUT](PUT), [RSET](RSET)
