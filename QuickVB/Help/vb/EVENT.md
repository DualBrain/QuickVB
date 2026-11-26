# EVENT, RAISEEVENT, HANDLES and ADDHANDLER

<!-- keywords: EVENT, RAISEEVENT, HANDLES, WITHEVENTS, ADDHANDLER, REMOVEHANDLER -->

Lets an object tell others that something happened.

## Syntax

`EVENT` *name*(*parameters*)
`RAISEEVENT` *name*(*arguments*)
`SUB` *name*(*parameters*) `HANDLES` *variable*.*event*
`ADDHANDLER` *variable*.*event*, `ADDRESSOF` *procedure*
`REMOVEHANDLER` *variable*.*event*, `ADDRESSOF` *procedure*

## Comments

`HANDLES` needs a variable declared `WITHEVENTS`; `ADDHANDLER` hooks a procedure up while the program runs.

## Example

```vb
CLASS Clock
  EVENT Tick(count AS INTEGER)
  SUB Run()
    RAISEEVENT Tick(1)
  END SUB
END CLASS
```

## See Also

- [DELEGATE](DELEGATE), [CLASS](CLASS)

## Online

- [Microsoft Learn: Event statement](https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/event-statement)
