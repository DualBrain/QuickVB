# KEY

To allow rapid entry of as many as 15 characters into a program with one keystroke by redefining BASIC special function keys.

## Syntax

`KEY` *function_key*,*string_expression*

`KEY ON`

`KEY OFF`

`KEY LIST`

## Comments

*function_key* is the number of the key to be redefined. 1 corresponds to F1; 10 corresponds to F10. Use 30 and 31 to represent keys F11 and F12.

*string_expression* is the key assignment. Any valid string of 1 to 15 characters may be used. If a string is longer than 15 characters, only the first 15 will be assigned. Constants must be enclosed in double quotation marks.

## Note

`KEY LIST` displays the entire 15-character assignment for each key.

`KEY ON` displays across the bottom of the screen the first 6 letters of the strings assigned to keys F1 through F10.

`KEY OFF` erases the display of the key assignments from the screen.

Assigning a null string (length 0) disables the key as a function key.

When a function key is redefined, the [INKEY$](INKEY$) function returns one character of the assigned string per invocation. If the function key is disabled, [INKEY$](INKEY$) returns a string of two characters: the first is binary zero; the second is the key scan code.

## Examples

```vb
'Assign a string to F1
KEY 1. "F1 function key"
'Display key assignments
KEY ON
INPUT "Press the F1 key"; x$
PRINT x$
```

## See Also

- [INKEY$](INKEY$)
