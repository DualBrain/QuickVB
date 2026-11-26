# PEN

Enables, disables or returns the light pen coordinates.

## Syntax

As a statement:

`PEN ON`

`PEN OFF`

`PEN STOP `

As a function:

x = `PEN`(*numeric_expression*)

## Comments

*numeric_expression* is an integer value (in the range 0 through 9) that specifies the information `PEN` returns:

- `0` If `PEN` was down since last poll, returns -1; if not, returns 0.
- `1` Returns the x-pixel coordinate when `PEN` was last activated. The range is within 0 to 319 for medium resolution; 0 to 639, for high resolution.
- `2` Returns the y-pixel coordinate when `PEN` was last activated. The range is within 0 to 199.
- `3` Returns the current `PEN` switch value. Returns -1 if down; 0 if up.
- `4` Returns the last known valid x-pixel coordinate. The range is within 0 to 319 for medium resolution; or 0 to 639 for high resolution.
- `5` Returns the last known valid y-pixel coordinate. The range is within 0 to 199.
- `6` Returns the character row position when `PEN` was last activated. The range is within 1 to 24.
- `7` Returns the character column position when `PEN` was last activated. The range is within 1 to 40, or 1 to 80, depending on the screen width.
- `8` Returns the last known valid character row. The range is within 1 to 24.
- `9` Returns the last known valid character column position. The range is within 1 to 40, or 1 to 80, depending on the screen width.

`PEN ON` enables the `PEN` read function.

`PEN OFF` disables the `PEN` read function.

`PEN STOP` suspends trapping. It remembers the event so immediate trapping occurs when `PEN ON` is executed.

`PEN` does not work when a mouse driver is active.

The `PEN` function is initially off. A `PEN ON` statement must be executed before any `PEN` read function calls can be made, or a `PEN` read function call results in an "Illegal function call" error.

For execution speed improvements, turn the pen off with a `PEN OFF` statement for those programs not using the light pen.

When the pen is in the border area of the screen, the values returned will be inaccurate.

## Example

```vb
PEN ON
FOR I=1 to 500
  X=PEN(0): X1=PEN(3)
  PRINT X, X1
NEXT
PEN OFF
```

## See Also

- [ON PEN](ON-PEN)
