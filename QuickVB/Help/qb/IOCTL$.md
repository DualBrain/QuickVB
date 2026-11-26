# IOCTL$

Returns a control string from a device driver.

## Syntax

`IOCTL$`([#]*file_number*)

## Comments

*file_number* is the file number assigned to the device in its [OPEN](OPEN) statement.

The `IOCTL$` function is generally used to get acknowledgement that an [IOCTL](IOCTL) statement succeeded or failed. It is also used to get device information, such as device width after an [IOCTL](IOCTL) statement requests it.

## Example

```vb
'GW is a possible command
'for get device width
OPEN "\DEV\MYLPT" AS#1
IOCTYL#1, "GW"
'Save it in WID
WID=VAL(IOCTL$(#1))
```

## See Also

- [IOCTL](IOCTL)
