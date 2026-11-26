# KILL

Deletes a file from disk.

## Syntax

`KILL` *filename*

## Comments

*filename* is a string expression specifying the file to delete. The string can contain the MS-DOS wildcard characters ? and *.

## Note

You must specify the filename's extension when using the `KILL` command. Remember that files saved in BASIC are given the default extension *.BAS*.

If a `KILL` command is given for a file that is currently open, a `File already open` error occurs.

## Examples

```vb
KILL "TEST.DAT"
KILL "*.OLD"
```

## See Also

- [FILES](FILES)
