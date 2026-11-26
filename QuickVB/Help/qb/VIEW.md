# VIEW

Defines the screen coordinates within which graphics can be displayed.

## Syntax

`VIEW` [ [`SCREEN` ] (*x1*,*y1*)-(*x2*,*y2*) [, [ *fill_color*] [, *border_present*]]]

## Comments

`VIEW` allows you to restrict graphics output to specific coordinates on the screen. Coordinates outside this range are not drawn.

The keyword [SCREEN](SCREEN) states that graphics coordinates are relative to the screen, not to the viewport.

*xl* and *yl* are the coordinates of one comer of the viewport; *x2* and *y2* are the coordinates of the opposite comer.

*fill_color* specifies the color with which to fill the viewport.

*border_present* is any numeric expression that, when present, directs `VIEW` to draw a border around the viewport.

If you omit all arguments, `VIEW` sets the viewport to the entire screen.

The [SCREEN](SCREEN) and [RUN](RUN) statements set the viewport back to the entire screen.

## Example

```vb
SCREEN 1
VIEW (0, 0)-(20, 20), 1, 2
LINE (10, 10)-(100, 100)
```

## See Also

- [CLS](CLS), [SCREEN](SCREEN), [WINDOW](WINDOW)
