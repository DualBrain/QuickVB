# ENVIRON

Changes an existing entry or places a new entry in the MS-DOS environment.

## Syntax

`ENVIRON` *string_expression*

## Comments

The `ENVIRON` statement expects a string expression of the same form as the MS-DOS `SET` command, *entry*=*value*.

The change to the environment table is valid only for the life of the program.

You cannot increase the size of the MS-DOS environment in QBasic. To make space in the MS-DOS environment for use by QBasic programs, create a dummy entry with the DOS `SET` command. Then erase the contents of the entry in a QBasic program to make space for new or changed variables.

## Examples

Assuming the environment string table is empty, the following statement will create a default path to the root directory on Disk A:

```vb
ENVIRON "PROGRAM=TEST"
PRINT ENVIRON$("PROGRAM")
```

Running this program produces the following:

```txt
TEST
```

## See Also

* [ENVIRON$](ENVIRON$)
