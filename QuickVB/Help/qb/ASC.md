# ASC

To return a numeric value that is the ASCII code for the first character of the a string expression.

## Syntax

**ASC**(*string_expression*)

## Comments

If *string_expression* is null, an `Illegal Function Call` error is returned. If *string_expression* begins with an uppercase letter, the value returned will be within the range of `65` to `90`. If *string_expression* begins with a lowercase letter, the range is `97` to `122`. Numbers `0` to `9` return `48` to `57`, sequentially. See the [CHR$](CHR$) function for ASCII-to-string conversion.

## Example

```vb
X$="TEN"
PRINT ASC(X$)
```

```text
 84
```

`84` is the ASCII code for the letter `T`.

See Also:

* [PRINT](PRINT), [CHR$](CHR$)
