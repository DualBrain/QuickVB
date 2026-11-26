# HEX$

To return a string which represents the hexadecimal value of the numeric argument.

## Syntax

v$=`HEX$`(*numeric_expression*)

## Comments

Hexadecimal notation is the base 16 numbering system. It uses the numbers 1 through 9 and the letters A through F. To store the hexadecimal representation of a number, you must use a string variable.

## Example

```vb
' Display octal, decimal and hext values from 0 to 255
FOR i = 0 TO 255
  PRINT OCT$(i), i, HEX$(i)
NEXT i
```

## See Also

- [OCT$](OCT$)
