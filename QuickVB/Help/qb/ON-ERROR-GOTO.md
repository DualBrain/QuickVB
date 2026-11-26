# ON ERROR GOTO

Enables error handling and specifies the first line of the error handler.

## Syntax

`ON ERROR GOTO` *location*

## Comments

See [ERR](ERR) for a list of possible errors.

*location* is the line number or label of the first line in the handler. A line number of 0 disables error handling.

If error handling is disabled, an error results in an error message and program termination.

## Example

```vb
ON ERROR GOTO Handler
OPEN "NOFILE.DAT" FOR INPUT AS #1
PRINT "File opened"
CLOSE #1
Done:
  END
Handler:
  IF ERR = 53 THEN
    PRINT "File not found"
  END IF 
  RESUME Done
```

## See Also

- [ERDEV](ERDEV), [ERDEV$](ERDEV$), [ERL](ERL), [ERR](ERR), [ERROR](ERROR), [RESUME](RESUME)
