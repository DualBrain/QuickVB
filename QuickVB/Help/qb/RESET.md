# RESET

Writes on disk any data still in the file buffers and closes all disk files.

## Syntax

`RESET`

## Comments

`RESET` closes all open files at once. You can use the `CLOSE` statement to close files individually.

## Example

```vb
RESET
```

## See Also

- [CLOSE](CLOSE), [OPEN](OPEN)
