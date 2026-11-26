# LOCATE

Moves the cursor to the specified position on the screen and, optionally, sets the cursor size.

## Syntax

`LOCATE` [*row*][,[*col*][,[*visible*][,[*scan_start*] [ ,*scan_stop*]]]]

## Comments

*row* is the number of the desired row.

*col* is the number of the desired column.

*visible*, when 1, causes the cursor to be displayed. When 0, it causes the cursor to be hidden.

*scan_start* is an integer specifying the first cursor scan line.

*scan_stop* is an integer specifying the last cursor scan line.

If the *scan_start* parameter is given and the *scan_stop* parameter is omitted, *scan_stop* assumes the *scan_start* value.

## Examples

```vb
CLS
FOR i = 5 TO 20
  LOCATE i, i
  PRINT "Location is"; i; i
NEXT i
```

## See Also

- [CSRLIN](CSRLIN), [POS](POS)
