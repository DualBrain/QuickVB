# RUN

Runs the program currently in memory or an existing program from disk.

## Syntax

`RUN` [ *line_number*]

*or*

`RUN` [ *filename*]

## Comments

*line_number* is a line number in the current program at which execution should begin. If you omit *line_number*, `RUN` begins at the first line number.

*file_name* is a string expression containing the name of a file to execute. QBasic assumes the `BAS` extension.

`RUN` closes all files and erases all variables. To share variables, use the [CHAIN](CHAIN) statement.

## Example

```vb
RUN "FILENAME"
```

## See Also

- [CHAIN](CHAIN)
