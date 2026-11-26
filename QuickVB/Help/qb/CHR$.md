# CHR$

Returns a one-character string containing the character that corresponds to the specified ASCII value.

## Syntax

`CHR$`(*ascii_value*)

## Comments

*ascii_value* is a value from 0 to 255. `CHR$` is commonly used to send a special character to the terminal or printer. For example, you could send `CHR$(7)` to sound a beep through the speaker as a preface to an error message, or you could send a form feed, `CHR$(12)`, to the printer. See the [ASC](ASC) function for ASCII-to-numeric conversion.

The standard ASCII values range from 0 through 127. The IBM PC and compatibles extended ASCII characters from 128 through 255. Many extended characters are useful for drawing boxes and other simple graphics.

## Examples

```vb
PRINT CHR$(66);
```

```text
 B
```

This prints the ASCII character code `66`, which is the uppercase letter `B`.

```vb
PRINT CHR$(13);
```

This command prints a carriage return.

```vb
FOR i = 0 TO 255
  PRINT i; CHR$(i)
NEXT
```

Displays the ASCII and extended ASCII character sets.

## See Also

* [ASC](ASC), [PRINT](PRINT)
