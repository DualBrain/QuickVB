# WINDOW

Defines the logical coordinates for the current viewport.

## Syntax

`WINDOW` [ [`SCREEN` ] (*x1*,*y1*)-(*x2*,*y2*)]

## Comments

The keyword `SCREEN` inverts the screen's coordinate system such that *y* values increase in value from the top to the bottom of the screen.

The coordinates *x1*, *y1* and *x2*, *y2* specify the logical coordinates of the viewport.

## Example

```vb
SCREEN 1
WINDOW (0, 0)-(50, 50)
LINE (10, 10)-(40, 40), 2, B
```

## See Also

- [CLS](CLS), [SCREEN](SCREEN), [VIEW](VIEW)
