Option Explicit On
Option Strict On
Option Infer On

Imports Xunit

Public Class ExpressionEvaluatorTests

  Private Shared Function Lookup(name As String) As Tuple(Of Boolean, Object)
    Select Case name.ToUpperInvariant()
      Case "X" : Return Tuple.Create(True, CObj(7L))
      Case "Y#" : Return Tuple.Create(True, CObj(2.5))
      Case "NAME$" : Return Tuple.Create(True, CObj("hello"))
      Case "FLAG" : Return Tuple.Create(True, CObj(True))
      Case "A" : Return Tuple.Create(True, CObj(New Object() {10L, 20L, 30L}))
      Case Else : Return Tuple.Create(False, CType(Nothing, Object))
    End Select
  End Function

  Private Shared Function Eval(text As String) As String
    Return ExpressionEvaluator.Format(ExpressionEvaluator.Evaluate(text, AddressOf Lookup))
  End Function

  <Theory>
  <InlineData("x", "7")>
  <InlineData("x + 1", "8")>
  <InlineData("x * 2 + 1", "15")>
  <InlineData("x * (2 + 1)", "21")>
  <InlineData("-x", "-7")>
  <InlineData("x / 2", "3.5")>
  <InlineData("x \ 2", "3")>
  <InlineData("x Mod 4", "3")>
  <InlineData("2 ^ 3 ^ 2", "64")>
  <InlineData("y# * 2", "5")>
  <InlineData("name$", """hello""")>
  <InlineData("name$ + "" there""", """hello there""")>
  <InlineData("name$ & x", """hello7""")>
  <InlineData("x > 5", "True")>
  <InlineData("x = 7 And y# < 3", "True")>
  <InlineData("x <> 7 Or Not flag", "False")>
  <InlineData("name$ = ""hello""", "True")>
  <InlineData("a(1)", "20")>
  <InlineData("a(0) + a(2)", "40")>
  <InlineData("a", "{10, 20, 30}")>
  <InlineData("a.Length", "3")>
  <InlineData("len(name$)", "5")>
  <InlineData("ucase$(name$)", """HELLO""")>
  <InlineData("left$(name$, 2)", """he""")>
  <InlineData("mid$(name$, 2, 3)", """ell""")>
  <InlineData("abs(-4)", "4")>
  <InlineData("5 xor 3", "6")>
  <InlineData("""a"" < ""b""", "True")>
  Public Sub ExpressionsAreEvaluated(expression As String, expected As String)
    Assert.Equal(expected, Eval(expression))
  End Sub

  <Theory>
  <InlineData("nothing_here", "Variable not defined")>
  <InlineData("x +", "Syntax error")>
  <InlineData("name$ - 1", "Type mismatch")>
  <InlineData("x \ 0", "Division by zero")>
  <InlineData("a(5)", "Subscript out of range")>
  <InlineData("""abc", "Syntax error")>
  <InlineData("x > ""a""", "Type mismatch")>
  Public Sub ProblemsAreReportedTheWayBasicDoes(expression As String, message As String)
    Dim ex = Assert.Throws(Of EvaluationException)(Function() ExpressionEvaluator.Evaluate(expression, AddressOf Lookup))
    Assert.Equal(message, ex.Message)
  End Sub

  <Fact>
  Public Sub BigIntegersBecomeDoubles()
    Assert.Equal(Math.Pow(2, 64), CDbl(ExpressionEvaluator.Evaluate("4294967296 * 4294967296", AddressOf Lookup)))
  End Sub

End Class
