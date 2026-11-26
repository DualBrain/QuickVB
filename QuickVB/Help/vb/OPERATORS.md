# Operators

<!-- keywords: AND OR NOT XOR ANDALSO ORELSE MOD IS ISNOT LIKE -->

The operators of Visual Basic.

## Syntax

*a* `AND` *b*   *a* `OR` *b*   *a* `XOR` *b*   `NOT` *a*
*a* `ANDALSO` *b*   *a* `ORELSE` *b*
*a* `MOD` *b*   *a* \ *b*   *a* ^ *b*
*a* & *b*   *a* `LIKE` *pattern*

## Comments

`AND`, `OR`, `XOR`, `NOT` work on Booleans and on the bits of whole numbers. `ANDALSO` and `ORELSE` stop as soon as the answer is known (the second part is not worked out).

`MOD` is the remainder of a division, `\` divides whole numbers, `^` is a power, `&` joins strings. `LIKE` compares a string with a pattern (`?` one character, `*` any, `#` a digit).

QuickBASIC's `EQV` and `IMP` have no keywords: `a EQV b` is `NOT (a XOR b)`.

## Example

```vb
IF x > 0 ANDALSO y > 0 THEN PRINT(x MOD y)
```

## See Also

- [IF...THEN...ELSE](IF-THEN-ELSE)

## Online

- [Microsoft Learn: Operators](https://learn.microsoft.com/dotnet/visual-basic/language-reference/operators/)
