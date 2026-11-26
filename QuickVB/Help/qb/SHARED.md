# SHARED

Gives a subprogram or function access to module-level variables.

## Syntax

`SHARED` *variable*[ `AS` *typename*] [, *variable*[ `AS` *typename*] ]...

## Comments

By default, a subprogram or function has access to a variable only if you pass the variable as a parameter.

*variable* is the name of the module-level variable to share.

*typename* is the variable's type: [INTEGER](INTEGER), [LONG](LONG), [SINGLE](SINGLE), [DOUBLE][DOUBLE], [STRING](STRING), or a user-defined type.

## Example

```vb
DIM a AS INTEGER
a = 5
CALL Test
END

SUB Test
  SHARED a AS INTEGER
  PRINT "Value of variable A is"; a 
END SUB
```

## See Also

- [DIM](DIM)
