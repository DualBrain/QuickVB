# ERROR

Simulates an occurrence of the error number specified. Allows a program to define its own error codes.

## Syntax

`ERROR` *numeric_expression*

## Comments

*numeric_expression* is an integer value in the range of 1 to 255.

See the [Error Codes](error-codes) for a list of predefined error status codes. To define your own error, use an undefined error value.

The `ERROR` statement assigns the error value specified to [ERR](ERR) and passes control to the error handler.

## Examples

```vb
ON ERROR GOTO Handler

' Test error handler with error 222
ERROR 222

EndTest:
  END

Handler:
  PRINT "In error handler with error"; ERR
  RESUME EndTest
```

## See Also

- [ERDEV](ERDEV), [ERDEV$](ERDEV$), [ERL](ERL), [ERR](ERR), [ON ERROR GOTO](ON-ERROR-GOTO), [RESUME](RESUME)
