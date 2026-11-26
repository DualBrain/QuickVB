# INPUT$

Reads the specified number of characters from a file or the keyboard.

## Syntax

`INPUT$`(*num_characters*[,[#]*file number*])

## Comments

*num_characters* is the number of characters for `INPUT$` to read. It must be less than or equal to the record length of the file, which is 128 by default.

*file_number* is the number assigned to the file in its [OPEN](OPEN) statement. If you omit a file number, `INPUT$` reads from the keyboard.

If the keyboard is used for input, no characters will appear on the screen. All control characters (except CTRL-BREAK) are passed through. CTRL-BREAK interrupts the execution of the `INPUT$` function.

The `INPUT$` function is preferred over [INPUT](INPUT) and [LINE INPUT](LINE-INPUT) statements for reading communications files, because all ASCII characters may be significant in communications. [INPUT](INPUT) is the least desirable because input stops when a comma or carriage return is seen. [LINE INPUT](LINE-INPUT) terminates when a carriage return is seen.

`INPUT$` allows all characters read to be assigned to a string. `INPUT$` will return *num_characters* characters from the file number or keyboard.

For more information about communications, refer to [Appendix F](communications) in the BASIC User's Guide.

## Examples

The following example lists the contents of a sequential file in hexadecimal.

```vb
'Display a file in UPPERCASE
OPEN "\CONFIG.SYS" FOR INPUT AS #1
DO WHILE NOT EOF(l)
  char$ = INPUT$(1, 1)
  PRINT UCASE$(char$):
LOOP
CLOSE #1
```

## See Also

- [INPUT](INPUT), [INPUT#](INPUT-FILE), [LINE INPUT](LINE-INPUT)
