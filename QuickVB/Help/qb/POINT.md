# POINT

Returns a pixel's color or coordinates.

## Syntax

`POINT`(*x*, *y*)

`POINT`(*mapping*)

## Comments

The `POINT(x, y)` function returns the color of the pixel at the *x*, *y* coordinate.

The `POINT(mapping)` function returns the coordinates of the graphics cursor based on the value of *mapping*.

| Function | Returns |
| -------- | ------- |
| 0 | the current physical x coordinate. |
| 1 | the current physical y coordinate. |
| 2 | the current logical x coordinate if WINDOW is active; otherwise, it returns the current physical x coordinate as in 0 above. |
| 3 | the current logical y coordinate if WINDOW is active; otherwise, it returns the current physical y coordinate as in 1 above. |

## Examples

```vb
SCREEN 1
FOR C=0 TO 3
  PSET (10, 10),C
  IF POINT(10, 10) <> C THEN PRINT "BROKEN BASIC="
NEXT C
```

The following inverts the current state of a point:

```vb
SCREEN 2
IF POINT(I, I) <> 0 THEN PRESET(I, I) ELSE PSET(I, I)
```

The following is another way to invert a point:

```vb
PSET (I, I), 1-POINT(I, I)
```

## See Also

- [CIRCLE](CIRCLE), [COLOR](COLOR), [DRAW](DRAW), [LINE](LINE), [PAINT](PAINT), [PRESET](PRESET), [PSET](PSET), [SCREEN](SCREEN)
