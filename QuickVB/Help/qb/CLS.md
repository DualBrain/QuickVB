# CLS

Clears the screen display.

## Syntax

`CLS` [ { 0 | 1 | 2 } ]`

## Comments

Depending on the region of the screen you want to clear, `CLS` gives you four options.

| ------------ | ------ |
| CLS | Clears text or graphics viewport |
| CLS 0 | Clears the screen of all text and graphics |
| CLS 1 | Clears only the graphics viewport |
| CLS 2 | Clears only the text viewport, leaving the bottom line unchanged |

## Example

```vb
CLS
```

This clears the screen.

## See Also

* [VIEW](VIEW), [VIEW PRINT](VIEW-PRINT), [WINDOW](WINDOW)
