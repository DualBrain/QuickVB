# GOTO

To branch unconditionally out of the normal program sequence to a specified line number or label.

## Syntax

`GOTO` *location*

## Comments

*location* is the line number or label at which execution is to continue.

Early versions of BASIC did not have [DO...LOOP](DO...LOOP), [ELSE](ELSE) clauses for [IF](IF) statements, or [SELECT CASE](SELECT) statements. They used `GOTO` to implement these constructs. To improve your programs' readability and simplify debugging, the general recommendation is to restrict the use of `GOTO`.

## Example

```vb
i = 0
Start:
  PRINT "i ="; i
  INPUT "Again"; reply$
  IF reply$="N" THEN
    END
  ELSE
    i = i + 1
  END IF
  GOTO Start
```

## See Also

- [DO UNTIL](DO-UNTIL), [DO WHILE](DO-WHILE), [IF](IF), [SELECT](SELECT)
