# PLAY(n)

Returns the number of notes in the background music queue.

## Syntax

`PLAY`(*dummy_argument*)

## Comments

*dummy_argument* is any numeric argument. It simply distinguishes the `PLAY` function from the [PLAY](PLAY) statement.

`PLAY` returns 0 when music is in foreground mode.

## Example

```vb
notes% = PLAY(0)
```

## See Also

- [ON PLAY(n)](ON-PLAY(n)), [PLAY](PLAY), [SOUND](SOUND)
