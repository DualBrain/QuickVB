# SUB

Declares a BASIC subprogram.

## Syntax

`SUB` *subprogram_name*[ (*parameter_list*)] [`STATIC` ]
  ...
`END SUB`

## Comments

*subprogram_name* is the name of the subprogram (up to 40 characters).

*parameter_list* is a list of parameters in the following form: *variable*[() ] [AS *typename*] [, *variable*[() ] [ AS *typename*]]...

The keyword [STATIC](STATIC) directs QBasic to retain the value of the subprogram's local variables between calls.

## Example

```vb
CALL Test (1, 5.5, "TEST")
END

SUB Test (a AS INTEGER, b AS SINGLE, c AS STRING)
  PRINT a, b, c
END SUB
```

## See Also

- [CALL](CALL), [DECLARE](DECLARE), [END](END), [EXIT](EXIT), [GOSUB](GOSUB)
