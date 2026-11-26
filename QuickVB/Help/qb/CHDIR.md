# CHDIR

Changes the default directory for the specified drive.

## Syntax

`CHDIR` *directory_name*

## Comments

*directory_name* is a string expression containing the desired directory name. This value is limited to a maximum of 64 characters.

To change the default directory for a drive other than the current, precede the pathname with a disk drive letter and a colon.

`CHDIR` does not change the default drive.

You cannot abbreviate `CHDIR`.

## Example

To make *sales* the working directory on Drive A: and *inventory* the working directory on Drive B: (assume A: is the default drive), type the following commands:

```vb
CHDIR "SALES" 
CHDIR "B:INVENTORY" 
```

## See Also

- [FILES](FILES), [MKDIR](MKDIR), [RMDIR](RMDIR)
