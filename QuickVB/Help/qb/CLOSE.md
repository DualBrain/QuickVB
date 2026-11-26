# CLOSE

Closes one or more files or devices opened by the [OPEN](OPEN) statement.

## Syntax

`CLOSE` [[ # ]*file_number*[, [ # ]*file_number*]]...

## Comments

*file_number* is the file number assigned to the file or device in its [OPEN](OPEN) statement.

You can specify more than one file number in a single `CLOSE` statement.

Once you close a file number, you cannot use the file number for read or write operations until you open a new file.

The [CLEAR](CLEAR), [END](END), [RESET](RESET), [RUN](RUN) and [SYSTEM](SYSTEM) statements close your files automatically. For housekeeping purposes, however, you should issue a corresponding `CLOSE` statement for each [OPEN](OPEN) statement in your program.

`CLOSE` with no arguments closes all open files and devices.

## Examples

```vb
OPEN "TEST.DAT" FOR OUTPUT AS #1
PRINT #1, "This is a test"
CLOSE #1
```

## See Also

* [OPEN](OPEN), [RESET](RESET)
