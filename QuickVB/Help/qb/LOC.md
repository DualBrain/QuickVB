# LOC

Returns the current offset or record number within a file.

## Syntax

`LOC`(*file_number*)

## Comments

*file_number* is the file number assigned to the file in its [OPEN](OPEN) statement.

For binary files, [LOC] returns the current byte offset in the file. For random-access files, `LOC` returns the current record number. For sequential files, `LOC` returns the current offset, divided by 128. For a `COM` device, `LOC` returns the number of bytes in the input queue.

## Example

```vb
IF LOC(1) > 100 THEN CALL ReadIt(salary)
```

## See Also

- [SEEK](SEEK)
