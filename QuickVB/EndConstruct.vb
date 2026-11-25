Option Explicit On
Option Strict On
Option Infer On

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

''' <summary>
''' Closing a block when its first line is entered: after <c>Module Program</c> and Enter the program gets its <c>End Module</c>.
''' Reads the program the way the compiler does, so a block whose end is missing shows up as such.
''' </summary>
Public Module EndConstruct

  ''' <summary>
  ''' The line that closes the block whose first line is <paramref name="row"/> of <paramref name="text"/> (zero based), when the block has
  ''' no end yet ("End Module", "Next", "Loop", ...); Nothing when the line starts no such block.
  ''' </summary>
  Public Function MissingEnd(text As String, row As Integer) As String

    Dim tree = VisualBasicSyntaxTree.ParseText(text)
    Dim found As String = Nothing

    For Each node In tree.GetRoot().DescendantNodes()

      Dim header As SyntaxNode = Nothing
      Dim missing As Boolean = False
      Dim closing As String = Nothing

      If Not Describe(node, header, missing, closing) OrElse Not missing OrElse header Is Nothing Then Continue For
      If tree.GetLineSpan(header.Span).EndLinePosition.Line <> row Then Continue For

      found = closing   ' the last one found is the innermost

    Next

    Return found

  End Function

  Private Function Describe(node As SyntaxNode, ByRef header As SyntaxNode, ByRef missing As Boolean, ByRef closing As String) As Boolean

    Dim typeBlock = TryCast(node, TypeBlockSyntax)
    If typeBlock IsNot Nothing Then
      header = typeBlock.BlockStatement
      missing = typeBlock.EndBlockStatement.IsMissing
      closing = "End " & If(TypeOf node Is ModuleBlockSyntax, "Module", If(TypeOf node Is ClassBlockSyntax, "Class", If(TypeOf node Is StructureBlockSyntax, "Structure", "Interface")))
      Return True
    End If

    Dim method = TryCast(node, MethodBlockBaseSyntax)
    If method IsNot Nothing Then
      Select Case method.BlockStatement.Kind()
        Case SyntaxKind.SubStatement, SyntaxKind.SubNewStatement : closing = "End Sub"
        Case SyntaxKind.FunctionStatement : closing = "End Function"
        Case SyntaxKind.OperatorStatement : closing = "End Operator"
        Case SyntaxKind.GetAccessorStatement : closing = "End Get"
        Case SyntaxKind.SetAccessorStatement : closing = "End Set"
        Case Else : Return False
      End Select
      header = method.BlockStatement
      missing = method.EndBlockStatement.IsMissing
      Return True
    End If

    Dim enumBlock = TryCast(node, EnumBlockSyntax)
    If enumBlock IsNot Nothing Then
      header = enumBlock.EnumStatement : missing = enumBlock.EndEnumStatement.IsMissing : closing = "End Enum"
      Return True
    End If

    Dim space = TryCast(node, NamespaceBlockSyntax)
    If space IsNot Nothing Then
      header = space.NamespaceStatement : missing = space.EndNamespaceStatement.IsMissing : closing = "End Namespace"
      Return True
    End If

    Dim prop = TryCast(node, PropertyBlockSyntax)
    If prop IsNot Nothing Then
      header = prop.PropertyStatement : missing = prop.EndPropertyStatement.IsMissing : closing = "End Property"
      Return True
    End If

    Dim ifBlock = TryCast(node, MultiLineIfBlockSyntax)
    If ifBlock IsNot Nothing Then
      header = ifBlock.IfStatement : missing = ifBlock.EndIfStatement.IsMissing : closing = "End If"
      Return True
    End If

    Dim forBlock = TryCast(node, ForOrForEachBlockSyntax)
    If forBlock IsNot Nothing Then
      header = forBlock.ForOrForEachStatement : missing = forBlock.NextStatement Is Nothing OrElse forBlock.NextStatement.IsMissing : closing = "Next"
      Return True
    End If

    Dim whileBlock = TryCast(node, WhileBlockSyntax)
    If whileBlock IsNot Nothing Then
      header = whileBlock.WhileStatement : missing = whileBlock.EndWhileStatement.IsMissing : closing = "End While"
      Return True
    End If

    Dim doBlock = TryCast(node, DoLoopBlockSyntax)
    If doBlock IsNot Nothing Then
      header = doBlock.DoStatement : missing = doBlock.LoopStatement.IsMissing : closing = "Loop"
      Return True
    End If

    Dim selectBlock = TryCast(node, SelectBlockSyntax)
    If selectBlock IsNot Nothing Then
      header = selectBlock.SelectStatement : missing = selectBlock.EndSelectStatement.IsMissing : closing = "End Select"
      Return True
    End If

    Dim tryBlock = TryCast(node, TryBlockSyntax)
    If tryBlock IsNot Nothing Then
      header = tryBlock.TryStatement : missing = tryBlock.EndTryStatement.IsMissing : closing = "End Try"
      Return True
    End If

    Dim withBlock = TryCast(node, WithBlockSyntax)
    If withBlock IsNot Nothing Then
      header = withBlock.WithStatement : missing = withBlock.EndWithStatement.IsMissing : closing = "End With"
      Return True
    End If

    Dim lockBlock = TryCast(node, SyncLockBlockSyntax)
    If lockBlock IsNot Nothing Then
      header = lockBlock.SyncLockStatement : missing = lockBlock.EndSyncLockStatement.IsMissing : closing = "End SyncLock"
      Return True
    End If

    Dim usingBlock = TryCast(node, UsingBlockSyntax)
    If usingBlock IsNot Nothing Then
      header = usingBlock.UsingStatement : missing = usingBlock.EndUsingStatement.IsMissing : closing = "End Using"
      Return True
    End If

    Return False

  End Function

End Module
