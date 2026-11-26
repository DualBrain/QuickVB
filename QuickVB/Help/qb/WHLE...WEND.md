# WHILE...WEND

Repeats as set of statements as long as a specified condition is true.

## Syntax

`WHILE` *condition*
  [*statements* ]
`WEND`

## Comments

*condition* is a Boolean expression. As long as *condition* is true, QBasic executes the statements within the loop.

*statements* is any list of statements.

`WHILE`/`WEND` is an older looping construct. The use of [DO](DO) is preferred.

## Examples

```vb
'BUBBLE SORT ARRAY A$
FLIPS=1
WHILE FLIPS
  FLIPS=0
  FOR N=1 TO J-1
    IF A$(N)>A$(N+1) THEN SWAP A$(N), A$(N+1): FLIPS=1
  NEXT N
WEND
```

## See Also

- [DO UNTIL](DO-UNTIL), [DO WHILE](DO-WHILE), [EXIT](EXIT)
