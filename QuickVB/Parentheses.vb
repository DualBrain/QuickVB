Option Explicit On
Option Strict On
Option Infer On

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.VisualBasic

''' <summary>
''' QuickBASIC calls a statement without parentheses (<c>PRINT "hello world"</c>, <c>LOCATE 1, 1</c>); Visual Basic wants them
''' (BC30800, "Method arguments must be enclosed in parentheses"). When a line is left, the parentheses are written for it.
''' </summary>
Public Module Parentheses

  ''' <summary>
  ''' What to insert into line <paramref name="row"/> (zero based) of <paramref name="text"/> to put its call arguments in parentheses:
  ''' (column, text) pairs, the rightmost first, so they can be applied one after the other. Empty when the line needs none, or when what
  ''' follows the name is not an argument list (<c>PRINT "a"; b</c>), which is left for the compiler to complain about.
  ''' </summary>
  Public Function Edits(text As String, row As Integer) As List(Of Tuple(Of Integer, String))

    Dim result As New List(Of Tuple(Of Integer, String))()
    Dim tree = VisualBasicSyntaxTree.ParseText(text)

    For Each diagnostic In tree.GetDiagnostics()

      If diagnostic.Id <> "BC30800" Then Continue For

      Dim span = tree.GetLineSpan(diagnostic.Location.SourceSpan)
      If span.StartLinePosition.Line <> row OrElse span.EndLinePosition.Line <> row Then Continue For

      Dim arguments = tree.GetText().ToString(diagnostic.Location.SourceSpan)
      If Not IsArgumentList(arguments) Then Continue For

      ' the list ends where its text does, not at the blanks in front of a comment
      Dim endColumn = span.StartLinePosition.Character + arguments.TrimEnd().Length

      result.Add(Tuple.Create(span.StartLinePosition.Character, "("))
      result.Add(Tuple.Create(endColumn, ")"))

    Next

    ' rightmost first (a "(" and its ")" are never at the same column unless the list is empty, which has no diagnostic)
    Return result.OrderByDescending(Function(e) e.Item1).ToList()

  End Function

  ' true when "F(arguments)" is a call the compiler accepts
  Private Function IsArgumentList(arguments As String) As Boolean

    If arguments.Trim().Length = 0 Then Return False

    Dim probe = VisualBasicSyntaxTree.ParseText("Module M" & vbLf & "Sub S()" & vbLf & "F(" & arguments & ")" & vbLf & "End Sub" & vbLf & "End Module")
    Return Not probe.GetDiagnostics().Any(Function(d) d.Severity = DiagnosticSeverity.Error)

  End Function

End Module
