# $DYNAMIC

Set the default array storage.

## Example

`REM $DYNAMIC`

`REM $STATIC`

`$DYNAMIC` specifies that arrays declared in subsequent [DIM](DIM) statements are dynamic arrays. Array storage is allocated dynamically while the program runs.

`$STATIC` specifies that arrays declared in subsequent [DIM](DIM) statements are static arrays (unless they are declared in a non-static [SUB](SUB) or [FUNCTION](FUNCTION) procedure). Array storage is allocated when you start the program and remains fixed.

NOTE: It is generally better to use [DIM](DIM) and [REDIM](REDIM) to specify whether arrays are dynamic or static.

## See Also

- [DIM](DIM), [REDIM](REDIM), [REM](REM), [SHARED](SHARED), [STATIC](STATIC)
