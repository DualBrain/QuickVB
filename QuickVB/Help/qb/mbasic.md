# MBASIC

## 4K BASIC

- Multiple statements per line, separated by a colon `:` (72 characters per line).
- Approximately 750 bytes available for program and variable storage before `SIN` or `SIN`, `SQR`, `RND` are deleted.
- `@` deletes a whole line and *CR* (or underline) deletes last character typed.
- Direct execution of any statement except `INPUT`.
- Two character error code and line number printed when error occurs.
  Example: `? US ERROR IN 50` would indicate a reference to an undefined statement in a `GOTO`, etc., during execution of line `50`.
- Control C - interrupt program (print `BREAK IN LINE XX`).
- Control O - toggles suppress output switch.
- All results are calculated to at least six decimal digits of precision.
  Exponents may range from 10(-38) to 10(37).
- Maximum line number of 65,535.

### STATEMENTS (4K)

`DATA`

`DIM`

`END`

`FOR`

NOTES: `IF...THEN` can be followed by a statement. Example: `IF A<5 THEN PRINT B`

`GOSUB`

`IF-THEN`

`INPUT`

`LET`

NOTES: `LET` is optional in variable assignments. Example: `A=5` is identical to `LET A-5`

`NEXT`

`READ`

`REM`

`RESTORE`

`RETURN`

`PRINT`

NOTES: `TAB(X)` within `PRINT` statement tabs to print column X.

`STOP`

### COMMANDS (4K)

`CLEAR`

NOTES: `CLEAR` deletes all variables.

`LIST`

`RUN`

`SCRATCH`

### FUNCTIONS (4K)

`ABS`

`INT`

`RND`

`SGN`

`SIN`

`SQR`

## 8K

- Approximately 2K bytes available for program and variable storage before `ATN` or `ATN`, `COS`, `SIN`, `TAN` are deleted.
- Multi-dimensioned (up to 255) arrays for both strings and numbers.
- `AND`, `OR`, `NOT` operators can be used in `IF` statements for formulas.
- `STRINGS`
  - Maximum length = 255 characters
  - String concatenation (`A$ + B$`)
  - String functions:
    - `LEN` - length of string.
    - `ASC` - returns the equivalent ASCII decimal number for the specified argument.
    - `CHR$` - truncates the numeric formula to an integer, interprets the integer as a decimal number, and converts it to its equivalent ASCII character.
    - `RIGHT$` - Return substrings of specified string formulas; beginning at leftmost character (`LEFT$`) or ending at rightmost (`RIGHT$`) or beginning at specified position (`MID$`) of the string formula, and containing the number of characters specified by the numeric formula.
    - `LEFT$`
    - `MID$`
    - `STR$` - number converted to a string.
    - `VAL` - string converted to a number.

### STATEMENTS (8K)

`DEF`

NOTES: `DEF` allows for single variable single statement user defined functions.

`ON-GOSUB`

`ON-GOTO`

`OUT`

NOTES: `OUT` sets status of a hardware I/O channel.

### COMMANDS (8K)

`CONT`

NOTES: `CONT` continues program execution after Control C or `STOP`.

### FUNCTIONS (8K)

`ATN`

`COS`

`EXP`

`FRE`

NOTES: `FRE` returns number of free bytes for program or variable storage. With a string argument, `FRE` returns amount of free string space.

`INP`

NOTES: `INP` returns status of a hardware I/O channel.

`LOG`

`POS`

`TAN`

---

`$`
`AND`
`ASC`
`CHR$`
`CLOAD`
`CSAVE`
`EQV`
`GOTO`
`IF-GOTO`
`IMP`
`LEFT$`
`LEN`
`MID$`
`NEW`
`NOT`
`NULL`
`OPTION BASE`
`OR`
`PEEK`
`POKE`
`RIGHT$`
`SPC`
`STR$`
`TAB`
`USR`
`VAL`
`WAIT`
`XOR`

## EXTENDED

`%`
`!`
`#`
`&`
`'`
`AUTO`
`CALL`
`CDBL`
`CINT`
`CONSOLE`
`CSNG`
`DEF DBL`
`DEF INT`
`DEF SNG`
`DEF STR`
`DEF USR`
`EDIT`
`ERASE`
`ERL`
`ERR`
`FIX`
`HEX$`
`IF-THEN-ELSE`
`INKEY$`
`INSTR`
`LINE INPUT`
`LLIST`
`LPOS`
`LPRINT`
`LPRINT USING`
`MOD`
`OCT$`
`ON ERROR GOTO`
`PRINT USING`
`RANDOMIZE`
`RENUM`
`RESUME`
`RESUME NEXT`
`SPACE$`
`STRING$`
`SWAP`
`TROFF`
`TRON`
`VARPTR`
`WEND`
`WHILE`
`WIDTH`

## DISK

`CHAIN`
`CLOSE`
`COMMON`
`CVD`
`CVI`
`CVS`
`EOF`
`FIELD`
`FILES`
`GET`
`INPUT#`
`INPUT$`
`KILL`
`LINE INPUT#`
`LOAD`
`LOC`
`LOF`
`LSET`
`MERGE`
`MKD$`
`MKI$`
`MKS$`
`NAME`
`OPEN`
`PRINT#`
`PRINT# USING`
`PUT`
`RSET`
`SAVE`
`WRITE`
`WRITE#`

