# USR

To call an assembly language subroutine.

## Syntax

`v=USR[n](argument)`

## Comments:

*n* specifies which `USR` routine is being called.

*argument* can be any numeric or string expression.

Although the [CALL](CALL) statement is recommended for calling assembly language subroutines, the `USR` function call may also be used. See Appendix D in the BASIC User's Guide for a comparison of [CALL](CALL) and `USR` and for a detailed discussion of calling assembly language subroutines.

Only values 0-9 are valid for *n*. If *n* is omitted, `USR0` is assumed (see [DEF USR](DEF-USR) for the rules governing *n*).

If a segment other than the default segment (BASIC data segment, DS) is used, a [DEF SEG](DEF-SEG) statement must be executed prior to a `USR` call. This ensures that the code segment points to the subroutine being called.

The segment address given in the [DEF SEG](DEF-SEG) statement determines the starting segment of the subroutine.

For each `USR` function, a corresponding [DEF USR](DEF-USR) statement must have been executed to define the `USR` call offset. This offset and the currently active [DEF SEG](DEF-SEG) segment address determine the starting address of the subroutine.

If more than 10 user routines are required, the value(s) of [DEF USR](DEF-USR) may be redefined for the other starting addresses as many times as needed.

The type (numeric or string) of the variable receiving the function call must be consistent with the argument passed. If no argument is required by the assembly language routine, then a dummy argument must be supplied.
