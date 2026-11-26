# ERDEV

Returns an integer error code from the last device that declared an error.

## Syntax

`ERDEV`

## Comments

The MS-DOS critical error handler sets the value for `ERDEV`. The lower byte contains the MS-DOS error code (0 through 12). The upper byte contains device attribute information.

## Example

```vb
ON ERROR GOTO Handler
...
Handler:
  PRINT "Error accessing device "; ERDEV$
  PRINT "Error status code "; ERDEV
  ...
```

## See Also

- [ERDEV$](ERDEV$), [ERL](ERL), [ERR](ERR), [ERROR](ERROR), [ON ERROR GOTO](ON-ERROR-GOTO), [RESUME](RESUME)
