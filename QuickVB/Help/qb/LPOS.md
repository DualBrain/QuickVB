# LPOS

Returns the current position of the printer head within a print buffer.

## Syntax

`LPOS`(*printer_number*)

## Comments

*printer_number* is the number of the printer of interest. 1 is LPT1, 2 is LPT2 and so on.

Not all printers support `LPOS`.

## Example

```vb
FOR i = 1 TO 100
  LPRINT i;                     'Print number on same line
  IF LPOS(1) > 50 THEN LPRINT   'Start a new line
NEXT i
```
