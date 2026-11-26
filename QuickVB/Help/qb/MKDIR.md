# MKDIR

Creates the specified MS-DOS subdirectory.

## Syntax

`MKDIR` *directory_name*

## Comments

*directory_name* is a string expression that specifies the subdirectory to create.

## Example

```vb
ON ERROR GOTO CheckExists
MKDIR "TESTDIR"
PRINT "Directory TESTDIR created"
Done:
  END
CheckExists:
  IF ERR = 75 THEN
    PRINT "Directory TESTDIR already exists"
    RESUME Done
  END IF 
  ON ERROR GOTO 0
```

## See Also

- [CHDIR](CHDIR), [RMDIR](RMDIR)
