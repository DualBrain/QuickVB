# EDIT

To display a specified line, and to position the cursor under the first digit of the line number, so that the line may be edited.

## Syntax

`EDIT line number`

`EDIT .`

## Comments

*line number* is the number of a line existing in the program.

A period (.) refers to the current line. The following command enters `EDIT` at the current line:

`EDIT .`

When a line is entered, it becomes the current line.

The current line is always the last line referenced by an `EDIT` statement, [LIST](LIST) command, or error message.

If line number refers to a line which does not exist in the program, an "Undefined Line Number" error occurs.

## Example

```vb
EDIT 150
```

Displays program line number 150 for editing.

## See Also

* [LIST](LIST)
