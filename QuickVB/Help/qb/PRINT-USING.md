# PRINT USING

Writes formatted output to the screen display or a file.

## Syntax

`PRINT` [[#]*file_number*,] `USING` *format_list*; *output_list*[{ ;|,}]

## Comments

*file_number* is the number of the file to which output is written. If you omit *file_number*, QBasic writes the data to the screen.

*format_list* is a string expression containing one or more of the format specifiers, see below.

*string expressions* is a string literal or variable consisting of special formatting characters. The formatting characters determine the field and the format of printed strings or numbers.

*list of expressions* consists of the string or numeric expressions separated by semicolons.

### String Fields

The following three characters may be used to format the string field:

`!`	Specifies that only the first character in the string is to be printed.

`\n spaces\` Specifies that 2+n characters from the string are to be printed.

If the backslashes are typed with no spaces, two characters are printed; if the backslashes are typed with one space, three characters are printed, and so on.

If the string is longer than the field, the extra characters are ignored. If the field is longer than the string, the string is left-justified in the field and padded with spaces on the right. For example:

```vb
10 A$="LOOK": B$="OUT"
30 PRINT USING "!"; A$; B$
40 PRINT USING"\  \"; A$; B$
50 PRINT USING"\   \"; A$; B$;"!!"
```

```text
 LO
 LOOKOUT
 LOOK OUT!!
```

`&` Specifies a variable length string field. When the field is specified with &, the string is output exactly as input. For example:

```vb
10 A$="LOOK": B$="OUT"
20 PRINT USING "!"; A$
30 PRINT USING "&"; B$
```

```text
 LOUT
```

### Numeric Fields

The following special characters may be used to format the numeric field:

`#` A pound sign is used to represent each digit position. Digit positions are always filled. If the number to be printed has fewer digits than positions specified, the number is right-justified (preceded by spaces) in the field.

`.` A decimal point may be inserted at any position in the field. If the format string specifies that a digit is to precede the decimal point, the digit always is printed (as 0 if necessary). Numbers are rounded as necessary. For example:

```text
PRINT USING "##.##";.78
 0.78
PRINT USING "###.##";987.654
 987.65
PRINT USING "##.##" ;10.2,5.3,66.789,.234
 10.20 5.30 66.79 0.23
```

In the last example, three spaces were inserted at the end of the format string to separate the printed values on the line.

`+` A plus sign at the beginning or end of the format string causes the sign of the number (plus or minus) to be printed before or after the number.

`-` A minus sign at the end of the format field causes negative numbers to be printed with a trailing minus sign. For example:

```text
PRINT USING"+##.##";-68.95,2.4,55.6,-9
 -68.95 +2.40 +55.60 -0.90
PRINT USING"##.##-";-68.95,22.449,-7.01
 68.95 22.45 7.01-
```

`**` A double asterisk at the beginning of the format string causes leading spaces in the numeric field to be filled with asterisks. The `**` also specifies two more digit positions. For example:

```text
PRINT USING "**#.#";12.39,-0.9,765.1
 *12.4* -09765.1
```

`$$` A double dollar sign at the beginning of the format string causes a dollar sign to be printed to the immediate left of the formatted number. The `$$` specifies two more digit positions, one of which is the dollar sign. The exponential format cannot be used with $$. Negative numbers cannot be used unless the minus sign trails to the right. For example:

```text
PRINT USING "$$###.##";456.78
 $456.78
```

`**$` The `**$` at the beginning of a format string combines the effects of the above two symbols. Leading spaces are filled with asterisks, and a dollar sign is printed before the number. **$ specifies three more digit positions, one of which is the dollar sign. For example:

```text
PRINT USING "**$##.##";2.34
 ***$2.34
```

`,`	A comma to the left of the decimal point in the format string causes a comma to be printed to the left of every third digit to the left of the decimal point. A comma at the end of the format string is printed as part of the string.

```text
PRINT USING "####.##";1234.5
 1234.50
text

`^^^^` Four carets may be placed after the digit position characters to specify exponential format. The four carets allow space for E+xx to be printed. Any decimal point position may be specified. The significant digits are left-justified, and the exponent is adjusted. Unless a leading + or trailing + or - is specified, one digit position is used to the left of the decimal point to print a space or a minus sign. For example:

```text
PRINT USING "##.##^^^^";234.56
 2.35E+02
PRINT USING ".####^^^^-";888888
OK
PRINT USING "+.##^^^^";123
 +.12E+03
```

Note that in the above examples the comma is not used as a delimiter with the exponential format.

`_` An underscore in the format string causes the next character to be output as a literal character. For example:

```text
PRINT USING "_!##.##_!";12.34
 !12.34!
```

The literal character itself may be an underscore by placing "_" in the format string.

`%` A percent sign is printed in front of the number if the number to be printed is larger than the specified numeric field. If rounding causes the number to exceed the field, a percent sign is printed in front of the rounded number. For example:

```text
PRINT USING "##.##";111.22
 %111.22
PRINT USING ".##"';.999
 %1.00
text
 
If the number of digits specified exceeds 24, an "Illegal function call" error results.
