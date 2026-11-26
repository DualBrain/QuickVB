# RND

Returns a single-precision random number between 0 and 1.

## Syntax

`RND`[ (*numeric_expression*)]

## Comments

*numeric_expression* specifies how `RND` generates the next random number:

| Value | Generates |
| --- | --- |
| Less than 0 | The same number for any given *numeric_expression* |
| Equal to 0 | The last number generated |
| Greater than 0 | The next random number |

If you omit *numeric_expression*, `RND` generates the next number in the sequence.

## Example

```vb
'Print ten random numbers
FOR i = 1 TO 10
  PRINT INT(RND * 10)
NEXT i
```

## See Also

- [RANDOMIZE](RANDOMIZE)
