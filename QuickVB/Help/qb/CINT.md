# CINT

Rounds a numeric expression to an integer value.

## Syntax

`CINT`(*numeric_expression*)

## Comments

If *numeric_expression* is not within the range of `-32768` to `32767`, an `Overflow` error occurs.

`CINT` rounds; it does not truncate.

See the [FIX](FIX) and [INT](INT) functions, both of which return integers.

## Examples

```vb
PRINT CINT(45.67)
```

```text
 46
```

45.67 is rounded up to 46.

## Note

See the [CDBL](CDBL), [CLNG](CLNG) and [CSNG](CSNG) functions for converting numbers to the double-precision and single-precision data types, respectively.

## See Also

* [CDBL](CDBL), [CLNG](CLNG), [CSNG](CSNG), [FIX](FIX), [INT](INT), [PRINT](PRINT)
