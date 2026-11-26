# CSNG

Converts a numeric expression to a single-precision value.

## Syntax

`CSNG`(*numeric_expression*)

## Comments

*numeric_expression* is any numeric expression.

Using `CSNG` is equivalent to assigning the expression to a single-precision variable.

Single-precision values have seven significant digits.

## Example

```vb
A#=975.3421222#
PRINT A#; CSNG(A#)
```

```text
 975.3421222 975.3421
```

## See Also

* [CINT](CINT), [CDBL](CDBL), [CLNG](CLNG)
