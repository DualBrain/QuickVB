# IS, ISNOT and TYPEOF

<!-- keywords: ISNOT, TYPEOF, DIRECTCAST, CTYPE, TRYCAST -->

Compare objects and test or change their types.

## Syntax

*a* `IS` *b*     *a* `ISNOT` *b*
`TYPEOF` *expression* `IS` *type*
`CTYPE`(*expression*, *type*)
`DIRECTCAST`(*expression*, *type*)
`TRYCAST`(*expression*, *type*)

## Comments

`IS` compares whether two variables are the same object. `TRYCAST` gives `NOTHING` when the conversion is not possible; `CTYPE` and `DIRECTCAST` raise an error.

## Example

```vb
IF TYPEOF x IS STRING THEN PRINT("text")
```

## See Also

- [Operators](OPERATORS), [NEW](NEW)

## Online

- [Microsoft Learn: TypeOf operator](https://learn.microsoft.com/dotnet/visual-basic/language-reference/operators/typeof-operator)
