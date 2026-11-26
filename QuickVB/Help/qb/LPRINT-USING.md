# LPRINT USING

Prints formatted output on the printer LPT1.

## Syntax

`LPRINT USING` *format_string*; *output_list*[ {;|,}]

## Comments

*format_string* is the output format. See [PRINT USING](PRINT-USING) for a list of formatting characters.

*output_list* is a list of numeric and string expressions to be printed. Expressions must be separated by commas or semicolons.

A semicolon following the output list leaves the print head at the next character position. A comma leaves the print head at the next print zone. Print zones are 14 characters in length. Omitting a semicolon or comma causes the printer to advance to the beginning of the next line.

## Example

See [PRINT USING](PRINT-USING).

## See Also

- [LPRINT](LPRINT), [PRINT USING](PRINT-USING), [WIDTH](WIDTH)
