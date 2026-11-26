# COS

Returns the cosine of the specified angle.

## Syntax

`COS`(*angle*)

## Comments

*angle* is a numeric expression that specifies an angle in radians.

You can express an angle in radians or degrees. The QBasic trigonometric routines support only radians. To convert from degrees to radians, use the following equation:

*radians* = 3.141593 * (*degrees*/180)

## Examples

```vb
angle = .785
PRINT "Cosine of"; angle; "is"; COS(angle)
```

```text
Cosine of .785 is .7073882
```

```vb
PI=3.141593
PRINT COS(PI)
DEGREES=180
RADIANS=DEGREES*PI/180
PRINT COS(RADIANS)
```

```text
 -1
 -1
```

## See Also

* [ATN](ATN), [SIN](SIN), [TAN](TAN)
