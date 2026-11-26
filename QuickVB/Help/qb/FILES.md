# FILES

Displays the names of files in the current or specified directory.

## Syntax

`FILES` [ *string_expression* ]

## Comments

*string_expression* is a string expression that contains an MS-DOS file specification of the files to display. Wildcard characters can be used.

Ifr you omit a file specification, `FILES` displays the files in the current directory.

## Examples

```vb
FILES           ' List all files
FILES "*.BAS"   ' List all .BAS files
FILES "A:*"     ' List all files on drive A
```

## See Also

- [CHDIR](CHDIR), [KILL](KILL), [NAME](NAME)
