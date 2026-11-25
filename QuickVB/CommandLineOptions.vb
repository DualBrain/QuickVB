' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Imports System.CommandLine
Imports System.CommandLine.Invocation
Imports System.IO

''' <summary>
''' The QuickVB command line, in the usual POSIX style (parsed by System.CommandLine):
''' <c>quickvb [options] [program [text for COMMAND$...]]</c>. <c>--help</c> and <c>--version</c> come with it.
''' </summary>
Public NotInheritable Class CommandLineOptions

  ''' <summary>The program or project to load at start-up, or Nothing.</summary>
  Public Property ProgramName As String

  ''' <summary>--run: run the program before showing it.</summary>
  Public Property RunProgram As Boolean

  ''' <summary>--colors 16: use the terminal's own 16 colours.</summary>
  Public Property SixteenColors As Boolean

  ''' <summary>--colors truecolor: use 24 bit colour whatever the terminal says.</summary>
  Public Property TrueColors As Boolean

  ''' <summary>--classic: draw with the classic Windows console instead of VT escape sequences.</summary>
  Public Property ClassicRenderer As Boolean

  ''' <summary>The text the program gets as COMMAND$: what follows the program name.</summary>
  Public Property CommandText As String = ""

  Public Const Usage As String = "Usage: quickvb [options] [program [text for COMMAND$...]]"

  ''' <summary>What came of reading a command line.</summary>
  Public NotInheritable Class Result

    ''' <summary>The options to start with; Nothing when there is nothing to start (help or the version was shown, or the command line was wrong).</summary>
    Public Property Options As CommandLineOptions

    ''' <summary>0, or 1 when the command line was wrong.</summary>
    Public Property ExitCode As Integer

  End Class

  ''' <summary>Reads <paramref name="args"/>. Help, the version and any complaint go to <paramref name="output"/> and <paramref name="errors"/> (the console when Nothing).</summary>
  Public Shared Function Parse(args As IEnumerable(Of String), Optional output As TextWriter = Nothing, Optional errors As TextWriter = Nothing) As Result

    Dim program As New Argument(Of String)("program") With {
      .Description = "A program (.bas, .vb) or a project or solution (.vbproj, .sln, .slnx) to open.",
      .Arity = ArgumentArity.ZeroOrOne}

    Dim commandText As New Argument(Of String())("command-text") With {
      .Description = "Text the program receives as COMMAND$. Put -- in front of it when it starts with a dash.",
      .Arity = ArgumentArity.ZeroOrMore}

    Dim run As New [Option](Of Boolean)("--run", "-r") With {
      .Description = "Run the program before showing it (needs a program)."}

    Dim colors As New [Option](Of String)("--colors") With {
      .Description = "Colours to draw with: 16 (the terminal's own palette) or truecolor (the VGA palette)."}
    colors.AcceptOnlyFromAmong("16", "truecolor")

    Dim classic As New [Option](Of Boolean)("--classic") With {
      .Description = "Draw with the classic Windows console instead of VT/ANSI escape sequences (Windows only)."}

    Dim arguments = If(args, Enumerable.Empty(Of String)()).ToArray()

    Dim root As New RootCommand("QuickVB - a QuickBASIC-style editor and debugger for Visual Basic.")
    root.Arguments.Add(program)
    root.Arguments.Add(commandText)
    root.Options.Add(run)
    root.Options.Add(colors)
    root.Options.Add(classic)

    root.Validators.Add(Sub(commandResult)
                          If commandResult.GetValue(run) AndAlso commandResult.GetValue(program) Is Nothing Then
                            commandResult.AddError("--run needs a program to run.")
                          End If
                        End Sub)

    ' something that looks like an option but isn't one must not be taken for the program or the text (unless -- says so)
    root.Validators.Add(Sub(commandResult)
                          If arguments.Contains("--") Then Return
                          Dim words = New List(Of String)()
                          Dim name = commandResult.GetValue(program)
                          If name IsNot Nothing Then words.Add(name)
                          words.AddRange(If(commandResult.GetValue(commandText), Array.Empty(Of String)()))
                          For Each word In words
                            If word.Length > 1 AndAlso word(0) = "-"c Then commandResult.AddError("Unrecognized option '" & word & "'.")
                          Next
                        End Sub)

    ' --version describes this program, whatever started it
    For Each existing In root.Options.OfType(Of VersionOption)()
      existing.Action = New VersionAction()
    Next

    Dim options As CommandLineOptions = Nothing

    root.SetAction(Sub(parsed)
                     options = New CommandLineOptions With {
                       .ProgramName = parsed.GetValue(program),
                       .RunProgram = parsed.GetValue(run),
                       .ClassicRenderer = parsed.GetValue(classic),
                       .SixteenColors = String.Equals(parsed.GetValue(colors), "16", StringComparison.Ordinal),
                       .TrueColors = String.Equals(parsed.GetValue(colors), "truecolor", StringComparison.Ordinal),
                       .CommandText = String.Join(" ", If(parsed.GetValue(commandText), Array.Empty(Of String)()))}
                   End Sub)

    Dim configuration As New InvocationConfiguration()
    If output IsNot Nothing Then configuration.Output = output
    If errors IsNot Nothing Then configuration.Error = errors

    Dim exitCode = root.Parse(arguments).Invoke(configuration)

    Return New Result With {.Options = options, .ExitCode = exitCode}

  End Function

  ''' <summary>Applies the display options (they are read from the environment when the screen starts).</summary>
  Public Sub ApplyEnvironment()
    If Me.SixteenColors Then Environment.SetEnvironmentVariable("QUICKVB_COLORS", "16")
    If Me.TrueColors Then Environment.SetEnvironmentVariable("QUICKVB_COLORS", "truecolor")
    If Me.ClassicRenderer Then Environment.SetEnvironmentVariable("QUICKVB_RENDERER", "win32")
  End Sub

End Class

''' <summary>--version: the version of QuickVB (the entry assembly may be something else, for example a test host).</summary>
Friend NotInheritable Class VersionAction
  Inherits SynchronousCommandLineAction

  Public Overrides Function Invoke(parseResult As ParseResult) As Integer
    Dim attribute = GetType(CommandLineOptions).Assembly.GetCustomAttributes(GetType(System.Reflection.AssemblyInformationalVersionAttribute), False).
                      Cast(Of System.Reflection.AssemblyInformationalVersionAttribute)().FirstOrDefault()
    Dim version = If(attribute IsNot Nothing, attribute.InformationalVersion.Split("+"c)(0), GetType(CommandLineOptions).Assembly.GetName().Version.ToString())
    parseResult.InvocationConfiguration.Output.WriteLine(version)
    Return 0
  End Function

End Class
