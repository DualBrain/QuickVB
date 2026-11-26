# WAIT

Suspends program execution until the specified bit pattern is read from an input port.

## Syntax

`WAIT` *port_number*, *AND_expression*[ , *XOR_expression*]

## Comments

*port_number* is an integer expression (from 0 through 255) that identifies the port.

*AND_expression* is an integer expression that `WAIT` combines with the port value using an [AND](AND) operation.

*XOR_expression* is an integer expression that `WAIT` combines with the port value using an [XOR](XOR) operation.

Data from the specified port is first combined with *XOR_expression* if supplied. The result is then combined with *AND_expression*. If the result is zero, `WAIT` continues reading port values; otherwise, QBasic executes the next statement.

## Example

```vb
WAIT 45, 64
```
