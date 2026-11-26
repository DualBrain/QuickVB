# PAINT

Fills a graphics screen image with the specified color or pattern.

## Syntax

`PAINT`[`STEP`] (*x*, *y*) [, [{*color* | *tile*}] [, [ *bordercolor*] [, *background*]]]

## Comments

The keyword `STEP` indicates that *x* and *y* are offsets from the current graphics position as opposed to physical coordinates.

*x*, *y* is a set of coordinates within the graphics image.

*color* is the desired fill color. If you omit *color*, `PAINT` uses the current foreground color.

*tile* is a fill pattern 8 bits wide and up to 64 bits long that is defined as follows:

*tile$* = [CHR$](CHR$)(*n*) [+ [CHR$](CHR$)(*n*)]...

Where the values of *n* are integers between 0 and 255. Each [CHR$](CHR$)(*n*) defines a 1-byte, 8-pixel slice of the fill pattern based on the binary form of the number.

*bordercolor* is the color of the border surrounding the graphics image. If you omit *bordercolor*, `PAINT` uses *color*.

*background* is a one-character string that specifies a background pattern that can be painted over. If you omit *background*, `PAINT` uses [CHR$](CHR$)(0).

## Paint Tiling

`PAINT` tiling is similar to [LINE](LINE) styling. Like [LINE](LINE), `PAINT` looks at a tiling mask each time a point is put down on the screen.

If *paint attribute* is omitted, the standard foreground attribute is used.

If *paint attribute* is a numeric formula, then the number must be a valid color, and it is used to paint the area as before.

If *paint attribute* is a string formula, then tiling is performed as follows:

The tile mask is always eight bits wide and may be from 1 to 64 bytes long.

Each byte in the tile string masks eight bits along the x axis when putting down points. Each byte of the tile string is rotated as required to align along the y axis, such that:

`tile_byte_mask=y MOD tile_length`

where *y* is the position of the graphics cursor on the y-axis.

*tile_length* is the length in bytes of the tile string defined by the user (1 to 64 bytes).

This is done so that the tile pattern is replicated uniformly over the entire screen (as if a `PAINT (0,0)..` were used).

```text
x Increases -->        Bit of Tile Byte
x,y  8 7 6 5 4 3 2 1
0,0 |x|x|x|x|x|x|x|x|  Tile byte 1
0,1 |x|x|x|x|x|x|x|x|  Tile byte 2
0,2 |x|x|x|x|x|x|x|x|  Tile byte 3
.
.
.
0,63 |x|x|x|x|x|x|x|x| Tile byte 64
                       (maximum allowed)
```

In high-resolution mode ([SCREEN](SCREEN) `2`), the screen can be painted with Xs by the following statement:

`PAINT (320,100),CHR$(&H81)+CHR$(&H42)+CHR$(&H24)+CHR$(&H18)+CHR$(&H18)+CHR$(&H24)+CHR$(&H81)`

This appears on the screen as follows:

```text
x increases -->
0,0 |x| | | | | | |x|  CHR$(&H81)  Tile byte 1
0,1 | |x| | | | |x| |  CHR$(&H42)  Tile byte 2
0,2 | | |x| | |x| | |  CHR$(&H24)  Tile byte 3
0,3 | | | |x|x| | | |  CHR$(&H18)  Tile byte 4
0,4 | | | |x|x| | | |  CHR$(&H18)  Tile byte 5
0,5 | | |x| | |x| | |  CHR$(&H24)  Tile byte 6
0,6 | |x| | | | |x| |  CHR$(&H42)  Tile byte 7
0,7 |x| | | | | | |x|  CHR$(&H81)  Tile byte 8
```

Since there are two bits per pixel in medium-resolution mode ([SCREEN](SCREEN) `1`), each byte of the tile pattern only describes four pixels. In this case, every two bits of the tile byte describes one of the four possible colors associated with each of the four pixels to be put down.

*bckgrnd attribute* specifies the background tile pattern or color byte to skip when checking for boundary termination. *bckgrnd attribute* is a string formula returning one character. When omitted, the default is [CHR$](CHR$)`(0)`.

Occasionally, you may want to paint tile over an already painted area that is the same color as two consecutive lines in the tile pattern. `PAINT` quits when it encounters two consecutive lines of the same color as the point being set (the point is surrounded). It is not possible to draw alternating blue and red lines on a red background without *bckgrnd attribute*. `PAINT` stops as soon as the first red pixel is drawn. By specifying red ([CHR$](CHR$)`(&HAA)`) as the background attribute, the red line is drawn over the red background.

You cannot specify more than two consecutive bytes in the tile string that match the background attribute. Specifying more than two results in an "Illegal function call" error.

## Example

```vb
CLS
SCREEN 1
LINE (0, 0)-(100, 150), 2, B
PAINT (50, 50), 1, 2
LOCATE 20, 1
```

## See Also

- [CIRCLE](CIRCLE), [COLOR](COLOR), [DRAW](DRAW), [LINE](LINE), [POINT](POINT), [PRESET](PRESET), [PSET](PSET), [SCREEN](SCREEN)
