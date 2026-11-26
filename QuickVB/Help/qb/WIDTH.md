# WIDTH

Sets the number of columns on the screen or other device or sets the width of a file.

## Syntax

`WIDTH` [*columns* ] [, *lines*]

`WIDTH` #*file_number*, *columns*

`WIDTH` *device_name*, *columns*

`WIDTH LPRINT` *columns*

## Comments

*columns* is the number of columns. The value must be 40 or 80 for the screen.

*lines* is the number of rows of text that appear on the screen. The value can be 25, 30, 43, 50 or 60, depending on your display adapter and current screen mode.

*file_number* is the file number assigned to the file in its [OPEN](OPEN) statement.

*device_name* is a string expression that contains the name of the desired device.

The keyword `LPRINT` sets the number of columns for the printer.

After you specify a width for a device, output statements automatically wrap output to the next line when the width is exceeded.

## Example

```vb
'EGA monitor
WIDTH 80, 43
FOR i = 1 TO 100
  PRINT i
NEXT i
```

## See Also

- [LPRINT](LPRINT), [LPRINT USING](LPRINT-USING), [PRINT](PRINT), [PRINT USING](PRINT-USING), [SCREEN](SCREEN)
