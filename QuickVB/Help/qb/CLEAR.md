# CLEAR

Initializes all program variables, closes all files and optionally defines the stack size.

## Syntax

`CLEAR` [ ,,*stack_size* ]

## Comments

`CLEAR` closes all open files, sets all numeric variables and arrays to 0 and sets all string variables to zero length.

If your program uses recursion or performs several levels of subroutine calls, you might need to increase your program's stack size.

*stack_size* is the size of the stack in bytes. You must precede the stack size with two commas as shown.

The default stack size is 2048 bytes.

Do not execute `CLEAR` within a subroutine.

The `CLEAR` command:

- Closes all files
- Clears all [COMMON](COMMON) and user variables
- Resets the stack and string space
- Releases all disk buffers
- Turns off any sound
- Resets sound to music foreground
- Resets [PEN](PEN) to off
- Resets [STRIG](STRIG) to off
- Disables [ON ERROR GOTO](ON-ERROR-GOTO) trapping

## Examples

```vb
'Initialize variables and create a stack of 4096 bytes
CLEAR,,4096
```

## See Also

- [ERASE](ERASE), [FRE](COMMON)
