# DO...UNTIL

Repeats a set of instructions until a condition becomes true.

## Syntax

`DO UNTIL` *boolean_expression*
  *statements*
`LOOP`

*or*

`DO`
  *statements*
`LOOP UNTIL` *boolean_expression*

## Comments

*boolean_expression* is an expression that evaluates to true or false, such as `I > 100`.

The first form of the statement first test the *boolean_expression*. If the expression is true, QBasic skips the statements within the loop and continues execution at the first statement following the loop. If the expression is false, QBasic executes the statements within the loop until the *boolean_expression* is true.

The second form of the statement first performs the statements within the loop and then tests the *boolean_expression*. If the expression is false, QBasic repeats the statements in the loop. If the *boolean_expression* is true, QBasic continues execution at the first statement following the loop.

## Example

```vb
i = 0
DO
  PRINT i
  i = i + 1
LOOP UNTIL i = 100
```

## See Also

* [DO WHILE](DO-WHILE), [EXIT](EXIT), [FOR](FOR), [WHILE...WEND](WHILE...WEND)
