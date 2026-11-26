# POKE

Places a byte in the specified memory location.

## Syntax

`POKE` *offset*, *byte_value*

## Comments

*offset* is the offset (from 0 through 65,535) within the current segment where you want to place the byte.

*byte_value* is the value (from 0 through 255) to poke into memory.

## Examples

```vb
'Fill CGA screen with A's
DEF SEG = &HB800
FOR i = 0 TO 3999
  IF i MOD 2 = 0 THEN
    POKE i, 65    'letter A
  ELSE
    POKE i, 7     'display attribute
  END IF
NEXT i
DEF SEG
```

## See Also

- [DEF SEG](DEF-SEG), [PEEK](PEEK)
