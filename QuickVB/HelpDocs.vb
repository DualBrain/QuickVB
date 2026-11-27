Option Explicit On
Option Strict On
Option Infer On

Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports ConsoleGUI

''' <summary>
''' The help topics of the language: a topic for each Visual Basic keyword (written for QuickVB) and for each QuickBASIC statement and function
''' of QB.Compatibility and the Visual Basic runtime, in the style of the help of QBasic: what it does, its syntax, comments, an example and what to
''' see next. The topics are Markdown files embedded in QuickVB (<c>Help/vb</c> and <c>Help/qb</c>); a QuickBASIC topic is only there for a
''' statement the program can actually call, and says how to call it from Visual Basic (what to import, the forms it has).
''' </summary>
Public Module HelpDocs

  Public Const Prefix As String = "kw:"
  ''' <summary>All the keywords, A to Z.</summary>
  Public Const AlphabeticalId As String = "keywords"
  ''' <summary>The Visual Basic keywords by category.</summary>
  Public Const VbCategoriesId As String = "vbkeywords"
  ''' <summary>The QuickBASIC statements and functions by category.</summary>
  Public Const QbCategoriesId As String = "qbcategories"

  Private Const Width As Integer = 76
  Private Const Gap As String = "   "

  Private NotInheritable Class Doc
    Public Key As String
    Public Text As String
    Public Title As String
    Public Aliases As New List(Of String)()
  End Class

  Private ReadOnly _lock As New Object()
  Private _vb As Dictionary(Of String, Doc)
  Private _qb As Dictionary(Of String, Doc)
  Private _aliases As Dictionary(Of String, String)

  Private ReadOnly LinkPattern As New Regex("\[([^\]]+)\]\(((?:[^()]|\([^)]*\))+)\)", RegexOptions.Compiled)
  Private ReadOnly CodePattern As New Regex("`([^`]+)`", RegexOptions.Compiled)
  Private ReadOnly StarPattern As New Regex("\*\*([^*]+)\*\*|\*([^*]+)\*", RegexOptions.Compiled)
  Private ReadOnly MarkupPattern As New Regex("\[\[(?<t>[^|\]]+)\|[^\]]+\]\]|\{\{(?<e>[^}]+)\}\}", RegexOptions.Compiled)
  Private ReadOnly KeywordsComment As New Regex("<!--\s*keywords:\s*(.*?)\s*-->", RegexOptions.Compiled Or RegexOptions.IgnoreCase)

  ' ---- the embedded files

  Private Sub Load()

    SyncLock _lock

      If _vb IsNot Nothing Then Return

      Dim vb As New Dictionary(Of String, Doc)(StringComparer.Ordinal)
      Dim qb As New Dictionary(Of String, Doc)(StringComparer.Ordinal)
      Dim aliases As New Dictionary(Of String, String)(StringComparer.Ordinal)
      Dim assembly = GetType(HelpDocs).Assembly

      For Each resourceName In assembly.GetManifestResourceNames()

        Dim resource = resourceName.Replace("\"c, "/"c)
        Dim isVb = resource.StartsWith("help/vb/", StringComparison.OrdinalIgnoreCase)
        Dim isQb = resource.StartsWith("help/qb/", StringComparison.OrdinalIgnoreCase)
        If Not (isVb OrElse isQb) OrElse Not resource.EndsWith(".md", StringComparison.OrdinalIgnoreCase) Then Continue For

        Dim text As String
        Using stream = assembly.GetManifestResourceStream(resourceName)
          Using reader As New StreamReader(stream, Encoding.UTF8)
            text = reader.ReadToEnd()
          End Using
        End Using

        Dim name = Path.GetFileNameWithoutExtension(resource)
        Dim doc As New Doc With {.Key = StatementHelp.Key(name), .Text = text.Replace(vbCrLf, vbLf)}

        Dim first = doc.Text.Split(ControlChars.Lf).FirstOrDefault(Function(l) l.StartsWith("# ", StringComparison.Ordinal))
        doc.Title = If(first Is Nothing, name, first.Substring(2).Trim())

        If isVb Then
          For Each m As Match In KeywordsComment.Matches(doc.Text)
            For Each word In m.Groups(1).Value.Split({" "c, ","c}, StringSplitOptions.RemoveEmptyEntries)
              doc.Aliases.Add(word)
              aliases(StatementHelp.Key(word)) = doc.Key
            Next
          Next
          vb(doc.Key) = doc
        Else
          qb(doc.Key) = doc
        End If

      Next

      _qb = qb
      _aliases = aliases
      _vb = vb

    End SyncLock

  End Sub

  ' ---- finding topics

  ''' <summary>The id of the topic for a word of a program (IF, print, Locate, left$), or Nothing when there is none.</summary>
  Public Function TopicIdOf(word As String) As String

    Load()
    Dim key = StatementHelp.Key(word)
    If key.Length = 0 Then Return Nothing

    Dim target As String = Nothing
    If _aliases.TryGetValue(key, target) Then Return Prefix & target
    If _vb.ContainsKey(key) Then Return Prefix & key
    If StatementHelp.Lookup(key) IsNot Nothing Then Return Prefix & key

    Return Nothing

  End Function

  ' the id a Markdown link leads to, or Nothing when there is no such topic
  Private Function TargetOf(target As String) As String

    If target.StartsWith("https://", StringComparison.OrdinalIgnoreCase) Then Return target
    If target = AlphabeticalId OrElse target = VbCategoriesId OrElse target = QbCategoriesId OrElse HelpContent.Order.Contains(target) Then Return target

    Dim key = StatementHelp.Key(target)
    Dim alias_ As String = Nothing
    If _vb.ContainsKey(key) Then Return Prefix & key
    If _aliases.TryGetValue(key, alias_) Then Return Prefix & alias_
    If _qb.ContainsKey(key) AndAlso StatementHelp.Lookup(key) IsNot Nothing Then Return Prefix & key

    Return Nothing

  End Function

  ''' <summary>True for the ids of the topics this module makes.</summary>
  Public Function IsDocId(id As String) As Boolean
    Return id.StartsWith(Prefix, StringComparison.Ordinal) OrElse id = AlphabeticalId OrElse id = VbCategoriesId OrElse id = QbCategoriesId
  End Function

  ''' <summary>The topic with this id (<c>kw:NAME</c>, or one of the lists), or Nothing.</summary>
  Public Function Find(id As String) As HelpTopic

    Load()

    If id = AlphabeticalId Then Return AlphabeticalTopic()
    If id = VbCategoriesId Then Return CategoriesTopic(id, "Visual Basic Keywords", "_categories", _vb)
    If id = QbCategoriesId Then Return CategoriesTopic(id, "Statements and Functions by Category", "categories", _qb)
    If Not id.StartsWith(Prefix, StringComparison.Ordinal) Then Return Nothing

    Dim key = id.Substring(Prefix.Length)
    Dim entry = StatementHelp.Lookup(key)
    Dim doc As Doc = Nothing
    Dim isVb = _vb.TryGetValue(key, doc)

    If Not isVb AndAlso Not _qb.TryGetValue(key, doc) Then doc = Nothing
    If Not isVb AndAlso entry Is Nothing Then Return Nothing   ' a QuickBASIC topic for something the program cannot call
    If doc Is Nothing AndAlso entry Is Nothing Then Return Nothing

    Dim lines As List(Of String)
    Dim title As String

    If doc IsNot Nothing Then
      title = doc.Title
      If Not isVb Then title &= If(entry IsNot Nothing AndAlso entry.IsFunction, " Function", " Statement")
      lines = Render(doc.Text, If(isVb, Nothing, entry), title)
    Else
      title = entry.Name.ToUpperInvariant() & If(entry.IsFunction, " Function", " Statement")
      lines = New List(Of String) From {"{{" & title & "}}", "<hr>"}
      If entry.Summary.Length > 0 Then
        lines.Add(" {{Action}}")
        lines.AddRange(Wrap(entry.Summary, 3, 3))
        lines.Add("")
      End If
      lines.AddRange(InQuickVb(entry))
    End If

    AddOnline(lines, entry, isVb)

    Dim nav As New List(Of HelpNavItem) From {
      New HelpNavItem(title, id, isCurrent:=True),
      New HelpNavItem("Keywords", AlphabeticalId),
      New HelpNavItem("Contents", HelpContent.ContentsId),
      New HelpNavItem("Index", HelpContent.IndexId)}

    ' the rule under the title is part of the topic's lines
    Return New HelpTopic(id, title, nav, lines.Skip(2))

  End Function

  ' ---- what the library says about a statement

  Private Function InQuickVb(entry As StatementHelp.Entry) As List(Of String)

    Dim lines As New List(Of String) From {" {{In QuickVB}}"}
    If entry.IsRuntime Then
      lines.Add(Gap & "Built in to Visual Basic (" & entry.Owner & "): nothing to import.")
    Else
      lines.Add(Gap & "Imports " & entry.Owner)
    End If
    lines.Add("")
    lines.Add(Gap & "Forms:")
    For Each form In StatementHelp.FormLines(entry)
      lines.Add(Gap & "  " & form.Replace("[[", "[ ["))
    Next
    lines.Add("")
    Return lines

  End Function

  Private Sub AddOnline(lines As List(Of String), entry As StatementHelp.Entry, isVb As Boolean)
    If isVb OrElse entry Is Nothing OrElse entry.LearnUrl Is Nothing Then Return
    lines.Add(" {{Online}}")
    lines.Add(Gap & "[[Microsoft Learn: " & entry.Owner & "." & entry.Name & "|" & entry.LearnUrl & "]]")
    lines.Add("")
  End Sub

  ' ---- the list of all the keywords

  Private Function AlphabeticalTopic() As HelpTopic

    Dim names As New SortedDictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

    For Each doc In _vb.Values
      If doc.Key.StartsWith("_", StringComparison.Ordinal) Then Continue For
      For Each word In doc.Aliases
        names(word.ToUpperInvariant()) = Prefix & doc.Key
      Next
    Next

    For Each pair In StatementHelp.All()
      ' what has a topic written for it; the rest of the library is found with F1
      If _qb.ContainsKey(pair.Key) Then names(pair.Value.Name.TrimEnd("_"c).Replace("_"c, " "c).ToUpperInvariant()) = Prefix & pair.Key
    Next

    Dim lines As New List(Of String) From {" {{Keywords}}", "<hr>"}

    If names.Count = 0 Then
      lines.Add("   Nothing to list yet.")
    Else
      For Each group In names.GroupBy(Function(p) If(Char.IsLetter(p.Key(0)), p.Key.Substring(0, 1).ToUpperInvariant(), "#"))
        lines.Add(" {{" & group.Key & "}}")
        Dim items = group.ToList()
        Dim columnWidth = items.Max(Function(p) p.Key.Length) + 2
        Dim perRow = Math.Max(1, (Width - 3) \ columnWidth)
        For i = 0 To items.Count - 1 Step perRow
          Dim line As New StringBuilder("   ")
          For j = i To Math.Min(i + perRow, items.Count) - 1
            line.Append("[[").Append(items(j).Key).Append("|").Append(items(j).Value).Append("]]")
            line.Append(New String(" "c, columnWidth - items(j).Key.Length))
          Next
          lines.Add(line.ToString().TrimEnd())
        Next
        lines.Add("")
      Next
    End If

    Dim nav As New List(Of HelpNavItem) From {
      New HelpNavItem("Keywords", AlphabeticalId, isCurrent:=True),
      New HelpNavItem("Contents", HelpContent.ContentsId),
      New HelpNavItem("Index", HelpContent.IndexId)}

    Return New HelpTopic(AlphabeticalId, "Keywords", nav, lines.Skip(2))

  End Function

  Private Function CategoriesTopic(id As String, title As String, fileKey As String, source As Dictionary(Of String, Doc)) As HelpTopic

    Dim doc As Doc = Nothing
    If Not source.TryGetValue(StatementHelp.Key(fileKey), doc) AndAlso Not source.TryGetValue(fileKey.ToUpperInvariant(), doc) Then Return Nothing

    Dim lines = Render(doc.Text, Nothing, title)
    Dim nav As New List(Of HelpNavItem) From {
      New HelpNavItem(title, id, isCurrent:=True),
      New HelpNavItem("Keywords", AlphabeticalId),
      New HelpNavItem("Contents", HelpContent.ContentsId),
      New HelpNavItem("Index", HelpContent.IndexId)}

    Return New HelpTopic(id, title, nav, lines.Skip(2))

  End Function

  ' ---- Markdown to help lines

  ' The lines of a topic: the title and a rule, then the sections.
  Private Function Render(markdown As String, entry As StatementHelp.Entry, title As String) As List(Of String)

    Dim output As New List(Of String) From {"{{" & title & "}}", "<hr>"}
    Dim raw = markdown.Split(ControlChars.Lf)

    Dim paragraph As New List(Of String)()
    Dim table As New List(Of String)()
    Dim section = ""
    Dim inCode = False
    Dim seenTitle = False
    Dim generated = (entry Is Nothing)

    Dim flushParagraph = Sub()
                           If paragraph.Count = 0 Then Return
                           output.AddRange(Wrap(String.Join(" ", paragraph), 3, 3))
                           output.Add("")
                           paragraph.Clear()
                         End Sub

    Dim flushTable = Sub()
                       If table.Count = 0 Then Return
                       output.AddRange(RenderTable(table))
                       output.Add("")
                       table.Clear()
                     End Sub

    For Each line In raw

      Dim trimmed = line.TrimEnd()

      If trimmed.TrimStart().StartsWith("```", StringComparison.Ordinal) Then
        flushParagraph()
        flushTable()
        inCode = Not inCode
        If Not inCode Then output.Add("")
        Continue For
      End If

      If inCode Then
        output.Add(Gap & "  " & trimmed.Replace("[[", "[ [").Replace("{{", "{ {"))
        Continue For
      End If

      If trimmed.StartsWith("<!--", StringComparison.Ordinal) Then Continue For

      If trimmed.StartsWith("# ", StringComparison.Ordinal) AndAlso Not seenTitle Then
        seenTitle = True
        section = "action"
        Continue For
      End If

      If trimmed.StartsWith("## ", StringComparison.Ordinal) OrElse trimmed.StartsWith("### ", StringComparison.Ordinal) Then
        flushParagraph()
        flushTable()
        Dim name = trimmed.TrimStart("#"c).Trim()
        Dim lower = name.ToLowerInvariant()
        ' the library's part of the topic comes before the comments, the example and the links
        If Not generated AndAlso (lower = "comments" OrElse lower.StartsWith("example", StringComparison.Ordinal) OrElse lower = "see also" OrElse lower = "online") Then
          output.AddRange(InQuickVb(entry))
          generated = True
        End If
        If output.Count > 0 AndAlso output(output.Count - 1).Length > 0 Then output.Add("")
        output.Add(" {{" & name & "}}")
        section = lower
        Continue For
      End If

      If trimmed.Length = 0 Then
        flushParagraph()
        flushTable()
        If section = "syntax" AndAlso output.Count > 0 AndAlso output(output.Count - 1).Length > 0 AndAlso Not output(output.Count - 1).StartsWith(" {{", StringComparison.Ordinal) Then output.Add("")
        Continue For
      End If

      ' the first paragraph is the action
      If section = "action" AndAlso paragraph.Count = 0 AndAlso output.Count = 2 Then output.Add(" {{Action}}")

      If trimmed.StartsWith("|", StringComparison.Ordinal) Then
        flushParagraph()
        table.Add(trimmed)
        Continue For
      End If

      flushTable()

      Dim bullet = Regex.Match(trimmed, "^\s*[*-]\s+(.*)$")
      If bullet.Success Then
        flushParagraph()
        Dim text = bullet.Groups(1).Value
        ' a list of links in which none leads anywhere is left out
        If LinkPattern.IsMatch(text) AndAlso Not LinkPattern.Matches(text).Cast(Of Match)().Any(Function(m) TargetOf(m.Groups(2).Value) IsNot Nothing) Then Continue For
        output.AddRange(Wrap(text, 3, 5, ChrW(&H25A0) & " "))
        Continue For
      End If

      ' the syntax and lines that are indented are kept as they were written
      If section = "syntax" OrElse trimmed.StartsWith("  ", StringComparison.Ordinal) Then
        flushParagraph()
        output.Add(Gap & Inline(trimmed.TrimEnd()))
        Continue For
      End If

      paragraph.Add(trimmed.Trim())

    Next

    flushParagraph()
    flushTable()
    If Not generated Then output.AddRange(InQuickVb(entry))

    ' no blank line at the end
    Do While output.Count > 2 AndAlso output(output.Count - 1).Length = 0
      output.RemoveAt(output.Count - 1)
    Loop

    Return output

  End Function

  ''' <summary>Inline Markdown as the markup of the help window: `code` and links; *emphasis* is dropped.</summary>
  Private Function Inline(text As String) As String

    Dim result = text.Replace("[[", "[ [").Replace("{{", "{ {")

    result = LinkPattern.Replace(result, Function(m As Match)
                                           Dim target = TargetOf(m.Groups(2).Value)
                                           Return If(target Is Nothing, m.Groups(1).Value, "[[" & m.Groups(1).Value & "|" & target & "]]")
                                         End Function)
    result = CodePattern.Replace(result, "{{$1}}")
    result = result.Replace("`", "")   ' a stray quote of code
    result = StarPattern.Replace(result, Function(m As Match) If(m.Groups(1).Success, m.Groups(1).Value, m.Groups(2).Value))

    Return result

  End Function

  ' what shows of a line of markup
  Private Function VisibleLength(text As String) As Integer
    Return MarkupPattern.Replace(text, Function(m As Match) If(m.Groups("t").Success, "<" & m.Groups("t").Value & ">", m.Groups("e").Value)).Length
  End Function

  ''' <summary>A paragraph as lines of at most the width of the help window; the first line starts at <paramref name="indent"/>, the others at <paramref name="hanging"/>.</summary>
  Private Function Wrap(text As String, indent As Integer, hanging As Integer, Optional bullet As String = "") As List(Of String)

    Dim marked = Inline(text)
    ' a space inside a link or a bit of code is not a place to break
    marked = Regex.Replace(marked, "\[\[[^\]]+\]\]|\{\{[^}]+\}\}", Function(m As Match) m.Value.Replace(" "c, ChrW(&HA0)))

    Dim lines As New List(Of String)()
    Dim current As New StringBuilder(New String(" "c, indent) & bullet)
    Dim lineStart = current.Length
    Dim hang = New String(" "c, hanging)

    For Each word In marked.Split({" "c}, StringSplitOptions.RemoveEmptyEntries)

      Dim shown = word.Replace(ChrW(&HA0), " "c)
      Dim currentLength = VisibleLength(current.ToString())

      If current.Length > lineStart AndAlso currentLength + 1 + VisibleLength(shown) > Width Then
        lines.Add(current.ToString())
        current.Clear().Append(hang)
        lineStart = current.Length
      End If

      If current.Length > lineStart Then current.Append(" "c)
      current.Append(shown)

    Next

    If current.Length > lineStart OrElse lines.Count = 0 Then lines.Add(current.ToString())
    Return lines

  End Function

  Private Function RenderTable(rows As List(Of String)) As List(Of String)

    Dim cells As New List(Of List(Of String))()

    For Each row In rows
      Dim parts = row.Trim().Trim("|"c).Split("|"c).Select(Function(c) c.Trim()).ToList()
      If parts.All(Function(c) c.Length > 0 AndAlso c.All(Function(ch) ch = "-"c OrElse ch = ":"c)) Then Continue For
      cells.Add(parts.Select(Function(c) Inline(c)).ToList())
    Next

    Dim result As New List(Of String)()
    If cells.Count = 0 Then Return result

    Dim columns = cells.Max(Function(r) r.Count)
    Dim widths(columns - 1) As Integer
    For Each row In cells
      For i = 0 To row.Count - 1
        widths(i) = Math.Max(widths(i), VisibleLength(row(i)))
      Next
    Next

    For Each row In cells
      Dim line As New StringBuilder(Gap)
      For i = 0 To row.Count - 1
        line.Append(row(i))
        If i < row.Count - 1 Then line.Append(New String(" "c, widths(i) - VisibleLength(row(i)) + 2))
      Next
      result.Add(line.ToString().TrimEnd())
    Next

    Return result

  End Function

End Module
