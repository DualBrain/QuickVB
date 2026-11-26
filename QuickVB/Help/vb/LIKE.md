# LIKE Operator

<!-- keywords: LIKE -->

Compares a string with a pattern.

## Syntax

*string* `LIKE` *pattern*

## Comments

In a pattern, `?` is any one character, `*` any run of them, `#` any digit and `[abc]` one of the listed characters.

## Example

```vb
IF name LIKE "A*" THEN PRINT("starts with A")
```

## See Also

- [Operators](OPERATORS), [IF...THEN...ELSE](IF-THEN-ELSE)

## Online

- [Microsoft Learn: Like operator](https://learn.microsoft.com/dotnet/visual-basic/language-reference/operators/like-operator)
