# DEFLNG

Sets the default type for variables whose names begin with specified letters to LONG type (32-bit integer).

## Syntax

```vb
DEFLNG letterRange[, letterRange]...
```

**letterRange formats:**

- Single letter: `A`
- Letter range: `A-Z` 
- Multiple ranges: `A-C, X-Z`

## Description

DEFLNG establishes LONG as the default type for variables starting with specified letters. Long integers provide much larger range than regular integers while maintaining exact precision.

**Range:** -2,147,483,648 to 2,147,483,647
**Precision:** Exact integer values
**Storage:** 4 bytes

## Examples

### Basic DEFLNG Usage

```vb
DEFLNG F-L
fileSize = 2147483647     ' Long (DEFLNG applies)
lineCount = 50000         ' Long (DEFLNG applies)
count = 42                ' Single (default, no DEFLNG rule)
```

### Large Number Handling

```vb
DEFLNG T-Z
totalRecords = 1500000     ' Long (DEFLNG applies)
memoryAddress = &H8000000  ' Long (DEFLNG applies, hex notation)
timestamp = 1234567890     ' Long (DEFLNG applies)
```

### Traditional BASIC Pattern

```vb
DEFINT I-L          ' Loop counters
DEFLNG M, N         ' Large counters
DEFSNG O-T          ' General numbers

i = 1               ' Integer (DEFINT)
recordCount = 50000 ' Long (DEFLNG)
average = 87.5      ' Single (DEFSNG)
```

### System Programming

```vb
DEFLNG A, H, P      ' System variables
address = &H00400000    ' Memory address
handle = 12345678       ' File or system handle
pointer = 0            ' Null pointer
```

### File Operations

```vb
DEFLNG F, L, P, S
filePos = 1048576      ' File position in bytes
length = 2097152       ' File length in bytes
position = 0           ' Current position
size = 512             ' Buffer size
```

## When to Use LONG

1. **Large Counts**: When values may exceed 32,767 (Integer limit)
2. **File Sizes**: File positions and sizes
3. **Memory Addresses**: System programming
4. **System Handles**: File handles, window handles
5. **Timestamps**: Large integer time values

## Performance Considerations

- **Larger than INTEGER**: Uses 4 bytes instead of 2
- **Slower than INTEGER**: More CPU cycles for operations
- **Exact Precision**: No floating-point rounding
- **32-bit Efficient**: Optimized on 32-bit systems

## Type Character Override

```vb
DEFLNG A-Z
count = 2147483647    ' Long (DEFLNG applies)
index% = 42           ' Integer (type character overrides)
total! = 3.14         ' Single (type character overrides)
```

## Hexadecimal Notation

```vb
DEFLNG A-Z
hexValue = &H7FFFFFFF  ' Maximum positive Long
hexValue2 = &H80000000 ' Minimum negative Long
address = &H00400000   ' Typical memory address
```

## Common Patterns

### File Handling

```vb
DEFLNG F, L, P
DIM fileSize AS LONG, filePos AS LONG
fileSize = LOF(1)           ' Get file length
filePos = SEEK(1)           ' Get current position
```

### Counters with Potential Overflow

```vb
DEFLNG T, C
totalCount = 0
FOR i% = 1 TO 100000
  totalCount = totalCount + i%  ' Could overflow INTEGER
NEXT i%
```

## Best Practices

1. Use when values may exceed Integer range
2. Traditional ranges: F for file variables, T for totals
3. Consider memory usage for large arrays of Long variables
4. Use with AND/OR for bit manipulation on large numbers

## Common Pitfalls

### Unnecessary LONG Usage

```vb
DEFLNG A-Z    ' Overkill if values stay under 32,767
counter = 42  ' Could be Integer
```

**Solution:** Use INTEGER for smaller ranges:

```vb
DEFINT I-L     ' Better for loop counters and small counts
DEFLNG F, T    ' Only use LONG for potentially large values
```

## Related Commands

- [DEFINT](DEFINT) - Set Integer as default type (smaller range)
- [DIM AS LONG](DIM) - Explicit Long variable declaration
- [&](Type-System) - Long type declaration character
- [CINT](CINT) - Convert to Integer
- [CLNG](CLNG) - Convert to Long
