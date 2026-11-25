Option Explicit On
Option Strict On
Option Infer On

Imports System.IO
Imports System.Reflection
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

''' <summary>
''' The libraries a program written in QuickVB can use, all of them embedded in QuickVB itself:
''' QB.Compatibility (the QuickBASIC and QBasic statements: PRINT, LOCATE, OPEN, ...), QB.Compatibility.Sdl with SDL2 (graphics) and
''' QuickVB.Runtime (the part of the debugger that runs inside the program).
''' A program only carries what it uses: QuickVB adds <c>Imports QB.Video</c> (and the like) when the code uses a QuickBASIC statement,
''' and a library is only written next to the program when the program refers to it.
''' </summary>
Public Module QBLibrary

  Public Const Core As String = "QB.Compatibility"
  Public Const Sdl As String = "QB.Compatibility.Sdl"
  Public Const Runtime As String = "QuickVB.Runtime"
  Private Const SdlManagedName As String = "SDL2-CS"

  Private ReadOnly _images As New Dictionary(Of String, Byte())(StringComparer.OrdinalIgnoreCase)

  ''' <summary>The bytes of an embedded file ("QB.Compatibility.dll", ...), or Nothing when QuickVB does not carry it.</summary>
  Public Function Image(fileName As String) As Byte()

    SyncLock _images

      Dim bytes As Byte() = Nothing
      If _images.TryGetValue(fileName, bytes) Then Return bytes

      Using stream = GetType(QBLibrary).Assembly.GetManifestResourceStream(fileName)
        If stream IsNot Nothing Then
          Using buffer As New MemoryStream()
            stream.CopyTo(buffer)
            bytes = buffer.ToArray()
          End Using
        End If
      End Using

      _images(fileName) = bytes
      Return bytes

    End SyncLock

  End Function

  ''' <summary>The libraries a program is compiled against (all of them: whether one is used decides if it travels with the program).</summary>
  Public Function MetadataReferences() As IEnumerable(Of MetadataReference)

    Dim refs As New List(Of MetadataReference)()

    For Each name In {Core, Sdl, SdlManagedName, Runtime}
      Dim bytes = Image(name & ".dll")
      If bytes IsNot Nothing Then refs.Add(MetadataReference.CreateFromImage(bytes, filePath:=name & ".dll"))
    Next

    Return refs

  End Function

  ''' <summary>The runtime library, for the compilations of programs run under the debugger.</summary>
  Public Function RuntimeReference() As MetadataReference
    Return MetadataReference.CreateFromImage(Image(Runtime & ".dll"), filePath:=Runtime & ".dll")
  End Function

  ''' <summary>The names (without ".dll") of the libraries a compilation uses that QuickVB carries.</summary>
  Public Function LibrariesUsedBy(compilation As Compilation) As HashSet(Of String)

    Dim used As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    Dim ours = {Core, Sdl, SdlManagedName, Runtime}

    For Each reference In compilation.GetUsedAssemblyReferences()
      Dim symbol = TryCast(compilation.GetAssemblyOrModuleSymbol(reference), IAssemblySymbol)
      If symbol IsNot Nothing AndAlso ours.Contains(symbol.Identity.Name, StringComparer.OrdinalIgnoreCase) Then used.Add(symbol.Identity.Name)
    Next

    ' the window library needs the core one (and the SDL2 binding) at run time even when the program only touches one of them
    If used.Contains(Sdl) Then
      used.Add(Core)
      used.Add(SdlManagedName)
    End If

    Return used

  End Function

  ''' <summary>Writes the libraries <paramref name="used"/> (see <see cref="LibrariesUsedBy"/>) next to a program.</summary>
  Public Sub WriteTo(folder As String, used As IEnumerable(Of String))

    Dim names = used.ToList()

    For Each name In names
      Dim bytes = Image(name & ".dll")
      If bytes IsNot Nothing Then File.WriteAllBytes(Path.Combine(folder, name & ".dll"), bytes)
    Next

    ' SDL2 itself (the native library for the platform QuickVB was built for)
    If names.Contains(Sdl, StringComparer.OrdinalIgnoreCase) Then
      For Each native In {"SDL2.dll", "libSDL2.so", "libSDL2.dylib"}
        Dim bytes = Image("native/" & native)
        If bytes IsNot Nothing Then File.WriteAllBytes(Path.Combine(folder, native), bytes)
      Next
    End If

  End Sub

  ''' <summary>The files a program may have next to it that came from here (Make EXE File copies them with the program).</summary>
  Public Function FileNamesIn(folder As String) As IEnumerable(Of String)
    Dim candidates = {Core & ".dll", Sdl & ".dll", SdlManagedName & ".dll", Runtime & ".dll", "SDL2.dll", "libSDL2.so", "libSDL2.dylib"}
    Return candidates.Where(Function(f) File.Exists(Path.Combine(folder, f))).ToList()
  End Function

  ' ---- QuickBASIC keywords

  Private ReadOnly _keywordLock As New Object()
  Private _keywords As Dictionary(Of String, String)

  ' where a name that several classes have is imported from (the first of these that has it)
  Private ReadOnly Preferred As String() = {"Video", "File", "Development", "Multimedia", "Events", "Devices", "Printer", "Serial", "Graphics", "Speaker"}

  ''' <summary>
  ''' The statements and functions of QB.Compatibility, each with the namespace to import to use it (<c>QB.Video</c> for LOCATE).
  ''' Read from the library itself, so a newer library brings its new statements.
  ''' </summary>
  Public Function Keywords(compilation As Compilation) As Dictionary(Of String, String)

    SyncLock _keywordLock

      If _keywords Is Nothing Then

        Dim map As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        Dim qb = compilation.GlobalNamespace.GetNamespaceMembers().FirstOrDefault(Function(n) n.Name = "QB")

        If qb IsNot Nothing Then

          Dim types = qb.GetTypeMembers().
                         Where(Function(t) t.DeclaredAccessibility = Accessibility.Public AndAlso t.TypeKind = TypeKind.Class).
                         OrderBy(Function(t) If(Array.IndexOf(Preferred, t.Name) < 0, Preferred.Length, Array.IndexOf(Preferred, t.Name))).
                         ThenBy(Function(t) t.Name, StringComparer.Ordinal)

          For Each owner In types
            For Each member In owner.GetMembers()
              If member.IsStatic AndAlso member.DeclaredAccessibility = Accessibility.Public AndAlso
                 (member.Kind = SymbolKind.Method AndAlso DirectCast(member, IMethodSymbol).MethodKind = MethodKind.Ordinary) Then
                If Not map.ContainsKey(member.Name) Then map(member.Name) = "QB." & owner.Name
              End If
            Next
          Next

        End If

        _keywords = map

      End If

      Return _keywords

    End SyncLock

  End Function

  ''' <summary>
  ''' The QB namespaces each file of a compilation needs and does not import yet: a file that calls a QuickBASIC statement (LOCATE, CLS, ...)
  ''' that nothing in the program declares. Keyed by the file's path.
  ''' </summary>
  Public Function MissingImports(compilation As Compilation) As Dictionary(Of String, List(Of String))

    Dim result As New Dictionary(Of String, List(Of String))(StringComparer.OrdinalIgnoreCase)
    Dim keywords = QBLibrary.Keywords(compilation)
    If keywords.Count = 0 Then Return result

    For Each tree In compilation.SyntaxTrees

      Dim root = tree.GetRoot()
      Dim existing = ImportedNames(root)
      Dim model = compilation.GetSemanticModel(tree)
      Dim needed As New List(Of String)()

      For Each node In root.DescendantNodes().OfType(Of IdentifierNameSyntax)()

        Dim target As String = Nothing
        If Not keywords.TryGetValue(node.Identifier.ValueText, target) Then Continue For
        If existing.Contains(target) OrElse needed.Contains(target) Then Continue For
        If Not IsStatementPosition(node) Then Continue For

        ' something the program declares (a variable, a procedure of its own) is not a QuickBASIC statement
        Dim info = model.GetSymbolInfo(node)
        If info.Symbol IsNot Nothing AndAlso Not IsImplicitVariable(info.Symbol) Then Continue For
        If info.CandidateSymbols.Any(Function(s) s.ContainingAssembly Is Nothing OrElse s.ContainingAssembly.Identity.Name <> "Microsoft.VisualBasic") AndAlso info.Symbol Is Nothing Then Continue For

        needed.Add(target)

      Next

      If needed.Count > 0 Then result(tree.FilePath) = needed

    Next

    Return result

  End Function

  ' LOCATE 1, 1 / LOCATE(1, 1) / CLS: the name is called, or stands alone as a statement
  Private Function IsStatementPosition(node As IdentifierNameSyntax) As Boolean

    Dim parent = node.Parent
    If TypeOf parent Is InvocationExpressionSyntax AndAlso Object.ReferenceEquals(DirectCast(parent, InvocationExpressionSyntax).Expression, node) Then Return True
    If TypeOf parent Is ExpressionStatementSyntax Then Return True
    If TypeOf parent Is CallStatementSyntax Then Return True

    Return False

  End Function

  ' with Option Explicit off a name nothing declares is an implicit local variable
  Private Function IsImplicitVariable(symbol As ISymbol) As Boolean
    Return symbol.Kind = SymbolKind.Local AndAlso symbol.DeclaringSyntaxReferences.Length = 0 OrElse
           symbol.Kind = SymbolKind.Local AndAlso symbol.IsImplicitlyDeclared
  End Function

  Private Function ImportedNames(root As SyntaxNode) As HashSet(Of String)

    Dim names As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

    For Each statement In root.DescendantNodes().OfType(Of ImportsStatementSyntax)()
      For Each clause In statement.ImportsClauses.OfType(Of SimpleImportsClauseSyntax)()
        names.Add(clause.Name.ToString().Trim())
      Next
    Next

    Return names

  End Function

End Module
