# STATIC

Makes the specified variable local to a subprogram or function and directs QBasic to preserve the variable's value between calls.

## Syntax

`STATIC` *variable*[ `AS` *typename*] [, *variable*[ `AS` *typename*]]...

## Comments

*variable* is the name of the variable to make static.

*typename* is the variable's type: [INTEGER](INTEGER), [LONG](LONG), [SINGLE](SINGLE), [DOUBLE](DOUBLE), [STRING](STRING) or a user-defined type.

## Example

```vb
CALL Test
CALL Test
END

SUB Test
  STATIC a AS INTEGER
  PRINT a 
  a = a + 1 
END SUB
```

Results in:

```txt
0
`
```

## See Also

- [COMMON](COMMON), [SHARED](SHARED)
