# COMMON

Defines variables as global within a module or between chained programs.

## Syntax

`COMMON` `SHARED` *variable_list*

## Comments

The `SHARED` keyword states that the specified variables are shared by all subprograms and functions in a module.

*variable_list* is the list of global variables with variable names separated by commas. QBasic allows you to specify variables as:

*variable_name* [ () ] [ `AS` *type* ]

`COMMON` statements must appear before any executable statements in your program. QBasic associates variables in common blocks by position, not by name.

## Example

```vb
COMMON a, b, c
a = 1: b = 2: c = 3
CHAIN "COMMON.BAS"

'Code for COMMON.BAS
COMMON x, y, z
PRINT x, y, z
```

Results in the following:

```txt
1    2    3
```

## See Also

* [STATIC](STATIC)
