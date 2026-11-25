' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Imports ConsoleGUI

''' <summary>The help topics: the keyboard reference pages and the command line, laid out like QuickBASIC's help screens.</summary>
Public Module HelpContent

  Public Const ContentsId As String = "contents"
  Public Const IndexId As String = "index"
  Public Const HelpOnHelpId As String = "helponhelp"
  Public Const SupportId As String = "support"
  Public Const ShortcutId As String = "shortcut"
  Public Const EditingId As String = "editing"
  Public Const CursorId As String = "cursor"
  Public Const ScrollingId As String = "scrolling"
  Public Const SelectingId As String = "selecting"
  Public Const InsertCopyId As String = "insertcopy"
  Public Const DeleteId As String = "delete"
  Public Const ViewKeysId As String = "viewkeys"
  Public Const SearchKeysId As String = "searchkeys"
  Public Const DebuggingId As String = "debugkeys"
  Public Const HelpKeysId As String = "helpkeys"
  Public Const CommandLineId As String = "commandline"

  ''' <summary>The order Ctrl+F1 steps through the topics.</summary>
  Public ReadOnly Order As String() = {ContentsId, IndexId, HelpOnHelpId, SupportId, ShortcutId, EditingId, CursorId, ScrollingId, SelectingId,
                                       InsertCopyId, DeleteId, ViewKeysId, SearchKeysId, DebuggingId, HelpKeysId, CommandLineId}

  Private _topics As Dictionary(Of String, HelpTopic)

  ''' <summary>Finds a topic (Nothing when there is none with that id).</summary>
  Public Function Find(id As String) As HelpTopic
    If _topics Is Nothing Then _topics = Build()
    Dim topic As HelpTopic = Nothing
    Return If(_topics.TryGetValue(id, topic), topic, Nothing)
  End Function

  Public ReadOnly Property Topics As IReadOnlyCollection(Of HelpTopic)
    Get
      If _topics Is Nothing Then _topics = Build()
      Return _topics.Values
    End Get
  End Property

  ' ---- building blocks

  Private Const Rule As String = "<hr>"

  Private Function Nav(current As String, currentId As String, ParamArray others() As (Text As String, Id As String)) As List(Of HelpNavItem)
    Dim items As New List(Of HelpNavItem) From {New HelpNavItem(current, currentId, isCurrent:=True)}
    For Each other In others
      items.Add(New HelpNavItem(other.Text, other.Id))
    Next
    Return items
  End Function

  Private Function Standard(current As String, currentId As String) As List(Of HelpNavItem)
    Return Nav(current, currentId, ("Contents", ContentsId), ("Index", IndexId))
  End Function

  Private Function Editing(current As String, currentId As String) As List(Of HelpNavItem)
    Return New List(Of HelpNavItem) From {New HelpNavItem("Editing Keys", EditingId),
                                         New HelpNavItem(current, currentId, isCurrent:=True),
                                         New HelpNavItem("Contents", ContentsId),
                                         New HelpNavItem("Index", IndexId)}
  End Function

  ''' <summary>A line with up to three columns of text: the label, then one or two columns of keys.</summary>
  Private Function Columns(label As String, indent As Integer, column2 As Integer, text2 As String, Optional column3 As Integer = 0, Optional text3 As String = "") As String
    Dim line = New String(" "c, indent) & label
    If text2.Length > 0 Then line = line.PadRight(column2) & text2
    If text3.Length > 0 Then line = line.PadRight(column3) & text3
    Return line
  End Function

  Private Function BoxTop(title As String, width As Integer) As String
    Return ChrW(&H250C) & " " & title & " " & New String(ChrW(&H2500), width - 4 - title.Length) & ChrW(&H2510)
  End Function

  Private Function BoxRow(label As String, keys As String, width As Integer) As String
    Return ChrW(&H2502) & " " & label.PadRight(width - 4 - keys.Length) & keys & " " & ChrW(&H2502)
  End Function

  Private Function BoxBottom(width As Integer) As String
    Return ChrW(&H2514) & New String(ChrW(&H2500), width - 2) & ChrW(&H2518)
  End Function

  Private Function Box(title As String, width As Integer, ParamArray rows() As (Label As String, Keys As String)) As List(Of String)
    Dim lines As New List(Of String) From {BoxTop(title, width)}
    For Each row In rows
      lines.Add(BoxRow(row.Label, row.Keys, width))
    Next
    lines.Add(BoxBottom(width))
    Return lines
  End Function

  ' ---- the topics

  Private Function Build() As Dictionary(Of String, HelpTopic)

    Dim topics As New Dictionary(Of String, HelpTopic)()

    Dim add = Sub(topic As HelpTopic) topics(topic.Id) = topic

    add(New HelpTopic(ContentsId, "Contents", Nav("Contents", ContentsId, ("Index", IndexId)),
                      {"QuickVB Help",
                       Rule,
                       " {{Keys}}",
                       "     [[Shortcut Keys|" & ShortcutId & "]]",
                       "     [[Editing Keys|" & EditingId & "]]",
                       "     [[View Keys|" & ViewKeysId & "]]",
                       "     [[Search Keys|" & SearchKeysId & "]]",
                       "     [[Debugging Keys|" & DebuggingId & "]]",
                       "     [[Help Keys|" & HelpKeysId & "]]",
                       "",
                       " {{Reference}}",
                       "     [[QuickVB Command Line|" & CommandLineId & "]]",
                       "     [[Help on Help|" & HelpOnHelpId & "]]",
                       "     [[Product Support|" & SupportId & "]]"}))

    add(New HelpTopic(IndexId, "Index", Nav("Index", IndexId, ("Contents", ContentsId)),
                      {"Index of topics",
                       Rule,
                       "     [[Command Line, QuickVB|" & CommandLineId & "]]",
                       "     [[Cursor Movement Keys|" & CursorId & "]]",
                       "     [[Debugging Keys|" & DebuggingId & "]]",
                       "     [[Delete Keys|" & DeleteId & "]]",
                       "     [[Editing Keys|" & EditingId & "]]",
                       "     [[Help Keys|" & HelpKeysId & "]]",
                       "     [[Help on Help|" & HelpOnHelpId & "]]",
                       "     [[Insert and Copy Keys|" & InsertCopyId & "]]",
                       "     [[Product Support|" & SupportId & "]]",
                       "     [[Search Keys|" & SearchKeysId & "]]",
                       "     [[Shortcut Keys|" & ShortcutId & "]]",
                       "     [[Text Scrolling Keys|" & ScrollingId & "]]",
                       "     [[Text Selection Keys|" & SelectingId & "]]",
                       "     [[View Keys|" & ViewKeysId & "]]"}))

    add(New HelpTopic(HelpOnHelpId, "Help on Help", Nav("Help on Help", HelpOnHelpId, ("Contents", ContentsId), ("Index", IndexId), ("Product Support", SupportId)),
                      {"Use help as follows:",
                       "",
                       "  " & ChrW(&H25A0) & " To get help at any time, {{press F1}}.",
                       "  " & ChrW(&H25A0) & " To get help on any part of a BASIC statement, a QuickVB menu item",
                       "    or dialog box, position the cursor there {{press F1}}.",
                       "  " & ChrW(&H25A0) & " To remove the help information from the screen, {{press ESC}}.",
                       "  " & ChrW(&H25A0) & " To scroll help information, {{press PGDN}}.",
                       "",
                       "Look for the help display in either",
                       "  " & ChrW(&H25A0) & " The Help window at the top of the screen",
                       "  " & ChrW(&H25A0) & " A dialog box which pops up in the center of the screen",
                       "",
                       "Move between hyperlinks with {{TAB}}, follow one with {{ENTER}},",
                       "and return to the previous topic with {{ALT+F1}}.",
                       "",
                       "See [[Help Keys|" & HelpKeysId & "]] for all of the keys that work in the Help window."}))

    add(New HelpTopic(SupportId, "Product Support", Nav("Product Support", SupportId, ("Contents", ContentsId), ("Index", IndexId)),
                      {"QuickVB is an open source project.",
                       "",
                       "The project page, with the source code and the place to report",
                       "problems and suggestions, is",
                       "",
                       "     {{https://github.com/DualBrain/QuickVB}}",
                       "",
                       "QuickVB is based on the QuickVB sample of the Visual Basic team at",
                       "Microsoft, and is powered by the .NET Compiler Platform (""Roslyn"")."}))

    ' ---- Shortcut Keys: two columns of boxes

    Dim left As New List(Of String)()
    left.AddRange(Box("Inserting", 36, ("Toggle insert/overtype", "INS"), ("Line above", "HOME CTRL+N"), ("Line below", "END ENTER"), ("From clipboard", "SHIFT+INS")))
    left.AddRange(Box("Selecting", 36, ("Characters/lines", "SHIFT+ARROW"), ("Words", "SHIFT+CTRL+ARROW")))
    left.AddRange(Box("Deleting", 36, ("Current line (save)", "CTRL+Y"), ("To end of line (save)", "CTRL+Q+Y"), ("Selected text (save)", "SHIFT+DEL"), ("Selected text (no save)", "DEL")))

    Dim right As New List(Of String)()
    right.AddRange(Box("Copying", 30, ("Selected text", "CTRL+INS")))
    right.AddRange(Box("Finding", 30, ("Selected text", "CTRL+\"), ("Repeat find", "F3")))
    right.AddRange(Box("Debugging", 30, ("View output screen", "F4"), ("Continue", "F5"), ("Execute to cursor", "F7"), ("Toggle breakpoint", "F9"),
                                         ("Single step", "F8"), ("Procedure step", "F10"), ("Instant Watch", "SHIFT+F9")))

    Dim shortcut As New List(Of String) From {Rule}
    For i = 0 To Math.Max(left.Count, right.Count) - 1
      shortcut.Add("    " & If(i < left.Count, left(i), "").PadRight(36) & " " & If(i < right.Count, right(i), ""))
    Next

    add(New HelpTopic(ShortcutId, "Shortcut Keys", Standard("Shortcut Keys", ShortcutId), shortcut.Skip(1)))

    ' ---- Editing Keys

    add(New HelpTopic(EditingId, "Editing Keys", Standard("Editing Keys", EditingId),
                      {"Editing keys",
                       Rule,
                       "     [[Cursor Movement Keys|" & CursorId & "]]",
                       "     [[Text Scrolling Keys|" & ScrollingId & "]]",
                       "     [[Text Selection Keys|" & SelectingId & "]]",
                       "     [[Insert and Copy Keys|" & InsertCopyId & "]]",
                       "     [[Delete Keys|" & DeleteId & "]]"}))

    Dim cm = Function(label As String, k1 As String, k2 As String) Columns(label, 1, 44, k1, 64, k2)

    add(New HelpTopic(CursorId, "Cursor Movement Keys", Editing("Cursor Movement", CursorId),
                      {cm("Character left", "LEFT", "CTRL+S"),
                       cm("Character right", "RIGHT", "CTRL+D"),
                       cm("Word left", "CTRL+LEFT", "CTRL+A"),
                       cm("Word right", "CTRL+RIGHT", "CTRL+F"),
                       cm("Line up", "UP", "CTRL+E"),
                       cm("Line down", "DOWN", "CTRL+X"),
                       Rule,
                       cm("First indentation level of current line", "HOME", ""),
                       cm("Beginning of current line", "", "CTRL+Q+S"),
                       cm("Beginning of next line", "CTRL+ENTER", "CTRL+J"),
                       cm("End of line", "END", "CTRL+Q+D"),
                       Rule,
                       cm("Top of window", "", "CTRL+Q+E"),
                       cm("Bottom of window", "", "CTRL+Q+X"),
                       Rule,
                       cm("Beginning of module/procedure", "CTRL+HOME", "CTRL+Q+R"),
                       cm("End of module/procedure", "CTRL+END", "CTRL+Q+C"),
                       Rule,
                       cm("Set place marker number 'n' (where n = 0-3)", "", "CTRL+K+n"),
                       cm("Move to place marker number 'n'", "", "CTRL+Q+n"),
                       Rule}))

    Dim sc = Function(label As String, k1 As String, k2 As String) Columns(label, 5, 42, k1, 64, k2)

    add(New HelpTopic(ScrollingId, "Text Scrolling Keys", Editing("Scrolling", ScrollingId),
                      {sc("Line up", "CTRL+UP", "CTRL+W"),
                       sc("Line down", "CTRL+DOWN", "CTRL+Z"),
                       sc("Page up", "PGUP", "CTRL+R"),
                       sc("Page down", "PGDN", "CTRL+C"),
                       Rule,
                       sc("Left one window", "CTRL+PGUP", ""),
                       sc("Right one window", "CTRL+PGDN", ""),
                       Rule}))

    Dim sel = Function(label As String, k1 As String) Columns(label, 5, 45, k1)

    add(New HelpTopic(SelectingId, "Text Selection Keys", Editing("Select Text", SelectingId),
                      {sel("Character left", "SHIFT+LEFT"),
                       sel("Character right", "SHIFT+RIGHT"),
                       sel("Word left", "SHIFT+CTRL+LEFT"),
                       sel("Word right", "SHIFT+CTRL+RIGHT"),
                       Rule,
                       sel("Current line", "SHIFT+DOWN"),
                       sel("Line above", "SHIFT+UP"),
                       sel("Screen up", "SHIFT+PGUP"),
                       sel("Screen down", "SHIFT+PGDN"),
                       sel("To beginning of module/procedure", "SHIFT+CTRL+HOME"),
                       sel("To end of module/procedure", "SHIFT+CTRL+END"),
                       Rule}))

    Dim ic = Function(label As String, k1 As String, k2 As String) Columns(label, 1, 52, k1, 66, k2)

    add(New HelpTopic(InsertCopyId, "Insert and Copy Keys", Editing("Insert and Copy", InsertCopyId),
                      {ic("Keyboard insert mode on or off", "INS", "CTRL+V"),
                       Rule,
                       ic("Copy selected text to the Clipboard, keeping it", "CTRL+INS", ""),
                       Rule,
                       ic("Copy selected text to Clipboard, deleting it", "SHIFT+DEL", ""),
                       ic("Copy current line to Clipboard, deleting it", "", "CTRL+Y"),
                       ic("Copy to end of line to Clipboard, deleting it", "", "CTRL+Q+Y"),
                       Rule,
                       ic("Insert contents of Clipboard", "SHIFT+INS", ""),
                       ic("Insert blank line below", "END ENTER", ""),
                       ic("Insert blank line above", "", "HOME+CTRL+N"),
                       Rule}))

    add(New HelpTopic(DeleteId, "Delete Keys", Editing("Delete", DeleteId),
                      {ic("Delete character to left of cursor", "BKSP", "CTRL+H"),
                       ic("Delete character at cursor", "DEL", "CTRL+G"),
                       ic("Delete rest of word cursor is on", "", "CTRL+T"),
                       Rule,
                       ic("Delete selected text", "DEL", "CTRL+G"),
                       ic("Delete leading spaces from selected lines", "SHIFT+TAB", ""),
                       Rule}))

    Dim vk = Function(label As String, k1 As String) Columns(label, 1, 67, k1)

    add(New HelpTopic(ViewKeysId, "View Keys", Nav("View Keys", ViewKeysId, ("Contents", ContentsId), ("Index", IndexId)),
                      {vk("Toggle display of the output screen and the View window", "F4"),
                       Rule,
                       vk("Display a list of loaded SUBs, modules, and files", "F2"),
                       vk("Display the next procedure", "SHIFT+F2"),
                       vk("Display the previous procedure", "CTRL+F2"),
                       Rule,
                       vk("Make the next window the active window", "F6"),
                       vk("Make the previous window the active window", "SHIFT+F6"),
                       Rule,
                       vk("Toggle between multiple windows and full screen active window", "CTRL+F10"),
                       vk("Increase size of active window", "ALT+PLUS"),
                       vk("Decrease size of active window", "ALT+MINUS"),
                       Rule}))

    Dim sk = Function(label As String, k1 As String) Columns(label, 5, 64, k1)

    add(New HelpTopic(SearchKeysId, "Search Keys", Standard("Search Keys", SearchKeysId),
                      {sk("Search for selected (highlighted) text", "Ctrl+\"),
                       sk("Repeat the last find", "F3")}, ruleTitle:="Search Keys"))

    Dim dk = Function(label As String, k1 As String) Columns(label, 1, 59, k1)

    add(New HelpTopic(DebuggingId, "Debugging Keys", Standard("Debugging Keys", DebuggingId),
                      {dk("Start program execution from beginning", "SHIFT+F5"),
                       dk("Continue program execution from current statement", "F5"),
                       dk("Execute program to current cursor position", "F7"),
                       Rule,
                       dk("Execute next program statement as a single step", "F8"),
                       dk("Single step, tracing around a procedure call", "F10"),
                       dk("Trace execution history backward", "SHIFT+F8"),
                       dk("Trace execution history forward", "SHIFT+F10"),
                       Rule,
                       dk("Toggle the Debug menu Breakpoint command", "F9"),
                       dk("Instant Watch", "SHIFT+F9"),
                       "(with mouse, the Instant Watch shortcut is SHIFT+Right mouse button)",
                       Rule}))

    Dim hk = Function(label As String, k1 As String) Columns(label, 1, 59, k1)

    add(New HelpTopic(HelpKeysId, "Help Keys", Standard("Help Keys", HelpKeysId),
                      {hk("Look at help on the item where the cursor is located", "F1 (or click right"),
                       Columns("", 0, 64, "mouse button)"),
                       hk("Clear help from the screen", "ESC"),
                       Rule,
                       hk("Look at Help on Help", "SHIFT+F1"),
                       hk("Look at help through Help menu commands", "ALT+H"),
                       Rule,
                       hk("Move cursor to next hyperlink", "TAB"),
                       hk("Move cursor to previous hyperlink", "SHIFT+TAB"),
                       hk("Move cursor to next hyperlink with starting character", "character"),
                       hk("Move cursor to previous hyperlink with starting character", "SHIFT+character"),
                       hk("Look at previous hyperlink topic (repeat up to 20 times)", "ALT+F1"),
                       Rule,
                       hk("Look at help topic stored next in the Help file", "CTRL+F1"),
                       hk("Look at help topic stored previously in the Help file", "SHIFT+CTRL+F1"),
                       Rule}))

    ' ---- the command line

    Dim option_ = Function(name As String, text As String) Columns(name, 0, 24, text)

    add(New HelpTopic(CommandLineId, "QuickVB Command Line", Standard("QuickVB Command Line", CommandLineId),
                      {"QuickVB Command Line Options",
                       "",
                       "The full syntax for the QuickVB command is the following:",
                       "",
                       "quickvb [options] [program [text for COMMAND$...]]",
                       "",
                       "Options start with - (one letter) or -- (a word). Put -- in front of",
                       "text for the program that itself starts with a dash.",
                       "",
                       "Option                  Description",
                       "",
                       option_("program", "Names the program (.bas, .vb) or the project or"),
                       option_("", "solution (.vbproj, .sln, .slnx) to open."),
                       "",
                       option_("text", "Everything after the program is passed to it as"),
                       option_("", "COMMAND$."),
                       "",
                       option_("-r, --run", "Runs the program before displaying it."),
                       "",
                       option_("--colors 16", "Draws with the terminal's own 16 colors instead"),
                       option_("", "of the VGA color palette QuickVB normally uses."),
                       option_("--colors truecolor", "Draws with the VGA palette whatever the terminal"),
                       option_("", "says about its colors."),
                       "",
                       option_("--classic", "Draws with the classic Windows console instead"),
                       option_("", "of VT/ANSI escape sequences. Use it if your"),
                       option_("", "terminal does not support them."),
                       "",
                       option_("-h, --help, -?", "Shows the options and ends."),
                       option_("--version", "Shows the version and ends."),
                       "",
                       "The same choices can be made with the QUICKVB_COLORS (16 or truecolor)",
                       "and QUICKVB_RENDERER (win32) environment variables."}))

    Return topics

  End Function

End Module
