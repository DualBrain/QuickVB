# STRIG

Returns the status of a joystick trigger.

## Syntax

x=`STRIG`(*numeric_expression*)

## Comments

*numeric_expression* is an unsigned integer (from 0 through 7) that specifies the type of information desired:

| Value of n | Returns |
| ---------- | ------- |
| 0 | -1 if trigger A1 was pressed since the last STRIG(0) statement; returns 0, if not. |
| 1 | -1 if trigger A1 is currently pressed; returns 0, if not. |
| 2 | -1 if trigger B1 was pressed since the last STRIG(2) statement; returns 0, if not. |
| 3 | -1 if trigger B1 is currently pressed; returns 0, if not. |
| 4 | -1 if trigger A2 was pressed since the last STRIG(4) statement; returns 0 if not. |
| 5 | -1 if trigger A2 is currently pressed; returns 0, if not. |
| 6 | -1 if trigger B2 was pressed since the last STRIG(6) statement; returns 0 if not. |
| 7 | -1 if trigger B2 is currently pressed; returns 0, if not. |

If the specified condition is true, `STRIG` returns -1; otherwise, `STRIG` returns 0.

## Example

```vb
'Wait for use to press
'the lower button of joystick A
DO
LOOP UNTIL STRIG(0)
```

## See Also

- [ON...GOSUB](ON...GOSUB), [STRIG(n)](STRIG(n))
