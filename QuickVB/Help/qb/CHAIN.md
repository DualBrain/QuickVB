# CHAIN

Transfers control from one QBasic program to another.

## Syntax

`CHAIN` *filename*

## Comments

*filename* is a character string containing the filename of the program to which control is to be passed.

To exchange information between chained programs, you must use the [COMMON](COMMON) statement.

After you transfer control to another program using `CHAIN`, the program that called `CHAIN` does not resume control when the chained program completes.

## Note

*filename* is the name of the program that is called to be chained to. The .BAS extension is assumed unless another is specified.

## Example

```vb
CHAIN "TEST.BAS"
```

## See Also

- [CALL](CALL), [COMMON](COMMON), [RUN](RUN)
