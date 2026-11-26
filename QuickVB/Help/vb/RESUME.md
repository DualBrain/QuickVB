# RESUME Statement

<!-- keywords: RESUME -->

Continues the program after an error was handled.

## Syntax

`RESUME` [ `NEXT` | *label* ]

## Comments

`RESUME` runs the statement that failed again; `RESUME NEXT` goes on after it; `RESUME label` continues at a label.

## Example

```vb
RESUME NEXT
```

## See Also

- [ON ERROR](ON-ERROR), [TRY...CATCH...FINALLY](TRY-CATCH-FINALLY)

## Online

- [Microsoft Learn: Resume statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/resume-statement)
