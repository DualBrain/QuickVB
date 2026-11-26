# COMMAND LINE

These options can be typed on the command line following qbasic.exe.

## Syntax

`QBASIC [--bw] [--50] [--mbf] [[--run] sourcefile] [--log logfile] [--cmd string]`

## Options

| Option                 | Description |
| ---------------------- | ----------- |
| *sourcefile*           | Names the file to load when QBasic starts. To load a file created with GW-BASIC or BASICA, the file must be saved from GW-BASIC or BASICA with the `,A` option. |
| `--run` *sourcefile*   | Causes QBasic to load and run a program file before displaying it. (Legacy: `/RUN`) |
| `--bw`                 | Allows the use of a composite (monochrome) monitor with a color graphics card. The `--bw` option displays QBasic in monochrome if you have a color monitor. (Legacy: `/B`) |
| `--50`                 | Displays the maximum number of lines possible on your hardware. (Legacy: `/H`) |
| `--mbf`                | Causes the QBasic conversion functions (`CVS`, `CVD`, `MKS$`, `MKD$`) to treat IEEE-format numbers as Microsoft-Binary-format numbers. (Legacy: `/MBF`) |
| `--log` *logfile*      | Logs interpreter activity to the specified file. |
| `--cmd` *string*       | Passes *string* to the `COMMAND$` function. This option must be the last option on the line. (Legacy: `/CMD`) |
| `/EDITOR`              | Invokes the MS-DOS Editor text editor. Can be abbreviated as `/ED`. |
| `/G`                   | Sets QBasic to update a CGA screen as fast as possible (works only with machines using CGA monitors). If you see snow (dots flickering on the screen) when QBasic updates your screen, your hardware cannot fully support this option. If you prefer a clean screen, restart QBasic without the `/G` option. |
| `/NOHI`                | Allows the use of a monitor that does not support high intensity. Not for use with Compaq laptop computers. |
| `/C:` *buffersize*     | Sets the size of the buffer receiving data. This option works only with an asynchronous communications card. The default buffer size is 512 bytes; the maximum size is 32,767 bytes. |
| `/L` [ *libraryname* ] | Loads the Quick library that is specified by *libraryname*. If *libraryname* is not specified, the default Quick library, `QB.QLB`, is loaded. |
| `/AH`                  | Allows dynamic arrays of records, fixed-length strings, and numeric data to be larger than 64K each. |

## Example

```vb
> qbasic --mbf
```
