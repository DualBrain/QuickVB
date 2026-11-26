# LOCK

Prevents access to all or specific portions of a file by network programs.

## Syntax

`LOCK` [#]*file_number* [,{*record_number* | [ *start*] `TO` *end*}]

## Comments

*file_number* is the file number assigned to the file in its [OPEN](OPEN) statement.

For random-access files, `LOCK` locks the specified record or range of records. For binary files, `LOCK` locks the specified byte or range of bytes. For sequential files, `LOCK` locks the entire file.

`LOCK` is necessary only in network environments.

If a network program tries to access a locked record or byte, QBasic generates an error.

To use `LOCK`, you should first run the `SHARE.EXE` program, an MS-DOS program that supports file sharing and locking.

## Example

```vb
INPUT "Enter record number to update"; rec
LOCK #1, rec         'Restrict access
emp.name$ = "SMITH"
PUT #1, rec
UNLOCK rec           'Allow access
```

## See Also

- [UNLOCK](UNLOCK)
