# LPRINT

PRints on the printer LPT1.

## Syntax

`LPRINT` [*output_list*][{;|,}]

## Comments

*output_list* is a list of numeric and string expressions to be printed. Expressions must be separated by commas or semicolons.

A semicolon following the output list leaves the print head at the next character position. A comma leaves the print head at the next print zone. Print zones are 14 characters in length. Omitting a semicolon or comma causes the printer to advance to the beginning of the next line.

## Example

```vb
LPRINT "This is on line 1"
LPRINT "This is on";
LPRINT " line 2"
```

Resulting in:

```txt
This is on line 1
This is on line 2
```

## See Also

- [LPRINT USING](LPRINT-USING), [WIDTH](WIDTH)
