# SHELL

Temporarily exits the program to execute an MS-DOS command or batch file.

## Syntax

`SHELL` [ *MS-DOS_command*]

## Comments

*MS-DOS_command* is a string expression that specifies the command to execute. When the MS-DOS command completes, your program continues. If you omit *MS-DOS_command*, `SHELL` displays the MS-DOS prompt. When you complete your work with MS-DOS, use the MS-DOS `EXIT` command to resume your program.

## Examples

```text
SHELL "DIR"     'Display directory listing

SHELL           'MS-DOS prompt
```
