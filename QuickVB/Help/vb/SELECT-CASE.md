# SELECT CASE Statement

<!-- keywords: SELECT CASE -->

Runs one of several blocks of statements depending on the value of an expression.

## Syntax

`SELECT CASE` *test_expression*
  `CASE` *match_expression*
    [ *statements* ]
  [ `CASE` *match_expression*
    [ *statements* ] ]
  [ `CASE ELSE`
    [ *statements* ] ]
`END SELECT`

## Comments

*match_expression* can be a value, a list of values separated by commas, a range (`1 TO 5`), or a comparison (`IS > 10`).

The statements of the first matching `CASE` run; then the program goes on after `END SELECT`. `CASE ELSE` runs when nothing matched.

## Example

```vb
n = 3
SELECT CASE n
  CASE 1, 2
    PRINT("low")
  CASE 3 TO 5
    PRINT("middle")
  CASE IS > 5
    PRINT("high")
  CASE ELSE
    PRINT("?")
END SELECT
```

Results in:

```text
middle
```

## See Also

- [IF...THEN...ELSE](IF-THEN-ELSE)

## Online

- [Microsoft Learn: Select...Case statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/select-case-statement)
