# Dartmouth

## "The First" circa May 1964

At some point the different versions of Dartmouth BASIC was referred to as "The Sixth", "The Seventh", etc.  So for the purposes of this documentation
will be referring to the original release as "The First".

### Specs

- Line numbers required and must be unique/sequential; 1 to 99999.
- Variables are floating point numeric only (no string variables).
- Variables (except arrays) could be named A-Z and optionally contain a number (0-9).
- Numeric values could only hold 9-digits of precision, with or without a decimal point. E-notation supported and referred to as "exponent of ten".
- Math symbols supported: `+-*/^`
- Supported parens in expressions.
- Evaluators: `= <> > < >= <=`
- Arrays are mentioned (and apparently discussed in "Part II"); however, I don't have the documentation for this so not sure of the details.
- Arrays start at index 1 (note that this is modified in later editions).
- String constants allowed.
- Only has support for the simplest version of [IF...THEN](IF...THEN); the [GOTO](GOTO) variant.
- [LET](LET) required.
- [NEXT](NEXT) requires the variable.
- [GOTO](GOTO) can optionally be written as `GO TO`.

### Functions

- [SIN](SIN)
- [COS](COS)
- [TAN](TAN)
- [ATN](ATN)
- [EXP](EXP)
- [LOG](LOG)
- [SQR](SQR)
- [ABS](ABS)
- [RND](RND)
- [INT](INT)

### Keywords

- [LET](LET)
- [PRINT](PRINT)
- [END](END)
- [FOR...NEXT](FOR...NEXT) ' variable required on [NEXT](NEXT)
- [GOTO](GOTO)
- [IF...THEN](IF...THEN) *goto*
- [DEF FN](DEF-FN)
- [DATA](DATA)
- [READ](READ)

### Commands

- HELLO
- {CATALOG|CAT}
- OLD *filename*
- NEW [*filename*]
- RENAME [*filename*]
- LIST
- RUN
- SAVE
- REPLACE
- UNSAVE
- STOP
- SYSTEM {BASIC|ALGOL}
- BYE

## "The Second" circa October 1964

### Specs

* Mention of minus sign for numbers.
* Mention of operator precedence; `(), ^, */, +-` with the `*/+-` being done left to right.
* Discusses *arrays* in the context of "Lists" (single-dimensional) and "Tables" (two-dimensional). They must be a single letter variable name. The dimensions are initialized automatically 0 to 10; the [DIM](DIM) statement allows increasing (or decreasing) the upperbound.
* The [INT](INT) statement is mentioned in the May documentation, but the details are in the missing "Part II".  In this manual, it is clear that [INT](INT) is very different in this BASIC than in the Microsoft variants.
  - "round numbers to the nearest whole integer", "operates by chopping off the fractional part"; which is different in newer BASIC's, which *truncates*.
* [PRINT](PRINT) introduces the "packed form of output" using the semi-colon character.  Instead of moving a *full zone* using the comma (which is 6 or more characters *depending*), it will "move next multiple of 3 characters". It is also mentioned that the [SPC](SPC) and [TAB](TAB) will hold off the CR (like semi-colon and comma).  There is also a discussion regarding `PRINT "X="X` where the *x* variable will print immediately after the string constant.
* [GOSUB](GOSUB) and [RETURN](RETURN) introduced. Can optionally be written as `GO SUB`.
* A new feature under "Some Ideas for More Advanced Programmers" is the [REM](REM) keyword.
* This manual also includes a section for "CARDBASIC".
   - Arrays are limited to 1000 components in any one vector or list; no matrix or table dimension may exceed 500 and total number of components in all vectors and matrices (lists and tables) may not exceed 4000.
   - Matrices must start with component 1 (not 0).
   - Although matrices can be used in regular math functions/statements after they've been DIM (DIM required), a matrix is not a matrix until the first MAT command is applied.  

## Keywords

- [DIM](DIM)
- [GOSUB](GOSUB)
- [RETURN](RETURN)
- [REM](REM)
- [MAT READ](MAT-READ) A(M, N) ' Read one matrix, dimensions shown.
- [MAT PRINT](MAT-PRINT) A ' Print one matrix.
- [MAT](MAT) C = A + B ' Add two matrices.
- [MAT](MAT) C = A - B ' Subtract matrices.
- [MAT](MAT) C = A * B ' Multiply matrices.
- [MAT](MAT) C = [ZER](ZER)(M, N) ' Introduce a 0 matrix, dimensions shown.
- [MAT](MAT) C = [CON](CON)(M, N) ' Matrix of all 1's, dimensions shown.
- [MAT](MAT) C = [IDN](IDN)(N) ' Identity matrix, dimensions shown.
- [MAT](MAT) C = [TRN](TRN)(A) ' Transpose.
- [MAT](MAT) C = [INV](INV)(A) ' Inverse.
- [MAT](MAT) C = (k)*A ' Constant multiple, note parentheses.

## Commands

- SCRATCH ' Similar to NEW except retains *filename*.

## "The Third" circa 1966

### Specs

- `MAT` merged with main from *CARDBASIC*.
- Added [SGN](SGN) and [RESTORE](RESTORE).
- Modified [INT](INT) from *toward zero* to *floor*.
- Introduced [INPUT](INPUT) for *numerical input*.
- This is also about the time that the concept of "structured" begins to make its way into BASIC by allowing white space indentation.

## "The Fourth" circa 1967

- Added [RANDOMIZE](RANDOMIZE), [ON...GOTO](ON...GOTO), [ON...THEN](ON...THEN) and [TAB](TAB).
- Introduction of string variables (denoted by appending that `$` character).
- [INPUT](INPUT) enhanced to support strings.
- Introduction of the `CHANGE` statements - `CHANGE a$ to a` and `CHANGE a to a$`; which converts between string variable and a numeric array where the 0'th element is the total number of characters (the max index).

## "The Fifth" circa 1970

- Introduction of several file handling keywords.
- [INPUT](INPUT) enhanced for reading from a file.
- Mention of `FILES` (not to be confused with [FILES](FILES)). (RESEARCH PENDING)
- Mention of `RESET`. (RESEARCH PENDING)
- Added [STR$](STR$), [VAL](VAL), [ASC](ASC), [LOC](LOC), [LOF](LOF), [LEN](LEN), [POS](POS). (RESEARCH PENDING)
- Mention of `SEG$`. (RESEARCH PENDING)
- Introduced the use of `&` to concatenate strings.
- [DEF FN](DEF-FN) enhanced for string support.
- Mention of `TIM`, `CLK$` and `DAT$`. (RESEARCH PENDING)
- Introduction of the `'` as an option for [REM](REM).

## "The Sixth" circa September 1971

- Mention of [CHAIN](CHAIN) and [CALL](CALL). (RESEARCH PENDING)
- Introduction of [SUB...END SUB](SUB...END SUB]. (RESEARCH PENDING)
- Move to utilize file numbers. (RESEARCH PENDING)

## "The Seventh" circa ????

(RESEARCH PENDING)
