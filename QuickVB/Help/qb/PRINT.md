# PRINT

Writes to the screen or a sequential file.

## Syntax

`PRINT` [# *file_number*,] [*output_list*][{;|,}]

## Comments

*file_number* is the number of the file to which output is written. If you omit *file_number*, QBasic writes the data to the screen.

*output_list* is a list of one or more expressions to output.

If a semicolon ends the `PRINT` line, the next `PRINT` statement continues on the same line in the next character position. If a comma ends the line, the next `PRINT` statement continues on the same line in the next print zone. Print zones are 14 characters in length. Omitting a semicolon or comma causes the next `PRINT` statement to begin on the next line.

## Print Positions

BASIC divides the line into print zones of 14 spaces. The position of each item printed is determined by the punctuation used to separate the items in the list:

| Separator | Print Position |
| --------- | -------------- |
| ,         | Beginning of next zone |
| ;         | Immediately after last value |
| space(s)  | Immediately after last value |

If a comma, semicolon, or SPC or TAB function ends an expression list, the next `PRINT` statement begins printing on the same line, accordingly spaced. If the expression list ends without a comma, semicolon, or [SPC](SPC) or [TAB](TAB) function, a carriage return is placed at the end of the lines (BASIC places the cursor at the beginning of the next line).

A carriage return/line feed is automatically inserted after printing width characters, where width is 40 or 80, as defined in the [WIDTH](WIDTH) statement. This results in two lines being skipped when you print exactly 40 (or 80) characters, unless the `PRINT` statement ends in a semicolon.

When numbers are printed on the screen, the numbers are always followed by a space. Positive number are preceded by a space. Negative numbers are preceded by a minus (-) sign. Single-precision numbers are represented with seven or fewer digits in a fixed-point or integer format.

See the [LPRINT](LPRINT) and [LPRINT USING](LPRINT-USING) statements for information on sending data to be printed on a printer.

## Example

```vb
X$= STRING$(10,45)
PRINT X$"MONTHLY REPORT" X$
```

```text
----------MONTHLY REPORT----------
```

## See Also

- [LPRINT](LPRINT), [LPRINT USING](LPRINT-USING), [PRINT USING](PRINT-USING)
