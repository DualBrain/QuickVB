# ENVIRON$

Returns an entry from the MS-DOS environment.

## Syntax

v$=`ENVIRON$`(*entry_string*)

*or*

v$=`ENVIRON$`(*n*)

## Comments

The MS-DOS `SET` command allows you to set and display environment strings from the MS-DOS prompt.

The first form of `ENVIRON$` allows your program to access the value of an environment variable. The name of the desired environment variable is *entry_string*.

The second form of `ENVIRON$` allows your program to access the *n*th environment string.

If the specified entry does not exist, `ENVIRON$` returns a null string.

## Examples

```vb
PRINT ENVIRON$("PATH")

i = 1
DO WHILE ENVIRON$(i) <> ""
  PRINT ENVIRON$(i)
  i = i + 1
LOOP
```

## See Also

* [ENVIRON](ENVIRON)
