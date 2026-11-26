# OPTION BASE

Sets the default lower array bound.

## Syntax

`OPTION BASE` {0 | 1}

## Comments

If you don't specify the default bound, QBasic uses 0.

You can specify only one `OPTION BASE` statement per module.

For more flexibility, use the `TO` clause in the [DIM](DIM) statement.

## Example

```vb
OPTION BASE 0
DIM a(1 TO 20)
DIM b(20)
PRINT LBOUND(a). LBOUND(b)
PRINT UBOUND(a), UBOUND(b)
```

Results in:

```txt
1           0
20          20
```

## See Also

- [DIM](DIM), [LBOUND](LBOUND), [REDIM](REDIM), [UBOUND](UBOUND)
