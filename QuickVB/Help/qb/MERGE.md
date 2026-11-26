# MERGE

To merge the lines from an ASCII program file into the program already in memory.

## Syntax

`MERGE filename`

## Comments

*filename* is a valid string expression containing the filename. If no extension is specified, then BASIC assumes an extension of *.BAS*.

The diskette is searched for the named file. If found, the program lines on the diskette are merged with the lines in memory. After the `MERGE` command, the merged program resides in memory, and BASIC returns to the direct mode.

If the program being merged was not saved in ASCII code with the a option to the SAVE command, a "Bad file mode" error is issued. The program in memory remains unchanged.

If any line numbers in the file have the same number as lines in the program in memory, the lines from the file replace the corresponding lines in memory.

## Example

```vb
MERGE "SUBRTN"
```

Merges the file subrtn.bas with the program currently in memory, provided subrtn was previously saved with the a option. If some of the program lines are the same as those in the subrtn.bas file being merged, then the original program lines are replaced by the lines from subrtn.bas.
