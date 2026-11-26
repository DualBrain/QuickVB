# DIM

Declares variables and arrays with explicit types, specifying storage allocation and array dimensions.

## Syntax

```vb
DIM [SHARED] variableName [ (subscripts) ] [ AS type ]
      [, [SHARED] variableName [ (subscripts) ] [ AS type] ]...
```

**Parameters:**
- `SHARED` - Optional: Makes variables accessible to subprograms without parameter passing
- `variableName` - Name of the variable or array
- `subscripts` - Optional: Array dimensions and bounds
- `type` - Optional: Variable data type (INTEGER, LONG, SINGLE, DOUBLE, STRING, etc.)

## Variable Types

| Type | Keyword | Declaration Character | Range | Storage |
| ---- | ------- | --------------------- | ----- | ------- |
| Integer | `INTEGER` | `%` | -32,768 to 32,767 | 2 bytes |
| Long | `LONG` | `&` | -2,147,483,648 to 2,147,483,647 | 4 bytes |
| Single | `SINGLE` | `!` | ±3.402823E+38 | 4 bytes |
| Double | `DOUBLE` | `#` | ±1.797693134862315D+308 | 8 bytes |
| String | `STRING` | `$` | 0-65,535 characters | Variable |

## Simple Variable Declarations

```vb
' Basic variable declarations
DIM count AS INTEGER        ' Integer variable
DIM name AS STRING          ' String variable
DIM price AS SINGLE         ' Single-precision
DIM precision AS DOUBLE     ' Double-precision

' Multiple variables in one statement
DIM a AS INTEGER, b AS STRING, c AS DOUBLE

' With default values (variables are initialized to 0 or empty string)
DIM score AS INTEGER        ' score = 0
DIM text AS STRING          ' text = ""
```

## Array Declarations

### Basic Arrays

```vb
' One-dimensional arrays
DIM numbers(10) AS INTEGER   ' numbers(0) to numbers(10)
DIM names(20) AS STRING     ' names(0) to names(20)

' Two-dimensional arrays  
DIM matrix(5, 5) AS DOUBLE   ' 6x6 matrix (0-5, 0-5)
DIM grid(3, 4) AS INTEGER    ' 4x5 grid (0-3, 0-4)
```

### Array Bounds

```vb
' Custom lower bounds
DIM a(1 TO 10) AS INTEGER     ' a(1) to a(10)
DIM b(-5 TO 5) AS SINGLE      ' b(-5) to b(5)

' Multidimensional with custom bounds
DIM matrix(1 TO 5, 1 TO 5) AS INTEGER  ' 5x5 matrix
DIM cube(0 TO 2, 0 TO 2, 0 TO 2) AS INTEGER  ' 3x3x3 cube
```

### String Arrays

```vb
' String arrays
DIM names$(20)               ' String array using type character
DIM customers(50) AS STRING  ' String array using AS clause

' Fixed-length strings
DIM codes(10) AS STRING * 5  ' Each element holds exactly 5 characters
```

## SHARED Variables

```vb
' Global accessible variables
SHARED globalCounter AS INTEGER
SHARED userName AS STRING

' In subprograms without passing parameters
SUB UpdateCounter()
  globalCounter = globalCounter + 1
END SUB
```

## Type Characters with DIM

```vb
' Type characters can be used with array names
DIM intArray%(10)       ' Integer array using type character
DIM strArray$(20)       ' String array using type character
DIM sngArray!(5)        ' Single array using type character

' Can combine with AS clause (AS takes precedence)
DIM mixed%(10) AS STRING  ' String array (AS overrides %)
```

## Mixed Declaration Styles

```vb
' Various declaration combinations
DIM a, b AS INTEGER          ' a is Single default, b is Integer
DIM c$ AS STRING             ' String variable (both type char and AS)
DIM d(10), e(5) AS DOUBLE    ' d is Single array, e is Double array

' Complex mixed declaration
DIM sharedVar AS INTEGER
DIM array1%(5), array2(10) AS SINGLE, scalarVar AS DOUBLE
```

## Dynamic and Static Arrays

```vb
' Static arrays (fixed size)
DIM staticArray(100) AS INTEGER

' Dynamic arrays (can be resized with REDIM)
DIM dynamicArray() AS SINGLE  ' Empty declaration
REDIM dynamicArray(50)        ' Actual allocation later
```

## User-Defined Types

```vb
' Define custom type
TYPE Employee
  name AS STRING * 30
  age AS INTEGER
  salary AS SINGLE
  department AS STRING * 20
END TYPE

' Declare variables of custom type
DIM manager AS Employee
DIM staff(10) AS Employee

' Access fields
manager.name = "John Smith"
manager.age = 45
manager.salary = 75000.00

staff(1).name = "Jane Doe"
staff(1).age = 32
staff(1).salary = 55000.00
```

## Type Precedence with DIM

When DIM conflicts with other declarations:

1. **Type Declaration Characters** (highest precedence)
2. **DIM AS Statements**
3. **DEF Type Statements** (lowest precedence)

```vb
DEFINT A-Z
DIM a AS SINGLE    ' Single (DIM AS overrides DEFINT)
b = 42             ' Integer (DEFINT applies)
c! = 3.14          ' Single (type character overrides DEFINT)
```

## Practical Examples

### Game Programming Example

```vb
' Game variables with DIM
DIM playerX AS SINGLE, playerY AS SINGLE
DIM score AS INTEGER, lives AS INTEGER
DIM level AS INTEGER
DIM enemies(10) AS INTEGER  ' Enemy positions/types
DIM gameState AS STRING     ' "playing", "paused", "gameover"

' Initialize
playerX = 100.0
playerY = 50.0
score = 0
lives = 3
level = 1
```

### Data Processing Example

```vb
' Data storage arrays
DIM customerNames(100) AS STRING
DIM customerAges(100) AS INTEGER
DIM customerBalances(100) AS DOUBLE
DIM recordCount AS INTEGER

' Process data
recordCount = 0
FOR i = 1 TO 100
  customerNames(i) = ""
  customerAges(i) = 0
  customerBalances(i) = 0.0
NEXT i
```

## Memory Considerations

- **Integer arrays**: Most memory efficient for whole numbers
- **Single arrays**: Good for most floating-point needs
- **Double arrays**: Use when high precision is required
- **String arrays**: Variable memory usage based on content length

## Common Patterns

### Counter Variables

```vb
DIM i AS INTEGER, j AS INTEGER  ' Loop counters
DIM total AS INTEGER            ' Summation variable
```

### Coordinate Storage

```vb
DIM x AS SINGLE, y AS SINGLE    ' 2D coordinates
DIM z AS SINGLE                  ' 3D coordinate
DIM points(100, 2) AS SINGLE     ' Array of 2D points
```

### Text Processing

```vb
DIM buffer AS STRING * 80        ' Fixed text buffer
DIM lines(100) AS STRING        ' Array of text lines
DIM currentLine AS INTEGER       ' Current line index
```

## Error Prevention

### Array Bounds Checking

```vb
' Always initialize array bounds clearly
DIM data(1 TO MAX_SIZE) AS INTEGER  ' Clear start/end points
DIM result(0 TO 9) AS SINGLE        ' Explicit 0-based array
```

### Type Consistency

```vb
' Keep consistent types for related variables
DIM minX AS SINGLE, maxX AS SINGLE
DIM minY AS SINGLE, maxY AS SINGLE
' All coordinates use same type
```

## Related Commands

- [DEFINT](DEFINT) - Default type declarations
- [AS](AS) - Type specification clause
- [REDIM](REDIM) - Resize dynamic arrays
- [SHARED](SHARED) - Shared variable declaration
- [TYPE](TYPE) - User-defined type definition
- [OPTION BASE](OPTION-BASE) - Set array lower bound default

## See Also

- [ERASE](ERASE) - Clear arrays
- [LBOUND](LBOUND) - Get array lower bound
- [UBOUND](UBOUND) - Get array upper bound
- [REDIM](REDIM) - Resize dynamic arrays
