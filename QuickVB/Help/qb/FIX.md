# FIX

Returns the integer portion of a floating-point expression.

## Syntax

`FIX`(*numeric_expression*)

## Comments

*numeric_expression* is any numeric expression.

`FIX` does not round off numbers, it simply eliminates the decimal point and all characters to the right of the decimal point.

`FIX(x)` is equivalent to `SGN(x)*INT(ABS(x))`. The major difference between `FIX` and [INT](INT) is that `FIX` does not return the next lower number for negative *x*.

`FIX` is useful in modulus arithmetic.

## Examples

```vb
PRINT FIX(58.75)
```

```text
58
```

```vb
PRINT FIX(-58.75)
```

```text
-58
```

## See Also

- [INT](INT)
