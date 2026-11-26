# DO...WHILE

Repeats a set of instructions while a condition is true.

## Syntax

`DO WHILE` *boolean_expression*
  *statements*
`LOOP`

*or*

`DO`
  *statements*
`LOOP WHILE` *boolean_expression*

## Comments

*boolean_expression* is an expression that evaluates to true or false, such as `I > 100`.

The first form of the statement first test the *boolean_expression*. If the expression is true, QBasic executes the statements within the loop until the *boolean_expression* becomes false. Once the *boolean_expression* is false, the program continues execution at the first statement that follows the loop.

The second form of the statement first executes the statements within the loop and then tests the *boolean_expression*. If th expression is true, QBasic repeats the statements; otherwise, execution continues at the first statement after the loop.

## Example

```vb
i = 0
DO WHILE i < 100
  PRINT i
  i = i + 1
LOOP
```

## See Also

* [DO UNTIL](DO-UNTIL), [EXIT](EXIT), [FOR](FOR), [WHILE...WEND](WHILE...WEND)
