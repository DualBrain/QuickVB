# Appendix A

Error Codes and Messages

Code | Message
-- | --
1 | NEXT without FORNEXT statement does not have a corresponding FOR statement. Check variable at FOR statement for a match with the NEXT statement variable.
2 | Syntax errorA line is encountered that contains an incorrect sequence of characters (such as unmatched parentheses, a misspelled command or statement, incorrect punctuation). This error causes GW-BASIC to display the incorrect line in edit mode.
3 | RETURN without GOSUBA RETURN statement is encountered for which there is no previous GOSUB statement.
4 | Out of DATAA READ statement is executed when there are no DATA statements with unread data remaining in the program.
5 | Illegal function callAn out-of-range parameter is passed to a math or string function. An illegal function call error may also occur as the result of:a negative or unreasonably large subscripta negative or zero argument with LOGa negative argument to SQRa negative mantissa with a noninteger powera call to a USR function for which the starting address has not yet been givenan improper argument to MID$, LEFT$, RIGHT$, INP, OUT, WAIT, PEEK, POKE, TAB, SPC, STRING$, SPACE$, INSTR, or ON...GOTO
6 | OverflowThe result of a calculation is too large to be represented in GW-BASIC's number format. If underflow occurs, the result is zero, and execution continues without an error.
7 | Out of memoryA program is too large, has too many FOR loops, GOSUBs, variables, or expressions that are too complicated. Use the CLEAR statement to set aside more stack space or memory area.
8 | Undefined line numberA line reference in a GOTO, GOSUB, IF-THEN...ELSE, or DELETE is a nonexistent line.
9 | Subscript out of rangeAn array element is referenced either with a subscript that is outside the dimensions of the array, or with the wrong number of subscripts.
10 | Duplicate DefinitionTwo DIM statements are given for the same array, or a DIM statement is given for an array after the default dimension of 10 has been established for that array.
11 | Division by zeroA division by zero is encountered in an expression, or the operation of involution results in zero being raised to a negative power. Machine infinity with the sign of the numerator is supplied as the result of the division, or positive machine infinity is supplied as the result of the involution, and execution continues.
12 | Illegal directA statement that is illegal in direct mode is entered as a direct mode command.
13 | Type mismatchA string variable name is assigned a numeric value or vice versa; a function that expects a numeric argument is given a string argument or vice versa.
14 | Out of string spaceString variables have caused GW-BASIC to exceed the amount of free memory remaining. GW-BASIC allocates string space dynamically until it runs out of memory.
15 | String too longAn attempt is made to create a string more than 255 characters long.
16 | String formula too complexA string expression is too long or too complex. Break the expression into smaller expressions.
17 | Can't continueAn attempt is made to continue a program thatHas halted because of an errorHas been modified during a break in executionDoes not exist
18 | Undefined user functionA USR function is called before the function definition (DEF statement) is given.
19 | No RESUMEAn error-trapping routine is entered but contains no RESUME statement.
20 | RESUME without errorA RESUME statement is encountered before an error-trapping routine is entered.
21 | Unprintable errorNo error message is available for the existing error condition. This is usually caused by an error with an undefined error code.
22 | Missing operandAn expression contains an operator with no operand following it.
23 | Line buffer overflowAn attempt is made to input a line that has too many characters.
24 | Device TimeoutGW-BASIC did not receive information from an I/O device within a predetermined amount of time.
25 | Device FaultIndicates a hardware error in the printer or interface card.
26 | FOR Without NEXTA FOR was encountered without a matching NEXT.
27 | Out of PaperThe printer is out of paper; or, a printer fault.
28 | Unprintable errorNo error message is available for the existing error condition. This is usually caused by an error with an undefined error code.
29 | WHILE without WENDA WHILE statement does not have a matching WEND.
30 | WEND without WHILEA WEND was encountered without a matching WHILE.
31-49 | Unprintable errorNo error message is available for the existing error condition. This is usually caused by an error with an undefined error code.
50 | FIELD overflowA FIELD statement is attempting to allocate more bytes than were specified for the record length of a random file.
51 | Internal errorAn internal malfunction has occurred in GW-BASIC. Report to your dealer the conditions under which the message appeared.
52 | Bad file numberA statement or command references a file with a file number that is not open or is out of range of file numbers specified at initialization.
53 | File not foundA LOAD, KILL, NAME, FILES, or OPEN statement references a file that does not exist on the current diskette.
54 | Bad file modeAn attempt is made to use PUT, GET, or LOF with a sequential file, to LOAD a random file, or to execute an OPEN with a file mode other than I, O, A, or R.
55 | File already openA sequential output mode OPEN is issued for a file that is already open, or a KILL is given for a file that is open.
56 | Unprintable errorAn error message is not available for the error condition which exists. This is usually caused by an error with an undefined error code.
57 | Device I/O ErrorUsually a disk I/O error, but generalized to include all I/O devices. It is a fatal error; that is, the operating system cannot recover from the error.
58 | File already existsThe filename specified in a NAME statement is identical to a filename already in use on the diskette.
59-60 | Unprintable errorNo error message is available for the existing error condition. This is usually caused by an error with an undefined error code.
61 | Disk fullAll disk storage space is in use.
62 | Input past endAn INPUT statement is executed after all the data in the file has been input, or for a null (empty) file. To avoid this error, use the EOF function to detect the end of file.
63 | Bad record numberIn a PUT or GET statement, the record number is either greater than the maximum allowed (16,777,215) or equal to zero.
64 | Bad filenameAn illegal form is used for the filename with LOAD, SAVE, KILL, or OPEN; for example, a filename with too many characters.
65 | Unprintable errorNo error message is available for the existing error condition. This is usually caused by an error with an undefined error code.
66 | Direct statement in fileA direct statement is encountered while loading a ASCII-format file. The LOAD is terminated.
67 | Too many filesAn attempt is made to create a new file (using SAVE or OPEN) when all directory entries are full or the file specifications are invalid.
68 | Device UnavailableAn attempt is made to open a file to a nonexistent device. It may be that hardware does not exist to support the device, such as lpt2: or lpt3:, or is disabled by the user. This occurs if an OPEN "COM1: statement is executed but the user disables RS-232 support with the /c: switch directive on the command line.
69 | Communication buffer overflowOccurs when a communications input statement is executed, but the input queue is already full. Use an ON ERROR GOTO statement to retry the input when this condition occurs. Subsequent inputs attempt to clear this fault unless characters continue to be received faster than the program can process them. In this case several options are available:Increase the size of the COM receive buffer with the /c: switch.Implement a hand-shaking protocol with the host/satellite (such as: XON/XOFF, as demonstrated in the TTY programming example) to turn transmit off long enough to catch up.Use a lower baud rate for transmit and receive.
70 | Permission DeniedThis is one of three hard disk errors returned from the diskette controller.An attempt has been made to write onto a diskette that is write protected.Another process has attempted to access a file already in use.The UNLOCK range specified does not match the preceding LOCK statement.
71 | Disk not ReadyOccurs when the diskette drive door is open or a diskette is not in the drive. Use an ON ERROR GOTO statement to recover.
72 | Disk media errorOccurs when the diskette controller detects a hardware or media fault. This usually indicates damaged media. Copy any existing files to a new diskette, and reformat the damaged diskette. FORMAT maps the bad tracks in the file allocation table. The remainder of the diskette is now usable.
73 | Advanced FeatureAn attempt was made to use a reserved word that is not available in this version of GW-BASIC.
74 | Rename across disksOccurs when an attempt is made to rename a file to a new name declared to be on a disk other than the disk specified for the old name. The naming operation is not performed.
75 | Path/File Access ErrorDuring an OPEN, MKDIR, CHDIR, or RMDIR operation, MS-DOS is unable to make a correct path-to-filename connection. The operation is not completed.
76 | Path not foundDuring an OPEN, MKDIR, CHDIR, or RMDIR operation, MS-DOS is unable to find the path specified. The operation is not completed.
