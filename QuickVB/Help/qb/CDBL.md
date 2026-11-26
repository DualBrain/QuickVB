# CDBL

Converts a numeric expression to a double-precision value.

## Syntax

**CDBL**(*numeric_expression*)

## Comments

*numeric_expression* is any numeric expression.

Using `CDBL` is equivalent to assigning the expression to a double-precision variable.

Single-precision values have 7 significant digits. Double-precision values have 15 significant digits.

## Example

```vb
A=454.67
PRINT A; CDBL(A)
```

```text
 454.67 454.6700134277344
```

Prints a double-precision version of the single-precision value stored in the variable named `A`.

The last 11 numbers in the double-precision number have no meaning in this example, since A was previously defined to only two-decimal place accuracy.

## Note

See the [CINT](CINT), [CLNG](CLNG) and [CSNG](CSNG) functions for converting numbers to integer, long and single precision, respectively.

## See Also

* [PRINT](PRINT), [CINT](CINT), [CLNG](CLNG), [CSNG](CSNG)
