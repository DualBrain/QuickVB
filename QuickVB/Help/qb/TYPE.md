# TYPE

Creates a user-defined type.

## Syntax

`TYPE` *user_typename*
  *element_name* AS *typename*
`END TYPE`

## Comments

*user_typename* is the name of the user-defined type.

*element_name* is the name of an element in a record.

*typename* is the element's type: [INTEGER](INTEGER), [LONG](LONG), [SINGLE](SINGLE), [DOUBLE](DOUBLE), fixed-length string (for example, [STRING](STRING) * 4), or another user-defined type.

`TYPE` creates a template for future variable declarations. To create a variable of this type, you must use [DIM](DIM), [REDIM](REDIM), [COMMON](COMMON), [STATIC](STATIC) or [SHARED](SHARED).

## Example

```vb
TYPE Employee
  ename AS STRING * 20
  salary AS SINGLE
END TYPE

DIM emp AS Employee

emp.ename = "Stephanie"
emp.salary = 30000
PRINT emp.ename, emp.salary
```

Results in:

```txt
Stephanie 30000
```

## See Also

- [COMMON](COMMON), [DIM](DIM), [REDIM](REDIM), [SHARED](SHARED), [STATIC](STATIC)
