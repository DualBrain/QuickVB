# AUTO

To generate and increment line numbers automatically each time you press the RETURN key.

## Syntax

`AUTO [line number][,[increment]]`

`AUTO .[,[increment]]`

## Comments

`AUTO` is useful for program entry because it makes typing line numbers unnecessary.

`AUTO` begins numbering at *line number* and increments each subsequent line number by *increment*. The default for both values is 10.

The period (.) can be used as a substitute for *line number* to indicate the current line.

If *line number* is followed by a comma, and *increment* is not specified, the last increment specified in an `AUTO` command is assumed.

If `AUTO` generates a *line number* that is already being used, an asterisk appears after the number to warn that any input will replace the existing line. However, pressing RETURN immediately after the asterisk saves the line and generates the next line number.

`AUTO` is terminated by entering CTRL-BREAK or CTRL-C. BASIC will then return to command level.

> The line in which CTRL-BREAK or CTRL-C is entered is not saved. To be sure that you save all desired text, use CTRL-BREAK and CTRL-C only on lines by themselves.

## Examples

```vb
AUTO 100, 50
```

Generates line numbers 100, 150, 200, and so on.

```vb
AUTO
```

Generates line numbers 10, 20, 30, 40, and so on.
