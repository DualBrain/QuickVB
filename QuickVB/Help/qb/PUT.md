# PUT

Displays a graphics image on the screen.

## Syntax

`PUT` [ `STEP`] (*x*,*y*), *array*[(*index*) ] [, *display_verb*]

## Comments

The keyword `STEP` indicates that *x* and *y* are offsets from the last point drawn, as opposed to physical coordinates.

*array* is the name of the array containing the image to display.

*index* is the index where the graphics begins in the array.

*display_verb* indicates how the image is displayed.

- `PSET` transfers the data onto the screen verbatim.
- `PRESET` is the same as `PSET` except that an inverse image (black on white) is produced.
- `AND` transfers the image only if an image already exists under the transferred image.
- `OR` superimposes the image onto the existing image.
- `XOR` is a special mode often used for animation. `XOR` causes the points on the screen to be inverted where a point exists in the array image. This behavior is exactly like the cursor on the screen. `XOR` is especially useful for animation. When an image is put against a complex background twice, the background is restored unchanged. An object can be moved around the screen without obliterating the background. The default action mode is `XOR`.

Animation of an object is usually performed as follows:

- Put the object(s) on the screen.
- Recalculate the new position of the object(s).
- Put the object(s) on the screen a second time at the old location(s) to remove the old image(s).
- Return to Step 1, this time putting the object(s) at the new location.

Movement done this way leaves the background unchanged. Flicker can be cut down by minimizing the time between Steps 4 and 1, and by making sure that there is enough time delay between Steps 1 and 3. If more than one object is being animated, process every object at once, one step at a time.

If it is not important to preserve the background, animation can be performed using the `PSET` action verb.

Leave a border around the image (when it is first gotten) as large or larger than the maximum distance the object will move. Thus, when an object is moved, this border effectively erases any points. This method may be somewhat faster than the method using `XOR` described above since only one `PUT` is required to move an object. However, the image to be `PUT` must be larger than the existing image.

## Example

```vb
CLS: SCREEN 1
PSET (130, 120)
DRAW "U25; E7; R20; D32; L6; U12; L14"
DRAW "D12; L6": PSET(137, 102)
DRAW "U4; E4; R8; D8; L12"
PSET (137, 88)
DRAW "E4; R20; D32; G4": PAINT (131,119)
DIM A (500)
GET (125, 130)-(170, 80), A
FOR I= 1 TO 1000: NEXT I
PUT (20, 20), A, PSET
FOR I= 1 TO 1000: NEXT i
GET (125, 130)-(170, 80), A
FOR I= 1 TO 1000: NEXT I
PUT (220, 130), A, PRESET
```

```vb
SCREEN 1 
DIM box(1 TO 200)
x1 = 0
x2 = 10
y1 = 0
y2 = 10
LINE (x1, y1)-(x2, y2), 2, BF 'Create a box
GET (x1, y1)-(x2, y2), box    'Save image
FOR i = 1 TO 10000
  PUT (x1, y1), box, XOR      'Move box around
  x1 = RND * 300              'screen randomly
  y1 = RND * 180
  PUT (x1, y1), box
NEXT i 
```

## See Also

- [GET](GET)
