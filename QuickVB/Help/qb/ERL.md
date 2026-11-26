# ERL

Returns the line number of the statement that caused the error or the closest line number before the error-causing statement.

## Syntax

`ERL`

## Comments

`ERL` returns only line numbers. It does not return line labels. If you are not using line numbers, `ERL` returns 0.

## Example

```vb
100 PRINT 1/0
1000 Handler:
1010   PRINT "Error processing at line"; ERL
1020   PRINT "Error number"; ERR
1030   RESUME
```

Running this program produces the following:

```txt
Error processing at line 100
Error number 11
```

## See Also

- [ERDEV](ERDEV), [ERDEV$](ERDEV$), [ERR](ERR), [ERROR](ERROR), [ON ERROR GOTO](ON-ERROR-GOTO), [RESUME](RESUME)
