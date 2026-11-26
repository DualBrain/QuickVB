# OVERRIDABLE and OVERRIDES

<!-- keywords: OVERRIDABLE, OVERRIDES, MUSTOVERRIDE, NOTOVERRIDABLE, SHADOWS -->

Lets a class replace a procedure of the class it inherits.

## Syntax

`PUBLIC OVERRIDABLE` `SUB` | `FUNCTION` *name*     (in the base class)
`PUBLIC OVERRIDES` `SUB` | `FUNCTION` *name*        (in the class that inherits)

## Comments

`MUSTOVERRIDE` leaves out the body: every class that inherits has to supply it. `NOTOVERRIDABLE` stops a further replacement. `SHADOWS` hides a member of the base instead of replacing it.

## Example

```vb
PUBLIC OVERRIDES FUNCTION ToString() AS STRING
  RETURN "Point"
END FUNCTION
```

## See Also

- [INHERITS](INHERITS), [CLASS](CLASS)

## Online

- [Microsoft Learn: Overrides modifier](https://learn.microsoft.com/dotnet/visual-basic/language-reference/modifiers/overrides)
