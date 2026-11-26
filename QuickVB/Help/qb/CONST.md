# CONST

Defines a symbolic constant.

## Syntax

`CONST` *symbol_name* = *expression* [ , *symbol_name* = *expression* ]...

## Comments

Constants allow your programs to use symbolic names in place of numeric or string values.

*symbol_name* is the name that the constant will have throughout your program. You cannot change the value of a constant once you have defined it.

*expression* is a numeric or string expression assigned to the constant. You cannot use variables or functions in the expression.

Constants defined in a subprogram or function are local to that subprogram or function.

## Example

```vb
CONST true = 1
CONST daysperweek = 7

'Use constant in array declaration
DIM Days(daysperweek)
```
