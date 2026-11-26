# CALL ABSOLUTE

Transfers control to a machine-language subroutine.

## Syntax

**CALL ABSOLUTE** ([ *parameter_list*,] *offset*)

## Comments

*parameter_list* is an optional list of parameters separated
by commas.

*offset* is the location within the current code segment of
the start of the procedure.

## Example

```vb
'Create a machine-language procedure and 
'call it using CALL ABSOLUTE

'Array to store machine code
DIM asmroutine(1 TO 6) AS INTEGER

'Data that makes up machine-code routine
DATA &H55           : ' PUSH BP
DATA &H8B, &HEC     : ' MOV BP, SP
DATA &HB4, 2        : ' MOV AH, 2
DATA &H2, 65        : ' MOV DL, 65
DATA &HCD, &H21     : ' INT 21H
DATA &H5D           : ' POP BP
DATA &HCB, 0        : ' RET

'Get array offset
offset = VARPTR(asmroutine(1))

'Change the segment to the start of the array
DEF SEG = VARSEG(asmroutine(1))

'Fill the array with machine code
FOR i = 0 TO 11
  READ asmcode
  POKE (offset + i), asmcode
NEXT i

'Call the routine and restore the segment
CALL ABSOLUTE(VARPTR(asmroutine(1)))
DEF SEG
```

## See Also

- [CALL](CALL)
