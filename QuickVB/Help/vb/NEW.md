# NEW Operator

<!-- keywords: NEW, NOTHING -->

Creates an object, and runs its constructor.

## Syntax

`NEW` *type*(*arguments*)
`SUB NEW`(*parameters*)     (a constructor)

## Comments

`NOTHING` is the value of an object variable that holds no object; test it with `IS NOTHING`.

## Example

```vb
DIM list = NEW System.Collections.Generic.List(OF STRING)()
list.Add("a")
IF list IS NOTHING THEN PRINT("none")
```

## See Also

- [CLASS](CLASS), [Operators](OPERATORS), [DIM](DIM)

## Online

- [Microsoft Learn: New operator](https://learn.microsoft.com/dotnet/visual-basic/language-reference/operators/new-operator)
