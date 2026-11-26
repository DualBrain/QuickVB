# STICK

Returns a joystick's x or y coordinate.

## Syntax

x=`STICK`(*numeric_expression*)

## Comments

*numeric_expression* is an unsigned integer in the range of 0 through 3 that specifies the desired value:

| Value of n | Coordinate Returned |
| ---------- | ------------------- |
| 0          | *x* coordinate of joystick A. |
| 1          | *y* coordinate of joystick A. |
| 2          | *x* coordinate of joystick B. |
| 3          | *y* coordinate of joystick B. |

*x* and *y* coordinates can range from 1 through 200.

## Example

```vb
x% = STICK(0)
y% = STICK(1)
```
