# POS

Returns the cursor column position.

## Syntax

`POS`(*dummy_argument*)

## Comments

`POS` doe snot use the parameter *dummy_argument*.

## Example

```vb
FOR i = 1 TO 100
  IF (POS(0) > 50) THEN
    PRINT i
  ELSE
    PRINT i; 'same line
  END IF
NEXT
```

## See Also

- [CSRLIN](CSRLIN), [LOCATE](LOCATE), [LPOS](LPOS)
