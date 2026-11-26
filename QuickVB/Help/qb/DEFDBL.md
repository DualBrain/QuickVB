# DEFDBL

Sets the default type for variables whose names begin with specified letters to DOUBLE type (double-precision floating-point).

## Syntax

```vb
DEFDBL letterRange[, letterRange]...
```

**letterRange formats:**

- Single letter: `A`
- Letter range: `A-Z` 
- Multiple ranges: `A-C, X-Z`

## Description

DEFDBL establishes DOUBLE as the default type for variables starting with specified letters. Double-precision provides the highest floating-point accuracy available in BASIC.

**Range:** ±1.797693134862315D+308
**Precision:** Approximately 15-16 decimal digits
**Storage:** 8 bytes

## Examples

### Basic DEFDBL Usage

```vb
DEFDBL A-Z
x = 3.141592653589793    ' Double (DEFDBL applies)
y = 2.718281828459045    ' Double (DEFDBL applies)
result = x * y           ' Double multiplication
```

### Scientific Calculations

```vb
DEFDBL P, R, V
pi = 3.14159265358979323846
radius = 6371.0           ' Earth radius in km
volume = 4/3 * pi * radius^3  ' Earth volume calculation
```

### Financial Calculations

```vb
DEFDBL B, I, P, R
balance = 1000000.0      ' Large monetary value
interestRate = 0.0425    ' Precise interest rate
principal = 500000.0     ' Loan principal
result = principal * (1 + interestRate)^12  ' Annual compounding
```

### High-Precision Mathematics

```vb
DEFDBL E, F, G
euler = 2.7182818284590452353602874713527
fibonacci = 354224848179261915075  ' Large Fibonacci number
golden = (1 + SQR(5)) / 2          ' Golden ratio
```

### Traditional BASIC Pattern

```vb
DEFINT I-L          ' Loop counters
DEFSNG M-P          ' General numbers
DEFDBL Q, R, S      ' High-precision variables

i = 1               ' Integer (DEFINT)
average = 87.5      ' Single (DEFSNG)
precise = 1.23456789012345  ' Double (DEFDBL)
```

## When to Use DOUBLE

1. **Scientific Computing**: High-precision mathematical calculations
2. **Financial Applications**: Large monetary values with exact cents
3. **Engineering**: Precise physical constants and calculations
4. **Statistics**: Large datasets requiring precision
5. **Graphics**: High-precision coordinate transformations

## Scientific Notation

```vb
DEFDBL A-Z
avogadro = 6.02214076E+23      ' Avogadro's number
planck = 6.62607015E-34         ' Planck's constant
lightSpeed = 2.99792458E+8      ' Speed of light (m/s)
```

## Type Character Override

```vb
DEFDBL A-Z
value = 3.141592653589793    ' Double (DEFDBL applies)
count% = 42                  ' Integer (type character overrides)
total! = 3.14                ' Single (type character overrides)
```

## Performance Considerations

- **Highest Precision**: 15-16 decimal digits accuracy
- **Largest Range**: Can handle extremely large/small numbers
- **Memory Usage**: Uses 8 bytes (twice Single)
- **Slower**: More CPU cycles than Single precision
- **Cache Impact**: Larger memory footprint affects cache performance

## Best Practices

1. Use only when high precision is truly needed
2. Avoid for simple graphics or game coordinates
3. Traditional ranges: Q-S for scientific/mathematical variables
4. Consider memory impact for large arrays

## Common Patterns

### Constants

```vb
DEFDBL C
pi = 3.14159265358979323846
e = 2.71828182845904523536
sqrt2 = 1.41421356237309504880
```

### Financial Functions

```vb
DEFDBL A, F, I, P, R
amount = 0
futureValue = 0
interestRate = 0
principal = 0
rate = 0
```

### Engineering Calculations

```vb
DEFDBL G, K, T
gravity = 9.80665              ' Standard gravity
boltzmann = 1.380649E-23       ' Boltzmann constant
temperature = 273.15            ' Absolute temperature
```

## Common Pitfalls

### Unnecessary Precision

```vb
DEFDBL A-Z    ' Overkill for simple calculations
x = 3.14      ' Could be Single precision
```

**Solution:** Use Single when Double precision isn't needed:

```vb
DEFSNG A-Z     ' Better for most general-purpose use
DEFDBL C, F    ' Only use Double for high-precision needs
```

### Performance Impact

```vb
DEFDBL ARRAY(10000)  ' Large array of Doubles uses ~80KB
' Consider if Single precision would suffice:
' DEFSNG ARRAY(10000)  ' Single array uses ~40KB
```

## Mathematical Functions

Many BASIC functions automatically return Double precision:

```vb
DEFDBL R
result = SQR(2.0)      ' Returns Double
result = SIN(3.14159)   ' Returns Double  
result = EXP(1.0)      ' Returns Double
```

## Related Commands

- [DEFSNG](DEFSNG) - Set Single as default type (faster, less precise)
- [DIM AS DOUBLE](DIM) - Explicit Double variable declaration
- [#](Type-System) - Double type declaration character
- [CDBL](CDBL) - Convert to Double precision
- [CSNG](CSNG) - Convert to Single precision
