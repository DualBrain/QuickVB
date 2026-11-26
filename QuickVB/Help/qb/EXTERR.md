# EXTERR

To return extended error information.

## Syntax

`EXTERR(n)`

## Comments

`EXTERR` returns "extended" error information provided by versions of DOS 3.0 and greater. For versions of DOS earlier than 3.0, `EXTERR` always returns zero. The single integer argument must be in the range 0-3 as follows:

| Value of n | Return Value |
| ---------- | ------------ |
| 0 | Extended error code |
| 1 | Extended error class |
| 2 | Extended error suggested action |
| 3 | Extended error locus |

The values returned are not defined by BASIC, but by DOS. Refer to the *MS-DOS Programmer's Reference (version 3.0 or later) for a description of the values returned by the DOS extended error function.

The extended error code is actually retrieved and saved by BASIC each time appropriate DOS functions are performed. Thus when an `EXTERR` function call is made, these saved values are returned.
