# COM(n)

Enables or disables data communications event trapping on the specified port.

## Syntax

`COM`(*n*) `ON`

`COM`(*n*) `OFF`

`COM`(*n*) `STOP`

## Comments

*n* is the number of the communications adapter 1 or 2.

Execute a `COM(n) ON` statement before [ON COM(n)](ON-COM(n)) statement to allow trapping. After `COM(n) ON`, if a nonzero number is specified in the [ON COM(n)](ON-COM(n)) statement, BASIC checks every new statement to see if any characters have come in the communication adapter.

With `COM(n) OFF`, no trapping takes place, and all communications activity will be lost.

With `COM(n) STOP`, no trapping takes place. However, any communication that takes place will be remembered so that immediate trapping will occur when `COM(n) ON` is executed.

## Example

```vb
ON COM(1) GOSUB ComHandler
COM(1) ON
```

## See Also

* [ON COM(n)](ON-COM(n))
