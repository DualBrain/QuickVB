# READ

Reads values from a data list and assigns them to variables.

## Syntax

`READ` *variable_list*

## Comments

*variable_list* is a list of variables separated by commas. `READ` and [DATA](DATA) statements work hand in hand to assign values to these variables.

`READ` statement variables may be numeric or string, and the values read must agree with the variable types specified. If they do not agree, a `Syntax error` results.

A single `READ` statement may access one or more [DATA](DATA) statements. They are accessed in order. Several `READ` statements may access the same [DATA](DATA) statement.

If the number of variables in list of variables exceeds the number of elements in the [DATA](DATA) statement(s), an `Out of data` message is printed.

If the number of variables specified is fewer than the number of elements in the [DATA](DATA) statement(s), subsequent `READ` statements begin reading data at the first unread element. If there are no subsequent `READ` statements, the extra data is ignored.

To reread [DATA](DATA) statements from the start, use the [RESTORE](RESTORE) statement.

## Example

```vb
FOR I = 1 TO 10
  READ A(I)
NEXT I

DATA 3.08, 5.19, 3.12, 3.98, 4.24
DATA 5.08, 5.55, 4.00, 3.16, 3.37
```

This program segment reads the values from the [DATA](DATA) statements into array `A`. After execution, the value of `A(1)` is 3.08, and so on. The [DATA](DATA) statement may be placed anywhere in the program; they may even be placed ahead of the `READ` statement.

```vb
PRINT
PRINT "CITY", "STATE", "ZIP"
READ C$, S$, Z
DATA "DENVER,", "COLORADO", 80211
PRINT C$, S$, Z
```

This program reads string and numeric data from the [DATA](DATA) statement.

## See Also

- [DATA](DATA), [RESTORE](RESTORE)
