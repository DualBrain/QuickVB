# DEFINT / DEFLNG / DEFSNG / DEFDBL / DEFSTR

Sets default variable types based on the first letter of variable names. DEF statements establish type defaults that apply throughout the program scope.

## Syntax

```vb
DEFINT letterRange[, letterRange]...    ' Set default Integer type
DEFLNG letterRange[, letterRange]...    ' Set default Long type  
DEFSNG letterRange[, letterRange]...    ' Set default Single type
DEFDBL letterRange[, letterRange]...    ' Set default Double type
DEFSTR letterRange[, letterRange]...    ' Set default String type
```

**letterRange formats:**

- Single letter: `A`
- Letter range: `A-Z` 
- Multiple ranges: `A-C, X-Z`

## Type Specifications

| Statement | Type | Declaration Character | Range | Storage |
| --------- | ---- | --------------------- | ----- | ------- |
| `DEFINT` | Integer | `%` | -32,768 to 32,767 | 2 bytes |
| `DEFLNG` | Long | `&` | -2,147,483,648 to 2,147,483,647 | 4 bytes |
| `DEFSNG` | Single | `!` | ±3.402823E+38 | 4 bytes |
| `DEFDBL` | Double | `#` | ±1.797693134862315D+308 | 8 bytes |
| `DEFSTR` | String | `$` | 0-65,535 characters | Variable |

## Usage Rules

1. **Case Insensitive**: DEF statements apply to both uppercase and lowercase variable names
2. **Scope**: DEF statements affect the entire program from the point of declaration
3. **Default Type**: Without DEF statements, variables default to SINGLE type
4. **Precedence**: Type declaration characters override DEF statements

## Examples

### Basic DEF Statements

```vb
' Set all variables to Integer by default
DEFINT A-Z
a = 42          ' Integer
b = 100         ' Integer
total = a + b   ' Integer result

' Mixed type defaults
DEFINT I-N      ' Loop counters: i, j, k, l, m, n are Integer
DEFSNG O-T      ' General numeric: o, p, q, r, s, t are Single  
DEFSTR A-C      ' String variables: a, b, c are String
```

### Multiple Letter Ranges

```vb
' Traditional QBasic naming pattern
DEFINT I-L      ' Loop variables
DEFSNG M-P      ' Math variables
DEFSTR S-Z      ' String variables

i = 1           ' Integer (DEFINT)
sum = 0.0       ' Single (DEFSNG)
name = "Test"   ' String (DEFSTR)
```

### Type Character Override

```vb
DEFINT A-Z
a = 42          ' Integer (DEFINT applies)
b! = 3.14       ' Single (type character overrides DEFINT)
c$ = "hello"    ' String (type character overrides DEFINT)
d# = 1.234      ' Double (type character overrides DEFINT)
```

### Complex Range Specifications

```vb
' Set specific letter ranges
DEFINT A-C, X-Z      ' A,B,C and X,Y,Z are Integer
DEFSNG D-F, M-O      ' D,E,F and M,N,O are Single
DEFSTR G-L, P-W      ' G through L and P through W are String
```

### Practical Programming Example

```vb
' Traditional QBasic variable naming with DEF statements
DEFINT I-L           ' Loop counters
DEFSNG A-H           ' Numeric variables
DEFSTR M-R           ' String variables

' Variables use appropriate types automatically
FOR i = 1 TO 10      ' i is Integer (DEFINT)
  score = score + i  ' score is Single (DEFSNG)
NEXT i

name$ = "Player"     ' name is String (DEFSTR)
result = score / 2   ' result is Single (DEFSNG)
```

## Type Precedence

When DEF statements conflict with other declarations, this precedence applies:

1. **Type Declaration Characters** (highest precedence)
2. **DIM AS Statements**  
3. **DEF Type Statements** (lowest precedence)

### Precedence Example

```vb
DEFINT A-Z
DIM a AS SINGLE     ' Explicit Single (overrides DEFINT)
b = 42              ' Integer (DEFINT applies)
c! = 3.14           ' Single (type character overrides DEFINT)
```

## Best Practices

1. **Use Traditional Patterns**: Follow BASIC conventions with `I-L` for loops, `S-Z` for strings
2. **Document DEF Statements**: Comment DEF declarations at program start
3. **Avoid Conflicts**: Don't use DEFINT on letters you'll frequently need as other types
4. **Combine with Type Characters**: Use type characters for exceptions to DEF rules

## Common Pitfalls

### Unexpected String Variables

```vb
DEFSTR S
score = 100      ' Integer (fine)
sum = score      ' Integer (fine) 
total = 50       ' Integer (fine)
status = "Done"  ' String (unexpected if you meant numeric)
```

### Solution: Use Different Letter Ranges

```vb
DEFSTR S-T       ' More specific range
DEFSNG U-Z       ' Numeric variables use U-Z
```

### Loop Counter Conflicts

```vb
DEFSTR I-Z       ' Bad: makes all variables strings
FOR i = 1 TO 10  ' Error: i is string, can't be loop counter
```

### Solution: Exclude Loop Letters

```vb
DEFSTR J-Z       ' Skip I for loop counters
```

## Related Commands

- [DIM](DIM) - Explicit variable declarations
- [AS](AS) - Type specification clause
