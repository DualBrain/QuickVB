# RMDIR

Removes the specified directory.

## Syntax

`RMDIR` *directory_name*

## Comment

*directory_name* is a string expression containing the name of the directory to delete.

`RMDIR` works like the MS-DOS `RMDIR` command. It cannot delete the current directory or a directory containing files.

## Example

```vb
RMDIR "A:\TEST"
```

## See Also

- [CHDIR](CHDIR), [MKDIR](MKDIR)
