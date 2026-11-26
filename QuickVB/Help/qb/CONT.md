# CONT

To continue program execution after a break.

## Syntax

`CONT`

## Comments

Resumes program execution after CTRL-BREAK, [STOP](STOP), or [END](END) halts a program. Execution continues at the point where the break happened. If the break took place during an [INPUT](INPUT) statement, execution continues after reprinting the prompt.

`CONT` is useful in debugging, in that it lets you set break points with the [STOP](STOP) statement, modify variables using direct statements, continue program execution, or use [GOTO](GOTO) to resume execution at a particular line number. If a program line is modified, `CONT` will be invalid.

## See Also

[STOP](STOP), [END](END), [INPUT](INPUT), [GOTO](GOTO)
