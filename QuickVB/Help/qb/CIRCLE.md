# CIRCLE

To draw a circle, ellipse, and angles on the screen during use of the Graphics mode.

## Syntax

`CIRCLE` [ `STEP` ](*x*, *y*), *radius*[, [ *color* ][,[ *start_angle* ],[ *end_angle* ][, *aspect_ratio* ]]]

## Comments

`STEP` is an optional keyword that tells `CIRCLE` that the *x* and *y* values are offsets from the current graphics cursor position.

*x* and *y* are the coordinates of the circle's center.

*color* is the border color for the circle. The circle is not filled.

*start_angle* is the starting angle in radians for the arc. The default is 0.

*end_angle* is the ending angle in radians for the arc. The default is 2π.

*aspect_ratio* is the ratio of the length of the *y* axis to the length of the *x* axis. By changing the aspect ratio, you can create ellipses.

## Examples

```vb
' Fill the screen with random circles
SCREEN 1
FOR i = 1 TO 100
  x = INT(320 * RND)
  y = INT(200 * RND)
  RADIUS = INT(100 * RND)
  CIRCLE (x,y), RADIUS
NEXT
SCREEN 0
```

```vb
' This will draw 17 ellipses
CLS
SCREEN 1
FOR R=160 TO 0 STEP-10
  CIRCLE (160,100),R,,,,5/18
NEXT
```

```vb
10 'This will draw 5 spheres
20 GOTO 160
50 IF VERT GOTO 100
60 CIRCLE (X,Y),R,C,,,.07
70 FOR I = 1 TO 5
80 CIRCLE (X,Y),R,C,,,I*.2:NEXT I
90 IF VERT THEN RETURN
100 CIRCLE (X,Y),R,C,,,1.3
110 CIRCLE (X,Y),R,C,,,1.9
120 CIRCLE (X,Y),R,C,,,3.6
130 CIRCLE (X,Y),R,C,,,9.8
140 IF VERT GOTO 60
150 RETURN
160 CLS: SCREEN 1: COLOR 0,1: KEY OFF: VERT=0
170 X=160: Y=100: C=1: R=50: GOSUB 50
180 X=30: Y=30: C=2: R=30: GOSUB 50
190 X=30: Y=169: GOSUB 50
200 X=289: Y=30: GOSUB 50
210 X=289: Y=169: GOSUB 50
220 LINE (30,30)-(289,169),1
230 LINE (30,169)-(289,30),1
240 LINE (30,169)-(289,30),1,B
250 Z$=INKEY$: IF Z$="" THEN 250
```

## See Also

- [COLOR](COLOR), [DRAW](DRAW), [LINE](LINE), [PAINT](PAINT), [PRESET](PRESET), [PSET](PSET), [SCREEN](SCREEN)
