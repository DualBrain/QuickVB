# UBOUND

Returns the highest array subscript for the specified array dimension.

## Syntax

v=`UBOUND`(*array_name*[, *dimension*])

## Comments

*array_name* is the variable name of the array.

*dimension* is an integer value specifying the dimension in a multidimensional array. The default value is 1.

## Example

```vb
DIM a(1 TO 5, 1 TO 10, 1 TO 25)
PRINT UBOUND(a), UBOUND(a, 2), UBOUND(a, 3)
```

```text
5    10    25
```

## See Also

- [DIM](DIM), [LBOUND](LBOUND), [OPTION BASE](OPTION-BASE), [REDIM](REDIM)
