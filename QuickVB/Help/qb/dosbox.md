# DosBox

Until Community QBasic is released, or as a tool to verify side-by-side feature comparison, you can execute Microsoft QBasic 1.1 on modern devices using the DosBox tool.

## Download

Download [DosBox](https://www.dosbox.com/).

A couple of options for where to find QBasic...

- The original MS-DOS installation diskettes, version 5.0+.
- It's also included on the original disc of the Windows 95 installation CD-ROM.
- It may also be on later versions of Windows...
- *At your risk*, you can also download from [WinWorld](https://winworldpc.com/product/qbasic/1x).

To my knowledge the "last version" is the one available on the Windows 95 installation disc under the `OTHER\OLDMSDOS` folder.

## Setup/Install QBasic in DosBox

### Windows

I typically install DosBox into `C:\DosBox`. In this folder, I create a `C-Drive` subfolder that will represent the `C:` drive within the DosBox environment. In this folder, like the original MS-DOS installation, I create a `DOS` folder where I will then place the `QBASIC.EXE` and `QBASIC.HLP` files.

```txt
C:
 : DOSBOX
   : C-Drive
     : AUTOEXEC.BAT
     : DOS
       : QBASIC.EXE
       : QBASIC.HLP
```

I then modify the default DosBox `.conf` (found by launching the *DosBox Options* shortcut) by adding the following to the end.

```bat
@ECHO OFF
ECHO MOUNT C C:\DOSBOX\C-DRIVE
MOUNT C C:\DosBox\C-Drive
C:
C:\AUTOEXEC.BAT
```

Create/modify an `AUTOEXEC.BAT` file (located in `C:\DosBox\C-Drive`):

```bat
@ECHO OFF
SET PATH=Z:\;C:\DOS;
CLS
ECHO Microsoft MS-DOS [Version 5.00 (DosBox 0.74)]
ECHO (c) 2019 DosBox. All rights reserved.
```

At this point you can start DosBox and should now have a `C:\>` prompt instead of the default of `Z:`.

To start QBasic, type `qbasic` and press enter.

If you wold like to start QBasic automatically when launching DosBox you can modify the `AUTOEXEC.BAT` file to add `qbasic.exe` as the last line.

### Linux

*Coming soon...*

## Of Interest

### Serial Ports

As it turns out it does appear that it is possible to use the hosts machines real serial ports from within the DosBox instance. *(Please note that I haven't actually tested this as yet, so will update this once I do.)*

The default configuration is:

```txt
serial1=dummy
serial2=dummy
serial3=disabled
serial4=disabled
```

To use the physical serial port, modify the configuration:

```txt
serial1=directserial realport:com1
serial2=dummy
serial3=disabled
serial4=disabled
```

For more information, [check here](https://www.dosbox.com/wiki/Configuration:SerialPort).
