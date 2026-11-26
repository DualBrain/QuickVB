# BASIC Variable Type System Documentation

This guide covers the complete variable type declaration system in BASIC, including type precedence rules, declaration methods, and practical usage examples.

## Overview

BASIC provides multiple ways to declare variable types:

1. **Type Declaration Characters** (`%`, `&`, `!`, `#`, `$`)
2. **DEF Type Statements** (`DEFINT`, `DEFLNG`, `DEFSNG`, `DEFDBL`, `DEFSTR`)
3. **DIM AS Statements** (`DIM variable AS type`)

## Type Precedence Rules

When multiple type declarations conflict, BASIC follows this precedence order:

1. **Type Declaration Characters** (highest precedence)
2. **DIM AS Statements**
3. **DEF Type Statements** (lowest precedence)

Type characters always override other declarations, while explicit `DIM AS` declarations override `DEF` type settings.

## Available Data Types

| Type | Storage | Range | Declaration Character | DIM AS Keyword |
| ---- | ------- | ----- | --------------------- | -------------- |
| Integer | 2 bytes | -32,768 to 32,767 | `%` | `INTEGER` |
| Long | 4 bytes | -2,147,483,648 to 2,147,483,647 | `&` | `LONG` |
| Single | 4 bytes | ±3.402823E+38 | `!` | `SINGLE` |
| Double | 8 bytes | ±1.797693134862315D+308 | `#` | `DOUBLE` |
| String | Variable | 0-65,535 characters | `$` | `STRING` |

## Type Declaration Characters

Type characters are suffixes added to variable names to specify their type.

### Syntax

```vb
variableName%  ' Integer
variableName&  ' Long
variableName!  ' Single
variableName#  ' Double
variableName$  ' String
```

### Examples

```vb
score% = 42                   ' Integer
total& = 2147483647           ' Long
average! = 3.14159            ' Single
precision# = 1.234567890123   ' Double
name$ = "John Doe"            ' String
```

**Key Points:**

- Type characters have the highest precedence
- They override any DEF type or DIM AS declarations
- Only one type character allowed per variable name

## DEF Type Statements

DEF statements establish default types for variables based on their starting letters.

### Syntax (DEF)

```vb
DEFINT letterRange[, letterRange]...    ' Set default Integer type
DEFLNG letterRange[, letterRange]...    ' Set default Long type  
DEFSNG letterRange[, letterRange]...    ' Set default Single type
DEFDBL letterRange[, letterRange]...    ' Set default Double type
DEFSTR letterRange[, letterRange]...    ' Set default String type
```

**letterRange formats:**

- Single letter: `A`
- Range: `A-Z`
- Multiple ranges: `A-C, X-Z`

### Examples (DEF)

```vb
' Set all variables to Integer by default
DEFINT A-Z

' Variables starting with I-N are Integer, S-Z are Single
DEFINT I-N
DEFSNG S-Z

' Variables starting with A are String, B-D are Double
DEFSTR A
DEFDBL B-D
```

**Usage Examples:**

```vb
DEFINT A-Z
a = 42              ' Integer (from DEFINT)
b = 100             ' Integer (from DEFINT)
c! = 3.14           ' Single (type character overrides DEFINT)
d$ = "hello"        ' String (type character overrides DEFINT)
```

## DIM AS Statements

DIM statements provide explicit type declarations with full control.

### Syntax (DIM AS)

```vb
DIM variableName AS type
DIM arrayName(size) AS type
DIM variable1 AS type, variable2 AS type
```

### Examples (DIM AS)

```vb
' Simple variable declarations
DIM count AS INTEGER
DIM name AS STRING
DIM price AS SINGLE

' Array declarations
DIM numbers(10) AS INTEGER
DIM matrix(5, 5) AS DOUBLE
DIM text$(20)               ' Array with type character

' Multiple variables
DIM a AS INTEGER, b AS STRING, c AS DOUBLE
```

## Type Precedence in Practice

### Example 1: Type Character Override

```vb
DEFINT A-Z
a = 42          ' Integer (DEFINT applies)
b! = 3.14       ' Single (type character overrides DEFINT)
c$ = "hello"    ' String (type character overrides DEFINT)
```

### Example 2: DIM AS Override

```vb
DEFINT A-Z
DIM a AS SINGLE  ' Explicit Single type
a = 3.14         ' Single (DIM AS overrides DEFINT)
b = 42           ' Integer (DEFINT applies)
```

### Example 3: Complex Precedence

```vb
DEFINT A-M
DEFSNG N-Z

a = 42           ' Integer (DEFINT applies)
n = 3.14         ' Single (DEFSNG applies)
x! = 2.7         ' Single (type character, same as DEFSNG)
y# = 1.23        ' Double (type character overrides DEFSNG)
DIM z AS INTEGER ' Integer (DIM AS overrides DEFSNG)
```

## Arrays and Type Declarations

Arrays can use any type declaration method:

### Type Character Arrays

```vb
DIM intArray%(10)       ' Integer array
DIM strArray$(20)       ' String array
DIM sngArray!(5)        ' Single array
```

### DIM AS Arrays

```vb
DIM intArray(10) AS INTEGER
DIM strArray(20) AS STRING
DIM sngArray(5) AS SINGLE
```

### Mixed Declaration

```vb
DEFINT A-Z
DIM sngArray(10) AS SINGLE    ' Overrides DEFINT for this array
intArray(5) = 0               ' Uses DEFINT default
```

## Functions and Subroutines

Type declarations work in function parameters and return types:

```vb
FUNCTION AddInt(x AS INTEGER, y AS INTEGER) AS INTEGER
    AddInt = x + y
END FUNCTION

FUNCTION ProcessString(txt$ AS STRING) AS STRING
    ProcessString = UCASE$(txt$)
END FUNCTION

SUB Calculate(x%, y&, result#)
    result# = x% * y&
END SUB
```

## Common Patterns and Best Practices

### 1. Use DEF Statements for Consistent Naming

```vb
' Traditional BASIC pattern
DEFINT I-L    ' Loop counters
DEFSNG A-E    ' General numeric variables
DEFSTR S-T    ' String variables
```

### 2. Use Type Characters for Variables

```vb
' Clear type indication in code
score% = 100
name$ = "Player"
average! = 87.5
```

### 3. Use DIM AS for Complex Declarations

```vb
' Explicit declarations for clarity
DIM buffer AS STRING
DIM errorCode AS INTEGER
DIM coordinates(100, 2) AS SINGLE
```

## Troubleshooting

### Type Conflicts

If you have conflicting declarations:

```vb
DEFINT A-Z
DIM a AS INTEGER
a$ = "hello"    ' Error: Type character conflicts
```

**Solution:** Use consistent declarations or change variable names.

### Unintended Type Conversions

```vb
DEFINT A-Z
result = a / b   ' Integer division, may lose precision
```

**Solution:** Use explicit type characters or DIM AS:

```vb
DEFINT A-Z
result! = a! / b!   ' Single precision division
```

### Array Type Conflicts

```vb
DIM arr(10) AS INTEGER
arr$(0) = "test"    ' Error: Array type mismatch
```

**Solution:** Keep consistent array types or use separate arrays.
