# OPEN

To establish input/output (I/O) to a file or device.

## Syntax

`OPEN` *filename* [ `FOR` *access_mode* ] [ `ACCESS` *network_access* ] [ *lock_type* ] `AS` [#] *file_number* [ `LEN` = *record_length* ]

`OPEN` *mode*, [#] *file_number*, *filename* [ , *record_length* ]

`OPEN "COM`*n*: *basic_com* *com_specifics*" [ `FOR` *access_mode* ] `AS` [#] *file_number* [ `LEN` = *record_length* ]

## Comments

*filename* is a string expression containing the name of the file or device to open.

*access_mode* specifies how the file is to be used: `INPUT`, `OUTPUT`, `APPEND`, `RANDOM` or `BINARY`. The default is `RANDOM`.

*network_access* provides more detail on how the file is to be used in shared-file network environments: `READ`, `WRITE` or `READ WRITE`.

*lock_type* is the type of file locking used in shared-file environments: `SHARED`, `LOCK READ`, `LOCK WRITE` or `LOCK READ WRITE`.

*file_number* is the integer number to associate with the file for read, write and close operations.

*record_length* is the number of bytes in each record. For sequential files, the default is 512; for random-access files, the default is 128; for communications, the default is 128. The value cannot exceed 32,767.

*mode* is used for older BASIC programs. It is a single-letter string that specifies the access mode:

- `A` Append
- `B` Binary
- `I` Input
- `O` Output
- `R` Random

*n* is the number of the communications port to open, either 1 or 2.

*basic_com* represents the basic data-communications parameters, separated by commas, in the following form:

[ *baud* ] [ ,[ *parity*] [, [ *databits* ] [ , [ *stopbits* ]]]]

- *baud* is the baud rate of the device used: 75, 110, 150, 300, 600, 1200, 1800, 2400, 4800, 9600 or 19200.
- *parity* is the parity of the device used: `N` (none), `E` (even), `O` (odd), `S` (space), `M` (mark) or `PE` (enable error checking).
- *databits* is the number of bits in each data word: 5, 6, 7 or 8. The sum of parity bits and data bits must be less than or equal to 8. If the parity is set to `O` (odd) or `E` (even), one bit is used for parity. If parity is set to `N` (none), no bits are used for parity.
- *stopbits* is the number of stop bits for each word: 1, 1.5 or 2.

*com_specifics* is a list of data-communications specifics, separated by commas:

- `ASC` opens device in ASCII mode
- `BIN` opens device in binary mode
- `CD`[ *milliseconds* ] specifies carrier-detect timeout
- `CS`[ *milliseconds* ] specifies clear-to-send timeout
- `DS`[ *milliseconds* ] specifies data-set-ready timeout
- `LF` sends linefeed after each carriage return
- `OP`[ *milliseconds* ] specifies the `OPEN` statement timeout period
- `RB`[ *bytes* ] specifies the size of the receive buffer
- `RS` suppresses detection of *Request To Send* (RTS)
- `TB`[ *bytes* ] specifies the size of the transmit buffer

## Example

```vb
OPEN "TEST.DAT" FOR INPUT AS #1

f% = FREEFILE
OPEN "NEW.DAT" FOR RANDOM AS f% LEN = 80

OPEN "C0M1:4800, E, 7, 1, BIN" AS #2
```

## See Also

- [CLOSE](CLOSE), [FREEFILE](FREEFILE)
