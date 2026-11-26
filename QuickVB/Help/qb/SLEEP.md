# SLEEP

Suspends program execution for the specified length of time.

## Syntax

`SLEEP` [ *seconds*]

## Comments

*seconds* is the number of seconds the program will be suspended.

The program remains suspended until the user presses a key, the specified number of seconds expires, or an event currently being trapped occurs.

If *seconds* is omitted or is less than 1, the program remains suspended until the user presses a key or an event currently being trapped occurs.

## Example

```vb
SLEEP 30
```
