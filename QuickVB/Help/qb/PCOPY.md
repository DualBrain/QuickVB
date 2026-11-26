# PCOPY

Copies one video display page to another.

## Syntax

`PCOPY` *source_page*, *target_page*

## Comments

*source_page* is an integer expression that specifies the video display page to be copied.

*target_page* is an integer expression that specifies the video display page to which the source page is to be copied.

The number of video display pages available is dependent on the video memory size and video display mode.

## Example

```vb
'Copy video page 1 to page 3
PCOPY 1, 3
```
