# IMPORTS Statement

<!-- keywords: IMPORTS -->

Makes the names of a namespace available without qualifying them.

## Syntax

`IMPORTS` *namespace*
`IMPORTS` *alias* = *namespace*

## Comments

`IMPORTS` lines go at the top of a file. QuickVB imports the QuickBASIC namespaces for the program when it needs them (`QB.Video` for `PRINT` and `LOCATE`, `QB.Graphics` for `SCREEN` and the drawing statements), so most programs need none.

## Example

```vb
IMPORTS System.Text
```

## See Also

- [MODULE](MODULE)

## Online

- [Microsoft Learn: Imports statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/imports-statement-net-namespace-and-type)
