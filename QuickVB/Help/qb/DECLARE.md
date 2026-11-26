# DECLARE

Declares a procedure and directs QBasic to perform type checking for each parameter.

## Syntax

**DECLARE** {**FUNCTION** | **SUB**} *procedure_name*[( [ *argument_list* ] )]

## Notes

`DECLARE` directs QBasic to ensure that the types of parameters passed to a procedure match the types that the procedure expects.

`DECLARE` is needed only when you don't use the [CALL](CALL) keyword.

The keyword [FUNCTION](FUNCTION) indicates that the procedure is a function; likewise, [SUB](SUB) indicates a subprogram.

*procedure_name* is the name of the function or subprogram.

*argument_list* is an optional list of parameters separated by commas. For compiler type checking, specify arguments as follows: *variable_name*[ **AS** *type*]

Valid types include [INTEGER](INTEGER), [LONG](LONG), [SINGLE](SINGLE), [DOUBLE](DOUBLE), [STRING](STRING), [ANY](ANY), or a user-defined type. [ANY](ANY) allows any type for that parameter.

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

- [CALL](CALL), [FUNCTION](FUNCTION), [SUB](SUB)
