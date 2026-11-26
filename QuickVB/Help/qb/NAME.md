# NAME

Renames a file or directory on disk.

## Syntax

`NAME` *old_filename* `AS` *new_filename*

## Comments

*old_filename* is a string expression containing the name of an existing MS-DOS file.

*new_filename* is a string expression containing the desired filename. A file with this name cannot already exist on disk.

*new_filename* must be on the same disk as *old_filename*.

## Example

```vb
NAME "OLDFILE.DAT" AS "NEWFILE.DAT"
```

## See Also

- [FILES](FILES)
