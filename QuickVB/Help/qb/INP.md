# INP

Returns a byte read from an I/O port.

## Syntax

`INP`(*port_number*)

## Comments

*port_number* is the number associated with the desired port. It must be in the range of 0 through 65,535.

## Example

```vb
' Turn on speaker through port 97
saveval = INP(97)
OUT 97, saveval + 3
DO
LOOP WHILE INKEY$ = ""
OUT 97, saveval
```

## See Also

- [OUT](OUT)
