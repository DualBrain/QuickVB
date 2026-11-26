# VIEW PRINT

Defines the text mode scrolling region.

## Syntax

`VIEW PRINT` [*top_row* `TO` *bottom_row* ]

## Comments

*top_row* and *bottom_row* are integer values that specify the top and bottom rows of the text mode scrolling region.

IF you omit all arguments, `VIEW PRINT` sets the scrolling region to the entire screen.

## Example

```vb
CLS
VIEW PRINT 5 TO 10
FOR i = 1 TO 100
  PRINT i, i, i, i
NEXT i
```

## See Also

- [CLS](CLS), [LOCATE](LOCATE), [PRINT](PRINT)
