# SPACE$

Returns a string containing the specified number of spaces.

## Syntax

`SPACE$`(*num_spaces*)

## Comments

*num_spaces* is an integer expression (from 0 through 32,767).

## Example

```vb
FOR i = 0 TO 5
  PRINT SPACE$(i); i
NEXT i
```

```text
0
 1
  2
   3
    4
     5
```

## See Also

- [SPC](SPC); [TAB](TAB)
