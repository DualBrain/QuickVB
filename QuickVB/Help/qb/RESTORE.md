# RESTORE

Allows [READ](READ) to reuse a previously read [DATA](DATA) statement.

## Syntax

`RESTORE` [ *location*]

## Comments

*location* is the line number or label of the [DATA](DATA) statement to read next. If you omit *location*, the next [READ](READ) uses the first [DATA](DATA) statement in the program.

## Example

```vb
READ A, B, C
RESTORE
READ D, E, F
DATA 57, 68, 79
```

Assigns the value `57` to both `A` and `D` variables, `68` to `B` and `E`, and so on.

## See Also

- [DATA](DATA), [READ](READ)
