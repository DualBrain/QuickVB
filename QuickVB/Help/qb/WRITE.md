# WRITE

Writes data to the screen or a sequential file.

## Syntax

`WRITE` [[# ] *file_number*,] *expression_list*

## Comments

*file_number* is the file number assigned to the file in its [OPEN](OPEN) statement. If you omit *file_number*, QBasic writes the data to the screen.

*expression_list* is a list of one or more variables or expressions separated by commas.

`WRITE` places a comma between each expression in the file; the [PRINT](PRINT) statement does not.

## Example

```vb
OPEN "TEST.DAT" FOR OUTPUT AS #1
WRITE #1, "TEST", 5, 3.21, "END"
CLOSE #1

```

The resulting text file will contain.

```text
"TEST",5,3.21,"END"
```

## See Also

- [PRINT](PRINT)
