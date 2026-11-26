# PLAY

Plays a tune specified by a string expression; additionally support play event trapping enabling/disabling.

## Syntax

`PLAY` *string_expression*

`PLAY ON`

`PLAY OFF`

`PLAY STOP`

## Comments

`PLAY ON` enables play event trapping.

`PLAY OFF` disables play event trapping. All events that occur are ignored.

`PLAY STOP` temporarily suspends play event trapping. All play events are processed after play event trapping is enabled.

The single-character commands in `PLAY` are as follows:

`A-G [#,+,-]`

`A`-`G` are notes. `#` or `+` following a note produces a sharp; `-` produces a flat.

Any note followed by `#`, `+`, or `-` must refer to a black key on a piano.

`L(n)`

Sets the length of each note. `L4` is a quarter note, `L1` is a whole note, and so on. *n* may be from 1 to 64.

Length may also follow the note to change the length for that note only. `A16` is equivalent to `L16A`.

`MF`

Music foreground. `PLAY` and [SOUND](SOUND) statements are to run in foreground. That is, each subsequent note or sound is not started until the previous note or sound is finished. This is the initial default.

`MB`

Music background. `PLAY` and [SOUND](SOUND) statements are to run in background. That is, each note or sound is placed in a buffer allowing the BASIC program to continue execution while music plays in the background. As many as 32 notes (or rests) can be played in background at one time.

`MN`

Music normal. Each note plays seven-eighths of the time determined by `L` (length).

`ML`

Music legato. Each note plays the full period set by `L`.

`MS`

Music staccato. Each note plays three-quarters of the time determined by `L`.

`N(n)`

Play note *n*. *n* may range from 0 to 84. In the 7 possible octaves, there are 84 notes. *n* set to 0 indicates a rest.

`O(n)`

Octave 0 sets the current octave. There are 7 octaves (0 through 6). Default is 4. Middle C is at the beginning of octave 3.

`P(n)`

Pause. `P` may range from 1-64.

`T(n)`

Tempo. `T` sets the number of `L4s` in a minute. *n* may range from 32-255. Default is 120.

`. (period)`

A period after a note increases the playing time of the note by 3/2 times the period determined by `L` (length of note) times `T` (tempo). Multiple periods can appear after a note, and the playing time is scaled accordingly. For example, `A.` will cause the note `A` to play one and half times the playing time determined by `L` (length of the note) times `T` (the tempo); two periods placed after `A` (`A..`) will cause the note to be played at 9/4 times its ascribed value; an `A` with three periods (`A...`) at 27/8, etc.

Periods may also appear after a `P` (pause), and increase the pause length as described above.

`Xstring;`

Executes a substring, where string is a variable assigned to a string of `PLAY` commands.

Because of the slow clock interrupt rate, some notes do not play at higher tempos; for example, 1.64 at `T255`. These note/tempo combinations must be determined through experimentation.

`>n`

A greater-than symbol preceding the note n plays the note in the next higher octave.

`<n`

A less-than symbol preceding the note n plays the note in the next lower octave.

> Numeric arguments follow the same syntax described under the [DRAW](DRAW) statement.

*n* as an argument can be a constant, or it can be a variable with = in front of it (*= variable*). A semicolon is required after the variable and also after the variable in *Xstring*.

## Example

```vb
'Play scale in 7 different octaves
scale$ = "CDEFGAB"
PLAY "L16"
FOR i = TO 6
  PLAY "0" + STR$(i)
  PLAY "X" + VARPTR$(scale$)
NEXT i
```

## See Also

- [ON PLAY(n)](ON-PLAY(n)), [PLAY(n)](PLAY(n)), [SOUND](SOUND)
