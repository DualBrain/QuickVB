# DEFSNG

Sets the default type for variables whose names begin with specified letters to SINGLE type (single-precision floating-point).

## Syntax

```vb
DEFSNG letterRange[, letterRange]...
```

**letterRange formats:**

- Single letter: `A`
- Letter range: `A-Z` 
- Multiple ranges: `A-C, X-Z`

## Description

DEFSNG establishes SINGLE as the default type for variables starting with specified letters. Single-precision numbers provide good precision for most calculations while using minimal memory (4 bytes).

**Range:** ±3.402823E+38
**Precision:** Approximately 7 decimal digits
**Storage:** 4 bytes

## Examples

### Basic DEFSNG Usage

```vb
DEFSNG A-Z
x = 3.14159         ' Single (DEFSNG applies)
y = 2.71828         ' Single (DEFSNG applies)
result = x * y      ' Single multiplication
```

### Traditional BASIC Pattern

```vb
DEFINT I-L          ' Loop counters
DEFSNG M-P          ' Math variables
DEFSTR S-Z          ' String variables

i = 1               ' Integer (DEFINT applies)
average = 87.5      ' Single (DEFSNG applies)
name = "Student"    ' String (DEFSTR applies)
```

### Multiple Letter Ranges

```vb
DEFSNG A-H, Q-Z     ' Most variables are Single
sum = 100.5         ' Single
total = 42.75       ' Single
index% = 5          ' Integer (type character overrides DEFSNG)
```

### Scientific Calculations

```vb
DEFSNG C-G
celsius = 25.5      ' Single
fahrenheit = c * 9/5 + 32  ' Single calculation
kelvin = c + 273.15         ' Single result
```

### Game Programming

```vb
DEFSNG X, Y, V, W   ' Coordinate and velocity variables
x = 100.5           ' Player X position
y = 75.25           ' Player Y position  
vx = 2.5            ' Velocity X component
vy = -1.5           ' Velocity Y component
```

## Performance Considerations

- **Faster than DOUBLE**: Single precision calculations are quicker
- **Memory Efficient**: Uses half the memory of Double
- **Adequate Precision**: Sufficient for most applications
- **Graphics**: Ideal for screen coordinates and simple physics

## Type Character Override

```vb
DEFSNG A-Z
value = 3.14        ' Single (DEFSNG applies)
count% = 42         ' Integer (type character overrides)
total# = 1.234567   ' Double (type character overrides)
```

## Best Practices

1. Use for general-purpose numeric variables
2. Ideal for coordinates, averages, percentages
3. Traditional ranges: M-P for math variables, X-Z for coordinates
4. Avoid for financial calculations requiring high precision

## Common Pitfalls

### Precision Loss

```vb
DEFSNG A-Z
result = 1.2345678 + 1.2345678  ' May lose precision due to Single limits
```

**Solution:** Use DOUBLE for high-precision needs:

```vb
DIM precise AS DOUBLE
precise = 1.2345678 + 1.2345678
```

## Related Commands

- [DEFINT](DEFINT) - Set Integer as default type
- [DEFDBL](DEFDBL) - Set Double as default type (higher precision)
- [DIM AS SINGLE](DIM) - Explicit Single variable declaration
- [!](Type-System) - Single type declaration character
- [CDBL](CDBL) - Convert to Double precision
