# UNLOCK

Unlocks portions of a shared file for access by other network programs.

## Syntax

`UNLOCK` [#]*file_number* [,{*record* | [*start* ] `TO` *end*}]

## Comments

*file_number* is the file number assigned to the file in its [OPEN](OPEN) statement.

*record* is an integer value that specifies a single record to release in a random-access file or a single byte to unlock in a binary file.

*start* and *end* are integer values that specify the range of record numbers to release in a random-access file or the range of bytes to unlock in a binary file.

`UNLOCK` is necessary only in network environments.

## Examples

```vb
OPEN "SHARED.DAT" FOR RANDOM AS #1
LOCK #1, 1 TO 10
'Perform file update operations
UNLOCK #1, 1 TO 10
CLOSE #1
```

## See Also

- [LOCK](LOCK)
