# FREEFILE

Returns the next available BASIC file number.

## Syntax

`FREEFILE`

## Comments

`FREEFILE` eliminates the need to hard-code file numbers and, accordingly, the risk of using a file number already in use.

## Example

```vb
filenumber = FREEFILE
OPEN "TEST.DAT" FOR OUTPUT AS filenumber
CLOSE filenumber
```

## See Also

- [OPEN](OPEN)
