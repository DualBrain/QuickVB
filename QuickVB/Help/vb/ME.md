# ME, MYBASE and MYCLASS

<!-- keywords: ME, MYBASE, MYCLASS -->

Name the object whose code is running.

## Syntax

`ME`.*member*
`MYBASE`.*member*
`MYCLASS`.*member*

## Comments

`ME` is the current object; `MYBASE` its base class; `MYCLASS` the class itself, ignoring replacements in classes that inherit.

## Example

```vb
PUBLIC SUB New(name AS STRING)
  ME.Name = name
END SUB
```

## See Also

- [CLASS](CLASS), [INHERITS](INHERITS), [NEW](NEW)

## Online

- [Microsoft Learn: Me, My, MyBase, MyClass](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/me-my-mybase-myclass)
