# BEEP

To sound the speaker at 800 Hz (800 cycles per second) for one-quarter of a second.

## Syntax

`BEEP`

## Comments

`BEEP` sounds a brief tone that is useful in getting the user's attention. `BEEP` does not affect the screen display.

`BEEP`, CTRL-G, and `PRINT CHR$(7)` have the same effect.

## Examples

```vb
IF X>20 THEN BEEP
```

If X is out of range, the computer beeps.

## See Also

* [CHR$](CHR$), [PLAY](PLAY), [PRINT](PRINT), [SOUND](SOUND)
