# BSAVE

Copies the context of a memory region to an output file or device.

## Syntax

**BSAVE** *filename*,*offset*,*length*

## Comments

*filename* is a string expression that specifies the file or device to which memory contents will be transferred.

*offset* is a valid numeric expression within the range of 0 to 65535. This is the offset into the segment, declared by the last [DEF SEG](DEF-SEG), where saving is to start.

*length* is a valid numeric expression within the range of 0 to 65535, specifying the length of the memory image to be saved.

If *filename* is less than one character, a "Bad File Number" error is issued and the load is aborted.

Execute a [DEF SEG](DEF-SEG) statement before the `BSAVE`. The last known [DEF SEG](DEF-SEG) address is always used for the save.

The [DEF SEG](DEF-SEG) statement must be used to set up the segment address to the start of the screen buffer. An offset of 0 and a length of 16384 specify that the entire 16K screen buffer is to be saved.

`BSAVE` performs a byte-by-byte copy. If you save the memory contents to a file, you can later use [BLOAD](BLOAD) to restore the memory image.

## Example

```vb
'Create an array with 500 elements
'Initialize the array with values from 1 through 500
DIM sales(1 TO 500)
FOR i = 1 TO 500
  sales(i) = i
NEXT i

'Set the segment address to the start of sales
DEF SEG = VARSEG(sales(1))

'Save the array using BSAVE
BSAVE "SALES.DAT", VARPTR(sales(1)), 2000

'Return to the BASIC data segment
DEF SEG
```

## See Also

- [DEF SEG](DEF-SEG), [BLOAD](BLOAD), [VARPTR](VARPTR), [VARSEG](VARSEG)
