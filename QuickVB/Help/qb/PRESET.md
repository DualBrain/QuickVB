# PRESET

Draws a pixel at the specified screen coordinates.

## Syntax

`PRESET` [ `STEP`] (*x*, *y*) [, *color*]

## Comments

The keyword `STEP` indicates that *x* and *y* are offsets from the last point drawn, as opposed to actual coordinates.

`PRESET` is similar to [PSET](PSET), except that if you omit *color*, `PRESET` uses the background color.

If the coordinates are outside the current viewport, no point is drawn.

## Example

See [PSET](PSET)

## See Also

- [CIRCLE](CIRCLE), [COLOR](COLOR), [DRAW](DRAW), [LINE](LINE), [PAINT](PAINT), [POINT](POINT), [PSET](PSET)
