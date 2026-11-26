# BLOAD

Loads a memory-image file created by [BSAVE](BSAVE) statement into memory at the specified location.

## Syntax

**BLOAD** *filename*[, *offset*]`

## Comments

*filename* is a string expression that specifies the file containing the image to load.

*offset* is a valid numeric expression within the range of 0 to 65535. This is the offset into the segment, declared by the last [DEF SEG](DEF-SEG) statement, where loading is to start.

If offset is omitted, the offset specified at [BSAVE](BSAVE) is assumed; that is, the file is loaded into the same location it was saved from.

`BLOAD` and [BSAVE](BSAVE) work together to provide you with a quick and convenient way to load array values or graphics images.

Do not use `BLOAD` with files created by *BASICA*. QBasic and `BASICA* store items differently in memory.

## Note

`BLOAD` does not perform an address range check. It is possible to `BLOAD` anywhere in memory. You must not `BLOAD` over the BASIC stack space, a BASIC program, or the BASIC variable area.

While `BLOAD` and [BSAVE](BSAVE) are useful for loading and saving machine language programs, they are not restricted to them. The [DEF SEG](DEF-SEG) statement lets you specify any segment as the source or target for `BLOAD` and [BSAVE](BSAVE). For example, this allows the video screen buffer to be read from or written to the diskette. `BLOAD` and [BSAVE](BSAVE) are useful in saving and displaying graphic images.

## Examples

```vb
' Create an array called SALES
DIM sales (1 TO 500)

' Set the segment address to the start of SALES
DEF SEG = VARSEG(sales(1))

' Load the array using BLOAD
BLOAD "SALES.DAT", VARPTR(sales(1))

' Return to the BASIC data segment
DEF SEG
```

## See Also

- [BSAVE](BSAVE), [DEF SEG](DEF-SEG), [VARPTR](VARPTR), [VARSEG](VARSEG)
