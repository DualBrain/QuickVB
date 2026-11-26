# INPUT

Gets keyboard input.

## Syntax

`INPUT`[;] [*prompt* {;|,}] *variables*

## Comments

A semicolon immediately after `INPUT` directs QBasic to leave the cursor on the same line after the user presses Enter.

*prompt* is the optional prompt `INPUT` displays on the screen.

A semicolon after the prompt directs `INPUT` to display a question mark after the prompt.

A comma after the prompt directs `INPUT` to suppress the question mark.

*variables* is the list of variables to input. Separate multiple variables with commas.

If you enter a type other than the expected variable type or enter too many or too few values, `INPUT` displays the message *Redo from start*, and you must reenter the data.

The user must separate multiple entries with commas.

The variable names in the list may be numeric or string variable names (including subscripted variables).

No assignment of input values is made until an acceptable response is given.

When an `INPUT` statement is encountered during program execution, the program halts, the prompt string is displayed, and the operator types in the requested data. Strings that input to an `INPUT` statement need not be surrounded by quotation marks unless they contain commas or leading or trailing blanks.

When the operator presses the RETURN key, program execution continues.

`INPUT` and [LINE INPUT](LINE-INPUT) statements have built-in [PRINT](PRINT) statements. When an `INPUT` statement with a quoted string is encountered during program execution, the quoted string is printed automatically (see the [PRINT](PRINT) statement).

The principal difference between the `INPUT` and [LINE INPUT](LINE-INPUT) statements is that [LINE INPUT](LINE-INPUT) accepts special characters (such as commas) within a string, without requiring double quotation marks, while the `INPUT` statement requires double quotation marks.

## Example

```vb
'Question mark
INPUT "Enter your name and age"; uname$, age
PRINT uname$, age
'No question mark
INPUT "Enter your name and age", uname$, age
PRINT uname$, age
```

## See Also

- [INPUT#](INPUT-FILE), [INPUT$](INPUT$), [LINE INPUT](LINE-INPUT)
