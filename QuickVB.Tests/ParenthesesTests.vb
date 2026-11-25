Option Explicit On
Option Strict On
Option Infer On

Imports Xunit

Public Class ParenthesesTests

  ' the text with the edits for the line applied
  Private Shared Function Fixed(line As String) As String
    Dim text = "Module M" & vbLf & "  Sub S()" & vbLf & line & vbLf & "  End Sub" & vbLf & "End Module"
    Dim lines = text.Split(ControlChars.Lf).ToList()
    For Each edit In Parentheses.Edits(text, 2)
      lines(2) = lines(2).Insert(edit.Item1, edit.Item2)
    Next
    Return lines(2)
  End Function

  <Theory>
  <InlineData("    PRINT ""hello world""", "    PRINT (""hello world"")")>
  <InlineData("    LOCATE 1, 1", "    LOCATE (1, 1)")>
  <InlineData("    COLOR 15, 0 ' bright", "    COLOR (15, 0) ' bright")>
  <InlineData("    Foo 1, x + 2", "    Foo (1, x + 2)")>
  Public Sub TheArgumentsOfACallWithoutParenthesesAreEnclosed(line As String, expected As String)
    Assert.Equal(expected, Fixed(line))
  End Sub

  <Theory>
  <InlineData("    PRINT(""hello world"")")>
  <InlineData("    CLS")>
  <InlineData("    Dim x = 1")>
  <InlineData("    PRINT ""a""; ""b""")>
  <InlineData("    ' PRINT ""a""")>
  Public Sub AnythingElseIsLeftAlone(line As String)
    Assert.Equal(line, Fixed(line))
  End Sub

End Class
