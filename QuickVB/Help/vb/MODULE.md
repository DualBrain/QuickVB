# MODULE Statement

<!-- keywords: MODULE -->

Declares a module: a group of procedures and variables.

## Syntax

`MODULE` *name*
  [ *declarations* ]
  [ *procedures* ]
`END MODULE`

## Comments

A program is a module with a `SUB Main`, where the program begins. What a module declares can be used by name from anywhere in the program.

## Example

```vb
MODULE Program
  SUB Main()
    PRINT("Hello World!")
  END SUB
END MODULE
```

## See Also

- [SUB](SUB), [IMPORTS](IMPORTS)

## Online

- [Microsoft Learn: Module statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/module-statement)
