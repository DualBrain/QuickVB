# DRAW

Draws an object specified in a string expression.

## Syntax

`DRAW` *string_expression*

## Comments

`DRAW` uses a string contianing graphics commands to draw an object. The string can contain cursor, color and scaling commands.

The `DRAW` statement combines most of the capabilities of the other graphics statements into an object definition language called Graphics Macro Language (GML). A GML command is a single character within a string, optionally followed by one or more arguments.

The `DRAW` statement is valid only in graphics mode.

## Movement Commands

Each of the following movement commands begins movement from the current graphics position. This is usually the coordinate of the last graphics point plotted with another GML command, [LINE](LINE), or [PSET](PSET). The current position defaults to the center of the screen (160,100 in medium resolution; 320,100 in high resolution) when a program is run. Movement commands move for a distance of scale factor **n*, where the default for *n* is 1; thus, they move one point if *n* is omitted and the default scale factor is used.

| Command | Moves |
| ------- | ----- |
| U*n* | up *n* units |
| D*n* | down *n* units |
| L*n* | left *n* units |
| R*n* | right *n* units |
| E*n* | diagonally up and right *n* units |
| F*n* | diagonally down and right *n* units |
| G*n* | diagonally down and left *n* units |
| H*n* | diagonally up and left *n* units |

This command moves as specified by the following argument:

| Command | Moves |
| ------- | ----- |
| M*x*, *y* | Move absolute or relative. If *x* is preceded by a *+* or *-*, *x* and *y* are added to the current graphics position, and connected to the current position by a line. Otherwise, a line is drawn to point *x, y* from the current position. |

The following prefix commands may precede any of the above movement commands:

| Command | Moves |
| ------- | ----- |
| B | Move, but plot no points. |
| N | Move, but return to original position when done. |

The following commands are also available:

| Command | Moves |
| ------- | ----- |
| A*n* | Set angle *n*. *n* may range from 0 to 3, where 0 is 0°, 1 is 90°, 2 is 180°, and 3 is 270°. Figures rotated 90° or 270° are scaled so that they will appear the same size as with 0° or 180° on a monitor screen with the standard aspect ratio of 4:3. |
| TA*n* | Turn angle *n*. *n* can be any value from negative 360 to positive 360. If the value specified by *n* is positive, it turns the angle counter-clockwise. If the value specified by *n* is negative, it turns clockwise. |
| C*n* | Set color *n*. See the [COLOR](COLOR), [PALETTE](PALETTE), and [SCREEN](SCREEN) statements for discussions of valid colors, numbers and attributes. |
| S*n* | Set scale factor. *n* may range from 1 to 255. n is divided by 4 to derive the scale factor. The scale factor is multiplied by the distances given with `U`, `D`, `L`, `R`, `E`, `F`, `G`, `H`, or relative `M` commands to get the actual distance traveled. The default for `S` is `4`. |
| x*string*; *variable* | Execute substring. This command executes a second substring from a string, much like [GOSUB](GOSUB). One string executes another, which executes a third, and so on. *string* is a variable assigned to a string of movement commands. |
| P*paint*, *boundary* | Specifies the colors for a graphics figure and creates a filled-in figure. *paint* specifies what color you want the figure filled in with. *boundary* specifies the border color (outline). See the [COLOR](COLOR), [PALETTE](PALETTE), and [SCREEN](SCREEN) statements for discussions of valid colors, numbers and attributes. You must specify values for both *paint* and *boundary* when used. This command (`Ppaint,boundary`) does not paint color tiling. |

## Numeric Arguments

Numeric arguments can be constants like "123" or "*=variable;*", where *variable* is the name of a variable.

When you use the second syntax, "*=variable;*", the semicolon must be used. Otherwise, the semicolon is optional between commands.

You can also specify variables using [VARPTR$](VARPTR$)(*variable*).

## Examples

To draw a box in medium resolution:

```vb
SCREEN 1
A=20
DRAW "U=A; R=A; D=A; L=A;"
```

The aspect ratio to draw a square on a standard screen is 4:3, as shown below:

To draw a 96 pixel-wide square on a 640 × 200 pixel screen ([SCREEN](SCREEN) `2`), do the following calculations:

```text
Horizontal value = 96
Vertical value = 96*(200/640)*(4/3)
```

or

```text
Vertical value = 40
Horizontal value = 40*(640/200)*(3/4)
```

The horizontal values equals 4/3 of the vertical values.

To draw a triangle in medium resolution:

```vb
CLS
SCREEN 1
PSET (60, 125)
DRAW "E100; F100; L199"
```

```vb
'Draw a box and fill it
SCREEN 1
DRAW "C3"
DRAW "L20U20R20D20"   ' draw box
DRAW "BH10"           ' move into box
DRAW "P2,3"           ' paint box
```

## See Also

* [CIRCLE](CIRCLE), [COLOR](COLOR), [LINE](LINE), [PAINT](PAINT), [POINT](POINT), [PRESET](PRESET), [PSET](PSET), [SCREEN](SCREEN)
