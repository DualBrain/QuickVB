# RANDOMIZE

Initializes the random number generator.

## Syntax

`RANDOMIZE` [ *seed*]

## Comments

*seed* is an integer expression that initializes the random number generator. If you omit *see*, `RANDOMIZE` prompts the user to enter it.

## Example

```vb
'Print fifty random numbers
'using ten different seeds
FOR i = 1 TO 100
  PRINT "Seed"; i
  RANDOMIZE i
  FOR j = 1 TO 5
    PRINT RND
  NEXT j
NEXT i
```

## See Also

- [RND](RND), [TIMER](TIMER)
