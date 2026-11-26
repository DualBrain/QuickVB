# ERASE

Reinitializes the elements of a static array or deallocates dynamic arrays.

## Syntax

`ERASE` *array* [ , *array* ]...

## Comments

*array* is the name of the array to reinitialize or deallocate.

For static numeric arrays, `ERASE` sets each element to zero. For static string arrays, `ERASE` sets each element to null.

For dynamic arrays, `ERASE` frees the memory used by the specified arrays.

## Example

```vb
DIM a(100)
FOR i = 1 TO 100
  a(i) = i
NEXT i
ERASE a     ' Reinitialize A
FOR i = 1 TO 100
  PRINT a(i)
NEXT i
```

## See Also

* [CLEAR](CLEAR), [DIM](DIM), [FRE](FRE), [REDIM](REDIM)
