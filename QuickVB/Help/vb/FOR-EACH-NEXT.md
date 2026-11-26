# FOR EACH...NEXT Statement

<!-- keywords: EACH IN -->

Repeats a block of statements for each element of an array or collection.

## Syntax

`FOR EACH` *element* [ `AS` *type* ] `IN` *group*
  [ *statements* ]
`NEXT` [ *element* ]

## Comments

*group* is an array or any collection. *element* takes each of its items in turn.

`EXIT FOR` leaves the loop; `CONTINUE FOR` goes on with the next item.

## Example

```vb
DIM names() AS STRING = {"Ann", "Bob", "Cy"}
FOR EACH n IN names
  PRINT(n)
NEXT
```

## See Also

- [FOR...NEXT](FOR-NEXT), [DIM](DIM)

## Online

- [Microsoft Learn: For Each...Next statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/for-each-next-statement)
