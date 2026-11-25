Option Explicit On
Option Strict On
Option Infer On

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

''' <summary>
''' The casing a line gets when it is committed, as QuickBASIC's did: keywords are written the way the language writes them
''' (<c>module</c> becomes <c>Module</c>) and a name takes the casing it was declared with (<c>print</c> becomes <c>PRINT</c>).
''' </summary>
Public Module Casing

  ''' <summary>One name to rewrite: the position (zero based line and column), what is there and what should be.</summary>
  Public NotInheritable Class NameFix
    Public Property Line As Integer
    Public Property Column As Integer
    Public Property Old As String
    Public Property [New] As String
  End Class

  ''' <summary>The changes that put every keyword of a tree in its proper case.</summary>
  Public Function KeywordChanges(root As SyntaxNode) As List(Of TextChange)

    Dim changes As New List(Of TextChange)()

    For Each token In root.DescendantTokens()

      If token.IsMissing OrElse Not SyntaxFacts.IsKeywordKind(token.Kind()) Then Continue For

      Dim proper = SyntaxFacts.GetText(token.Kind())
      If String.IsNullOrEmpty(proper) Then Continue For

      If String.Equals(proper, token.Text, StringComparison.OrdinalIgnoreCase) AndAlso Not String.Equals(proper, token.Text, StringComparison.Ordinal) Then
        changes.Add(New TextChange(token.Span, proper))
      End If

    Next

    Return changes

  End Function

  ''' <summary>
  ''' The names a compilation uses with other casing than they are declared with (a variable, a procedure, a QuickBASIC statement of the
  ''' library), by file path.
  ''' </summary>
  Public Function NameFixes(compilation As Compilation) As Dictionary(Of String, List(Of NameFix))

    Dim result As New Dictionary(Of String, List(Of NameFix))(StringComparer.OrdinalIgnoreCase)

    Dim keywords = QBLibrary.Keywords(compilation)

    For Each tree In compilation.SyntaxTrees

      Dim model = compilation.GetSemanticModel(tree)
      Dim fixes As New List(Of NameFix)()

      For Each node In tree.GetRoot().DescendantNodes().OfType(Of IdentifierNameSyntax)()

        Dim identifier = node.Identifier
        If identifier.IsMissing OrElse identifier.Text.StartsWith("[", StringComparison.Ordinal) Then Continue For

        Dim info = model.GetSymbolInfo(node)
        Dim symbol = info.Symbol
        If symbol Is Nothing Then
          ' a call that didn't resolve: only when every candidate is called the same (Print of Microsoft.VisualBasic is not PRINT of QB)
          Dim names = info.CandidateSymbols.Select(Function(c) c.Name).Distinct(StringComparer.Ordinal).ToList()
          If names.Count = 1 Then symbol = info.CandidateSymbols(0)
        End If
        If symbol Is Nothing OrElse symbol.IsImplicitlyDeclared Then Continue For
        If symbol.Kind = SymbolKind.Local AndAlso symbol.DeclaringSyntaxReferences.Length = 0 Then Continue For   ' an implicit variable

        ' PRINT before QB.Video is imported is the Print of Microsoft.VisualBasic: not to be written like that (the import is coming)
        If symbol.ContainingAssembly IsNot Nothing AndAlso symbol.ContainingAssembly.Name.StartsWith("Microsoft.VisualBasic", StringComparison.Ordinal) AndAlso keywords.ContainsKey(symbol.Name) Then Continue For

        Dim name = symbol.Name
        If String.IsNullOrEmpty(name) OrElse
           Not String.Equals(name, identifier.ValueText, StringComparison.OrdinalIgnoreCase) OrElse
           String.Equals(name, identifier.ValueText, StringComparison.Ordinal) Then Continue For

        Dim position = tree.GetLineSpan(identifier.Span).StartLinePosition
        fixes.Add(New NameFix With {.Line = position.Line, .Column = position.Character, .Old = identifier.ValueText, .[New] = name})

      Next

      If fixes.Count > 0 Then result(tree.FilePath) = fixes

    Next

    Return result

  End Function

End Module
