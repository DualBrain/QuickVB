# Attributes

<!-- keywords: ATTRIBUTE -->

Notes attached to a declaration, read by the compiler or the runtime.

## Syntax

`<`*attribute*(*arguments*)`>` *declaration*

## Comments

`<STAThread>` marks the thread model of `Main`; `<Obsolete>` makes uses warn; `<Serializable>` lets a class be saved.

## Example

```vb
<Obsolete("use NewSub")>
SUB OldSub()
END SUB
```

## See Also

- [SUB](SUB), [CLASS](CLASS)

## Online

- [Microsoft Learn: Attributes overview](https://learn.microsoft.com/dotnet/visual-basic/language-reference/programming-guide/concepts/attributes/)
