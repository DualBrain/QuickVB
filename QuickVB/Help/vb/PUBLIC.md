# PUBLIC, PRIVATE and Access Modifiers

<!-- keywords: PUBLIC, PRIVATE, PROTECTED, FRIEND -->

Say who can use a declaration, and whether it belongs to the class or to each object.

## Syntax

`PUBLIC` | `PRIVATE` | `PROTECTED` | `FRIEND` *declaration*
`SHARED` *declaration*
`STATIC` *variable*

## Comments

`PUBLIC` is open to all; `PRIVATE` only to the declaring type; `PROTECTED` also to what inherits; `FRIEND` to the whole program. `SHARED` belongs to the class, not to an object. `STATIC` keeps a local variable's value between calls.

## Example

```vb
SUB Counter()
  STATIC n AS INTEGER
  n += 1
  PRINT(n)
END SUB
```

## See Also

- [CLASS](CLASS), [DIM](DIM)

## Online

- [Microsoft Learn: Access levels](https://learn.microsoft.com/dotnet/visual-basic/language-reference/declared-elements/access-levels)
