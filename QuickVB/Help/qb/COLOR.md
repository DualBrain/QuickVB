# COLOR

Set the screen color.

## Syntax

`COLOR` [ *foreground* ][,[ *background* ][, *border* ]]
  (Screen mode 0)

`COLOR` [ *background* ][,[ *palette* ]]
  (Screen mode 1)

`COLOR` [ *foreground* ][,[ *background* ]]
  (Screen mode 7-10)

`COLOR` [ *foreground* ]
  (Screen mode 4, 12, 13)

## Comments

The `COLOR` statement allows you to set text foreground and background colors as well as color palettes in graphics mode.

See the [SCREEN](SCREEN) statement for specifics on each screen mode.

### SCREEN 0

Modifies the current default text foreground and background colors, and the screen border. The *foreground* color must be an integer expression in the range 0-31. It is used to determine the "foreground" color in text mode, which is the default color of text. Sixteen colors can be selected with the integers 0-15. A blinking version of each color can be selected by adding 16 to the color number; for example, a blinking color 7 is equal to 7 + 16, or 23. Thus, the legal integer range for *foreground* is 0-31.

The *background* color must be an integer expression in the range 0-7, and is the color of the background for each text character. Blinking colors are not permitted.

The *border* color is an integer expression in the range 0-15, and is the color used when drawing the screen border. Blinking colors are not permitted.

If no arguments are provided to `COLOR`, then the default color for background and border is black (`COLOR 0`), and for foreground, is as described in the [SCREEN](SCREEN) statement reference pages.

### SCREEN 1

In mode 1, the `COLOR` statement has a unique syntax that includes a *palette* argument, which is an odd or even integer expression. This argument determines the set of display colors to use when displaying particular color numbers.

For hardware configurations that do not have an IBM® Enhanced Graphics Adapter (EGA), the default color settings for the *palette* parameter are equivalent to the following:

```vb
COLOR ,0  'Same as the next three PALETTE statements
          '1 = green, 2 = red, 3 = yellow
COLOR ,1  'Same as the next three PALETTE statements
          '1 = cyan, 2 = magenta, 3 = white
```

With the EGA, the default color settings for the palette parameter are equivalent to the following:

```vb
COLOR ,0     'Same as the next three PALETTE statements
PALETTE 1,2  'Attribute 1 = color 3 (green)
PALETTE 2,4  'Attribute 2 = color 5 (red)
PALETTE 3,6  'Attribute 3 = color 6 (brown)
COLOR ,1     'Same as the next three PALETTE statements
PALETTE 1,3  'Attribute 1 = color 3 (cyan)
PALETTE 2,5  'Attribute 2 = color 5 (magenta)
PALETTE 3,7  'Attribute 3 = color 15 (white)
```

Note that a `COLOR` statement will override previous [PALETTE](PALETTE) statements.

### SCREEN 2

No effect. An "Illegal function call" error message occurs if `COLOR` is used in this mode.

### SCREEN 7 - SCREEN 10

In these modes, no *border* color can be specified. The graphics background is given by the *background* color number, which must be in the valid range of color numbers appropriate to the screen mode. See the [SCREEN](SCREEN) statement reference pages for more details. The *foreground* color argument is the default line drawing color.

Arguments outside valid numeric ranges result in "Illegal function call" errors.

The foreground color may be the same as the background color, making displayed characters invisible. The default background color is black, or color number 0, for all display hardware configurations and all screen modes.

With the Enhanced Graphics Adapter (EGA) installed, the [PALETTE](PALETTE) statement gives you flexibility in assigning different display colors to the actual color-number ranges for the *foreground*, *background*, and *border* colors discussed above. See the [PALETTE](PALETTE) statement reference pages for more details.

### SCREEN 4, 12 and 13

The foreground color is an attribute number. You cannot specify a background color.

## Example

The following series of examples show `COLOR` statements and their effects in the various screen modes:

```vb
SCREEN 0
COLOR 1, 2, 3  'foreground=1, background=2, border=3 
SCREEN 1
COLOR 1, 0     'foreground=1, even palette number
COLOR 2, 1     'foreground=2, odd palette number
SCREEN 7
COLOR 3, 5     'foreground=3, background=5
SCREEN 8
COLOR 6, 7     'foreground=6, background=7
SCREEN 9
COLOR 1, 2     'foreground=1, background=2
```

```vb
SCREEN 0
FOR fcolor = 0 TO 31
  COLOR fcolor
  PRINT "Current color is"; fcolor
  INPUT dummy$
NEXT fcolor
```

## See Also

* [CIRCLE](CIRCLE), [DRAW](DRAW), [LINE](LINE), [PAINT](PAINT), [PALETTE](PALETTE), [PALETTE USING](PALETTE-USING), [POINT](POINT), [PRESET](PRESET), [PSET](PSET), [SCREEN](SCREEN)
