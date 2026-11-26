# WITH...END WITH Statement

<!-- keywords: WITH -->

Runs statements on one object without naming it each time.

## Syntax

`WITH` *object*
  .*member* = *value*
`END WITH`

## Comments

Inside the block a name that starts with a period belongs to *object*.

## Example

```vb
WITH p
  .X = 1
  .Y = 2
END WITH
```

## See Also

- [DIM](DIM)

## Online

- [Microsoft Learn: With...End With statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/with-end-with-statement)
