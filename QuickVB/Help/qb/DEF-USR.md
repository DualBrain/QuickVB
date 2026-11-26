# DEF USR

To specify the starting address of an assembly language subroutine to be called from memory by the [USR](USR) function.

## Syntax

`DEF USR[n]=integer`

## Comments

*n* may be any digit from 0 to 9. The digit corresponds to the [USR](USR) routine address being specified. If *n* is omitted, `DEF USR0` is assumed.

*integer* is the offset address of the [USR](USR) routine. If more than 10 [USR](USR) routines are required, `DEF USR[n]` may appear in the program as many times as necessary to redefine the `USR[n]` starting address.

Add the current segment value to the integer to get the starting address of the user routine.

When an Assembly Language Subroutine is called, the BASIC program execution is paused, and control is transferred to the Assembly Language program. When that program is executed, control is returned to the BASIC program at the point of interruption.

## Example

```vb
190 DEF SEG=0
200 DEF USR0=24000
210 X=USR0(Y^2/2.82)
```

Lines 190 and 200 set the absolute address.

Line 210 calls the [USR](USR) routine located at that address, and passes the integer value of the expression contained within the parentheses to the user program (see [USR](USR)).

## Note

This statement is given here primarily to provide compatibility with other BASIC implementations. The more versatile [CALL](CALL) statement should be used if this downward compatibility is unimportant.

## See Also

* [USR](USR), [CALL](CALL)
