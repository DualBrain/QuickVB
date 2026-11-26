# SEEK

Returns the current file pointer position.

## Syntax

`SEEK`(*file_number*)

## Comments

*file_number* is the file number assigned to the file in its [OPEN](OPEN) statement.

For random-access files, `SEEK` returns a record number in the range 1 through 2,147,483,647. For binary and sequential files, `SEEK` returns the current byte offset.

## Example

```vb
position = SEEK(1)
```

## See Also

- [LOC](LOC), [OPEN](OPEN), [SEEK (Statement)](SEEK-Statement)
