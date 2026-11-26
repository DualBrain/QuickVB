# DATA

Stores numeric and string constants to be read with the [READ](READ) statement.

## Syntax

`DATA` *constant* [ ,*constant*]...

## Comments

*constant* is a numeric or string constant.

String constants in `DATA` statements must be surrounded by double quotation marks only if they contain commas, colons, or significant leading or trailing spaces. Otherwise, quotation marks are not needed.

[READ](READ) statements access the `DATA` statements in order. The data contained therein may be thought of as one continuous list of items, regardless of how many items are on a line or where the lines are placed in the program. The variable type (numeric or string) given in the [READ](READ) statement must agree with the corresponding constant in the `DATA` statement, or a `Type Mismatch` 
error occurs.

`DATA` statements may be reread from the beginning by use of the [RESTORE](RESTORE) statement.

You can only use data statements at the module level, never in a procedure.

```vb
FOR I=1 TO 10
  READ A(I)
NEXT I
DATA 3.08,5.19,3.12,3.98,4.24
DATA 5.08,5.55,4.00,3.16,3.37
```

This program segment reads the values from the `DATA` statements into array `A`. After execution, the value of `A(1)` is `3.08`, and so on. The `DATA` statements (last two lines) may be placed anywhere in the program; they may even be placed ahead of the [READ](READ) statement.

```vb
PRINT
PRINT "CITY","STATE","ZIP"
READ C$,S$,Z
DATA "NORTH RICHLAND HILLS,","TEXAS",76182
PRINT C$,S$,Z
```

```text
CITY STATE ZIP
NORTH RICHLAND HILLS, TEXAS 76182
```

This program reads string and numeric data from the `DATA` statement.

## See Also

* [READ](READ), [RESTORE](RESTORE)
