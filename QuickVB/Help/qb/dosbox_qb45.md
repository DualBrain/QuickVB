# DOSBox (QB45)

QBasic v1.0 started life as a *trimmed down* version of Microsoft QuickBASIC v4.5 (QB45). From what I understand, all of the *compiler* bits were removed and *something* was done with the interpreter to so that it would, under some circumstances, run a bit slower than the interpreter that was included as part of QB45.

So why am I bringing up Microsoft QuickBASIC v4.5 here?

There are several after-market extensions (libraries, IDE enhancements) made available for QB45 that really raised the bar as to what one could do with BASIC on the MS-DOS platform. I'm exploring incorporating some of these enhancements, so here are the instructions of how I configured QB45 to run in [DOSBox](https://www.dosbox.com/) along with installation/configuration of some of the enhancements.

## Get DOSBox

Download [DosBox](https://www.dosbox.com/).

## Get QB45

I, of course, have my original copy of both QB45 and Microsoft Professional Development System BASIC v7.1. With that said, where might you find a copy?

Unfortunately, Microsoft QuickBASIC v4.5 has been long pulled from the market by Microsoft, so your options are as follows...

- Scour the web for a used copy of QB45; however, probably not an option given that the prices I've seen as of January 2025 is somewhere between $300-$1499! There are also diskette only versions for sale... not originals, but copies... for $50!!!!
- At one point the QB45 binaries were available through the MSDN *subscription*; not sure if this is the case as it has been ages since I've had access to this. If you have any old copies of the CD-ROMs/DVDs for MSDN, you might find QB45 on them.
- Before I mention the next option, I'd like to provide a word of warning... you can indeed find many web sites offering downloads of QB45; however, many of these have clear signs that they've been *infected* with extreme advertising behavior and no telling who is actually behind these. With that said...
- *At your risk*, you can also download from [WinWorld](https://winworldpc.com/product/quickbasic/45). If were going to go this route, this *might* be the safest.

## Setup/Install

### Windows

#### DOSBox

The first step is to [install DOSBox](https://www.dosbox.com/wiki/Basic_Setup_and_Installation_of_DosBox). This guide will use the [0.74](https://www.dosbox.com/wiki/Version0.74) Windows 32-bit version which is available for [download](http://www.dosbox.com/download.php?main=1). Download the [release](https://www.dosbox.com/wiki/Releases) for your operating system. If you are a Windows user, get the **Win32 installer**.

After downloading, install [DOSBox](https://www.dosbox.com) to any directory. I put [DOSBox](https://www.dosbox.com) in `C:\DOSBOX`. I also create a sub-folder to represent the *hard drive* in `C:\DOSBOX\C-Drive`. At this point, I also create an additional folder to hold the QB45 installation files (see below).

Your directories should look like this:

```txt
C:
 : DOSBOX
   : C-Drive
     : DOS
   : QB45_INSTALL
```

Once you've installed [DOSBox](https://www.dosbox.com) into `C:\DOSBOX`. In this folder, I create a `C-Drive` sub-folder that will represent the `C:` drive within the [DOSBox](https://www.dosbox.com) environment. I then create an `AUTOEXEC.BAT` file in order to have more of that *MS-DOS experience*.

I then modify the default [DOSBox](https://www.dosbox.com) `.conf` (found by launching the *DosBox Options* shortcut) by adding the following to the end.

```bat
MOUNT C C:\DOSBOX\C-Drive
C:
C:\AUTOEXEC.BAT
```

Create/ an `AUTOEXEC.BAT` file (located in `C:\DOSBOX\C-Drive`) containing the following:

```bat
@ECHO OFF
SET PATH=Z:\;C:\DOS;C:\QB45;
SET LIB=C:\QB45\LIB;
SET INCLUDE=C:\QB45\INCLUDE;
SET HELPFILES=C:\QB45\HELP
CLS
```

At this point you can start [DOSBox](https://www.dosbox.com) and should now have a `C:\>` prompt instead of the default of `Z:`.

Another small adjustment I tend to make is modifying the shortcut that launches [DOSBox](https://www.dosbox.com) by adding `-noconsole` to then end of the `Target:` value.

#### QB45

I'm sure there's probably some method to loading floppy images and swapping them while inside of [DOSBox](https://www.dosbox.com)... but I couldn't bothered to figure that out since I already knew that you could simply copy all of the files to a single folder and then install from that copy. With that said, there are couple fo gotchas; but these are the steps I use.

I create a `C:\DOSBOX\QB45_INSTALL` folder and `xcopy` all of the files from every diskette to this folder (including sub-folders).

Once you've copied the files to this folder, we will *mount* it as if it were a floppy diskette within [DOSBox](https://www.dosbox.com). To so so, at the `C:\>` prompt, type `MOUNT A C:\DOSBOX\QB45_INSTALL` and press enter. Then type `A:` to switch to the "A: drive". Once at the `A:\>` prompt, type `SETUP` and press enter.

At this point you will need to go through the setup process as provide by the *Microsoft (R) QuickBASIC 4.50 Setup Program*.

- Press `C` to continue.
- I tend to use the "Full Setup"... so press `F`.
- Press `D`...
  - (Press `Tab` to switch between the entry field and menu items.)
  - Set EXE Path: `C:\QB45` and then `Continue`...
  - Set INCLUDE Path: `C:\QB45\INCLUDE` and then `Continue`...
  - Set LIB Path: `C:\QB45\LIB` and then `Continue`...
  - Set HELP Path: `C:\QB45\HELP` and then `Return to Full Setup Menu`
- Press `C`...
  - I tend to leave the colors as default, press `C` to continue.
  - I then modify the following on the *Display Options*...
    - Set Menus: to `Full`.
    - Set Tab Width: to `2`.
  - `Return to Full Setup Menu`.
- Press `S` to review all of the options you've chosen...
  - Press `C`, `C`, `C` and the `R` to `Return to Full Setup Menu`.
- Press `I` to `Install QuickBASIC With Current Options`.

At this point you will be prompted to `Insert the disk`... press `C` to continue as the files it is looking for are (should) already be on the "A: drive". You will also be prompted to `Create the Path`... again, press `C` to continue. You will repeat press the `C` key until you get to the `QuickBASIC Successfully Installed` screen. On this screen there is the following:

```txt
Microsoft QuickBASIC 4.5 is now installed on drive C: in directory
`QB45.

To enter QuickBASIC from DOS, type "QB" from the
C:\QB45 directory.

For best results, make sure this directory is in your PATH environment
variable defined in AUTOEXEC.BAT. See your DOS manual for details.
```

We've already taken care of this in a previous step (above).

Exit the installation and then close [DOSBox](https://www.dosbox.com) by typing `EXIT`.

Launch [DOSBox](https://www.dosbox.com) and you should be presented with a `C:\>` prompt. Type `QB` and press enter.

## More

### QBASIC and EDIT

Again, for that genuine MS-DOS *feel*, let's get a copy of `EDIT` included.

If you have a copy of the MS-DOS 5.0+ diskettes or, possibly, the Windows 95 ISO, you can copy `QBASIC.EXE`, `QBASIC.HLP`, `EDIT.COM` and `EDIT.HLP` files to the `C:\DOSBOX\C-Drive\DOS` folder. While you are at it, feel free to copy `GORILLA.BAS`, `MONEY.BAS`, `NIBBLES.BAS` and `REMLINE.BAS` while you are at it.

Although I'm mainly doing this for `EDIT`, I tend to add these others as, for example, `REMLINE.BAS` is kind of handy to have.

### GW-BASIC

I also tend to copy all of the PC-DOS v1.0 BASIC samples (23 of them) and Microsoft GW-BASIC v3.23 (`GWBASIC.EXE`) to the `C:\DOS` folder. These would have originally been saved in the *binary* format; I open each one of these and save them to ASCII using the `SAVE"*filename*",A` command so that I can then open them in QB45 (or `EDIT.COM`).

## Of Interest

*Coming soon...*
