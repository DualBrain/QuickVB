# FOR...NEXT

Repeats a given set of instructions a specific number of times.

## Syntax

`FOR` *control_variable* = *start_value* `TO` *end_value* [ `STEP` *increment* ]
  ...
`NEXT` [ *control_variable* [, *control_variable* ]...]

## Comments

*control_variable* is the variable that `FOR` increments with each iteration of the loop. It controls whether or not QBasic repeats the loop.

*start_value* is the initial value that QBasic assigns to the control variable.

*end_value* is the value that the control variable must equal before the loop ends.

*increment* is the amount that QBasic adds to the control variable with each iteration of the loop. The increment can be a positive or negative value. If you omit *increment*, the default increment is 1.

The `NEXT` statement directs QBasic to increment the control variable and to test whether it is greater than the end value. If it is not, execution continues at the first statement within the loop; otherwise, execution continues at the first statement following `NEXT`

## Example

```vb
FOR i = 1 TO 10
  PRINT "i ="; i
NEXT

FOR i = 1 TO 10
  FOR j = 1 TO 10
    PRINT i; "*"; j; "="; i * j
  NEXT j
NEXT i
```

## See Also

- [DO UNTIL](DO-UNTIL), [DO WHILE](DO-WHILE), [EXIT](EXIT), [WHILE...WEND](WHILE...WEND)
