# SELECT CASE

Evaluates an expression and executes the corresponding block of statements.

## Syntax

`SELECT CASE` *test_expression*
  `CASE` *match_expression*
    [ *statements*]
  [ `CASE` *match_expression*
    [ *statements*]]
  ...
  [ `CASE ELSE`
    [ *default_statements*]]
`END SELECT`

## Comments

The `SELECT CASE` statement evaluates an expression and searches the list of possible cases for a match. If a match is found, QBasic executes the statements for that case.

*test_expression* is a string or numeric expression to evaluate and compare to the possible cases.

*match_expression* is an expression to match *test_expression*. It can have the form
  
  *expression*[, *expression*]...

If any of the expressions listed match *test_expression*, *statements* executes.

*match_expression* can also take the form

  *expression* TO *expression*

This provides a range of possible values to match.

Lastly, *match_expression* can have the form
  
  `IS` *relation_operator* *expression*

where *relation_operator* is <, >, <=, >=, =, or < >.

*statements* is the list of statements that execute for a matching case.

*default_statements* is the list of statements that execute when no matching expression is found. These statements are associated with the `CASE ELSE` clause.

After the statements within a matching case execute, the program continues execution at the first statement following the `END SELECT` statement.

## Example

```vb
FOR i = 1 TO 5 
  SELECT CASE i 
    CASE 1
      PRINT "One"
    CASE 2, 3
      PRINT "Two or three"
    CASE IS = 4
      PRINT "Four"
    CASE ELSE
      PRINT "Five"
  END SELECT
NEXT
```

Results in:

```txt
One
Two or three
Two or three
Four
Five
```

## See Also

- [IF...THEN](IF...THEN)
