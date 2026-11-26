# OPTION Statement

<!-- keywords: OPTION EXPLICIT STRICT INFER COMPARE -->

Sets the rules the compiler holds a file to.

## Syntax

`OPTION EXPLICIT` { `ON` | `OFF` }
`OPTION STRICT` { `ON` | `OFF` }
`OPTION INFER` { `ON` | `OFF` }
`OPTION COMPARE` { `BINARY` | `TEXT` }

## Comments

`EXPLICIT`: every variable has to be declared. `STRICT`: no silent conversions that could lose data. `INFER`: a variable gets its type from its first value.

`COMPARE TEXT` compares strings without regard to case (`"a" = "A"`); `BINARY` is exact. The options of a program come from its project file.

## Example

```vb
OPTION EXPLICIT ON
OPTION STRICT ON
```

## See Also

- [DIM](DIM)

## Online

- [Microsoft Learn: Option statements](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/option-explicit-statement)
