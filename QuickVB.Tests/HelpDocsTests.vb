Option Explicit On
Option Strict On
Option Infer On

Imports ConsoleGUI
Imports Xunit

Public Class HelpDocsTests

  Private Shared Sub UseLibraries()
    Dim workspace As New QuickVBWorkspace()
    Dim projectId = workspace.CreateProject("Demo", "Demo")
    workspace.CreateDocument(projectId, "Demo.vb")
    StatementHelp.CompilationSource = Function() workspace.CurrentSolution.Projects.First().GetCompilationAsync().Result
  End Sub

  Private Shared Function TextOf(id As String) As String
    UseLibraries()
    Dim topic = HelpContent.Find(id)
    Assert.NotNull(topic)
    Return topic.Title & vbLf & String.Join(vbLf, topic.Lines)
  End Function

  <Fact>
  Public Sub AWordOfAProgramFindsItsTopic()
    UseLibraries()
    Assert.Equal("kw:PRINT", HelpDocs.TopicIdOf("print"))
    Assert.Equal("kw:IF_THEN_ELSE", HelpDocs.TopicIdOf("IF"))
    Assert.Equal("kw:IF_THEN_ELSE", HelpDocs.TopicIdOf("ElseIf"))
    Assert.Equal("kw:FOR_NEXT", HelpDocs.TopicIdOf("next"))
    Assert.Equal("kw:LEFT", HelpDocs.TopicIdOf("Left"))
    Assert.Null(HelpDocs.TopicIdOf("banana"))
  End Sub

  <Fact>
  Public Sub AQuickBasicTopicSaysHowToCallItFromVisualBasic()
    Dim text = TextOf("kw:LOCATE")
    Assert.Contains("LOCATE Statement", text)
    Assert.Contains("Imports QB.Video", text)
    Assert.Contains("Moves the cursor", text)
  End Sub

  <Fact>
  Public Sub ARuntimeFunctionLinksToMicrosoftsPage()
    Dim text = TextOf("kw:LEFT")
    Assert.Contains("Function", text)
    Assert.Contains("https://learn.microsoft.com/dotnet/api/microsoft.visualbasic.strings.left", text)
  End Sub

  <Fact>
  Public Sub AKeywordTopicLinksToMicrosoftsPage()
    Dim text = TextOf("kw:IF_THEN_ELSE")
    Assert.Contains("https://learn.microsoft.com/dotnet/visual-basic/language-reference/statements/if-then-else-statement", text)
    Assert.Contains("ELSEIF", text)
  End Sub

  <Fact>
  Public Sub TheListsOfKeywordsAreThere()
    Assert.Contains("PRINT", TextOf(HelpDocs.AlphabeticalId))
    Assert.Contains("kw:IF_THEN_ELSE", TextOf(HelpDocs.VbCategoriesId))
    Assert.Contains("kw:LOCATE", TextOf(HelpDocs.QbCategoriesId))
  End Sub

  <Fact>
  Public Sub DumpTopics()
    Dim path = Environment.GetEnvironmentVariable("QVB_HELP_DUMP")
    If path Is Nothing Then Return
    Dim sb As New System.Text.StringBuilder()
    For Each id In {"kw:PRINT", "kw:LOCATE", "kw:LEFT", "kw:IF_THEN_ELSE", "kw:SELECT_CASE", "kw:DATA_TYPES", "kw:CLS", HelpDocs.AlphabeticalId, HelpDocs.VbCategoriesId, HelpDocs.QbCategoriesId}
      sb.AppendLine("=================== " & id)
      sb.AppendLine(TextOf(id))
    Next
    IO.File.WriteAllText(path, sb.ToString())
  End Sub

End Class
