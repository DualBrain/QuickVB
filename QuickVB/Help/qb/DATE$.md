# DATE$

To set or retrieve the current date.

## Syntax

As a statement:

`DATE$`=*string_expression*

As a variable:

*v$*=`DATE$`

## Comments

*string_expression* is a valid string literal or variable.

*string_expression* can be any of the following formats when assigning the date:

* *mm-dd-yy*
* *mm/dd/yy*
* *mm-dd-yyyy*
* *mm/dd/yyyy*

If you specify only the last two digits of the year, `DATE$` assumes that the first two digits are 19.

If *string_expression* is not a valid string, a "Type Mismatch" error results. Previous values are retained.

If any of the values are out of range or missing, an "Illegal Function Call" error is issued. Any previous date is retained.

The current date (as assigned when the operating system was initialized) is fetched and assigned to the string variable if `DATE$` is the expression in a [LET](LET) or [PRINT](PRINT) statement.

The current date is stored if `DATE$` is the target of a string assignment.

With `v$=DATE$`, `DATE$` returns a 10-character string in the form *mm-dd-yyyy*. *mm* is the month (01 to 12), *dd* is the day (01 to 31), and *yyyy* is the year (1980 to 2099).

## Example

```vb
v$=DATE$
PRINT v$
```

```text
 01-01-1985
```

## See Also

* [TIME$](TIME$)
