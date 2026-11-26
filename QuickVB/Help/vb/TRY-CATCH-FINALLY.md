# TRY...CATCH...FINALLY Statement

<!-- keywords: TRY CATCH FINALLY THROW -->

Runs statements and handles the errors they cause.

## Syntax

`TRY`
  [ *statements* ]
[ `CATCH` [ *name* `AS` *exception* ] [ `WHEN` *condition* ]
  [ *statements* ] ]
[ `FINALLY`
  [ *statements* ] ]
`END TRY`

`THROW` [ *exception* ]

## Comments

When a statement in the `TRY` block fails, the matching `CATCH` runs. The `FINALLY` block runs always, error or not.

`THROW` raises an error of your own; alone, inside a `CATCH`, it passes the error on.

## Example

```vb
TRY
  PRINT(10 \ 0)
CATCH ex AS DivideByZeroException
  PRINT("Cannot divide by zero")
END TRY
```

Results in:

```text
Cannot divide by zero
```

## See Also

- [ON ERROR](ON-ERROR), [EXIT](EXIT)

## Online

- [Microsoft Learn: Try...Catch...Finally statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/try-catch-finally-statement)
