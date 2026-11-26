# SUB Statement

<!-- keywords: SUB -->

Declares a procedure that does something and returns no value.

## Syntax

`SUB` *name* [ ( *parameters* ) ]
  [ *statements* ]
`END SUB`

## Comments

A program starts in `SUB Main`, inside a `MODULE`. Call a `SUB` by its name: `Greet("Ann")`.

`EXIT SUB` leaves the procedure early.

Parameters are `ByVal` unless they say `ByRef`: QuickBASIC passed everything by reference.

## Example

```vb
SUB Greet(who AS STRING)
  PRINT("Hello, " & who)
END SUB
```

## See Also

- [FUNCTION](FUNCTION), [CALL](CALL), [MODULE](MODULE), [EXIT](EXIT)

## Online

- [Microsoft Learn: Sub statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/sub-statement)
