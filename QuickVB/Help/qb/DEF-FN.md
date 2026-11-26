# DEF FN

Defines a function.

## Syntax

`DEF FN`*name* [ (*argument_list*) ] = *expression*`

`DEF FN`*name* [ (*parameter_list*) ]
  ...
  `FN`*name* = *expression*
  ...
`END DEF`

## Comments

Function names must beging with `FN` and can contain up to 40 characters. The function name indicates the value type the function returns:

| Function Name | Returns |
| --- | --- |
| `FN`day$ | string |
| `FN`count% | integer |
| `FN`average# | single-preceision value |

For a function to return a value, the function must assign its result to the function name.

*argument_list* is a list of parameters to the function separated by commas as follows:

  *argument_name* [ `AS` *type* ]

You cannot use a function before your program defines it, nor can you use `DEF FN` functions recursively.

## Example

```vb
DEF FNsum (a AS INTEGER, b AS INTEGER) = a + b

DEF FNmax (a AS INTEGER, b AS INTEGER, c AS INTEGER)
  IF (a > b) THEN
    max = a 
  ELSE 
    max = b 
  END IF
  IF (max > c) THEN
    FNmax = max
  ELSE
    FNmax = c 
  END IF 
END DEF

PRINT FNsum(3, 5)
PRINT FNmax(1, 2, 3)
```

Running this program produces the following:

```txt
8
3
```
