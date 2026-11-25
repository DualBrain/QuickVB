Option Explicit On
Option Strict On
Option Infer On

Imports ConsoleGUI

' The Help button of the dialogs: a short explanation of the dialog, shown over it.
Partial Public Class MyScreen

  Private Shared ReadOnly DialogHelpText As New Dictionary(Of String, String())(StringComparer.OrdinalIgnoreCase) From {
    {"Open", {"Choose the program to open. Type a name or a pattern such as *.vb,", "or pick from the lists with Tab and the arrow keys. A .vbproj,", ".sln or .slnx opens all of its Visual Basic files."}},
    {"Save", {"Type the name to save the program under, or pick a folder", "and file from the lists."}},
    {"Save As", {"Type the name to save the program under, or pick a folder", "and file from the lists. An existing file is only replaced", "when you agree."}},
    {"Load File", {"Adds a file to the program. The files of a program are", "compiled together; View > Switch to moves between them."}},
    {"Merge", {"Inserts the text of a file into the program at the cursor."}},
    {"Create File", {"Starts a new, empty module and adds it to the program.", "It is written to disk when you save."}},
    {"Unload File", {"Takes the chosen file out of the program. Changes that", "were not saved can be saved first."}},
    {"Switch to", {"Shows another file of the program in the editor."}},
    {"Set Main Module", {"When several files have a Main, this one starts the program."}},
    {"SUBs", {"Lists the files of the program and their SUBs and FUNCTIONs.", "Edit in Active shows the chosen item; Delete removes a procedure."}},
    {"New SUB", {"Type the name of the new SUB. It is added at the end of the module."}},
    {"New FUNCTION", {"Type the name of the new FUNCTION. It is added at the end", "of the module."}},
    {"Find", {"Type the text to look for. F3 repeats the search; Ctrl+\ looks", "for the selected text."}},
    {"Change", {"Find and Verify asks about each match, Change All replaces", "every one. Both can be undone with Alt+Backspace."}},
    {"Print", {"Printing is not available yet."}},
    {"Display", {"Colors, scroll bars and tab stops of the editor. The choices", "are kept for the next time."}},
    {"Set Paths", {"Folders searched for the files of a program."}},
    {"Right Mouse", {"What the right mouse button does in the editor: show help", "or run the program up to the cursor."}},
    {"Add Watch", {"Type an expression whose value is shown in the Watch window", "while the program is stopped."}},
    {"Watchpoint", {"Type an expression; the program stops when it becomes true."}},
    {"Delete Watch", {"Choose the watch expression to remove."}},
    {"Instant Watch", {"The value of the expression at the cursor right now.", "Add Watch keeps it in the Watch window."}},
    {"Modify COMMAND$", {"The text the program receives as COMMAND$."}},
    {"Make EXE File", {"Writes a program that runs without QuickVB. It needs the", ".NET runtime and the QuickBasic.dll next to it."}},
    {"Make Library", {"Writes the program as a library (.dll) other programs can use."}}
  }

  ''' <summary>The Help button of a dialog: explains the dialog that is open.</summary>
  Public Sub ShowDialogHelp()

    Dim title = If(TryCast(Me.PopupControl, FormDialog)?.Title, TryCast(Me.PopupControl, FileDialog)?.Title)
    Dim lines As String() = Nothing

    If title Is Nothing OrElse Not DialogHelpText.TryGetValue(title, lines) Then
      lines = {If(title Is Nothing, "This box needs no further explanation.", "No help is available for this dialog.")}
    End If

    Dim d As New FormDialog() With {.Title = "Help", .MinimumWidth = Math.Max(30, lines.Max(Function(l) l.Length) + 6)}
    For Each line In lines
      d.AddText(line, centered:=False)
    Next
    d.AddButton("<  OK  >", "O"c, isDefault:=True, isCancel:=True)
    d.Show(Me)

  End Sub

End Class
