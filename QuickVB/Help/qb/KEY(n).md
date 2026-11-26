# KEY(n)

To initiate and terminate key capture in a GW-BASIC program.

## Syntax

`KEY`(*n*) `ON

`KEY`(*n*) `OFF`

`KEY`(*n*) `STOP`

## Comments

*n* is a number that indicates which key is to be captured. Keys are numbered as follows:

| Key Number | Key |
| ---------- | --- |
| 1-10 | Function keys F1 through F10 |
| 11 | CURSOR UP |
| 12 | CURSOR LEFT |
| 13 | CURSOR RIGHT |
| 14 | CURSOR DOWN |
| 15-25 | Keys defined in the following format (see KEY statement): `KEY` *n*, `CHR$`(*hex_code*) + `CHR$`(*scan_code*) |
| 30 and 31 | Function keys F11 and F12 |

Execution of the `KEY(n) ON` statement is required to activate keystroke capture from the function keys or cursor control keys. When the `KEY(n) ON` statement is activated and enabled, BASIC checks each new statement to see if the specified key is pressed. If so, BASIC performs a [GOSUB](GOSUB) to the line number specified in the [ON KEY(n)](ON-KEY(n)) statement. An [ON KEY(n)](ON-KEY(n)) statement must precede a `KEY(n)` statement.

When `KEY(n) OFF` is executed, no key capture occurs and no keystrokes are retained.

If `KEY(n) STOP` is executed, no key capture occurs, but if a specified key is pressed, the keystroke is retained so that immediate keystroke capture occurs when a `KEY(n) ON` is executed.

For further information on key trapping, see the [ON KEY(n)](ON-KEY(n)) statement.

## Example

```vb
ON KEY(10) GOSUB Handler
KEY(10) ON
PRINT "Press F10 to stop"
FOR i = TO 100000
  PRINT i 
NEXT i 
Handler:
  STOP
```

## See Also

- [ON...GOSUB](ON...GOSUB)