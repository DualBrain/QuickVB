# USING Statement

<!-- keywords: USING -->

Uses an object and releases it when the block ends.

## Syntax

`USING` *variable* = *expression*
  [ *statements* ]
`END USING`

## Comments

The object is disposed when the block ends, even after an error: the right way to use files and streams of .NET.

## Example

```vb
USING r = New System.IO.StreamReader("a.txt")
  PRINT(r.ReadLine())
END USING
```

## See Also

- [TRY...CATCH...FINALLY](TRY-CATCH-FINALLY)

## Online

- [Microsoft Learn: Using statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/using-statement)
