# ON ERROR Statement

<!-- keywords: ON ERROR RESUME -->

Says what to do when an error happens.

## Syntax

`ON ERROR GOTO` *label*
`ON ERROR RESUME NEXT`
`ON ERROR GOTO 0`

## Comments

`GOTO label` sends the program to an error handler; `RESUME NEXT` goes on with the statement after the one that failed; `GOTO 0` turns error handling off.

New programs usually use `TRY...CATCH` instead.

## Example

```vb
ON ERROR RESUME NEXT
OPEN "missing.txt" FOR INPUT AS #1
IF ERR <> 0 THEN PRINT("not there")
```

## See Also

- [TRY...CATCH...FINALLY](TRY-CATCH-FINALLY), [RESUME](RESUME)

## Online

- [Microsoft Learn: On Error statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/on-error-statement)
