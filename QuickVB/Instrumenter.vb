Option Explicit On
Option Strict On
Option Infer On

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

''' <summary>
''' Prepares a program for the debugger: in front of every statement it adds a call that stops the program when a breakpoint or a step says so,
''' and it counts how deeply procedures are nested so that Procedure Step can skip a call. The program the user sees is not changed.
''' </summary>
Public NotInheritable Class Instrumenter
  Inherits VisualBasicSyntaxRewriter

  Private ReadOnly _model As SemanticModel
  Private ReadOnly _tree As SyntaxTree
  Private ReadOnly _file As Integer
  Private ReadOnly _captureVariables As Boolean

  Private Sub New(model As SemanticModel, tree As SyntaxTree, file As Integer, captureVariables As Boolean)
    MyBase.New(False)
    Me._model = model
    Me._tree = tree
    Me._file = file
    Me._captureVariables = captureVariables
  End Sub

  ''' <summary>
  ''' The compilation with the debugger's calls added to every file. <paramref name="fileNames"/> receives the names of the files in the
  ''' order the calls number them (the first argument of the calls).
  ''' </summary>
  Public Shared Function Instrument(compilation As Compilation, captureVariables As Boolean, fileNames As List(Of String)) As Compilation

    Dim result = compilation
    fileNames.Clear()

    For Each tree In compilation.SyntaxTrees

      Dim file = fileNames.Count
      fileNames.Add(tree.FilePath)

      Dim model = compilation.GetSemanticModel(tree)
      Dim rewriter As New Instrumenter(model, tree, file, captureVariables)
      Dim newRoot = rewriter.Visit(tree.GetRoot())

      result = result.ReplaceSyntaxTree(tree, tree.WithRootAndOptions(newRoot, tree.Options))

    Next

    Return result

  End Function

  ' ---- which statement lists get calls

  Private Shared Function IsExecutableList(parent As SyntaxNode) As Boolean

    If parent Is Nothing Then Return False

    Select Case parent.Kind()
      Case SyntaxKind.SubBlock, SyntaxKind.FunctionBlock,
           SyntaxKind.WhileBlock, SyntaxKind.SimpleDoLoopBlock, SyntaxKind.DoWhileLoopBlock, SyntaxKind.DoUntilLoopBlock,
           SyntaxKind.DoLoopWhileBlock, SyntaxKind.DoLoopUntilBlock,
           SyntaxKind.ForBlock, SyntaxKind.ForEachBlock,
           SyntaxKind.MultiLineIfBlock, SyntaxKind.ElseIfBlock, SyntaxKind.ElseBlock,
           SyntaxKind.CaseBlock, SyntaxKind.CaseElseBlock,
           SyntaxKind.TryBlock, SyntaxKind.CatchBlock, SyntaxKind.FinallyBlock,
           SyntaxKind.UsingBlock, SyntaxKind.SyncLockBlock, SyntaxKind.WithBlock
        Return True
      Case Else
        Return False
    End Select

  End Function

  Private Shared Function IsInsideLambda(node As SyntaxNode) As Boolean
    Return node.Ancestors().Any(Function(a) TypeOf a Is LambdaExpressionSyntax OrElse TypeOf a Is QueryExpressionSyntax)
  End Function

  ''' <summary>A statement that can't have anything in front of it: the call of the base constructor, and labels (a Goto would skip the call anyway).</summary>
  Private Shared Function CannotBePreceded(statement As StatementSyntax) As Boolean

    Select Case statement.Kind()
      Case SyntaxKind.LabelStatement, SyntaxKind.EmptyStatement
        Return True
      Case SyntaxKind.CallStatement, SyntaxKind.ExpressionStatement
        Dim text = statement.ToString().Replace(" ", "")
        Return text.StartsWith("MyBase.New(", StringComparison.OrdinalIgnoreCase) OrElse
               text.StartsWith("MyClass.New(", StringComparison.OrdinalIgnoreCase) OrElse
               text.StartsWith("Me.New(", StringComparison.OrdinalIgnoreCase)
      Case Else
        Return False
    End Select

  End Function

  Public Overrides Function VisitList(Of TNode As SyntaxNode)(list As SyntaxList(Of TNode)) As SyntaxList(Of TNode)

    ' the statements of the lists are rewritten first (so that the blocks inside them get their calls) ...
    Dim visited = MyBase.VisitList(list)

    If list.Count = 0 OrElse Not GetType(StatementSyntax).IsAssignableFrom(GetType(TNode)) Then Return visited

    Dim parent = list(0).Parent
    If Not IsExecutableList(parent) OrElse IsInsideLambda(parent) Then Return visited

    ' ... then the calls go in front of them
    Dim result As New List(Of TNode)(visited.Count * 2)

    For i = 0 To list.Count - 1

      Dim original = DirectCast(CType(list(i), SyntaxNode), StatementSyntax)
      Dim rewritten = visited(i)

      If Not CannotBePreceded(original) Then
        result.Add(DirectCast(CType(Me.CallsFor(original), SyntaxNode), TNode))
      End If

      result.Add(rewritten)

    Next

    Return SyntaxFactory.List(result)

  End Function

  ' ---- the call in front of a statement

  Private Function CallsFor(statement As StatementSyntax) As StatementSyntax

    Dim line = Me._tree.GetLineSpan(statement.Span).StartLinePosition.Line
    Dim names As New List(Of String)()
    Dim values As New List(Of String)()

    If Me._captureVariables Then Me.VariablesInScope(statement, names, values)

    Dim arguments As String

    If names.Count = 0 Then
      arguments = "Nothing, Nothing"
    Else
      arguments = "New String() {" & String.Join(", ", names.Select(Function(n) """" & n & """")) & "}, New Object() {" & String.Join(", ", values) & "}"
    End If

    Dim text = "If QuickBasic.DebugHost.Check(" & Me._file & ", " & line & ") Then QuickBasic.DebugHost.Pause(" & Me._file & ", " & line & ", " & arguments & ")"

    Return ParseStatement(text)

  End Function

  Private Shared ReadOnly Templates As New Dictionary(Of String, StatementSyntax)()

  Private Shared Function ParseStatement(text As String) As StatementSyntax

    Dim parsed As StatementSyntax = Nothing

    If Templates.TryGetValue(text, parsed) Then Return parsed

    Dim tree = SyntaxFactory.ParseSyntaxTree("Module M" & vbCrLf & "Sub S()" & vbCrLf & text & vbCrLf & "End Sub" & vbCrLf & "End Module")
    Dim method = tree.GetRoot().DescendantNodes().OfType(Of MethodBlockSyntax)().First()
    parsed = method.Statements(0)

    ' keep the table from growing without end in a long session
    If Templates.Count > 5000 Then Templates.Clear()
    Templates(text) = parsed

    Return parsed

  End Function

  Private Sub VariablesInScope(statement As StatementSyntax, names As List(Of String), values As List(Of String))

    Dim position = statement.SpanStart
    Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

    For Each symbol In Me._model.LookupSymbols(position)

      If names.Count >= 64 Then Exit For

      Dim name = symbol.Name
      If name.Length = 0 OrElse name.IndexOf("$"c) >= 0 OrElse name.StartsWith("_$", StringComparison.Ordinal) Then Continue For

      Dim typeSymbol As ITypeSymbol = Nothing

      Select Case symbol.Kind

        Case SymbolKind.Local
          Dim local = DirectCast(symbol, ILocalSymbol)
          ' only what has been declared by now
          If local.Locations.Length = 0 OrElse local.Locations(0).SourceTree IsNot Me._tree OrElse local.Locations(0).SourceSpan.End > position Then Continue For
          typeSymbol = local.Type

        Case SymbolKind.Parameter
          typeSymbol = DirectCast(symbol, IParameterSymbol).Type

        Case SymbolKind.Field
          Dim field = DirectCast(symbol, IFieldSymbol)
          ' module level variables; the fields of an object would need the object
          If Not field.IsStatic OrElse field.IsImplicitlyDeclared Then Continue For
          typeSymbol = field.Type

        Case Else
          Continue For

      End Select

      If typeSymbol Is Nothing OrElse typeSymbol.IsRefLikeType OrElse typeSymbol.TypeKind = TypeKind.Pointer OrElse typeSymbol.SpecialType = SpecialType.System_Void Then Continue For
      If Not seen.Add(name) Then Continue For

      Dim identifier = If(SyntaxFacts.GetKeywordKind(name) <> SyntaxKind.None, "[" & name & "]", name)

      names.Add(name)
      values.Add("CObj(" & identifier & ")")

    Next

  End Sub

  ' ---- counting how deeply procedures are nested

  Public Overrides Function VisitMethodBlock(node As MethodBlockSyntax) As SyntaxNode

    Dim visited = DirectCast(MyBase.VisitMethodBlock(node), MethodBlockSyntax)

    Dim kind = node.Kind()
    If kind <> SyntaxKind.SubBlock AndAlso kind <> SyntaxKind.FunctionBlock Then Return visited

    ' iterators and async methods run in pieces; their depth means nothing
    Dim header = node.BlockStatement
    If header.Modifiers.Any(Function(m) m.IsKind(SyntaxKind.IteratorKeyword) OrElse m.IsKind(SyntaxKind.AsyncKeyword)) Then Return visited
    If IsInsideLambda(node) Then Return visited

    Dim procedureName = visited.SubOrFunctionStatement.Identifier.ValueText
    Dim wrapper = DirectCast(ParseStatement("Try" & vbCrLf & "QuickBasic.DebugHost.Enter(""" & procedureName & """)" & vbCrLf & "Finally" & vbCrLf & "QuickBasic.DebugHost.Leave()" & vbCrLf & "End Try"), TryBlockSyntax)

    ' Enter goes first, then the body (with its calls), all inside the Try
    Dim enter = wrapper.Statements(0)
    Dim body = SyntaxFactory.List(Of StatementSyntax)(visited.Statements)
    Dim tryBlock = wrapper.WithStatements(SyntaxFactory.List(Of StatementSyntax)({enter}.Concat(body)))

    Return visited.WithStatements(SyntaxFactory.List(Of StatementSyntax)({CType(tryBlock, StatementSyntax)}))

  End Function

End Class
