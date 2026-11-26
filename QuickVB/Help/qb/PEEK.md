# PEEK

Returns the byte stored at a specified memory offset.

## Syntax

`PEEK`(*offset*)

## Comments

*offset* is an integer expression (in the range 0 through 65,535) that specifies an offset within the current default segment.

The [DEF SEG](DEF-SEG) statement defines the default segment address.

## Example

```vb
' Save the screen's contents
DIM screensav(3999) AS INTEGER
DEF SEG = &HB800
FOR i = 0 TO 3999
  screensave(i) = PEEK(i)
NEXT i
DEF SEG
```

## See Also

- [DEF SEG](DEF-SEG), [POKE](POKE)
