# SEEK (Statement)

Sets the file pointer position for the next read or write operation.

## Syntax

`SEEK` [#]*file_number*, *position*

## Comments

*file_number* is the file number assigned to the file in its [OPEN](OPEN) statement.

*position* is the desired record number in a random-access file or the byte offset in a binary or sequential file. It must be in the range 1 to 2,147,483,647.

## Example

```vb
OPEN "SALARY.DAT" FOR RANDOM AS #1 LEN = 80
SEEK #1, 5  'Move pointer to record 5
```

## See Also

- [GET#](GET-File), [LOC](LOC), [OPEN](OPEN), [PUT#](PUT-File), [SEEK](SEEK)
