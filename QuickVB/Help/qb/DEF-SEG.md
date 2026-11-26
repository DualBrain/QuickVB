# DEF SEG

Sets the current segment address for subsequent [PEEK](PEEK) functions and [BLOAD](BLOAD), [BSAVE](BSAVE), [CALL ABSOLUTE](CALL-ABSOLUTE) and [POKE](POKE) statements.

## Syntax

`DEF SEG` [ = *address* ]

## Comments

Entry of any value outside the range (0-65535) results in an "Illegal Function Call" error, and the previous value is retained.

If the *address* option is omitted, the segment to be used is set to BASIC's data segment (DS). This is the initial default value.

If you specify the address option, base it on a 16-byte boundary.

Segment addresses are shifted 4 bits to the left; so to get the segment address, divide the memory location by 16.

For [BLOAD](BLOAD), [BSAVE](BSAVE), [CALL](CALL), [PEEK](PEEK) and [POKE](POKE) statements, the value is shifted left four bits (this is done by the microprocessor, not by BASIC) to form the code segment address for the subsequent call instruction (see the [BLOAD](BLOAD), [BSAVE](BSAVE), [CALL](CALL), [PEEK](PEEK) and [POKE](POKE) statements).

BASIC does not perform additional checking to assure that the resultant segment address is valid.

## Examples

See [CALL ABSOLUTE](CALL-ABSOLUTE).

## Note

`DEF` and `SEG` must be separated by a space. Otherwise, BASIC will interpret the statement `DEFSEG=100` to mean, "assign the value 100 to the variable DEFSEG."

## See Also

* [BLOAD](BLOAD), [BSAVE](BSAVE), [CALL ABSOLUTE](CALL-ABSOLUTE), [PEEK](PEEK), [POKE](POKE)
