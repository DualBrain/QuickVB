# String Interpolation and Literals

<!-- keywords: INTERPOLATION -->

Put values inside a string, and write numbers and dates the way the language reads them.

## Syntax

`$"` ...{*expression*}... `"`
`&H`*hex*   `&O`*octal*   `#`*date*`#`

## Comments

`{x:N2}` formats the value; `{{` gives a brace. `&HFF` is 255, and `#1/2/2030#` is a date.

## Example

```vb
DIM n = 7
PRINT($"n is {n} and half is {n / 2:N1}")
```

## See Also

- [Data Types](DATA-TYPES), [Operators](OPERATORS)

## Online

- [Microsoft Learn: Interpolated strings](https://learn.microsoft.com/dotnet/visual-basic/language-reference/programming-guide/language-features/strings/interpolated-strings)
