# PALETTE

Change one or more colors in the color palette.

## Syntax

`PALETTE` [*change_color*, *new_color*]

`PALETTE USING` *array*[( *index*)]

## Comments

*change_color* is the attribute to change.

*new_color* is the new color to assign to the attribute.

*array* is an array of color numbers to assign to the attributes available in the current screen mode.

*index* is the index of the first element in the array to use in setting the palette.

If you omit all arguments, `PALETTE` restores the default color values.

## History

The `PALETTE` statement works only for systems equipped with the IBM® Enhanced Graphics Adapter (EGA). A BASIC palette contains a set of colors, with each color specified by an *attribute*. Each *attribute* is paired with an actual display *color*. This *color* determines the actual visual color on the screen, and is dependent on the setting of your screen mode and your actual physical hardware display.

`PALETTE` with no arguments sets the palette to a known initial setting. This setting is the same as the setting when colors are first initialized.

If arguments are specified, *color* will be displayed whenever *attribute* is specified in any statement that specifies a color. Any color changes on the screen occur immediately. Note that when graphics statements use color arguments, they are actually referring to attributes and not actual colors. `PALETTE` pairs attributes with actual colors.

For example, assume that the current palette consists of colors 0, 1, 2, and 3. The following [DRAW](DRAW) statement:

```vb
DRAW "C3L100"
```

selects attribute 3, and draws a line of 100 pixels using the color associated with the attribute 3, in this case, also 3. If the statement:

```vb
PALETTE 3,2
```

is executed, then the color associated with attribute 3 is changed to color 2. All text or graphics currently displayed on the screen using attribute 3 are instantaneously changed to color 2. All text or graphics subsequently displayed with attribute 3 will also be displayed in color 2. The new palette of colors will contain 0, 1, 2, and 2.

With the `USING` option, all entries in the palette can be modified in one `PALETTE` statement. The *integer-array-name argument* is the name of an integer array, and the *arrayindex* specifies the index of the first array element in the *integer-array-name* to use in setting your palette. Each attribute in the palette is assigned a corresponding color from this integer array. The array must be dimensioned large enough to set all the palette entries after *arrayindex*. For example, if you are assigning colors to all 16 attributes, and the index of the first array element given in your `PALETTE USING` statement is 5, then the array must be dimensioned to hold at least 20 elements (since the number of elements from 5-20, inclusive, is 16):

```vb
DIM PAL%(20)
.
.
.
PALETTE USING PAL%(5)
```

If the *color* argument in an array entry is -1, then the mapping for the associated *attribute* is not changed. All other negative numbers are illegal values for *color*.

You can use the color argument in the [COLOR](COLOR) statement to set the default text color. (Remember that color arguments in other BASIC statements are actually what are called *attributes* in this discussion.) This color argument specifies the way that text characters appear on the display screen. Under a common initial palette setting, points colored with the *attribute 0* appear as black on the display screen. Using the `PALETTE` statement, you could, for example, change the mapping of *attribute 0* from black to white.

Remember that a `PALETTE` statement executed without any parameters assigns all *attributes* their default *colors*.

The following table lists *attribute* and *color* ranges for various monitor types and screen modes:

Table 1

SCREEN Color and Attribute Ranges

SCREEN

| Mode   | Monitor Attached | Adapter | Attribute Range | Color Range |
| ------ | ---------------- | ------- | --------------- | ----------- |
| 0      | Monochrome       | MDPA    | NA              | NA          |
|        | Monochrome       | EGA     | 0-15            | 0-2         |
|        | Color            | CGA     | NA              | 0-31<sup>a</sup>       |
|        | Color/Enhanced<sup>d</sup>  | EGA     | 0-31<sup>a</sup>           | 0-15        |
| 1      | Color            | CGA     | NA              | 0-3         |
|        | Color/Enhanced<sup>d</sup>  | EGA     | 0-3             | 0-15        |
| 2      | Color            | CGA     | NA              | 0-1         |
|        | Color/Enhanced<sup>d</sup>  | EGA     | 0-1             | 0-15        |
| 7      | Color/Enhanced<sup>d</sup>  | EGA     | 0-15            | 0-15        |
| 8      | Color/Enhanced<sup>d</sup>  | EGA     | 0-15            | 0-15        |
| 9      | Enhanced<sup>d</sup>        | EGA<sup>b</sup>    | 0-3             | 0-15        |
|        | Enhanced<sup>d</sup>        | EGA<sup>c</sup>    | 0-15            | 0-63        |
| 10     | Monochrome       | EGA     | 0-3             | 0-8         |

<sup>a</sup> Attributes 16-31 refer to blinking versions of colors 0-15
<sup>b</sup> With 64K of EGA memory
<sup>c</sup> With greater than 64K of EGA memory
<sup>d</sup> IBM Enhanced Color Display
NA = Not Applicable
CGA = IBM Color Graphics Adapter
EGA = IBM Enhanced Graphics Adapter
MDPA = IBM Monochrome Display and Printer Adapter

See the [SCREEN](SCREEN) statement reference page for the list of colors available for various [SCREEN](SCREEN) mode, monitor, and graphics adapter combinations.

## Example

```vb
PALETTE 0, 2         'Changes all points colored with attribute 0 
                     'to color 2
PALETTE 0,-1         'Does not modify the palette
PALETTE USING A%(0)  'Changes each palette entry. Since the 
                     'array is initialized to zero when it 
                     'is first declared, all attributes are 
                     'now mapped to display color zero. The 
                     'screen will now appear as one single 
                     'color. However, it will still be
                     'possible to execute BASIC statements.
PALETTE              'Sets each palette entry to its appropriate 
                     'initial display color. Actual initial colors 
                     'depend on your screen hardware configuration.
```

## See Also

- [COLOR](COLOR), [SCREEN](SCREEN)
