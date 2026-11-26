# EOF

Tests for end-of-file condition.

## Syntax

v=`EOF`(*file_number*)

## Comments

`EOF` returns true if the end of the file associated with the *file_number* specified has been reached; otherwise `EOF` returns false.

*file_number* is the number assigned to the file in its [OPEN](OPEN) statement.

Use `EOF` to test for end of file while inputting to avoid `Input Past End` errors.

## Example

```vb
OPEN "\CONFIG.SYS" FOR INPUT AS #1
DO UNTIL EOF(1)
  LINE INPUT #1, fdata$
  PRINT fdata$
LOOP
CLOSE #1
```

## See Also

* [CLOSE](CLOSE), [LOC](LOC), [LOF](LOF), [OPEN](OPEN)
