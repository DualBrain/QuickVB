# PMAP

Maps a physical coordinate to a logical coordinate defined by the [WINDOW](WINDOW) statement or vice versa.

## Syntax

x=`PMAP`(*coordinate*, *mapping*)

## Comments

*coordinate* is a numeric expression of the coordinate to be mapped.

*mapping* specifies the type of conversation:

| Value | Maps |
| -------- | ---- |
| 0 | logical expressions to physical x |
| 1 | logical expressions to physical y |
| 2 | physical expressions to logical x |
| 3 | physical expressions to logical y |

## Example

```vb
SCREEN 1 
WINDOW SCREEN (0, 0)-(100, 100)
'Convert logical to physical
x = PMAP(50, 0)
y = PMAP(50, 1)
```

## See Also

- [VIEW](VIEW), [WINDOW](WINDOW)
