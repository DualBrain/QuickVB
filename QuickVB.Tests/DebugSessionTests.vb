Option Explicit On
Option Strict On
Option Infer On

Imports System.Collections.Concurrent
Imports Microsoft.CodeAnalysis
Imports Xunit

' The debugger's engine: a real program, instrumented, run in a process of its own and driven over the pipe.
Public Class DebugSessionTests

  Private Const Program As String =
    "Module Demo" & vbLf &                        ' 0
    "  Dim total As Integer" & vbLf &             ' 1
    "  Sub Main()" & vbLf &                       ' 2
    "    Dim x As Integer = 1" & vbLf &           ' 3
    "    Dim name As String = ""hi""" & vbLf &    ' 4
    "    x = x + 1" & vbLf &                      ' 5
    "    Add(x)" & vbLf &                         ' 6
    "    x = x + 10" & vbLf &                     ' 7
    "    Console.WriteLine(total)" & vbLf &       ' 8
    "  End Sub" & vbLf &                          ' 9
    "  Sub Add(n As Integer)" & vbLf &            ' 10
    "    total = total + n" & vbLf &              ' 11
    "  End Sub" & vbLf &                          ' 12
    "End Module"

  Private NotInheritable Class Run
    Implements IDisposable

    Public ReadOnly Events As New BlockingCollection(Of Object)()
    Public ReadOnly Session As DebugSession
    Public ReadOnly Files As New List(Of String)()

    Public Sub New(source As String)

      Dim workspace As New QuickVBWorkspace()
      Dim projectId = workspace.CreateProject("Demo", "Demo")
      Dim documentId = workspace.CreateDocument(projectId, "Demo.vb")
      workspace.ChangedDocumentTextInternal(documentId, source)

      Dim diagnostics = workspace.BuildDebugProgram(Me.Files, True)
      Dim errors = diagnostics.Where(Function(d) d.Severity = DiagnosticSeverity.Error).ToList()
      Assert.True(errors.Count = 0, String.Join(Environment.NewLine, errors))

      Me.Session = workspace.CreateDebugSession("")
      Me.Session.OnPaused = Sub(p) Me.Events.Add(p)
      Me.Session.OnEnded = Sub(code) Me.Events.Add(code)

    End Sub

    Public Function [Next]() As Object
      Dim item As Object = Nothing
      Assert.True(Me.Events.TryTake(item, 30000), "the program did not stop or end in time")
      Return item
    End Function

    Public Function NextPause() As PauseInfo
      Dim item = Me.[Next]()
      Assert.True(TypeOf item Is PauseInfo, "the program ended instead of stopping")
      Return DirectCast(item, PauseInfo)
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
      Me.Session.Dispose()
    End Sub

  End Class

  Private Shared Function Variable(pause As PauseInfo, name As String) As Object
    Return pause.Variables.FirstOrDefault(Function(v) v.Name = name)?.Value
  End Function

  <Fact>
  Public Sub SteppingStopsInFrontOfEveryStatementWithTheVariablesInScope()
    Using r As New Run(Program)

      r.Session.Start(New DebugCommand With {.Mode = "into"})

      Dim p = r.NextPause()
      Assert.Equal(3, p.Line)
      Assert.Equal("step", p.Reason)
      Assert.Equal("Demo.vb", r.Files(p.File))
      Assert.Equal(0L, Variable(p, "total"))
      Assert.Null(p.Variables.FirstOrDefault(Function(v) v.Name = "x"))       ' not declared yet

      r.Session.Go(New DebugCommand With {.Mode = "into"})
      p = r.NextPause()
      Assert.Equal(4, p.Line)
      Assert.Equal(1L, Variable(p, "x"))

      r.Session.Go(New DebugCommand With {.Mode = "into"})
      p = r.NextPause()
      Assert.Equal(5, p.Line)
      Assert.Equal("hi", Variable(p, "name"))

      r.Session.Go(New DebugCommand With {.Mode = "run"})
      Assert.Equal(0, r.[Next]())
    End Using
  End Sub

  <Fact>
  Public Sub StepIntoEntersAProcedureWithItsParameters()
    Using r As New Run(Program)

      r.Session.Start(New DebugCommand With {.Mode = "to", .TargetFile = 0, .TargetLine = 6})
      Dim p = r.NextPause()
      Assert.Equal(6, p.Line)
      Assert.Equal(1, p.Depth)

      r.Session.Go(New DebugCommand With {.Mode = "into"})
      p = r.NextPause()
      Assert.Equal(11, p.Line)
      Assert.Equal(2, p.Depth)
      Assert.Equal(2L, Variable(p, "n"))

      r.Session.Go(New DebugCommand With {.Mode = "run"})
      Assert.Equal(0, r.[Next]())
    End Using
  End Sub

  <Fact>
  Public Sub StepOverSkipsAProcedureCall()
    Using r As New Run(Program)

      r.Session.Start(New DebugCommand With {.Mode = "to", .TargetFile = 0, .TargetLine = 6})
      Assert.Equal(6, r.NextPause().Line)

      r.Session.Go(New DebugCommand With {.Mode = "over"})
      Dim p = r.NextPause()
      Assert.Equal(7, p.Line)                                  ' not the inside of Add
      Assert.Equal(2L, Variable(p, "total"))                   ' but Add has run

      r.Session.Go(New DebugCommand With {.Mode = "run"})
      Assert.Equal(0, r.[Next]())
    End Using
  End Sub

  <Fact>
  Public Sub StepOutFinishesTheProcedure()
    Using r As New Run(Program)

      r.Session.Start(New DebugCommand With {.Mode = "to", .TargetFile = 0, .TargetLine = 11})
      Assert.Equal(11, r.NextPause().Line)

      r.Session.Go(New DebugCommand With {.Mode = "out"})
      Dim p = r.NextPause()
      Assert.Equal(7, p.Line)
      Assert.Equal(1, p.Depth)

      r.Session.Go(New DebugCommand With {.Mode = "run"})
      Assert.Equal(0, r.[Next]())
    End Using
  End Sub

  <Fact>
  Public Sub ABreakpointStopsARunningProgram()
    Using r As New Run(Program)

      r.Session.Start(New DebugCommand With {.Mode = "run", .Breakpoints = {Tuple.Create(0, 7)}})
      Dim p = r.NextPause()
      Assert.Equal(7, p.Line)
      Assert.Equal("break", p.Reason)
      Assert.Equal(2L, Variable(p, "x"))

      ' let it go again, now with a breakpoint in Add that it has already passed: it runs to the end
      r.Session.Go(New DebugCommand With {.Mode = "run", .Breakpoints = {Tuple.Create(0, 11)}})
      Assert.Equal(0, r.[Next]())
    End Using
  End Sub

  <Fact>
  Public Sub WatchingStopsAtEveryStatementForTheWatchpoints()
    Using r As New Run(Program)

      r.Session.Start(New DebugCommand With {.Mode = "run", .Watching = True})
      Dim p = r.NextPause()
      Assert.Equal(3, p.Line)
      Assert.Equal("watch", p.Reason)

      r.Session.Go(New DebugCommand With {.Mode = "run", .Watching = True})
      Assert.Equal(4, r.NextPause().Line)

      r.Session.Go(New DebugCommand With {.Mode = "run", .Watching = False})
      Assert.Equal(0, r.[Next]())
    End Using
  End Sub

  <Fact>
  Public Sub ArraysAndStringsAreDescribed()
    Using r As New Run("Module Demo" & vbLf & "  Sub Main()" & vbLf & "    Dim a(2) As Integer" & vbLf & "    Dim s As String = ""a b|c""" & vbLf & "    a(1) = 7" & vbLf & "    Console.WriteLine(s)" & vbLf & "  End Sub" & vbLf & "End Module")

      r.Session.Start(New DebugCommand With {.Mode = "to", .TargetFile = 0, .TargetLine = 5})
      Dim p = r.NextPause()

      Assert.Equal("a b|c", Variable(p, "s"))
      Dim items = DirectCast(Variable(p, "a"), Object())
      Assert.Equal(3, items.Length)
      Assert.Equal(0L, items(0))

      r.Session.Go(New DebugCommand With {.Mode = "run"})
      Assert.Equal(0, r.[Next]())
    End Using
  End Sub

  <Fact>
  Public Sub AProgramRunsToTheEndWhenNothingStopsIt()
    Using r As New Run(Program)
      r.Session.Start(New DebugCommand With {.Mode = "run"})
      Assert.Equal(0, r.[Next]())
    End Using
  End Sub

  <Fact>
  Public Sub TheCallStackIsReported()
    Using r As New Run(Program)

      r.Session.Start(New DebugCommand With {.Mode = "to", .TargetFile = 0, .TargetLine = 11})
      Dim p = r.NextPause()

      Assert.Equal({"Main", "Add"}, p.Frames.Select(Function(f) f.Name).ToArray())
      Assert.Equal(6, p.Frames(0).Line)                      ' Main is at the call of Add
      Assert.Equal(11, p.Frames(1).Line)

      r.Session.Go(New DebugCommand With {.Mode = "run"})
      Assert.Equal(0, r.[Next]())
    End Using
  End Sub

  Private Const Failing As String =
    "Module Demo" & vbLf &
    "  Sub Main()" & vbLf &
    "    Dim x As Integer = 3" & vbLf &
    "    Throw New InvalidOperationException(""boom"")" & vbLf &
    "  End Sub" & vbLf &
    "End Module"

  <Fact>
  Public Sub BreakOnErrorsStopsWhereAnUnhandledErrorHappened()
    Using r As New Run(Failing)

      r.Session.Start(New DebugCommand With {.Mode = "run", .BreakOnErrors = True})
      Dim p = r.NextPause()

      Assert.Equal("error", p.Reason)
      Assert.Equal("boom", p.ErrorMessage)
      Assert.Equal(3, p.Line)
      Assert.Equal({"Main"}, p.Frames.Select(Function(f) f.Name).ToArray())

      r.Session.Go(New DebugCommand With {.Mode = "run", .BreakOnErrors = True})
      Assert.NotEqual(0, DirectCast(r.[Next](), Integer))
    End Using
  End Sub

  <Fact>
  Public Sub WithoutBreakOnErrorsTheProgramJustEnds()
    Using r As New Run(Failing)
      r.Session.Start(New DebugCommand With {.Mode = "run", .BreakOnErrors = False})
      Assert.NotEqual(0, DirectCast(r.[Next](), Integer))
    End Using
  End Sub

End Class
