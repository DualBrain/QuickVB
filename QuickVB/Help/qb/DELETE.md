# DELETE

To delete program lines or line ranges.

## Syntax

`DELETE [line number1][-line number2]`

`DELETE line number1-`

## Comments

*line number1* is the first line to be deleted.

*line number2* is the last line to be deleted.

BASIC always returns to command level after a `DELETE` command is executed. Unless at least one line number is given, an "Illegal Function Call" error occurs.

The period (.) may be used to substitute for either line number to indicate the current line.

## Examples

```vb
DELETE 40 
```

Deletes line 40.

```vb
DELETE 40-100 
```

Deletes lines 40 through 100, inclusively.

```vb
DELETE -40
```

Deletes all lines up to and including line 40.

```vb
DELETE 40- 
```

Deletes all lines from line 40 to the end of the program.
