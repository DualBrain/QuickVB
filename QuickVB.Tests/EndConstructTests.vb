Option Explicit On
Option Strict On
Option Infer On

Imports Xunit

Public Class EndConstructTests

  <Theory>
  <InlineData("module program", "End Module")>
  <InlineData("Class C", "End Class")>
  <InlineData("Structure S", "End Structure")>
  <InlineData("Interface I", "End Interface")>
  <InlineData("Enum E", "End Enum")>
  <InlineData("Namespace N", "End Namespace")>
  <InlineData("sub Main()", "End Sub")>
  <InlineData("Function F() As Integer", "End Function")>
  <InlineData("Sub New()", "End Sub")>
  Public Sub ABlockHeaderWithoutAnEndAsksForOne(header As String, expected As String)
    Assert.Equal(expected, EndConstruct.MissingEnd(header & vbCrLf, 0))
  End Sub

  <Theory>
  <InlineData("If x > 1 Then", "End If")>
  <InlineData("For i = 1 To 3", "Next")>
  <InlineData("For Each a In b", "Next")>
  <InlineData("While x", "End While")>
  <InlineData("Do", "Loop")>
  <InlineData("Select Case x", "End Select")>
  <InlineData("Try", "End Try")>
  <InlineData("With x", "End With")>
  <InlineData("SyncLock x", "End SyncLock")>
  <InlineData("Using f = New Object()", "End Using")>
  Public Sub AStatementBlockInAProcedureAsksForItsEnd(header As String, expected As String)
    Assert.Equal(expected, EndConstruct.MissingEnd("Module M" & vbCrLf & "  Sub S()" & vbCrLf & "    " & header & vbCrLf & "  End Sub" & vbCrLf & "End Module", 2))
  End Sub

  <Fact>
  Public Sub AnAutoPropertyNeedsNoEnd()
    Assert.Null(EndConstruct.MissingEnd("Class C" & vbCrLf & "  Property P As Integer" & vbCrLf & "End Class", 1))
  End Sub

  <Fact>
  Public Sub ABlockThatIsClosedAsksForNothing()
    Assert.Null(EndConstruct.MissingEnd("Module M" & vbCrLf & vbCrLf & "End Module", 0))
    Assert.Null(EndConstruct.MissingEnd("Module M" & vbCrLf & "  Sub S()" & vbCrLf & "    If x Then" & vbCrLf & "    End If" & vbCrLf & "  End Sub" & vbCrLf & "End Module", 2))
  End Sub

  <Fact>
  Public Sub ALineThatIsNotABlockHeaderAsksForNothing()
    Assert.Null(EndConstruct.MissingEnd("Dim x = 1" & vbCrLf, 0))
    Assert.Null(EndConstruct.MissingEnd("If x Then y = 1" & vbCrLf, 0))   ' one line
    Assert.Null(EndConstruct.MissingEnd("' Module M" & vbCrLf, 0))
  End Sub

  <Fact>
  Public Sub ABlockInsideAnotherIsClosedWhileTheOuterOneIsLeftAlone()
    Dim text = "Module M" & vbCrLf & "  Sub S()" & vbCrLf & "    If x Then" & vbCrLf & "  End Sub" & vbCrLf & "End Module"

    Assert.Equal("End If", EndConstruct.MissingEnd(text, 2))
    Assert.Null(EndConstruct.MissingEnd(text, 1))
  End Sub

End Class
