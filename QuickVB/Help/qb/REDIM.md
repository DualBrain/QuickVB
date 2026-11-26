# REDIM

Changes the size of a dynamic array.

## Syntax

`REDIM` [ `PRESERVE`] [ `SHARED`] *array*(*subscripts*) [ `AS` *typename*] [, *array*(*subscripts*) [ `AS` *typename*]]...

## Comments

You can change the size of a dynamic array but not the number of dimensions or the data type.

The keyword `PRESERVE` indicates that the array's previous values should be preserved. Without `PRESERVE`, the array's previous values are erased.

The keyword `SHARED` indicates that the array can be used by all procedures in the module.

*array* is the name of the array to resize.

*subscripts* specifies the array subscripts in the form
  
  [*lowerbound* `TO`] *upperbound*

Multidimensional arrays are supported.

*typename* is the array type: [INTEGER](INTEGER), [LONG](LONG), [SINGLE](SINGLE),
[DOUBLE](DOUBLE), [STRING](STRING), or a user-defined type.

## Example

```vb
'$DYNAMIC
DIM box (1 TO 100)
'statements
REDIM box (1 TO 200)  ' Erases previous values
```

```vb
'$DYNAMIC
DIM box (1 TO 100)
'statements
REDIM PRESERVE box (1 TO 200)  ' Preserves previous values
```

## See Also

- [DIM](DIM), [ERASE](ERASE)
