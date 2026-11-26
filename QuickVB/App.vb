' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

'Imports ConsoleGUI
Imports System.Reflection
Imports System.IO


Module App

  Public TheWorkspace As QuickVBWorkspace

  Private TheScreen As MyScreen

  Public Sub NewUntitledWorkspace()

    Try
      TheWorkspace = New QuickVBWorkspace()
    Catch ex As ReflectionTypeLoadException
      Dim sb = New Text.StringBuilder()
      For Each exSub In ex.LoaderExceptions
        If exSub IsNot Nothing Then sb.AppendLine(exSub.Message)
      Next
      TheScreen.SetStatus("Unable to create the workspace: " & sb.ToString().Replace(vbCr, " ").Replace(vbLf, " "), True)
    End Try

    If TheWorkspace IsNot Nothing Then
      Dim projectId = TheWorkspace.CreateProject("Untitled", "Untitled.exe")
      Dim documentId = TheWorkspace.CreateDocument(projectId, "Untitled.vb")
    End If

  End Sub

  ''' <summary>Starts a fresh workspace holding the single program in <paramref name="fileName"/>.</summary>
  Public Sub OpenWorkspace(fileName As String)
    OpenWorkspace(System.IO.Path.GetFileNameWithoutExtension(fileName), {System.IO.Path.GetFileName(fileName)})
  End Sub

  ''' <summary>Starts a fresh workspace for a program named <paramref name="programName"/> made of the files <paramref name="documentNames"/>.</summary>
  Public Sub OpenWorkspace(programName As String, documentNames As IEnumerable(Of String), Optional programOptions As ProgramOptions = Nothing)

    TheWorkspace = New QuickVBWorkspace()

    Dim projectId = TheWorkspace.CreateProject(programName, programName & ".exe", programOptions)

    For Each name In documentNames
      TheWorkspace.CreateDocument(projectId, name)
    Next

  End Sub

  Function Main(args As String()) As Integer

    Dim parsed = CommandLineOptions.Parse(args)

    ' --help, --version or a mistake on the command line: said already
    If parsed.Options Is Nothing Then Return parsed.ExitCode

    Dim options = parsed.Options

    ' a program named on the command line is found relative to where QuickVB was started
    Dim programPath = If(options.ProgramName Is Nothing, Nothing, Path.GetFullPath(options.ProgramName))

    If programPath IsNot Nothing AndAlso Not File.Exists(programPath) Then
      Console.Error.WriteLine("File not found: " & programPath)
      Return 1
    End If

    If Console.IsInputRedirected OrElse Console.IsOutputRedirected Then
      Console.Error.WriteLine("QuickVB needs an interactive terminal: its input and output can't be redirected.")
      Return 2
    End If

    options.ApplyEnvironment()

    ' Programs are built in a folder of QuickVB's own; folders left behind by a QuickVB that crashed are cleared away.
    AppFolders.RemoveStaleWorkFolders()

    Dim previousTitle = RememberTitle()

    ' Ctrl+C (or Ctrl+Break) while a program runs from here stops that program, not QuickVB
    AddHandler Console.CancelKeyPress,
      Sub(sender, e)
        If TheScreen IsNot Nothing AndAlso TheScreen.IsConsoleSuspended Then e.Cancel = True
      End Sub

    Try

      TheScreen = New MyScreen
      TheScreen.CommandLineText = options.CommandText
      TheScreen.SettingsPath = AppFolders.SettingsPath
      TheScreen.Post(AddressOf TheScreen.LoadSettings)

      TheScreen.Post(Sub()
                       If programPath IsNot Nothing Then
                         TheScreen.OpenProgram(programPath)
                         TheScreen.EnableRoslyn()
                         If options.RunProgram Then TheScreen.StartProgram()
                       Else
                         TheScreen.StartNewProgram()
                         TheScreen.EnableRoslyn()
                         TheScreen.ShowWelcome()
                       End If
                     End Sub)

      ConsoleGUI.Screen.NavigateTo(TheScreen)

    Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is InvalidOperationException OrElse TypeOf ex Is UnauthorizedAccessException OrElse TypeOf ex Is PlatformNotSupportedException

      ' the terminal is given back before anything is said about it
      TheScreen?.Dispose()
      TheScreen = Nothing
      Console.Error.WriteLine("QuickVB stopped: " & ex.Message)
      Return 1

    Finally

      If TheScreen IsNot Nothing Then TheScreen.Dispose()
      If previousTitle IsNot Nothing Then Console.Title = previousTitle

    End Try

    Return 0

  End Function

  ' Windows can say what the title of the window was (so that it can be put back); other systems can't.
  Private Function RememberTitle() As String
    If Not OperatingSystem.IsWindows() Then Return Nothing
    Try
      Return Console.Title
    Catch ex As IOException
      Return Nothing
    End Try
  End Function

End Module