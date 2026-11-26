# CSRLIN

To return the current cursor row number.

## Syntax

y=`CSRLIN`

## Comments

`CSRLIN` returns the cursor's line (row) number. [POS](POS) returns the cursor's column number.

## Examples

```vb
' Save cursor row and column
saveline = CSRLIN
savecol = POS(0)
' Move to line 10, column 20
LOCATE 10, 20: PRINT "Message at 10, 20"
' Restore cursor to previous position
LOCATE saveline, savecol
```

## See Also

* [POS](POS), [LOCATE](LOCATE)
