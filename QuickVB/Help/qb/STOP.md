# STOP

Ends the program at any point.

## Syntax

`STOP`

## Comments

A program should only have one starting and ending point. The use of `STOP` to end a program from different locations is strongly discouraged.

## Example

```vb
Handler:
  PRINT "Failed to open file on"
  PRINT "the third attempt"
  STOP
```
