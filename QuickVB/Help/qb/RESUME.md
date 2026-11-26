# RESUME

Continues program execution from an error-trapping handler.

## Syntax

`RESUME` [ *location*]

`RESUME NEXT`

## Comments

*location* is the line number or label at which execution should continue. If you specify 0 or omit *location*, execution continues at the statement causing the error.

The keyword `NEXT` continues execution at the statement immediately following the statement causing the error.

## Example

```vb
ON ERROR GOTO Handler
OPEN "A:TEST.DAT" FOR INPUT AS #1
'statements
END

Handler:
  PRINT "Place disk containing TEST. DAT"
  PRINT "in drive A. Press Enter."
  INPUT dummy$
  RESUME
```

## See Also

- [ERROR](ERROR), [ON ERROR GOTO](ON-ERROR-GOTO)
