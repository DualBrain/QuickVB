# PSET

Draws a pixel at the specified screen coordinates.

## Syntax

`PSET`[ `STEP`] (*x*,*y*) [, *color*]

## Comments

The keyword `STEP` indicates that *x* and *y* are offsets from the last point drawn, as opposed to coordinates.

*color* is the desired pixel color. If you omit *color*, `PSET` uses the current foreground color.

If the coordinates are outside the current viewport, no point is drawn.

## Examples

The following draws a diagonal line from (0,0) to (100,100).

```vb
CLS
SCREEN 1
FOR I=0 TO 100
  PSET (I,I)
NEXT
LOCATE 14,1
```

The following clears out the line by setting each pixel to 0.

```vb
FOR I=100 TO 0 STEP -1
  PSET(I,I),0
NEXT I
```

```vb
SCREEN 1
COLOR 1, 2
CLS
FOR i = 1 TO 10000
  PSET(RND * 320, RND * 200), RND * 4
NEXT i
```

## See Also

- [CIRCLE](CIRCLE), [COLOR](COLOR), [DRAW](DRAW), [LINE](LINE), [PAINT](PAINT), [POINT](POINT), [PRESET](PRESET), [SCREEN](SCREEN)
