# CLNG

Rounds a numeric expression to an long (4-byte) integer.

## Syntax

`CLNG`(*numeric_expression*)

## Comments

*numeric_expression* must evaluate to a value in the range -2,147,483,648 through 2,147,483,647. If the result is outside this range, a runtime error occurs.

## Examples

```vb
PRINT CLNG(338457.8)
```

```text
 338458
```

## Note

See the [CDBL](CDBL), [CINT](CINT) and [CSNG](CSNG) functions for converting numbers to the double-precision and single-precision data types, respectively.

## See Also

* [CDBL](CDBL), [CINT](CINT), [CSNG](CSNG)
