# SCREEN (Function)

Returns the character or the color attribute for the character at the specified row and column.

## Syntax

x=`SCREEN`(*row*,*column*[, *get_color*])

## Comments

*row* and *column* are the coordinates of the character.

*get_color* is a numeric expression. If *get_color* is 1, `SCREEN` returns the color of the character. If *get_color* is 0 or is omitted, `SCREEN` returns the ASCII value of the character at the specified position.

## Examples

```vb
DIM scr(1 TO 25, 1 TO 80) AS INTEGER
'Store current screen contents
FOR row = 1 TO 25
  FOR column = 1 TO 80
    scr(row, column) = SCREEN(row, column)
  NEXT column
NEXT row
```
