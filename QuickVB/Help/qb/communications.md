# Appendix F

## Communications

This appendix describes the BASIC statements necessary to support RS-232 asynchronous communications with other computers and peripheral devices.

### F.1 Opening Communications Files

The [OPEN COM](OPEN-COM-FILE) statement allocates a buffer for input and output in the same manner as the [OPEN](OPEN) statement opens disk files.

### F.2 Communications I/O

Since the communications port is opened as a file, all I/O statements valid for disk files are valid for COM.

COM sequential input statements are the same as those for disk files:

* [INPUT#](INPUT-FILE)
* [LINE INPUT#](LINE-INPUT-FILE)
* [INPUT$](INPUT$)

COM sequential output statements are the same as those for diskette:

* [PRINT#](PRINT-FILE)
* [PRINT# USING](PRINT-FILE-USING)

See the BASIC User's Reference for more information on these statements.

### F.3 The COM I/O Functions

The most difficult aspect of asynchronous communications is processing characters as quickly as they are received. At rates above 2400 baud (bps), it is necessary to suspend character transmission from the host long enough for the receiver to catch up. This can be done by sending XOFF (CTRL-S) to the host to temporarily suspend transmission, and XON (CTRL-Q) to resume, if the application supports it.

BASIC provides three functions which help to determine when an overrun condition is imminent:

[LOC](LOC]`(x)` Returns the number of characters in the input queue waiting to be read. The input queue can hold more than 255 characters (determined by the /c: switch). If there are more than 255 characters in the queue, LOC(x) returns 255. Since a string is limited to 255 characters, this practical limit alleviates the need for the programmer to test for string size before reading data into it.

[LOF](LOF)`(x)` Returns the amount of free space in the input queue; that is

`/c:(size)-number of characters in the input queue`

[LOF](LOF) may be used to detect when the input queue is reaching storage capacity.

[EOF](EOF)`(x)` True (-1), indicates that the input queue is empty. False (0) is returned if any characters are waiting to be read.

### F.4 Possible Errors:

A "Communications buffer overflow" error occurs if a read is attempted after the input queue is full (that is, [LOC](LOC)`(x)` returns 0).

A "Device I/O" error occurs if any of the following line conditions are detected on receive: overrun error (OE), framing error (FE), or break interrupt (BI). The error is reset by subsequent inputs, but the character causing the error is lost.

A "Device fault" error occurs if data set ready (DSR) is lost during I/O.

A "Parity error" occurs if the PE (parity enable) option was used in the OPEN COM statement and incorrect parity was received.

### F.5 The INPUT$ Function

The [INPUT$](INPUT$) function is preferred over the [INPUT](INPUT) and [LINE INPUT](LINE-INPUT) statements for reading COM files, because all ASCII characters may be significant in communications. [INPUT](INPUT) is least desirable because input stops when a comma or an enter is seen. [LINE INPUT](LINE-INPUT) terminates when an enter is seen.

[INPUT$](INPUT$) allows all characters read to be assigned to a string.

[INPUT$](INPUT$) returns x characters from the y file. The following statements then are most efficient for reading a COM file:

```vb
10 WHILE NOT EOF(1)
20 A$=INPUT$(LOC(1),#1)
30 ...
40 ... Process data returned in A$ ...
50 ...
60 WEND
```

This sequence of statements translates: As long as something is in the input queue, return the number of characters in the queue and store them in A$. If there are more than 255 characters, only 255 are returned at a time to prevent string overflow. If this is the case, [EOF](EOF)`(1)` is false, and input continues until the input queue is empty.

## [GET#](GET-FILE) and [PUT#](PUT-FILE) Statements for COM Files

To allow fixed-length I/O for COM.

## Syntax

`GET filenumber, nbytes PUT filenumber, nbytes`

## Comments

*filenumber* is an integer expression returning a valid file number.

*nbytes* is an integer expression returning the number of bytes to be transferred into or out of the file buffer. *nbytes* cannot exceed the value set by the /s: switch when BASIC was invoked.

Because of the low performance associated with telephone line communications, it is recommended that [GET#](GET-FILE) and [PUT#](PUT-FILE) not be used in such applications.

## Example

The following TTY sample program is an exercise in communications I/O. It is designed to enable your computer to be used as a conventional terminal. Besides full-duplex communications with a host, the TTY program allows data to be downloaded to a file. Conversely, a file may be uploaded (transmitted) to another machine.

In addition to demonstrating the elements of asynchronous communications, this program is useful for transferring BASIC programs and data to and from a computer.

> This program is set up to communicate with a DEC® System-20 especially in the use of XON and XOFF. It may require modification to communicate with other types of hardware.

### F.6 The TTY Sample Program

```vb
10 SCREEN 0,0:WIDTH 80
15 KEY OFF:CLS:CLOSE
20 DEFINT A-Z
25 LOCATE 25,1
30 PRINT STRING$(60," ")
40 FALSE=0:TRUE=NOT FALSE
50 MENU=5 'Value of MENU Key (^E)
60 XOFF$=CHR$(19):XON$=CHR$(17)
100 LOCATE 25,1:PRINT "Async TTY Program";
110 LOCATE 1,1:LINE INPUT "Speed?";"SPEED$
120 COMFIL$="COM1:,+SPEED$+",E,7"
130 OPEN COMFIL$ AS #1
140 OPEN "SCRN:"FOR OUTPUT AS #3
200 PAUSE=FALSE
210 A$=INKEY$:IF A$=""THEN 230
220 IF ASC(A$)=MENU THEN 300 ELSE PRINT #1,A$;
230 IF EOF(1) THEN 210
240 IF LOC(1)>128 THEN PAUSE=TRUE:PRINT #1,XOFF$;
250 A$=INPUT$(LOC(1),#1)
260 PRINT #3,A$;:IF LOC(1)>0 THEN 240
270 IF PAUSE THEN PAUSE=FALSE:PRINT #1,XON$;
280 GOTO 210
300 LOCATE 1,1:PRINT STRING$(30,32):LOCATE 1,1
310 LINE INPUT "FILE?";DSKFIL$
400 LOCATE 1,1:PRINT STRING$(30,32):LOCATE 1,1
410 LINE INPUT"(T)ransmit or (R)eceive?";TXRX$
420 IF TXRX$="T" THEN OPEN DSKFIL$ FOR INPUT AS #2:GOTO 1000
430 OPEN DSKFIL$ FOR OUTPUT AS #2
440 PRINT #1,CHR$(13);
500 IF EOF(1) THEN GOSUB 600
510 IF LOC(1)>128 THEN PAUSE=TRUE:PRINT #1,XOFF$;
520 A$=INPUT$(LOC(1),#1)
530 PRINT #2,A$;:IF LOC(1)>0 THEN 510
540 IF PAUSE THEN PAUSE=FALSE:PRINT #1,XON$;
550 GOTO 500
600 FOR I=1 TO 5000
610 IF NOT EOF(1) THEN I=9999
620 NEXT I
630 IF I>9999 THEN RETURN
640 CLOSE #2;CLS:LOCATE 25,10:PRINT "* Download complete *";
650 RETURN 200
1000 WHILE NOT EOF(2)
1010 A$=INPUT$(1,#2)
1020 PRINT #1,A$;
1030 WEND
1040 PRINT #1,CHR$(28);^Z to make close file.
1050 CLOSE #2:CLS:LOCATE 25,10:PRINT "** Upload complete **";
1060 GOTO 200
9999 CLOSE:KEY ON
```

### F.7 Notes on the TTY Sample Program

> Asynchronous implies character I/O as opposed to line or block I/O. Therefore, all prints (either to the COM file or screen) are terminated with a semicolon (;). This retards the return line feed normally issued at the end of the [PRINT](PRINT) statement.

Line Number | Comments
--- | ---
10 | Sets the SCREEN to black and white alpha mode and sets the width to 80.
15 | Turns off the soft key display, clears the screen, and makes sure that all files are closed.
20 | Defines all numeric variables as integer, primarily for the benefit of the subroutine at 600-620. Any program looking for speed optimization should use integer counters in loops where possible.
40 | Defines Boolean true and false.
50 | Defines the ASCII (ASC) value of the MENU key.
60 | Defines the ASCII XON and XOFF characters.
100-130 | Prints program ID and asks for baud rate (speed). Opens communications to file number 1, even parity, 7 data bits.
200-280 | This section performs full-duplex I/O between the video screen and the device connected to the RS-232 connector as follows:
.       | 1. Read a character from the keyboard into A$. INKEY$ returns a null string if no character is waiting.
.       | 2. If a keyboard character is available, waiting, then:
.       |    * If the character is the MENU key, the operator is ready to down-load a file. Get filename.
.       |    * If the character (A$) is not the MENU key send it by writing to the communications file (PRINT #1...).
.       | 3. If no character is waiting, check to see if any characters are being received.
.       | 4. At 230, see if any characters are waiting in COM buffer. If not, go back and check the keyboard.
.       | 5. At 240, if more than 128 characters are waiting, set PAUSE flag to indicate that input is being suspended. Send XOFF to host, stopping further transmission.
.       | 6. At 250-260, read and display contents of COM buffer on screen until empty. Continue to monitor size of COM buffer (in 240). Suspend transmission if reception falls behind.
.       | 7. Resume host transmission by sending XON only if suspended by previous XOFF.
.       | 8. Repeat process until the MENU key is pressed.
300-320 | Get disk filename to be downloaded to. Open the file as number 2.
400-420 | Asks if file named is to be transmitted (uploaded) or received (down-loaded).
430 | Receive routine. Sends a RETURN to the host to begin the down-load. This program assumes that the last command sent to the host was to begin such a transfer and was missing only the terminating return. If a DEC® system is the host, such a command might be `COPY TTY:=MANUAL.MEM (MENU Key)` if the MENU key was struck instead of RETURN.
500 | When no more characters are being received, (LOC(x) returns 0), then performs a timeout routine.
510 | If more than 128 characters are waiting, signal a pause and send XOFF to the host.
520-530 | Read all characters in COM queue (LOC(x)) and write them to diskette (PRINT #2...) until reception is caught up to transmission.
540-550 | If a pause is issued, restart host by sending XON and clearing the pause flag. Continue the process until no characters are received for a predetermined time.
600-650 | Time-out subroutine. The FOR loop count was determined by experimentation. If no character is received from the host for 17-20 seconds, transmission is assumed complete. If any character is received during this time (line 610), then set n well above the FOR loop range to exit loop and return to caller. If host transmission is complete, close the disk file and resume regular activities.
1000-1060 | Transmit routine. Until end of disk file, read one character into A$ with INPUT$ statement. Send character to COM device in 1020. Send a ^Z at end of file in 1040 in case receiving device needs one to close its file. Lines 1050 and 1060 close disk file, print completion message, and go back to conversation mode in line 200.
9999 | Presently not executed. As an exercise, add some lines to the routine 400-420 to exit the program via line 9999. This line closes the COM file left open and restores the function key display.
