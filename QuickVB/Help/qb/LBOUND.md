# LBOUND

Returns the lowest array subscript for the specified array dimension.

## Syntax

v=`LBOUND`(*array_name*[ ,*dimension*])

## Comments

*array_name* is the name of the array of interest.

*dimension* is an integer value specifying the dimension in a multidimensional array. The default value is 1.

## Example

```vb
DIM a(50 TO 100) AS INTEGER
DIM box(1 TO 30, 3 TO 6) AS INTEGER

PRINT LBOUND(a)
PRINT LBOUND(box, 1), LBOUND(box, 2)
```

```text
50
1        3
```

## See Also

- [DIM](DIM), [UBOUND](UBOUND)
