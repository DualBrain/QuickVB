# NAMESPACE Statement

<!-- keywords: NAMESPACE -->

Groups declarations under a name, so they do not clash with others.

## Syntax

`NAMESPACE` *name*
  [ *declarations* ]
`END NAMESPACE`

## Comments

Code outside the namespace reaches its members as *name*.*member*, or lists the namespace in an `IMPORTS`.

## Example

```vb
NAMESPACE Shapes
  CLASS Circle
    PUBLIC Radius AS DOUBLE
  END CLASS
END NAMESPACE
```

## See Also

- [IMPORTS](IMPORTS), [CLASS](CLASS), [MODULE](MODULE)

## Online

- [Microsoft Learn: Namespace statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/namespace-statement)
