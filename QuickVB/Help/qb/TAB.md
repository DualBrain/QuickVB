# TAB

Moves the print position to the specified column.

## Syntax

`TAB`(*column*)

## Comments

*column* is the desired tab column. If the current position is beyond the specified column, `TAB` moves to the column on the next line.

## Example

```vb
FOR i = 1 TO 10
  PRINT TAB(i); i
NEXT i
```

## See Also

- [SPC](SPC)
