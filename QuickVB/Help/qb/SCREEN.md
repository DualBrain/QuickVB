# SCREEN

Defines the screen characteristics.

## Syntax

`SCREEN` [ *screen_mode*] [, [ *coloroff*] [, [ *active_page*] [, *visual_page*]]]

## Comments

*screen_mode* is an integer expression that specifies the mode of operation:

| Value | Mode | Adapter |
| --- | --- | --- |
| 0 | Text | CGA, EGA, VGA, MCGA |
| 1 | 320 x 200 graphics | CGA, EGA, VGA, MCGA |
| 2 | 640 x 200 graphics | EGA, VGA |
| 3 | 720 x 348 graphics | Hercules* |
| 4 | 640 x 400 graphics | Olivetti, AT&T 6300 |
| 7 | 320 x 200 graphics | EGA, VGA |
| 8 | 640 x 200 graphics | EGA, VGA |
| 9 | 640 x 350 graphics | EGA, VGA |
| 10 | 640 x 350 graphics | EGA, VGA |
| 11 | 640 x 480 graphics | VGA, MCGA |
| 12 | 640 x 480 graphics | VGA |
| 13 | 320 x 200 graphics | VGA, MCGA |

*The Hercules driver MSHERC.COM must be loaded.*

*coloroff* is a numeric expression. When true, it disables color on composite monitors. (Ignored in screen modes 2 and up.)

*active_page* is the video display page to which text output and graphics commands write.

*visual_page* is the video display page that appears on your screen.

### MDPA with Monochrome Display: Mode 0

The IBM Monochrome Display and Printer Adapter (MDPA) is used to connect only to a monochrome display. Programs written for this configuration must be text mode only.

### CGA with Color Display: Modes 0, 1, and 2

The IBM Color Graphics Adapter (CGA) and Color Display are typically paired with each other. This hardware configuration permits the running of text mode programs, and both medium-resolution and high-resolution graphics programs.

### EGA with Color Display: Modes 0, 1, 2, 7, and 8

The five screen modes 0, 1, 2, 7, and 8 allow you to interface to the IBM Color Display when it is connected to an IBM Enhanced Graphics Adapter (EGA). If EGA switches are set for CGA compatibility, programs written for modes 1 and 2 will run just as they would with the CGA. Modes 7 and 8 are similar to modes 1 and 2, except that a wider range of colors is available in modes 7 and 8.

### EGA with Enhanced Color Display: Modes 0, 1, 2, 7, and 8

With the EGA/IBM Enhanced Display configuration, modes 0, 1, 2, 7, and 8 are virtually identical to their EGA/Color Display counterparts. Two possible differences are as follows:

In mode 0, the border color cannot be the same as for the EGA/Color Display because the border cannot be set on an Enhanced Color Display when it is in 640 × 350 text mode.

The quality of the text is better on the Enhanced Color Display (an 8 × 14 character box for Enhanced Color Display versus an 8 × 8 character box for color display).

### EGA with Enhanced Color Display: Mode 9

The full capability of the Enhanced Color Display is taken advantage of in this mode. Mode 9 allows the highest resolution possible for the EGA/Enhanced Color Display configuration. Programs written for this mode will not work for any other hardware configuration.

### EGA with Monochrome Display: Mode 10

The IBM Monochrome Display can be used to display monochrome graphics at a very high resolution in this mode. Programs written for this mode will not work for any other hardware configuration.

### Arguments

Each of the `SCREEN` modes is described individually in the following paragraphs.

`SCREEN 0`

- Text mode only
- Either 40 × 25 or 80 × 25 text format with character-box size of 8 × 8 (8 × 14 with EGA)
- Assignment of 16 colors to any of 2 attributes
- Assignment of 16 colors to any of 16 attributes (with EGA)

`SCREEN 1`

- 320 × 200 pixel medium-resolution graphics
- 80 × 25 text format with character-box size of 8 × 8
- Assignment of 16 colors to any of 4 attributes
- Supports both EGA and CGA
- 2 bits per pixel

`SCREEN 2`

- 640 × 200 pixel high-resolution graphics
- 40 × 25 text format with character-box size of 8 × 8
- Assignment of 16 colors to any of 2 attributes
- Supports both EGA and CGA
- 1 bit per pixel

`SCREEN 7`

- 320 × 200 pixel medium-resolution graphics
- 40 × 25 text format with character-box size of 8 × 8
- 2, 4, or 8 memory pages with 64K, 128K, or 256K of memory, respectively, installed on the EGA
- Assignment of any of 16 colors to 16 attributes
- EGA required
- 4 bit per pixel

`SCREEN 8`

- 640 × 200 pixel high-resolution graphics
- 80 × 25 text format with character-box size of 8 × 8
- 1, 2, or 4 memory pages with 64K, 128K, or 256K of memory, respectively, installed on the EGA
- Assignment of any of 16 colors to 16 attributes
- EGA required
- 4 bits per pixel

`SCREEN 9`

- 640 × 350 pixel enhanced-resolution graphics
- 80 × 25 text format with character-box size of 8 × 14
- Assignment of either 64 colors to 16 attributes (more than 64K of EGA memory), or 16 colors to 4 attributes (64K of EGA memory)
- Two display pages if 256K of EGA memory installed
- EGA required
- 2 bits per pixel (64K EGA memory) 4 bits per pixel (more than 64K EGA memory)

`SCREEN 10`

- 640 × 350 enhanced-resolution graphics
- 80 × 25 text format with character-box size of 8 × 14
- Two display pages if 256K of EGA memory installed
- Assignment of up to 9 pseudo-colors to 4 attributes
- EGA required
- 2 bits per pixel

The following are default attributes for `SCREEN 10`, monochrome display:

| Attribute Value | Displayed Pseudo-Color |
| --------------- | ---------------------- |
| 0 | Off |
| 1 | On, normal intensity |
| 2 | Blink |
| 3 | On, high intensity |

The following are color values for `SCREEN 10`, monochrome display:

| Color Value | Displayed Pseudo-Color |
| ----------- | ---------------------- |
| 0 | Off |
| 1 | Blink, off to on |
| 2 | Blink, off to high intensity |
| 3 | Blink, on to off |
| 4 | On |
| 5 | Blink, on to high intensity |
| 6 | Blink, high intensity to off |
| 7 | Blink, high intensity to on |
| 8 | High intensity |

For both composite monitors and TVs, the colorswitch is a numeric expression that is either true (non-zero) or false (zero). A value of zero disables color and permits display of black and white images only. A nonzero value permits color. The meaning of the colorswitch argument is inverted in `SCREEN` mode 0.

For hardware configurations that include an EGA and enough memory to support multiple-screen pages, two arguments are available. These apage and vpage arguments determine the "active" and "visual" memory pages. The active page is the area in memory where graphics statements are written; the visual page is the area of memory that is displayed on the screen.

Animation can be achieved by alternating the display of graphics pages. The goal is to display the visual page with completed graphics output, while executing graphics statements in one or more active pages. A page is displayed only when graphics output to that page is complete. Thus, the following program fragment is typical:

```text
SCREEN 7,, 1, 2  'work in page 1, show page 2
.
. Graphics output to page 1
. while viewing page 2
.
SCREEN 7,,2,1  'work in page 2, show page 1
.
. Graphics output to page 2
. while viewing page 1
.
```

The number of pages available depends on the SCREEN mode and the amount of available memory, as described in the following table:

**Table 2**

`SCREEN` Mode Specifications

| Mode | Resolution     | Attribute Range | Color Range | EGA Memory | Pages | Page Size |
| ---- | -------------- | --------------- | ----------- | ---------- | ----- | --------- |
| 0    | 40-column text | NA              | 0-15<sup>a</sup>       | NA         | 1     | 2K        |
|      | 80-column text | NA              | 0-15<sup>a</sup>       | NA         | 1     | 4K        |
| 1    | 320×200        | 0-3<sup>b</sup>            | 0-3         | NA         | 1     | 16K       |
| 2    | 640×200        | 0-1<sup>b</sup>            | 0-1         | NA         | 1     | 16K       |
| 7    | 320×200        | 0-15            | 0-15        | 64K        | 2     | 32K       |
|      |                |                 |             | 128K       | 4     |           |
|      |                |                 |             | 256K       | 8     |           |
| 8    | 640×200        | 0-15            | 0-15        | 64K        | 1     | 64K       |
|      |                |                 |             | 128K       | 2     |           |
|      |                |                 |             | 256K       | 4     |           |
| 9    | 640×350        | 0-3             | 0-15        | 64K        | 1     | 64K       |
|      |                | 0-15            | 0-63        | 128K       | 1     | 128K      |
|      |                | 0-15            | 0-63        | 256K       | 2     |           |
| 10   | 640×350        | 0-3             | 0-8         | 128K       | 1     | 128K      |
|      |                |                 |             | 256K       | 2     |           |

- <sup>a</sup> Numbers in the range 16-31 are blinking versions of the colors 0-15.
- <sup>b</sup> Attributes applicable only with EGA.

### Attributes and Colors

For various screen modes and display hardware configurations, different attribute and color settings exist. (See the [PALETTE](PALETTE) statement for a discussion of attribute and color number.) The majority of these attribute and color configurations are summarized in the following table:

**Table 3**

Default Attributes and Colors for Most Screen Modes

| Attributes for Mode | Color Display                  | Monochrome Display                    |
| 1,9 | 2 | 0,7,8,9<sup>b</sup>  | Number<sup>c</sup> | Color                | Number<sup>c</sup> | Color                       |
| --- | - | --------- | ------- | -------------------- | ------- | --------------------------- |
| 0   | 0 |	0         | 0       | Black                | 0       | Off                         |
|     |   | 1         | 1       | Blue                 |         | (Underlined)<sup>a</sup>    |
|     |   | 2         | 2       | Green                | 1       | On<sup>a</sup>              |
|     |   | 3         | 3       | Cyan                 | 1       | On<sup>a</sup>              |
|     |   | 4         | 4       | Red                  | 1       | On<sup>a</sup>              |
|     |   | 5         | 5       | Magenta              | 1       | On<sup>a</sup>              |
|     |   | 6         | 6       | Brown                | 1       | On<sup>a</sup>              |
|     |   | 7         | 7       | White                | 1       | On<sup>a</sup>              |
|     |   | 8         | 8       | Gray                 | 0       | Off                         |
|     |   | 9         | 9       | Light Blue           |         | High intensity (underlined) |
|     |   | 10        | 10      | Light Green          | 2       | High intensity              |
| 1   |   | 11        | 11      | Light Cyan           | 2       | High intensity              |
|     |   | 12        | 12      | Light Red            | 2       | High intensity              |
| 2   |   | 13        | 13      | Light Magenta        | 2       | High intensity              |
|     |   | 14        | 14      | Yellow               | 2       | High intensity              |
| 3   | 1 | 15        | 15      | High-intensity White | 0       | Off                         |

- <sup>a</sup> Off when used for background.
- <sup>b</sup> With EGA memory > 64K.
- <sup>c</sup> Only for mode 0 monochrome.

The default foreground colors for the various modes are given in the following table:

**Table 4**

Default Foreground Colors

|            | Default foreground attribute                            | Default foreground color |
| Screen mode| Color/Extended Display<sup>a</sup> | Monochrome Display | Color/Extended Display<sup>a</sup> | Monochrome Display |
| ---------- | ----------------------- | ------------------ | ----------------------- | ------------------ |
| 0          | 7                       | 7                  | 7                       | 1 |
| 1          | 3                       | NA                 | 15                      | NA |
| 2          | 1                       | NA                 | 15                      | NA |
| 7          | 15                      | NA                 | 15                      | NA |
| 8          | 15                      | NA                 | 15                      | NA |
| 9          | 3<sup>b</sup>                      | NA                 | 63                      | NA |
| 10         | NA                      | 3                  | NA                      | 8 |

- <sup>a</sup> IBM Enhanced Color Display
- <sup>b</sup> 15 if greater than 64K of EGA memory
- NA=Not Applicable

## Example

```vb
SCREEN 1     ' 320x200 graphics
LINE (10, 10)-(20, 20), , B
```

## See Also

- [CIRCLE](CIRCLE), [COLOR](COLOR), [DRAW](DRAW), [GET](GET), [LINE](LINE), [PAINT](PAINT), [POINT](POINT), [PALETTE](PALETTE), [PALETTE USING](PALETTE-USING), [PRESET](PRESET), [PSET](PSET), [PUT](PUT), [VIEW](VIEW), [WINDOW](WINDOW)
