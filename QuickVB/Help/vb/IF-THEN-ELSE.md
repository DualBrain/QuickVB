# IF...THEN...ELSE Statement

<!-- keywords: IF THEN ELSE ELSEIF -->

Runs statements depending on whether a condition is true.

## Syntax

`IF` *condition* `THEN`
  [ *statements* ]
[ `ELSEIF` *condition* `THEN`
  [ *statements* ] ]
[ `ELSE`
  [ *statements* ] ]
`END IF`

`IF` *condition* `THEN` *statement* [ `ELSE` *statement* ]

## Comments

*condition* is an expression that is True or False. The statements of the first condition that is True run; when none is, the `ELSE` statements run.

The block form ends with `END IF`. The one-line form has no `END IF`: everything is on the line.

In QuickBASIC a non-zero number was True. In Visual Basic a condition must be a Boolean: write `IF x <> 0 THEN`, not `IF x THEN`, unless `x` is a Boolean.

## Example

```vb
x = 7
IF x < 5 THEN
  PRINT("small")
ELSEIF x < 10 THEN
  PRINT("medium")
ELSE
  PRINT("large")
END IF
```

Results in:

```text
medium
```

## See Also

- [SELECT CASE](SELECT-CASE), [AND](AND), [OR](OR), [NOT](NOT)

## Online

- [Microsoft Learn: If...Then...Else statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/if-then-else-statement)
