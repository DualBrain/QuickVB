# IOCTL

Transmits a device control string to a device driver.

## Syntax

`IOCTL`[#]*file_number*, *control_string*

## Comments

*file_number* is the file number assigned to the device in its [OPEN](OPEN) statement.

*control_string* is a string expression that specifies the command to send to the device.

`IOCTL` commands are generally 2 to 3 characters followed by an optional alphanumeric argument. An `IOCTL` string may be up to 255 bytes long, with commands within the string separated by semicolons.

## Example

If a user had installed a driver to replace LPT1, and that driver was able to set page length (the number of lines to print on a page before issuing a form feed), then the following lines would open the new LPT1 driver and set the page length to 66 lines:

```vb
OPEN "LPT1:" FOR OUTPUT AS #1
IOCTL #1, "PL66"
```

The following statements open LPT1 with an initial page length of 56 lines:

```vb
OPEN "\DEV\LPT1" FOR OUTPUT AS #1
IOCTL #1, "PL56"
```

## See Also

- [IOCTL$](IOCTL$)
