# Data Types

<!-- keywords: INTEGER LONG SHORT SINGLE DOUBLE DECIMAL STRING BOOLEAN BYTE CHAR DATE OBJECT -->

The types a variable can have.

## Syntax

*variable* `AS` *type*

## Comments

Each QuickBASIC type character has a Visual Basic type:

| QuickBASIC | Visual Basic | Holds |
| ---------- | ------------ | ----- |
| INTEGER% | `INTEGER` | 32 bit whole number |
| LONG& | `LONG` | 64 bit whole number |
| SINGLE! | `SINGLE` | single precision number |
| DOUBLE# | `DOUBLE` | double precision number |
| STRING$ | `STRING` | text |
| (none) | `BOOLEAN` | True or False |
| (none) | `BYTE` | 0 to 255 |
| (none) | `DECIMAL` | exact decimal number |

`SHORT` is a 16 bit whole number, `INTEGER` 32 bits, `LONG` 64 bits. `BOOLEAN` is True or False (not a number). `DECIMAL` is for money: exact, with 28 digits.

Each type is described on Microsoft's pages (see below).

## Example

```vb
DIM a AS INTEGER = 5
DIM b AS DOUBLE = 2.5
DIM s AS STRING = "text"
PRINT(a * b, s)
```

## See Also

- [DIM](DIM), [CONST](CONST)

## Online

- [Microsoft Learn: Data types](https://learn.microsoft.com/dotnet/visual-basic/language-reference/data-types/)
- [Microsoft Learn: Integer data type](https://learn.microsoft.com/dotnet/visual-basic/language-reference/data-types/integer-data-type)
- [Microsoft Learn: Long data type](https://learn.microsoft.com/dotnet/visual-basic/language-reference/data-types/long-data-type)
- [Microsoft Learn: Short data type](https://learn.microsoft.com/dotnet/visual-basic/language-reference/data-types/short-data-type)
- [Microsoft Learn: Single data type](https://learn.microsoft.com/dotnet/visual-basic/language-reference/data-types/single-data-type)
- [Microsoft Learn: Double data type](https://learn.microsoft.com/dotnet/visual-basic/language-reference/data-types/double-data-type)
- [Microsoft Learn: Decimal data type](https://learn.microsoft.com/dotnet/visual-basic/language-reference/data-types/decimal-data-type)
- [Microsoft Learn: String data type](https://learn.microsoft.com/dotnet/visual-basic/language-reference/data-types/string-data-type)
- [Microsoft Learn: Boolean data type](https://learn.microsoft.com/dotnet/visual-basic/language-reference/data-types/boolean-data-type)
- [Microsoft Learn: Byte data type](https://learn.microsoft.com/dotnet/visual-basic/language-reference/data-types/byte-data-type)
- [Microsoft Learn: Char data type](https://learn.microsoft.com/dotnet/visual-basic/language-reference/data-types/char-data-type)
- [Microsoft Learn: Date data type](https://learn.microsoft.com/dotnet/visual-basic/language-reference/data-types/date-data-type)
- [Microsoft Learn: Object data type](https://learn.microsoft.com/dotnet/visual-basic/language-reference/data-types/object-data-type)
