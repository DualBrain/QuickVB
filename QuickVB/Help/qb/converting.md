# Appendix E

## Converting BASIC Programs to GW-BASIC

Programs written in another BASIC language may require some minor adjustments before they can be run. The following sections describe these adjustments.

### E.1 String Dimensions

Delete all statements used to declare the length of strings. A statement such as the following:

`DIM A$(I,J)`

which dimensions a string array for J elements of length I, should be converted to the following statement:

`DIM A$(J)`

Some BASIC languages use a comma or ampersand (&) for string concatenation. Each of these must be changed to a plus sign (+), which is the operator for BASIC string concatenation.

In BASIC, the [MID$](MID$), [RIGHT$](RIGHT$), and [LEFT$](LEFT$) functions are used to take substrings of strings. Forms such as A$(I) to access the Ith character in A$, or A$(I,J) to take a substring of A$ from position I to position J, must be changed as follows:

Other BASIC | BASIC
--- | ---
X$=A$(I) | X$=MID$(A$,I,1)
X$=A$(I,J) | X$=MID$(A$,I,J-I+1)

If the substring reference is on the left side of an assignment, and X$ is used to replace characters in A$, convert as follows:

Other BASIC | BASIC
--- | ---
A$(I)=X$ | MID$(A$,I,1)=X$
A$(I,J)=X$ | MID$(A$,I,J-I+1)=X$

### E.2 Multiple Assignments

Some BASIC languages allow statements of the following form to set B and C equal to zero:

`10 LET B=C=0`

BASIC would interpret the second equal sign as a logical operator and set B equal to -1 if C equaled 0. Convert this statement to two assignment statements:

`10 C=0: B=0`

### E.3 Multiple Statements

Some BASIC languages use a backslash (\) to separate multiple statements on a line. With BASIC, be sure all elements on a line are separated by a colon (:).

### E.4 MAT Functions

Programs using the MAT functions available in some BASIC languages must be rewritten using [FOR...NEXT](FOR...NEXT) loops to execute properly.

### E.5 FOR-NEXT Loops

Some BASIC languages will always execute a [FOR...NEXT](FOR...NEXT) loop once, regardless of the limits. BASIC checks the limits first and does not execute the loop if past limits.
