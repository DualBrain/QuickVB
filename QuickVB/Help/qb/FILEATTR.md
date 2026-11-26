# FILEATTR

Returns either the file mode or the MS-DOS file handle for an open file.

## Syntax

`FILEATTR`( *file_number*, *file_info* )

## Comments

*file_number* is the file number assigned to the file in its [OPEN](OPEN) statement.

*file_info* dictates the information `FILEATTR` returns. If 1, `FILEATTR` returns the access mode:

| Value | Mode   |
| ----- | ------ |
|   1   | Input  |
|   2   | Output |
|   4   | Random |
|   8   | Append |
|  32   | Binary |

If *file_info* is 2, `FILEATTR` returns the MS-DOS file handle.

## Example

```vb
OPEN "APPEND.DAT" FOR APPEND AS #1
OPEN "OUTPUT.DAT" FOR OUTPUT AS #2

PRINT "File 1 Mode"; FILEATTR(1, 1)
PRINT "File 1 Handle"; FILEATTR(1, 2)
PRINT "File 2 Mode"; FILEATTR(2, 1)
PRINT "File 2 Handle"; FILEATTR(2. 2)
CLOSE #1: CLOSE #2
```

Resulting in:

```txt
File 1 Mode 8
File 1 Handle 5 
File 2 Mode 2 
File 2 Handle 6
```

## See Also

- [OPEN](OPEN)
