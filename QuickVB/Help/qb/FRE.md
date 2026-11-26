# FRE

Returns the amount of available stack space, string space or memory.

## Syntax

`FRE`(*numeric_expression*)

`FRE`(*string_expression*)

## Comments

If the argument to `FRE` is -1, `FRE` returns the size in bytes of the largest array you can create. If the argument is -2, `FRE` returns the available stack space. For any other numeric argument, `FRE` returns the amount of available string space.

If the argument to `FRE` is a string expression, `FRE` compacts the free string space into a single block and then returns the available string space.

## Example

```vb
PRINT "String space"; FRE("")
PRINT "Stack space"; FRE(-2)
PRINT "Array space"; FRE(-1)
```

```text
String space 48460
Stack space 784
Array space 184092
```

## See Also

- [CLEAR](CLEAR), [ERASE](ERASE)
