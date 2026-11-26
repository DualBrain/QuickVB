# LINE

Draws a line or box on the screen.

## Syntax

`LINE` [[`STEP`]( x1, y1)]-[`STEP`]( x2, y2) [,[*color*][,[B[F]][,*linestyle*]]]

## Comments

`LINE` draws either a line using the coordinate pairs (*x1*, *y1*) and (*x2*, *y2*) as endpoints or a box with (*x1*, *y1*) as one corner and (*x2*, *y2*) as the opposite corner.

The keyword `STEP` directs `LINE` to use the coordinates as an offset from the last point plotted as opposed to physical coordinates.

*color* is the line or box color.

`B` directs `LINE` to draw a box rather than a line.

`F` directs `LINE` to fill the box with the specified color.

*linestyle* is a 16-bit value whose bits determine whether or not pixels are drawn. By changing this value, you change the style of lines on your screen.

## Example

```vb
'Fill the screen with random boxes
SCREEN 1 
FOR i = 1 TO 1000
  x1 = RND * 320
  y1 = RND * 200
  x2 = RND * 320
  y2 = RND * 200
  scolor = RND * 4
  LINE (x1. y1)-(x2, y2). scolor. BF
NEXT i 
```

## See Also

- [CIRCLE](CIRCLE), [COLOR](COLOR), [DRAW](DRAW), [PAINT](PAINT), [POINT](POINT), [PRESET](PRESET), [PSET](PSET), [SCREEN](SCREEN)
