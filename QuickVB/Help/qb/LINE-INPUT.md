# LINE INPUT

Reads in a string of up to 255 characters.

## Syntax

`LINE INPUT` [;] [*prompt_string*; ] *string_variable*

`LINE INPUT` [#] *file_number*, *string_variable*

## Comments

Although the [INPUT](INPUT) statement interprets as comma as a separator between two entries, the `LINE INPUT` statement does not. The `LINE INPUT` statement reads all characters up to the carriage return and assigns them to a string variable.

If present, the semicolon immediately following the keyword `INPUT` directs `LINE INPUT` to leave the cursor on the same line after the user presses Enter.

*prompt_string* is an optional message that directs the user to enter data.

*string_variable* is the string variable to which `LINE INPUT` assigns the information entered.

*file_number* is the file number assigned to the file in its [OPEN](OPEN) statement.

## Example

```vb
LINE INPUT "Enter last name, first name, MI: "; fullname$
PRINT fullname$
```

## See Also

- [INPUT](INPUT), [INPUT$](INPUT$), [INPUT#](INPUT-FILE)
