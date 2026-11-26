# DEFSTR

Sets the default type for variables whose names begin with specified letters to STRING type.

## Syntax

```vb
DEFSTR letterRange[, letterRange]...
```

**letterRange formats:**

- Single letter: `A`
- Letter range: `A-Z`
- Multiple ranges: `A-C, X-Z`

## Description

DEFSTR establishes STRING as the default type for variables starting with specified letters. String variables can hold text from 0 to 65,535 characters.

## Examples

### Basic DEFSTR Usage

```vb
DEFSTR S-Z
name = "John Doe"      ' String (DEFSTR applies)
city = "New York"      ' String (DEFSTR applies)
count = 42             ' Single (no DEF rule, defaults to Single)
```

### Multiple Letter Ranges

```vb
DEFSTR A-C, M-O
first = "Alice"        ' String (A-C range)
middle = "Marie"       ' String (M-O range)
last = "Smith"         ' String (L is covered by A-C)
number = 123           ' Single (N is not in ranges)
```

### Traditional BASIC Pattern

```vb
DEFSTR S-T            ' Traditional string variable naming
student = "Bob"       ' String
score = 95.5          ' Single (not affected)
teacher$ = "Ms. Jones" ' String (type character overrides)
```

### Type Character Override

```vb
DEFSTR A-Z
text = "Hello"        ' String (DEFSTR applies)
count% = 42           ' Integer (type character overrides DEFSTR)
total! = 3.14         ' Single (type character overrides DEFSTR)
```

## Best Practices

1. Use for traditional BASIC string variable naming patterns
2. Common ranges: S-T for strings, A-C for alphabetically ordered strings
3. Avoid using on letters frequently needed for numeric variables

## Related Commands

- [DEFINT](DEFINT) - Set Integer as default type
- [DEFSNG](DEFSNG) - Set Single as default type  
- [DIM AS STRING](DIM) - Explicit string variable declaration
- [$](Type-System) - String type declaration character
