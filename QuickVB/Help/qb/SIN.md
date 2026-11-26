# SIN

Returns the sine of the specified angle.

## Syntax

`SIN`(*angle*)

## Comments

*angle* is a numeric expression that specifies an angle in radians.

You can express angles in radians or degrees. The QBasic trigonometric routines support only radians. To convert from degrees to radians, use the following equation:

*radians* = 3.141593 * (*degrees*/180)

## Example

```vb
pi = 3.141593
PRINT "Sine of pi", SIN(pi)
PRINT "Sine of pi/2", SIN(pi / 2)
```

Results in:

```text
Sine of pi      -3.258414E-07
Sine of pi/2    1
```

## See Also

- [ATN](ATN), [COS](COS), [TAN](TAN)
