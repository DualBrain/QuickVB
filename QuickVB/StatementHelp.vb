Option Explicit On
Option Strict On
Option Infer On

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.VisualBasic

''' <summary>
''' The statements and functions a program can call without declaring them: those of QB.Compatibility (PRINT, LOCATE, ...) and those the Visual Basic
''' runtime has (LEFT, MID, ...), read from the libraries a program is compiled against, so a newer library brings its new statements.
''' This is the data the help topics of <see cref="HelpDocs"/> are made from.
''' </summary>
Public Module StatementHelp

  ''' <summary>Gives the compilation the libraries are read from (any compilation of a program: they all have them).</summary>
  Public Property CompilationSource As Func(Of Compilation)

  Private Const MaxForms As Integer = 30

  ''' <summary>A statement or function: where it comes from and the forms it can be called in.</summary>
  Public NotInheritable Class Entry
    Public Name As String
    ''' <summary>The namespace to import to use it (QB.Video), or Microsoft.VisualBasic for what Visual Basic has built in.</summary>
    Public Owner As String
    Public IsFunction As Boolean
    Public IsRuntime As Boolean
    Public LearnUrl As String
    Public Summary As String = ""
    Public ReadOnly Forms As New List(Of String)()
  End Class

  Private ReadOnly _lock As New Object()
  Private _entries As SortedDictionary(Of String, Entry)

  Private ReadOnly Format As New SymbolDisplayFormat(
    memberOptions:=SymbolDisplayMemberOptions.IncludeParameters,
    parameterOptions:=SymbolDisplayParameterOptions.IncludeName Or SymbolDisplayParameterOptions.IncludeType Or SymbolDisplayParameterOptions.IncludeOptionalBrackets Or SymbolDisplayParameterOptions.IncludeDefaultValue Or SymbolDisplayParameterOptions.IncludeParamsRefOut,
    miscellaneousOptions:=SymbolDisplayMiscellaneousOptions.UseSpecialTypes Or SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers)

  Private ReadOnly RuntimeTypes As String() = {"Strings", "Conversion", "Interaction", "FileSystem", "DateAndTime", "Information", "VBMath", "Financial"}

  ''' <summary>The key a name is looked up by: upper case, without the type character of QuickBASIC (<c>LEFT$</c>), words joined by underscores.</summary>
  Public Function Key(name As String) As String
    Dim k = name.Trim().ToUpperInvariant().Replace("$", "").Replace("-", "_").Replace(" ", "_")
    Return k.TrimEnd("_"c)
  End Function

  Private Function Entries() As SortedDictionary(Of String, Entry)

    SyncLock _lock

      If _entries IsNot Nothing Then Return _entries

      Dim map As New SortedDictionary(Of String, Entry)(StringComparer.Ordinal)
      Dim compilation = If(CompilationSource Is Nothing, Nothing, CompilationSource.Invoke())

      If compilation IsNot Nothing Then

        Dim qb = compilation.GlobalNamespace.GetNamespaceMembers().FirstOrDefault(Function(n) n.Name = "QB")
        If qb IsNot Nothing Then
          For Each owner In qb.GetTypeMembers().Where(Function(t) t.DeclaredAccessibility = Accessibility.Public AndAlso t.TypeKind = TypeKind.Class).OrderBy(Function(t) t.Name, StringComparer.Ordinal)
            AddMembers(map, owner, "QB." & owner.Name, False)
          Next
        End If

        Dim vb = compilation.GlobalNamespace.GetNamespaceMembers().FirstOrDefault(Function(n) n.Name = "Microsoft")?.GetNamespaceMembers().FirstOrDefault(Function(n) n.Name = "VisualBasic")
        If vb IsNot Nothing Then
          For Each owner In vb.GetTypeMembers().Where(Function(t) t.DeclaredAccessibility = Accessibility.Public AndAlso RuntimeTypes.Contains(t.Name))
            AddMembers(map, owner, "Microsoft.VisualBasic", True)
          Next
        End If

      End If

      ' an empty answer (no compilation yet) is not kept: it is asked again next time
      If map.Count > 0 Then _entries = map
      Return map

    End SyncLock

  End Function

  Private Sub AddMembers(map As SortedDictionary(Of String, Entry), owner As INamedTypeSymbol, ownerName As String, runtime As Boolean)

    For Each member In owner.GetMembers().OfType(Of IMethodSymbol)()
      If Not member.IsStatic OrElse member.DeclaredAccessibility <> Accessibility.Public OrElse member.MethodKind <> MethodKind.Ordinary Then Continue For

      Dim k = Key(member.Name)
      Dim item As Entry = Nothing
      If Not map.TryGetValue(k, item) Then
        item = New Entry With {.Name = member.Name, .Owner = ownerName, .IsRuntime = runtime, .IsFunction = Not member.ReturnsVoid}
        If runtime Then item.LearnUrl = "https://learn.microsoft.com/dotnet/api/microsoft.visualbasic." & owner.Name.ToLowerInvariant() & "." & member.Name.ToLowerInvariant()
        map(k) = item
      ElseIf item.Owner <> ownerName Then
        ' a name that several classes have (Graphics has all of Video's): the first one stands; QB comes before the runtime
        Continue For
      End If

      Dim form = member.ToDisplayString(Format)
      If Not member.ReturnsVoid Then form = form & " As " & member.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
      If Not item.Forms.Contains(form) Then item.Forms.Add(form)
      If Not member.ReturnsVoid Then item.IsFunction = True

      If item.Summary.Length = 0 Then item.Summary = SummaryOf(member)
    Next

  End Sub

  Private Function SummaryOf(member As ISymbol) As String

    Try
      Dim xml = member.GetDocumentationCommentXml()
      If String.IsNullOrWhiteSpace(xml) Then Return ""
      Dim doc = System.Xml.Linq.XElement.Parse(xml)
      Dim summary = doc.Descendants("summary").FirstOrDefault()
      Return If(summary Is Nothing, "", String.Join(" ", summary.Value.Split({ControlChars.Lf, ControlChars.Cr, " "c}, StringSplitOptions.RemoveEmptyEntries)))
    Catch ex As Exception When TypeOf ex Is System.Xml.XmlException OrElse TypeOf ex Is InvalidOperationException
      Return ""
    End Try

  End Function

  ''' <summary>The statement or function with this name (any casing, with or without the $), or Nothing.</summary>
  Public Function Lookup(name As String) As Entry
    Dim item As Entry = Nothing
    Return If(Entries().TryGetValue(Key(name), item), item, Nothing)
  End Function

  ''' <summary>All of them, by key.</summary>
  Public Function All() As IEnumerable(Of KeyValuePair(Of String, Entry))
    Return Entries()
  End Function

  ''' <summary>The forms of an entry as lines of help (the most it shows are a few dozen).</summary>
  Public Function FormLines(item As Entry) As List(Of String)
    Dim lines As New List(Of String)()
    For Each form In item.Forms.Take(MaxForms)
      lines.Add(form)
    Next
    If item.Forms.Count > MaxForms Then lines.Add("... (" & (item.Forms.Count - MaxForms) & " more)")
    Return lines
  End Function

End Module
