# SGN

Returns a value indicating the sign of an expression.

## Syntax

`SGN`(*numeric_expression*)

## Comments

*numeric_expression* is any numeric expression. If the value of the expression is positive, `SGN` returns 1. If the value is negative, `SGN` returns -1. If the value is 0, `SGN` returns 0.

## Example

```vb
PRINT SGN(-3)
PRINT SGN(0)
PRINT SGN(127)
```

Results in:

```txt
-1
 0
 1
```
