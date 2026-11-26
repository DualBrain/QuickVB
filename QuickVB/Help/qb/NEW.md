# NEW

To delete the program currently in memory and clear all variables.

## Syntax

`NEW`

## Comments

`NEW` is entered at command level to clear memory before entering a new program. BASIC always returns to command level after a `NEW` is executed.

## Examples

```vb
NEW
```

or

```vb
980 PRINT "Do You Wish To Quit (Y/N)
990 ANS$=INKEY$: IF ANS$=""THEN 990
1000 IF ANS$="Y" THEN NEW
1010 IF ANS$="N" THEN 980
1020 GOTO 990
```
