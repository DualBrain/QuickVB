# RETURN Statement

<!-- keywords: RETURN -->

Gives back the value of a function, or leaves a sub.

## Syntax

`RETURN` [ *value* ]

## Comments

In a `FUNCTION` the value is the result. In a `SUB` `RETURN` just leaves it. (QuickBASIC's `RETURN` ended a `GOSUB`; Visual Basic has no `GOSUB`.)

## Example

```vb
FUNCTION Twice(n AS INTEGER) AS INTEGER
  RETURN n * 2
END FUNCTION
```

## See Also

- [FUNCTION](FUNCTION), [SUB](SUB)

## Online

- [Microsoft Learn: Return statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/return-statement)
