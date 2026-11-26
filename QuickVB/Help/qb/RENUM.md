# RENUM

To renumber program lines.

## Syntax

`RENUM[new number],[old number][,incrementR]]`

## Comments

*new number* is the first line number to be used in the new sequence. The default is 10.

*old number* is the line in the current program where renumbering is to begin. The default is the first line of the program.

*increment* is the increment to be used in the new sequence. The default is 10.

`RENUM` also changes all line number references following [ELSE](ELSE), [GOTO](GOTO), [GOSUB](GOSUB), [THEN](THEN), [ON...GOTO](ON...GOTO), [ON...GOSUB](ON...GOSUB), [RESTORE](RESTORE), [RESUME](RESUME), and [ERL](ERL) statements to reflect the new line numbers. If a nonexistent line number appears after one of these statements, the error message, "Undefined line x in y" appears. The incorrect line number reference *x* is not changed by `RENUM`, but line number *y* may be changed.

`RENUM` cannot be used to change the order of program lines (for example, `RENUM 15,30` when the program has three lines numbered 10, 20 and 30) or to create line numbers greater than 65529. An "Illegal function call" error results.

## Examples

```vb
RENUM
```

Renumbers the entire program. The first new line number will be 10. Lines increment by 10.

```vb
RENUM 300,,50
```

Renumbers the entire program. The first new line number will be 300. Lines increment by 50.

```vb
RENUM 1000,900,20
```

Renumbers the lines from 900 up so they start with line number 1000 and are incremented by 20.
