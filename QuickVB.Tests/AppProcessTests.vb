Option Explicit On
Option Strict On
Option Infer On

Imports System.IO
Imports Xunit

' QuickVB started as a process, the way a user starts it: what it says and the exit code it ends with.
Public Class AppProcessTests

  Private Shared Function Start(ParamArray args() As String) As (ExitCode As Integer, Output As String, Errors As String)

    Dim dll = Path.Combine(AppContext.BaseDirectory, "QuickVB.dll")
    Dim info As New ProcessStartInfo("dotnet") With {.RedirectStandardOutput = True, .RedirectStandardError = True, .RedirectStandardInput = True, .UseShellExecute = False}
    info.ArgumentList.Add(dll)
    For Each arg In args
      info.ArgumentList.Add(arg)
    Next

    Using proc = Process.Start(info)
      proc.StandardInput.Close()
      Dim output = proc.StandardOutput.ReadToEndAsync()
      Dim errors = proc.StandardError.ReadToEndAsync()
      Assert.True(proc.WaitForExit(60000), "QuickVB did not end")
      Return (proc.ExitCode, output.Result, errors.Result)
    End Using

  End Function

  <Fact>
  Public Sub VersionIsPrintedAndQuickVBEnds()
    Dim r = Start("--version")

    Assert.Equal(0, r.ExitCode)
    Assert.Matches("^\d+\.\d+\.\d+", r.Output.Trim())
  End Sub

  <Fact>
  Public Sub HelpIsPrintedAndQuickVBEnds()
    Dim r = Start("--help")

    Assert.Equal(0, r.ExitCode)
    Assert.Contains("--run", r.Output)
    Assert.Contains("--colors", r.Output)
  End Sub

  <Fact>
  Public Sub AMistakeOnTheCommandLineIsExplainedAndEndsWithOne()
    Dim r = Start("--nonsense")

    Assert.Equal(1, r.ExitCode)
    Assert.Contains("--nonsense", r.Errors)
  End Sub

  <Fact>
  Public Sub AProgramThatIsNotThereIsReported()
    Dim r = Start("no-such-program.bas")

    Assert.Equal(1, r.ExitCode)
    Assert.Contains("File not found", r.Errors)
  End Sub

  <Fact>
  Public Sub WithoutATerminalQuickVBSaysSoAndEndsWithTwo()
    Dim r = Start()

    Assert.Equal(2, r.ExitCode)
    Assert.Contains("interactive terminal", r.Errors)
  End Sub

End Class
