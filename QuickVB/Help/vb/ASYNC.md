# ASYNC and AWAIT

<!-- keywords: ASYNC, AWAIT, TASK -->

Runs a long operation without holding up the program.

## Syntax

`ASYNC FUNCTION` *name*(*parameters*) `AS TASK`(`OF` *type*)
`AWAIT` *expression*

## Comments

`AWAIT` pauses the procedure until the task is done and gives its result; the caller carries on meanwhile. An `ASYNC SUB` cannot be waited for.

## Example

```vb
ASYNC FUNCTION Fetch() AS TASK(OF STRING)
  AWAIT TASK.Delay(100)
  RETURN "done"
END FUNCTION
```

## See Also

- [FUNCTION](FUNCTION), [TRY...CATCH...FINALLY](TRY-CATCH-FINALLY)

## Online

- [Microsoft Learn: Async](https://learn.microsoft.com/dotnet/visual-basic/language-reference/modifiers/async)
