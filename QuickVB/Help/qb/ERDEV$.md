# ERDEV$

Returns a character string containing the name of the device that generated a critical error.

## Syntax

`ERDEV$`

## Comments

The MS-DOS critical error handler sets the value for `ERDEV$`.

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

- [ERDEV](ERDEV), [ERL](ERL), [ERR](ERR), [ERROR](ERROR), [ON ERROR GOTO](ON-ERROR-GOTO), [RESUME](RESUME)
