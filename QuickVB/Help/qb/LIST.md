# LIST

To list all or part of a program to the screen, line printer, or file.

## Syntax

`LIST [line number][-line number][,filename]`

`LIST [line number-][,filename]`

## Comments

*line number* is a valid line number within the range of 0 to 65529.

If *filename* is omitted, the specified lines are listed to the screen.

Use the hyphen to specify a line range. If the line range is omitted, the entire program is listed. *line number-* lists that line and all higher numbered lines. *-line number* lists lines from the beginning of the program through the specified line.

The period (.) can replace either *line number* to indicate the current line.

Any listing may be interrupted by pressing CTRL-BREAK.

## Examples

```vb
LIST
```

Lists all lines in the program.

```vb
LIST -20
```

Lists lines 1 through 20.

```vb
LIST 10-20
```

Lists lines 10 through 20.

```vb
LIST 20-
```

Lists lines 20 through the end of the program.
