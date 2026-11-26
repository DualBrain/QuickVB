# INSTR

Returns the location of the first occurrence of a string within another string.

## Syntax

`INSTR`([ *start_position* ], *search_string*, *substring*)

## Comments

`INSTR` returns the character position of *substring* within *search_string*.

*start_position* is the character position within *search_string* where the search should begin. If you omit *start_position*, `INSTR` begins as position 1.

IF `INSTR` locates the substring, it returns an index to the starting character. Otherwise, `INSTR` returns 0.

## Example

```vb
PRINT "RING in SUBSTRING", INSTR("SUBSTRING", "RING")
PRINT "X in STRING", INSTR("STRING", "X")
```

Results in:

```txt
RING in SUBSTRING 6
X in STRING       0
```

## See Also

- [LEFT$](LEFT$), [LEN](LEN), [MID$](MID$), [RIGHT$](RIGHT$)
