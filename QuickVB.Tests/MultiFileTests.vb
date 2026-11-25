Option Explicit On
Option Strict On
Option Infer On

Imports System.IO
Imports ConsoleGUI
Imports Microsoft.CodeAnalysis
Imports Xunit

' A program made of several files: Load File / Create File / Unload File / Save All / Set Main Module / SUBs / Switch to.
Public Class MultiFileTests
  Implements IDisposable

  Private ReadOnly _root As String = Path.Combine(Path.GetTempPath(), "QuickVB.Multi." & Guid.NewGuid().ToString("N"))

  Private NotInheritable Class Session
    Public ReadOnly Console As New HeadlessConsole(100, 30)
    Public ReadOnly Ui As MyScreen

    Public Sub New(mainPath As String)
      Me.Ui = New MyScreen(Me.Console)
      Dim ui2 = Me.Ui
      Me.Ui.Post(Sub()
                   ui2.OpenProgram(mainPath)
                   ui2.EnableRoslyn()
                 End Sub)
      Me.Run()
    End Sub

    Public Sub Run()
      Screen.NavigateTo(Me.Ui)
    End Sub

    Public Sub Do_(action As Action)
      Me.Ui.Post(action)
      Me.Run()
    End Sub

    Public Sub Press(key As VirtualKey, Optional state As ControlKeyState = 0)
      Me.Console.QueueKey(key, If(key = VirtualKey.VK_RETURN, ChrW(13), ChrW(0)), state)
      Me.Run()
    End Sub

    Public Sub Type(text As String)
      Me.Console.QueueText(text)
      Me.Run()
    End Sub

    Public Sub Menu(ParamArray letters() As String)
      Me.Console.QueueKey(VirtualKey.VK_MENU)
      For Each letter In letters
        Me.Console.QueueText(letter)
      Next
      Me.Run()
    End Sub

    Public Function Screen_() As String
      Return Me.Console.GetText()
    End Function
  End Class

  Private ReadOnly _mainPath As String
  Private ReadOnly _helperPath As String

  Public Sub New()
    Directory.CreateDirectory(Me._root)
    Me._mainPath = Path.Combine(Me._root, "Main.vb")
    Me._helperPath = Path.Combine(Me._root, "Helper.vb")
    File.WriteAllText(Me._mainPath, "Module MainModule" & vbCrLf & "  Sub Main()" & vbCrLf & "    Greeter.Hello()" & vbCrLf & "  End Sub" & vbCrLf & "End Module")
    File.WriteAllText(Me._helperPath, "Imports QB.Video" & vbCrLf & "Module Greeter" & vbCrLf & "  Sub Hello()" & vbCrLf & "    PRINT(""hi"")" & vbCrLf & "  End Sub" & vbCrLf & "End Module")
  End Sub

  Public Sub Dispose() Implements IDisposable.Dispose
    Try
      Directory.Delete(Me._root, True)
    Catch ex As IOException
    End Try
  End Sub

  Private Shared Function ErrorCount(diagnostics As IEnumerable(Of Diagnostic)) As Integer
    Return diagnostics.Where(Function(d) d.Severity = DiagnosticSeverity.Error).Count()
  End Function

  <Fact>
  Public Sub LoadingAFileAddsItToTheProgramAndShowsIt()
    Dim s As New Session(Me._mainPath)
    Assert.Equal({"Main.vb"}, s.Ui.FileNames.ToArray())

    s.Do_(Sub() s.Ui.LoadFile(Me._helperPath))

    Assert.Equal({"Main.vb", "Helper.vb"}, s.Ui.FileNames.ToArray())
    Assert.Equal("Helper.vb", s.Ui.ActiveFileName)
    Assert.Contains("Module Greeter", s.Ui.Editor.Text)
    Assert.Contains(" Helper.vb ", s.Screen_())
  End Sub

  <Fact>
  Public Sub TheFilesCompileTogether()
    Dim s As New Session(Me._mainPath)
    s.Do_(Sub() s.Ui.LoadFile(Me._helperPath))

    Dim diagnostics = App.TheWorkspace.CompileProgram(background:=True)

    Assert.Equal(0, ErrorCount(diagnostics))
  End Sub

  <Fact>
  Public Sub EditsSurviveSwitchingBetweenFiles()
    Dim s As New Session(Me._mainPath)
    s.Do_(Sub() s.Ui.LoadFile(Me._helperPath))

    s.Type("x")                                         ' typed into Helper.vb
    Dim helperText = s.Ui.Editor.Text
    Assert.StartsWith("x", helperText.TrimStart())

    s.Menu("v", "w")                                    ' View > Switch to...
    Assert.Contains("Switch to", s.Screen_())
    s.Press(VirtualKey.VK_UP)                           ' Main.vb
    s.Press(VirtualKey.VK_RETURN)
    Assert.True("Main.vb" = s.Ui.ActiveFileName, s.Screen_())
    Assert.Contains("Module MainModule", s.Ui.Editor.Text)

    s.Menu("v", "w")
    s.Press(VirtualKey.VK_DOWN)
    s.Press(VirtualKey.VK_RETURN)
    Assert.Equal("Helper.vb", s.Ui.ActiveFileName)
    Assert.Equal(helperText, s.Ui.Editor.Text)
  End Sub

  <Fact>
  Public Sub SaveAllWritesEveryChangedFile()
    Dim s As New Session(Me._mainPath)
    s.Do_(Sub() s.Ui.LoadFile(Me._helperPath))

    s.Type("x")                                         ' changes Helper.vb
    s.Do_(Sub() s.Ui.SaveAll())

    Assert.Contains("x", File.ReadAllText(Me._helperPath))
    Assert.Equal("Module MainModule", File.ReadAllLines(Me._mainPath)(0))
  End Sub

  <Fact>
  Public Sub CreateFileAddsAnEmptyModuleThatIsSavedWithTheProgram()
    Dim s As New Session(Me._mainPath)

    s.Do_(Sub() s.Ui.CreateFile("Extra"))

    Assert.Equal({"Main.vb", "Extra.vb"}, s.Ui.FileNames.ToArray())
    Assert.Equal("Extra.vb", s.Ui.ActiveFileName)
    Assert.Contains("Module Extra", s.Ui.Editor.Text)
    Assert.False(File.Exists(Path.Combine(Me._root, "Extra.vb")))

    s.Do_(Sub() s.Ui.SaveAll())
    Assert.True(File.Exists(Path.Combine(Me._root, "Extra.vb")))
  End Sub

  <Fact>
  Public Sub UnloadingAFileRemovesItFromTheProgram()
    Dim s As New Session(Me._mainPath)
    s.Do_(Sub() s.Ui.LoadFile(Me._helperPath))

    s.Menu("f", "u")                                    ' File > Unload File...
    Assert.Contains("Choose file to unload", s.Screen_())
    s.Press(VirtualKey.VK_RETURN)                       ' Main.vb is selected first

    Assert.Equal({"Helper.vb"}, s.Ui.FileNames.ToArray())
    Assert.Equal("Helper.vb", s.Ui.ActiveFileName)

    s.Menu("f", "u")                                    ' the last file stays
    Assert.Contains("Cannot unload the only file", s.Screen_())
  End Sub

  <Fact>
  Public Sub UnloadingAChangedFileAsksToSaveIt()
    Dim s As New Session(Me._mainPath)
    s.Do_(Sub() s.Ui.LoadFile(Me._helperPath))
    s.Type("x")

    s.Menu("f", "u")
    s.Press(VirtualKey.VK_DOWN)                         ' Helper.vb
    s.Press(VirtualKey.VK_RETURN)
    Assert.Contains("is not saved. Save it now?", s.Screen_())

    s.Type("n")
    Assert.Equal({"Main.vb"}, s.Ui.FileNames.ToArray())
    Assert.StartsWith("Imports QB.Video", File.ReadAllText(Me._helperPath))   ' the unsaved "x" was not written
  End Sub

  <Fact>
  Public Sub TheSubsDialogListsTheFilesAndTheirProceduresAndEditsInTheRightFile()
    Dim s As New Session(Me._mainPath)
    s.Do_(Sub() s.Ui.LoadFile(Me._helperPath))
    s.Do_(Sub() s.Ui.ShowSubsDialog())

    Dim text = s.Screen_()
    Assert.Contains("Main.vb", text)
    Assert.Contains("Helper.vb", text)
    Assert.Contains("Hello", text)

    s.Press(VirtualKey.VK_UP)
    s.Press(VirtualKey.VK_UP)
    s.Press(VirtualKey.VK_UP)                           ' the first entry: Main.vb
    Assert.Contains("Main.vb is the Main Module", s.Screen_())
    s.Press(VirtualKey.VK_DOWN)                         ' Main
    s.Press(VirtualKey.VK_RETURN)

    Assert.Equal("Main.vb", s.Ui.ActiveFileName)
    Assert.Equal(1, s.Ui.EditorView.CursorRow)
  End Sub

  <Fact>
  Public Sub ExitAsksAboutChangesInAnyFile()
    Dim s As New Session(Me._mainPath)
    Dim exited = False
    s.Ui.ExitAction = Sub() exited = True
    s.Do_(Sub() s.Ui.LoadFile(Me._helperPath))
    s.Type("x")                                         ' changes Helper.vb
    s.Do_(Sub() s.Ui.CreateFile("Third"))               ' ... and Third.vb becomes the active file

    s.Do_(Sub() s.Ui.RequestExit())
    Assert.Contains("is not saved", s.Screen_())
    Assert.False(exited)

    s.Type("y")
    Assert.True(exited)
    Assert.True(File.Exists(Path.Combine(Me._root, "Third.vb")))
    Assert.Contains("x", File.ReadAllText(Me._helperPath))
  End Sub

  <Fact>
  Public Sub SeveralMainsNeedAMainModule()
    File.WriteAllText(Me._mainPath, "Module MainModule" & vbCrLf & "  Sub Main()" & vbCrLf & "  End Sub" & vbCrLf & "End Module")
    File.WriteAllText(Me._helperPath, "Module Other" & vbCrLf & "  Sub Main()" & vbCrLf & "  End Sub" & vbCrLf & "End Module")
    Dim s As New Session(Me._mainPath)
    s.Do_(Sub() s.Ui.LoadFile(Me._helperPath))

    ' the first file with a Main wins ...
    Dim first = App.TheWorkspace.CompileProgram(background:=True)
    Assert.True(ErrorCount(first) = 0, String.Join(" | ", first.Select(Function(d) d.ToString())))

    ' ... until another is chosen
    s.Menu("r", "m")                                    ' Run > Set Main Module...
    Assert.Contains("Set Main Module", s.Screen_())
    s.Press(VirtualKey.VK_DOWN)
    s.Press(VirtualKey.VK_RETURN)
    Assert.Equal(0, ErrorCount(App.TheWorkspace.CompileProgram(background:=True)))
  End Sub

  <Fact>
  Public Sub ARunWithAnErrorInAnotherFileShowsThatFile()
    File.WriteAllText(Me._helperPath, "Module Greeter" & vbCrLf & "  Sub Hello()" & vbCrLf & "    Dim x As Integer = " & vbCrLf & "  End Sub" & vbCrLf & "End Module")
    Dim s As New Session(Me._mainPath)
    s.Do_(Sub() s.Ui.LoadFile(Me._helperPath))
    s.Do_(Sub() s.Ui.ShowSwitchToDialogForTests(0))      ' back to Main.vb
    Assert.Equal("Main.vb", s.Ui.ActiveFileName)

    s.Press(VirtualKey.VK_F5)
    Assert.Contains("Syntax error", s.Screen_())
    s.Press(VirtualKey.VK_RETURN)

    Assert.Equal("Helper.vb", s.Ui.ActiveFileName)
    Assert.Equal(2, s.Ui.EditorView.CursorRow)
  End Sub

End Class
