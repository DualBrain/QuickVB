# FUNCTION

Declares a user-defined function.

## Syntax

`FUNCTION` *function_name* [ ( *arguments* ) ] [ `STATIC` ]
  ...
  *function_name* = *expression*
  ...
`END FUNCTION`

## Notes

- *function_name* is the name of the user-defined function. The name can end with a type-declaration character (%, &, !, # or $) to indicate the type of value it returns.
- *arguments* is an optional list of parameters, separated by commas, to be passed to the function.
- To specify the type of each variable, use the following form: *variable*[()] AS *type*
- The [STATIC](STATIC) keyword directs QBasic to save the values of the function's local variables between function calls.
- For a function to return a value, the function must at some point assign an expression to the function name.

## Example

```vb
FUNCTION Min% (a AS INTEGER, b AS INTEGER)
  IF (a < b) THEN
    Min% = a
  ELSE
    Min% = b
  END IF
END FUNCTION

PRINT "Min of 5 and 3 is"; Min%(5, 3)
```

## See Also

- [DECLARE](DECLARE), [DEF FN](DEF-FN), [EXIT](EXIT), [STATIC](STATIC), [SUB](SUB)
