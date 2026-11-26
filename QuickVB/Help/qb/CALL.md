# CALL

Transfers control to a BASIC subprogram

## Syntax

**CALL** *subprogram_name*[( *argument_list* )]

*or*

*subprogram_name* [ *argument_list* ]`

## Notes

*subprogram_name* is the name of a BASIC subprogram created using a [SUB](SUB) statement.

*argument_list* is a list of parameters separated by commas. The subprogram can change the values of the parameters.

You can call subprograms with our without the `CALL` keyword. If you omit `CALL`, do not place the arguments within parentheses. Also, if you omit `CALL`, you must use the [DECLARE](DECLARE) statement. If you omit a declaration, QBasic will supply it automatically.

To prevent a subprogram from changing a parameter's value, simply place the parameter within parenthesis:

`CALL TEST (a, b, (c))`

In this case, the subprogram can change the values of the parameters *a* and *b*, but not *c*.

## Example

```vb
DECLARE SUB SwapVal (a, b)

a = 1
b = 2
CALL SwapVal(a, b)
PRINT a; b
END

SUB SwapVal(x, y)
  temp = x
  x = y
  y = temp
END SUB
```

Running this program produces the following:

```txt
2  1
```

## See Also

* [CALL ABSOLUTE](CALL-ABSOLUTE), [CHAIN](CHAIN), [DECLARE](DECLARE)
