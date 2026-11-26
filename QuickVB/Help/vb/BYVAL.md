# BYVAL, BYREF and OPTIONAL

<!-- keywords: BYVAL, BYREF, OPTIONAL, PARAMARRAY -->

Say how a procedure receives its arguments.

## Syntax

`BYVAL` | `BYREF` *parameter* `AS` *type*
`OPTIONAL` *parameter* `AS` *type* = *default*
`PARAMARRAY` *parameter*() `AS` *type*

## Comments

`BYVAL` (the default) gives a copy; `BYREF` the variable itself, so the procedure can change it. An `OPTIONAL` parameter needs a default and comes last; `PARAMARRAY` takes any number of arguments.

## Example

```vb
SUB Swap(BYREF a AS INTEGER, BYREF b AS INTEGER)
  DIM t = a
  a = b
  b = t
END SUB
```

## See Also

- [SUB](SUB), [FUNCTION](FUNCTION)

## Online

- [Microsoft Learn: Procedure parameters](https://learn.microsoft.com/dotnet/visual-basic/language-reference/declared-elements/procedure-parameters-and-arguments)
