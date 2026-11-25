Option Explicit On
Option Strict On
Option Infer On

Imports System.Globalization
Imports System.Text

''' <summary>Why an expression of a watch could not be evaluated ("Variable not defined", "Type mismatch" ...).</summary>
Public Class EvaluationException
  Inherits Exception

  Public Sub New(message As String)
    MyBase.New(message)
  End Sub

End Class

''' <summary>
''' Evaluates the expressions of Watch, Watchpoint and Instant Watch against the variables of a program that is stopped.
''' Values are Long, Double, String, Boolean, Nothing or an array of those. The language is a small part of BASIC: numbers, strings,
''' the usual operators (including AND, OR, XOR, NOT, MOD, \ and &amp;), array elements, and a few functions.
''' </summary>
Public NotInheritable Class ExpressionEvaluator

  Private ReadOnly _tokens As List(Of Token)
  Private ReadOnly _lookup As Func(Of String, Tuple(Of Boolean, Object))
  Private _position As Integer

  Private Enum TokenKind
    Number
    Text
    Name
    Symbol
    EndOfText
  End Enum

  Private NotInheritable Class Token
    Public Kind As TokenKind
    Public Text As String
    Public Value As Object
  End Class

  Private Sub New(tokens As List(Of Token), lookup As Func(Of String, Tuple(Of Boolean, Object)))
    Me._tokens = tokens
    Me._lookup = lookup
  End Sub

  ''' <summary>Evaluates <paramref name="expression"/>; <paramref name="lookup"/> returns (found, value) for a variable name.</summary>
  Public Shared Function Evaluate(expression As String, lookup As Func(Of String, Tuple(Of Boolean, Object))) As Object

    Dim evaluator As New ExpressionEvaluator(Tokenize(expression), lookup)
    Dim result = evaluator.ParseOr()

    If evaluator.Current.Kind <> TokenKind.EndOfText Then Throw New EvaluationException("Syntax error")

    Return result

  End Function

  ''' <summary>The value as a watch window shows it.</summary>
  Public Shared Function Format(value As Object) As String

    If value Is Nothing Then Return "Nothing"

    Dim list = TryCast(value, Object())
    If list IsNot Nothing Then
      Dim shown = list.Take(10).Select(Function(v) Format(v)).ToList()
      Return "{" & String.Join(", ", shown) & If(list.Length > 10, ", ...", "") & "}"
    End If

    If TypeOf value Is String Then Return """" & CStr(value) & """"
    If TypeOf value Is Boolean Then Return If(CBool(value), "True", "False")
    If TypeOf value Is Double Then Return FormatDouble(CDbl(value))

    Return Convert.ToString(value, CultureInfo.InvariantCulture)

  End Function

  Private Shared Function FormatDouble(value As Double) As String
    Return value.ToString("G15", CultureInfo.InvariantCulture)
  End Function

  ' ---- tokens

  Private Shared Function Tokenize(text As String) As List(Of Token)

    Dim tokens As New List(Of Token)()
    Dim i = 0

    Do While i < text.Length

      Dim c = text(i)

      If Char.IsWhiteSpace(c) Then
        i += 1

      ElseIf Char.IsDigit(c) OrElse (c = "."c AndAlso i + 1 < text.Length AndAlso Char.IsDigit(text(i + 1))) Then
        Dim start = i
        Dim isReal = False
        Do While i < text.Length AndAlso (Char.IsDigit(text(i)) OrElse text(i) = "."c)
          If text(i) = "."c Then isReal = True
          i += 1
        Loop
        Dim literal = text.Substring(start, i - start)
        Dim number As Object
        Dim asLong As Long
        If Not isReal AndAlso Long.TryParse(literal, NumberStyles.None, CultureInfo.InvariantCulture, asLong) Then
          number = asLong
        Else
          Dim asDouble As Double
          If Not Double.TryParse(literal, NumberStyles.Float, CultureInfo.InvariantCulture, asDouble) Then Throw New EvaluationException("Syntax error")
          number = asDouble
        End If
        tokens.Add(New Token With {.Kind = TokenKind.Number, .Value = number, .Text = literal})

      ElseIf c = """"c Then
        Dim builder As New StringBuilder()
        i += 1
        Do
          If i >= text.Length Then Throw New EvaluationException("Syntax error")
          If text(i) = """"c Then
            If i + 1 < text.Length AndAlso text(i + 1) = """"c Then
              builder.Append(""""c)
              i += 2
              Continue Do
            End If
            i += 1
            Exit Do
          End If
          builder.Append(text(i))
          i += 1
        Loop
        tokens.Add(New Token With {.Kind = TokenKind.Text, .Value = builder.ToString(), .Text = builder.ToString()})

      ElseIf Char.IsLetter(c) OrElse c = "_"c Then
        Dim start = i
        Do While i < text.Length AndAlso (Char.IsLetterOrDigit(text(i)) OrElse text(i) = "_"c)
          i += 1
        Loop
        ' a QuickBASIC type character stays with the name
        If i < text.Length AndAlso "$%&!#".IndexOf(text(i)) >= 0 AndAlso Not (text(i) = "&"c AndAlso i + 1 < text.Length AndAlso text(i + 1) = "h"c) Then i += 1
        tokens.Add(New Token With {.Kind = TokenKind.Name, .Text = text.Substring(start, i - start)})

      Else
        Dim two = If(i + 1 < text.Length, text.Substring(i, 2), "")
        If two = "<=" OrElse two = ">=" OrElse two = "<>" Then
          tokens.Add(New Token With {.Kind = TokenKind.Symbol, .Text = two})
          i += 2
        ElseIf "+-*/\^&=<>().,".IndexOf(c) >= 0 Then
          tokens.Add(New Token With {.Kind = TokenKind.Symbol, .Text = c.ToString()})
          i += 1
        Else
          Throw New EvaluationException("Syntax error")
        End If
      End If

    Loop

    tokens.Add(New Token With {.Kind = TokenKind.EndOfText, .Text = ""})

    Return tokens

  End Function

  Private ReadOnly Property Current As Token
    Get
      Return Me._tokens(Me._position)
    End Get
  End Property

  Private Function IsSymbol(text As String) As Boolean
    Return Me.Current.Kind = TokenKind.Symbol AndAlso Me.Current.Text = text
  End Function

  Private Function IsWord(text As String) As Boolean
    Return Me.Current.Kind = TokenKind.Name AndAlso String.Equals(Me.Current.Text, text, StringComparison.OrdinalIgnoreCase)
  End Function

  ' ---- grammar, lowest precedence first

  Private Function ParseOr() As Object

    Dim left = Me.ParseAnd()

    Do
      If Me.IsWord("Or") OrElse Me.IsWord("OrElse") Then
        Me._position += 1
        left = Logical(left, Me.ParseAnd(), "or")
      ElseIf Me.IsWord("Xor") Then
        Me._position += 1
        left = Logical(left, Me.ParseAnd(), "xor")
      Else
        Return left
      End If
    Loop

  End Function

  Private Function ParseAnd() As Object

    Dim left = Me.ParseNot()

    Do While Me.IsWord("And") OrElse Me.IsWord("AndAlso")
      Me._position += 1
      left = Logical(left, Me.ParseNot(), "and")
    Loop

    Return left

  End Function

  Private Function ParseNot() As Object

    If Me.IsWord("Not") Then
      Me._position += 1
      Dim operand = Me.ParseNot()
      If TypeOf operand Is Boolean Then Return Not CBool(operand)
      Return Not ToLong(operand)
    End If

    Return Me.ParseComparison()

  End Function

  Private Function ParseComparison() As Object

    Dim left = Me.ParseConcatenation()

    Do While Me.Current.Kind = TokenKind.Symbol AndAlso (Me.Current.Text = "=" OrElse Me.Current.Text = "<>" OrElse Me.Current.Text = "<" OrElse
                                                         Me.Current.Text = ">" OrElse Me.Current.Text = "<=" OrElse Me.Current.Text = ">=")
      Dim op = Me.Current.Text
      Me._position += 1
      left = Compare(left, Me.ParseConcatenation(), op)
    Loop

    Return left

  End Function

  Private Function ParseConcatenation() As Object

    Dim left = Me.ParseAdditive()

    Do While Me.IsSymbol("&")
      Me._position += 1
      left = ToText(left) & ToText(Me.ParseAdditive())
    Loop

    Return left

  End Function

  Private Function ParseAdditive() As Object

    Dim left = Me.ParseModulo()

    Do While Me.IsSymbol("+") OrElse Me.IsSymbol("-")
      Dim op = Me.Current.Text
      Me._position += 1
      Dim right = Me.ParseModulo()
      If op = "+" AndAlso TypeOf left Is String AndAlso TypeOf right Is String Then
        left = CStr(left) & CStr(right)
      Else
        left = Arithmetic(left, right, op)
      End If
    Loop

    Return left

  End Function

  Private Function ParseModulo() As Object

    Dim left = Me.ParseIntegerDivision()

    Do While Me.IsWord("Mod")
      Me._position += 1
      left = Arithmetic(left, Me.ParseIntegerDivision(), "mod")
    Loop

    Return left

  End Function

  Private Function ParseIntegerDivision() As Object

    Dim left = Me.ParseMultiplicative()

    Do While Me.IsSymbol("\")
      Me._position += 1
      left = Arithmetic(left, Me.ParseMultiplicative(), "\")
    Loop

    Return left

  End Function

  Private Function ParseMultiplicative() As Object

    Dim left = Me.ParseUnary()

    Do While Me.IsSymbol("*") OrElse Me.IsSymbol("/")
      Dim op = Me.Current.Text
      Me._position += 1
      left = Arithmetic(left, Me.ParseUnary(), op)
    Loop

    Return left

  End Function

  Private Function ParseUnary() As Object

    If Me.IsSymbol("-") Then
      Me._position += 1
      Dim operand = Me.ParseUnary()
      If TypeOf operand Is Long Then Return -CLng(operand)
      Return -ToDouble(operand)
    End If

    If Me.IsSymbol("+") Then
      Me._position += 1
      Return Me.ParseUnary()
    End If

    Return Me.ParsePower()

  End Function

  Private Function ParsePower() As Object

    Dim left = Me.ParsePrimary()

    Do While Me.IsSymbol("^")
      Me._position += 1
      left = Arithmetic(left, Me.ParsePrimary(), "^")
    Loop

    Return left

  End Function

  Private Function ParsePrimary() As Object

    Dim token = Me.Current

    Select Case token.Kind

      Case TokenKind.Number, TokenKind.Text
        Me._position += 1
        Return token.Value

      Case TokenKind.Symbol
        If token.Text = "(" Then
          Me._position += 1
          Dim inner = Me.ParseOr()
          Me.Expect(")")
          Return Me.ParsePostfix(inner)
        End If
        Throw New EvaluationException("Syntax error")

      Case TokenKind.Name
        Me._position += 1
        Return Me.ParsePostfix(Me.ParseName(token.Text))

      Case Else
        Throw New EvaluationException("Syntax error")

    End Select

  End Function

  Private Sub Expect(symbol As String)
    If Not Me.IsSymbol(symbol) Then Throw New EvaluationException("Syntax error")
    Me._position += 1
  End Sub

  Private Function ParseArguments() As List(Of Object)
    Dim arguments As New List(Of Object)()
    Me.Expect("(")
    If Not Me.IsSymbol(")") Then
      Do
        arguments.Add(Me.ParseOr())
        If Me.IsSymbol(",") Then
          Me._position += 1
        Else
          Exit Do
        End If
      Loop
    End If
    Me.Expect(")")
    Return arguments
  End Function

  ' a name is a constant, a function call, an array element or a variable
  Private Function ParseName(name As String) As Object

    Select Case name.ToUpperInvariant()
      Case "TRUE" : Return True
      Case "FALSE" : Return False
      Case "NOTHING" : Return Nothing
    End Select

    Dim found = Me._lookup(name)

    If Me.IsSymbol("(") Then

      If found.Item1 Then
        Dim list = TryCast(found.Item2, Object())
        If list Is Nothing Then Throw New EvaluationException("Type mismatch")
        Dim indexes = Me.ParseArguments()
        If indexes.Count <> 1 Then Throw New EvaluationException("Wrong number of dimensions")
        Dim index = ToLong(indexes(0))
        If index < 0 OrElse index >= list.Length Then Throw New EvaluationException("Subscript out of range")
        Return list(CInt(index))
      End If

      Return Me.CallFunction(name, Me.ParseArguments())

    End If

    If Not found.Item1 Then Throw New EvaluationException("Variable not defined")

    Return found.Item2

  End Function

  Private Function ParsePostfix(value As Object) As Object

    Do While Me.IsSymbol(".")
      Me._position += 1
      If Me.Current.Kind <> TokenKind.Name Then Throw New EvaluationException("Syntax error")
      Dim member = Me.Current.Text.ToUpperInvariant()
      Me._position += 1
      Select Case member
        Case "LENGTH", "COUNT"
          If TypeOf value Is String Then
            value = CLng(CStr(value).Length)
          ElseIf TypeOf value Is Object() Then
            value = CLng(DirectCast(value, Object()).Length)
          Else
            Throw New EvaluationException("Type mismatch")
          End If
        Case Else
          Throw New EvaluationException("Variable not defined")
      End Select
    Loop

    Return value

  End Function

  Private Function CallFunction(name As String, arguments As List(Of Object)) As Object

    Dim upper = name.ToUpperInvariant().TrimEnd("$"c)

    Select Case upper

      Case "LEN"
        Return CLng(ToText(Arg(arguments, 0, 1)).Length)
      Case "ABS"
        Dim v = Arg(arguments, 0, 1)
        Return If(TypeOf v Is Long, CObj(Math.Abs(CLng(v))), CObj(Math.Abs(ToDouble(v))))
      Case "INT"
        Return CLng(Math.Floor(ToDouble(Arg(arguments, 0, 1))))
      Case "SGN"
        Return CLng(Math.Sign(ToDouble(Arg(arguments, 0, 1))))
      Case "SQR"
        Return Math.Sqrt(ToDouble(Arg(arguments, 0, 1)))
      Case "UCASE"
        Return ToText(Arg(arguments, 0, 1)).ToUpperInvariant()
      Case "LCASE"
        Return ToText(Arg(arguments, 0, 1)).ToLowerInvariant()
      Case "STR"
        Dim v = Arg(arguments, 0, 1)
        Return If(ToDouble(v) >= 0, " ", "") & ToText(v)
      Case "VAL"
        Dim parsed As Double
        Double.TryParse(ToText(Arg(arguments, 0, 1)).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, parsed)
        Return If(parsed = Math.Floor(parsed) AndAlso Math.Abs(parsed) < 9.0E+18, CObj(CLng(parsed)), CObj(parsed))
      Case "CHR"
        Return ChrW(CInt(ToLong(Arg(arguments, 0, 1)))).ToString()
      Case "ASC"
        Dim text = ToText(Arg(arguments, 0, 1))
        If text.Length = 0 Then Throw New EvaluationException("Illegal function call")
        Return CLng(AscW(text(0)))
      Case "LEFT"
        Dim text = ToText(Arg(arguments, 0, 2))
        Return text.Substring(0, CInt(Math.Max(0, Math.Min(text.Length, ToLong(arguments(1))))))
      Case "RIGHT"
        Dim text = ToText(Arg(arguments, 0, 2))
        Dim count = CInt(Math.Max(0, Math.Min(text.Length, ToLong(arguments(1)))))
        Return text.Substring(text.Length - count)
      Case "MID"
        Dim text = ToText(Arg(arguments, 0, 2))
        Dim start = CInt(ToLong(arguments(1))) - 1
        If start < 0 Then Throw New EvaluationException("Illegal function call")
        If start >= text.Length Then Return ""
        Dim length = If(arguments.Count > 2, CInt(Math.Max(0, ToLong(arguments(2)))), text.Length)
        Return text.Substring(start, Math.Min(length, text.Length - start))

      Case Else
        Throw New EvaluationException("Variable not defined")

    End Select

  End Function

  Private Shared Function Arg(arguments As List(Of Object), index As Integer, minimum As Integer) As Object
    If arguments.Count < minimum Then Throw New EvaluationException("Wrong number of arguments")
    Return arguments(index)
  End Function

  ' ---- operations

  Private Shared Function ToLong(value As Object) As Long

    If TypeOf value Is Long Then Return CLng(value)
    If TypeOf value Is Boolean Then Return If(CBool(value), -1L, 0L)
    If TypeOf value Is Double Then Return CLng(Math.Round(CDbl(value), MidpointRounding.ToEven))

    Throw New EvaluationException("Type mismatch")

  End Function

  Private Shared Function ToDouble(value As Object) As Double

    If TypeOf value Is Long Then Return CLng(value)
    If TypeOf value Is Double Then Return CDbl(value)
    If TypeOf value Is Boolean Then Return If(CBool(value), -1.0, 0.0)

    Throw New EvaluationException("Type mismatch")

  End Function

  Private Shared Function ToText(value As Object) As String

    If value Is Nothing Then Return ""
    If TypeOf value Is String Then Return CStr(value)
    If TypeOf value Is Double Then Return FormatDouble(CDbl(value))
    If TypeOf value Is Boolean Then Return If(CBool(value), "True", "False")
    If TypeOf value Is Object() Then Throw New EvaluationException("Type mismatch")

    Return Convert.ToString(value, CultureInfo.InvariantCulture)

  End Function

  Private Shared Function IsNumber(value As Object) As Boolean
    Return TypeOf value Is Long OrElse TypeOf value Is Double
  End Function

  Private Shared Function Arithmetic(left As Object, right As Object, op As String) As Object

    If TypeOf left Is String OrElse TypeOf right Is String Then Throw New EvaluationException("Type mismatch")

    If TypeOf left Is Long AndAlso TypeOf right Is Long Then

      Dim a = CLng(left)
      Dim b = CLng(right)

      Try
        Select Case op
          Case "+" : Return a + b
          Case "-" : Return a - b
          Case "*" : Return a * b
          Case "\"
            If b = 0 Then Throw New EvaluationException("Division by zero")
            Return a \ b
          Case "mod"
            If b = 0 Then Throw New EvaluationException("Division by zero")
            Return a Mod b
        End Select
      Catch ex As OverflowException
        ' too big for a Long: carry on with Doubles
      End Try

    End If

    Dim x = ToDouble(left)
    Dim y = ToDouble(right)

    Select Case op
      Case "+" : Return x + y
      Case "-" : Return x - y
      Case "*" : Return x * y
      Case "/"
        If y = 0 Then Throw New EvaluationException("Division by zero")
        Return x / y
      Case "^" : Return Math.Pow(x, y)
      Case "\"
        If CLng(y) = 0 Then Throw New EvaluationException("Division by zero")
        Return CLng(x) \ CLng(y)
      Case "mod"
        If CLng(y) = 0 Then Throw New EvaluationException("Division by zero")
        Return CLng(x) Mod CLng(y)
      Case Else
        Throw New EvaluationException("Syntax error")
    End Select

  End Function

  Private Shared Function Logical(left As Object, right As Object, op As String) As Object

    If TypeOf left Is Boolean AndAlso TypeOf right Is Boolean Then
      Dim a = CBool(left)
      Dim b = CBool(right)
      Select Case op
        Case "and" : Return a AndAlso b
        Case "or" : Return a OrElse b
        Case Else : Return a Xor b
      End Select
    End If

    Dim x = ToLong(left)
    Dim y = ToLong(right)

    Select Case op
      Case "and" : Return x And y
      Case "or" : Return x Or y
      Case Else : Return x Xor y
    End Select

  End Function

  Private Shared Function Compare(left As Object, right As Object, op As String) As Object

    Dim order As Integer

    If TypeOf left Is String AndAlso TypeOf right Is String Then
      order = String.CompareOrdinal(CStr(left), CStr(right))
    ElseIf TypeOf left Is Boolean AndAlso TypeOf right Is Boolean Then
      order = CBool(left).CompareTo(CBool(right))
    ElseIf (IsNumber(left) OrElse TypeOf left Is Boolean) AndAlso (IsNumber(right) OrElse TypeOf right Is Boolean) Then
      order = ToDouble(left).CompareTo(ToDouble(right))
    ElseIf left Is Nothing AndAlso right Is Nothing Then
      order = 0
    ElseIf left Is Nothing OrElse right Is Nothing Then
      order = 1
    Else
      Throw New EvaluationException("Type mismatch")
    End If

    Select Case op
      Case "=" : Return order = 0
      Case "<>" : Return order <> 0
      Case "<" : Return order < 0
      Case ">" : Return order > 0
      Case "<=" : Return order <= 0
      Case Else : Return order >= 0
    End Select

  End Function

End Class
