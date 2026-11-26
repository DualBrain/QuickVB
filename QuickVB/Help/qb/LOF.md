# LOF

Returns the number of bytes in a file.

## Syntax

`LOF`(*file_number*)

## Comments

*file_number* is the file number assigned to the file in its [OPEN](OPEN) statement.

You cannot use `LOF` with devices.

## Example

```vb
OPEN "\CONFIG.SYS" FOR INPUT AS #1
PRINT "File size in bytes is"; LOF(1)
CLOSE #1
```
