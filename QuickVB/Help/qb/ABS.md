# ABS

To return the absolute value of a numeric expression.

## Syntax

**ABS**(*numeric_expression*)

## Comments

*numeric_expression* is any numeric expression.

`ABS` always returns a positive value, regardless of whether the result of the expression is positive or negative.

`ABS` returns a value of the same type as *numeric_expression* ([INTEGER](INTEGER), [LONG](LONG), [DOUBLE](DOUBLE) and so on).

## Example

```vb
PRINT ABS(7*(-5))
```

```text
 35
```

Prints `35` as the result of the action.

## See Also

* [PRINT](PRINT)
