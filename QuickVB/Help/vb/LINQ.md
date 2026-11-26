# LINQ Queries

<!-- keywords: LINQ, FROM, AGGREGATE -->

Pick, order and total the items of a list or array in a statement that reads like a question.

## Syntax

`FROM` *item* `IN` *collection*
  [ `WHERE` *condition* ]
  [ `ORDER BY` *expression* [ `DESCENDING` ] ]
  `SELECT` *expression*

## Comments

Needs `IMPORTS System.Linq`. The query runs when its result is walked through, for example by a `FOR EACH`.

## Example

```vb
DIM nums = {5, 3, 8, 1}
FOR EACH n IN FROM x IN nums WHERE x > 2 ORDER BY x SELECT x
  PRINT(n)
NEXT
```

## See Also

- [FOR EACH...NEXT](FOR-EACH-NEXT), [LAMBDA](LAMBDA)

## Online

- [Microsoft Learn: Introduction to LINQ](https://learn.microsoft.com/dotnet/visual-basic/language-reference/programming-guide/language-features/linq/introduction-to-linq)
