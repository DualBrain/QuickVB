# STRING$

Returns a string containing the specified number of occurences of a character.

## Syntax

`STRING$`(*num_char*,*ascii_character*)

`STRING$`(*num_char*,*string_expression*)

## Comments

*num_char* is the desired number of occurrences of a character.

*ascii_character* is the ASCII code of the character.

*string_expression* is any string expression. If you provide a string, `STRING$` uses the first character of the string.

## Example

```vb
X$ = STRING$(10, 45)
PRINT X$ "MONTHLY REPORT" X$
```

```text
----------MONTHLY REPORT----------
```
