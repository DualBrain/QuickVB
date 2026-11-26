# Lambda Expressions

<!-- keywords: LAMBDA -->

A procedure written in place, as a value, without a name.

## Syntax

`FUNCTION`(*parameters*) *expression*
`SUB`(*parameters*) *statement*

## Comments

A lambda sees the variables around it. It is mostly handed to a procedure that takes a delegate, such as the LINQ ones.

## Example

```vb
DIM square = FUNCTION(n AS INTEGER) n * n
PRINT(square(5))
```

## See Also

- [DELEGATE](DELEGATE), [FUNCTION](FUNCTION)

## Online

- [Microsoft Learn: Lambda expressions](https://learn.microsoft.com/dotnet/visual-basic/language-reference/program-structure/lambda-expressions)
