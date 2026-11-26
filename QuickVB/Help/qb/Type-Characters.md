# Type Declaration Characters

Type declaration characters are suffixes added to variable names to explicitly specify their data type. They provide the highest precedence in the BASIC type system and override both DEF statements and DIM AS declarations.

## Overview

Type characters are single-character suffixes that immediately follow a variable name to declare its type. They offer a concise way to specify types and make variable types immediately visible in code.

## Available Type Characters

| Character | Type | Range | Storage | Example |
| --------- | ---- | ----- | ------- | ------- |
| `%` | Integer | -32,768 to 32,767 | 2 bytes | `count%` |
| `&` | Long | -2,147,483,648 to 2,147,483,647 | 4 bytes | `total&` |
| `!` | Single | ±3.402823E+38 | 4 bytes | `average!` |
| `#` | Double | ±1.797693134862315D+308 | 8 bytes | `precision#` |
| `$` | String | 0-65,535 characters | Variable | `name$` |

## Syntax

```vb
variableName%    ' Integer
variableName&    ' Long
variableName!    ' Single  
variableName#    ' Double
variableName$    ' String
```

## Basic Examples

### Integer Type (%)

```vb
score% = 100
counter% = 42
year% = 2024
```

### Long Type (&)

```vb
population& = 7500000000
fileSize& = 2147483647
timestamp& = 1703123456
```

### Single Type (!)

```vb
temperature! = 98.6
average! = 87.5
coordinate! = 3.14159
```

### Double Type (#)

```vb
pi# = 3.141592653589793
precise# = 1.23456789012345
avogadro# = 6.02214076E+23
```

### String Type ($)

```vb
name$ = "John Doe"
filename$ = "data.txt"
message$ = "Hello, World!"
```

## Type Character Precedence

Type characters have the **highest precedence** in the BASIC type system:

1. **Type Declaration Characters** (highest precedence)
2. **DIM AS Statements**
3. **DEF Type Statements** (lowest precedence)

### Precedence Examples

```vb
DEFINT A-Z
a = 42          ' Integer (DEFINT applies)
b! = 3.14       ' Single (type character overrides DEFINT)
c$ = "hello"    ' String (type character overrides DEFINT)
d# = 1.23       ' Double (type character overrides DEFINT)
```

```vb
DIM x AS INTEGER
x = 42          ' Integer (DIM AS applies)
y! = 3.14       ' Single (type character overrides DIM AS)
z$ = "test"     ' String (type character overrides DIM AS)
```

## Arrays with Type Characters

### Syntax (Arrays)

```vb
arrayName%(size)    ' Integer array
arrayName&(size)    ' Long array  
arrayName!(size)    ' Single array
arrayName#(size)    ' Double array
arrayName$(size)    ' String array
```

### Array Examples

```vb
' One-dimensional arrays
DIM scores%(10)        ' Integer array
DIM names$(20)         ' String array
DIM coordinates!(5)    ' Single array
DIM preciseValues#(3)  ' Double array
' Two-dimensional arrays
DIM matrix%(5, 5)      ' Integer matrix
DIM textGrid$(10, 20)  ' String grid
```

## Mixed Declaration Examples

### Variables with Different Types

```vb
' Mixed type variables in same scope
count% = 42          ' Integer
total& = 1000000     ' Long  
average! = 87.5      ' Single
precise# = 1.234567  ' Double
name$ = "Data"       ' String
```

### Function Calls and Expressions

```vb
' Functions with type characters
result$ = STR$(count%)        ' Convert integer to string
value% = VAL(number$)         ' Convert string to integer
total# = sum!(array!())       ' Function returning single used in double
```

## Practical Programming Patterns

### Traditional BASIC Naming

```vb
' Classic BASIC variable naming with type characters
i% = 1                    ' Loop counter (Integer)
total% = 0                ' Running total (Integer)
average! = 0.0            ' Average value (Single)
name$ = ""                ' Name field (String)
score! = 0.0              ' Score value (Single)
```

### Game Programming

```vb
' Game variables with type characters
playerX! = 100.5          ' X coordinate (Single)
playerY! = 50.25          ' Y coordinate (Single)
lives% = 3                ' Lives count (Integer)
score& = 0                ' Score can be large (Long)
name$ = "Player"          ' Player name (String)
```

### Data Processing

```vb
' Data processing variables
recordCount% = 0          ' Record count (Integer)
fileSize& = 0             ' File size in bytes (Long)
averageSalary# = 0.0      ' Precise average (Double)
department$ = ""          ' Department name (String)
```

### Scientific Computing

```vb
' Scientific variables
measurements# = 100.0001  ' Precise measurements (Double)
iterations% = 1000        ' Iteration count (Integer)
result# = 0.0             ' Final result (Double)
formula$ = "E=mc^2"       ' Formula string (String)
```

## Type Conversion Functions

Type characters work with conversion functions:

```vb
' Type conversions
intValue% = CINT(3.14)            ' Convert to Integer
longValue& = CLNG(1000000)       ' Convert to Long  
singleValue! = CSNG(1.234567)    ' Convert to Single
doubleValue# = CDBL(1.23)         ' Convert to Double
stringValue$ = CSTR(42)           ' Convert to String

' String to numeric conversions
num% = VAL("123")                ' String to Integer
num! = VAL("3.14")               ' String to Single
num# = VAL("1.234567")           ' String to Double
```

## Common Pitfalls and Solutions

### Multiple Type Characters (Invalid)

```vb
' INVALID: Multiple type characters
var$% = "hello"     ' Error: Cannot use multiple type characters
```

### Type Character Conflicts

```vb
DIM x AS INTEGER
x! = 3.14           ' Warning: Type character conflicts with DIM AS
```

### Array Type Conflicts

```vb
DIM array(10) AS INTEGER
array%(0) = 42      ' Warning: Array type may conflict
```

## Best Practices

### 1. Use Consistent Naming

```vb
' Good: Consistent type character usage
i%, j%, k%          ' Loop counters are always Integer
score!, average!    ' Game scores are always Single
name$, address$     ' Text data is always String
```

### 2. Choose Appropriate Types

```vb
' Good: Choose right type for the data
count% = 100              ' Small count: Integer
population& = 7500000000  ' Large count: Long
temperature! = 98.6       ' Temperature: Single
pi# = 3.141592653589793   ' High precision: Double
```

### 3. Document Type Conventions

```vb
' Good: Comment unusual type choices
bigCount& = 100000   ' Long - may exceed Integer limit
precise! = 1.0000001 ' Single - need decimal precision
```

## Performance Considerations

### Memory Usage

```vb
' Memory-efficient for large datasets
DIM millions%(1000000)    ' Integer array: ~2MB
DIM millions&(1000000)    ' Long array: ~4MB  
DIM millions!(1000000)    ' Single array: ~4MB
DIM millions#(1000000)    ' Double array: ~8MB
```

### Calculation Speed

```vb
' Fastest to slowest
result% = a% + b%         ' Integer: fastest
result& = a& + b&         ' Long: fast
result! = a! + b!         ' Single: moderate
result# = a# + b#         ' Double: slowest but most precise
```

## Related Commands

- [DEFINT](DEFINT) - Default type declarations
- [DIM](DIM) - Explicit variable declarations  
- [CINT](CINT), [CLNG](CLNG), [CSNG](CSNG), [CDBL](CDBL), [CSTR](CSTR) - Type conversion functions
- [Type-System](Type-System) - Complete type system guide
