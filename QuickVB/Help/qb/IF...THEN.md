# IF...THEN

Provides conditional execution based on the evaluation of an expression.

## Syntax

`IF` *expression* `THEN` *true_statement* [ `ELSE` *false_statement* ]

*or*

`IF` *expression* `THEN`
  [ *true_statements* ]
[ `ELSEIF` *expression* `THEN`
  [ *true_statements* ]]
...
[ `ELSE`
  [ *false_statements* ]]
`END IF`

## Comments

The first form of the statement allows you to execute a single statement if the expression is true and a different statement if the expression is false.

The second form of the statement allows you to execute a series of statements if the expression is true and a different set if the expression is false. Also, this syntax allows you to test for a series of different conditions, one after another.

## Example

```vb
IF (a > b) THEN max = a ELSE max = b

IF (a > max) THEN CALL BigValue(a)

IF (day$ = "MONDAY") THEN
  CALL Meetings
  CALL Dinners
ELSE
  CALL Gym
END IF

IF (DAY$ = "MONDAY") THEN
  CALL Meetings
ELSEIF (DAY$ = "THURSDAY") THEN
  CALL Tickets
ELSEIF (DAY$ = "FRIDAY") THEN
  CALL Theatre
END IF

## See Also

- [ON](ON), [SELECT](SELECT)
