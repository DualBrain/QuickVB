# EXP

Returns `e` raised to a specified power where `e` is the base of natural logarithms.

## Syntax

`EXP`(*numeric_expression*)

## Comments

*numeric_expression* specifies the power to which `e` should be raised. It must be less or equal to `88.02969`; otherwise, `EXP` results in an overflow error.

`EXP(x)` is calculated in single precision.

## Example

```vb
PRINT EXP(0), EXP(1)
```

```text
1      2.71828
```

## See Also

- [LOG](LOG)
