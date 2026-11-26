# STRIG(n) Statements

Enable or disable joystick trapping.

## Syntax

`STRIG`(*button*) `ON`

`STRIG`(*button*) `OFF`

`STRIG`(*button*) `STOP`

## Comments

*button* is a numeric expression that specifies which joystick button to trap:

* 0 is button A1
* 2 is button B1
* 4 is button A2
* 6 is button B2

`STRIG`(*button*) `ON` enables joystick trapping for the specified button.

`STRIG`(*button*) `OFF` disables joystick trapping fo the specified button. All events are ignored.

`STRIG`(*button*) `STOP` temporarily disables joystick trapping for the specified button. Events are processed once trapping is enabled.

## Example

```vb
ON STRIG(0) GOSUB Handler
STRIG(0) ON
```

## See Also

- [ON...GOSUB](ON...GOSUB), [STRIG](STRIG)
