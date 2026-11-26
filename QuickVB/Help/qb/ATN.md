# ATN

Returns the arctangent of the specified value.

## Syntax

**ATN**(*numeric_expression*)

## Comments

*numeric_expression* is the value for which you want to find the arctangent.

You can express an angle in either degrees or radians. `ATN` returns an angle in radians. To convert radians to degrees, use the following equation:

*degrees* = *radians* * (180/3.141593)

## Example

```vb
X = 3
PRINT ATN(X)
```

```text
 1.249046
```

Prints the arctangent of `3` radians (`1.249046`).

## See Also

* [PRINT](PRINT)
