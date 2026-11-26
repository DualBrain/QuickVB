# EXIT Statements

Exit a [DO](DO), [FOR](FOR), [FUNCTION](FUNCTION) or [SUB](SUB).

## Syntax

`EXIT DEF`

`EXIT DO`

`EXIT FOR`

`EXIT FUNCTION`

`EXIT SUB`

## Comments

For [DO](DO) and [FOR](FOR) loops, execution continues at the first statement following the loop.

For [FUNCTION](FUNCTION) and [SUB](SUB), execution continues at the statement following the statement that called the function or subroutine.

## Examples

```vb
j = 30
FOR i = 1 TO 50
  IF i = j THEN
    EXIT FOR
  END IF
NEXT i
PRINT "Ending value is"; i
```

Resulting in...

```txt
Ending value is 30
```

## See Also

- [DEF FN](DEF-FN), [DO UNTIL](DO-UNTIL), [DO WHILE](DO-WHILE), [FOR](FOR), [FUNCTION](FUNCTION), [SUB](SUB)
