Option Strict On
Option Infer On

Imports System.IO
Imports ConsoleGUI
Imports Xunit

' Screen.GetScreen is static, so tests that run a Screen must not run in parallel (they live in one class).
Public Class FileDialogTests
  Implements IDisposable

  Private ReadOnly _root As String

  ' root\
  '   alpha\            (folder)
  '     inner.bas
  '   beta\             (folder)
  '   gorilla.bas
  '   Hello.vb
  '   notes.txt
  Public Sub New()
    Me._root = Path.Combine(Path.GetTempPath(), "ConsoleGUI.Tests." & Guid.NewGuid().ToString("N"))
    Directory.CreateDirectory(Path.Combine(Me._root, "alpha"))
    Directory.CreateDirectory(Path.Combine(Me._root, "beta"))
    File.WriteAllText(Path.Combine(Me._root, "alpha", "inner.bas"), "")
    File.WriteAllText(Path.Combine(Me._root, "gorilla.bas"), "")
    File.WriteAllText(Path.Combine(Me._root, "Hello.vb"), "")
    File.WriteAllText(Path.Combine(Me._root, "notes.txt"), "")
  End Sub

  Public Sub Dispose() Implements IDisposable.Dispose
    Try
      Directory.Delete(Me._root, True)
    Catch ex As IOException
    End Try
  End Sub

  ' ---- FileListing

  <Fact>
  Public Sub ListingHasUpFirstThenFoldersThenFilteredFiles()
    Dim entries = FileListing.Read(Me._root, "*.bas;*.vb")

    Assert.Equal({"..", "alpha", "beta", "gorilla.bas", "Hello.vb"}, entries.Select(Function(e) e.Name).ToArray())
    Assert.Equal({FileEntryKind.Up, FileEntryKind.Folder, FileEntryKind.Folder, FileEntryKind.File, FileEntryKind.File}, entries.Select(Function(e) e.Kind).ToArray())
  End Sub

  <Fact>
  Public Sub ListingWithoutAFilterShowsEveryFile()
    Dim names = FileListing.Read(Me._root).Select(Function(e) e.Name).ToArray()
    Assert.Contains("notes.txt", names)
    Assert.Contains("Hello.vb", names)
  End Sub

  <Theory>
  <InlineData("gorilla.bas", "*.bas", True)>
  <InlineData("GORILLA.BAS", "*.bas", True)>
  <InlineData("gorilla.bas", "g?rilla.*", True)>
  <InlineData("gorilla.bas", "*.vb", False)>
  <InlineData("a.b.vb", "*.vb", True)>
  <InlineData("x", "*", True)>
  <InlineData("", "*", True)>
  Public Sub WildcardMatching(name As String, pattern As String, expected As Boolean)
    Assert.Equal(expected, FileListing.WildcardMatch(name, pattern))
  End Sub

  <Fact>
  Public Sub ParentOfARootGoesToTheDriveListOnWindows()
    Dim root = Path.GetPathRoot(Me._root)
    Assert.Equal(If(OperatingSystem.IsWindows(), "", root), FileListing.ParentOf(root))
    Assert.Equal(Path.GetDirectoryName(Me._root), FileListing.ParentOf(Me._root))
  End Sub

  ' ---- FileDialog (QBasic style: File Name box, Files list, Dirs/Drives list, OK / Cancel / Help)

  Private Class DialogScreen
    Inherits Screen

    Public ReadOnly Property Dialog As New FileDialog With {.Title = "Open", .Pattern = "*.bas;*.vb"}
    Public Property Accepted As String
    Public Property Cancelled As Boolean

    ' The dialog is shown by the first queued event, so keys the test queues afterwards reach it.
    Public Sub New(driver As IConsoleDriver, startDirectory As String)
      MyBase.New(driver)
      Me.Post(Sub() Me.Dialog.Show(Me, startDirectory, Sub(path) Me.Accepted = path, Sub() Me.Cancelled = True))
    End Sub

    Public Sub Run()
      NavigateTo(Me)
    End Sub

  End Class

  Private Shared Sub Press(console As HeadlessConsole, ParamArray keys() As VirtualKey)
    For Each key In keys
      console.QueueKey(key, If(key = VirtualKey.VK_RETURN, ChrW(13), ChrW(0)))
    Next
  End Sub

  Private Shared Sub ShiftTab(console As HeadlessConsole)
    console.QueueKey(VirtualKey.VK_TAB, ChrW(9), ControlKeyState.SHIFT_PRESSED)
  End Sub

  ''' <summary>Where some text is on the screen (column, row) - clicks are aimed at what is drawn, not at layout constants.</summary>
  Private Shared Function Find(console As HeadlessConsole, text As String) As (Column As Integer, Row As Integer)
    For row = 0 To console.Height - 1
      Dim column = console.RowText(row).IndexOf(text, StringComparison.Ordinal)
      If column >= 0 Then Return (column, row)
    Next
    Throw New InvalidOperationException("'" & text & "' is not on the screen:" & Environment.NewLine & console.GetText())
  End Function

  Private Shared Sub RunAgain(screen As Screen)
    Screen.NavigateTo(screen)
  End Sub

  <Fact>
  Public Sub LooksLikeQBasicsOpenProgramDialog()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, Me._root)

    screen.Run()

    Dim text = console.GetText()
    Assert.Contains(" Open ", text)
    Assert.Contains("File Name:", text)
    Assert.Contains("*.bas;*.vb", text)
    Assert.Contains("Files", text)
    Assert.Contains("Dirs/Drives", text)
    Assert.Contains("< OK >", text)
    Assert.Contains("< Cancel >", text)
    Assert.Contains("< Help >", text)
    Assert.Contains(Me._root.Substring(Me._root.Length - 12), text)   ' the current path
    Assert.Contains("gorilla.bas", text)                              ' Files
    Assert.Contains("alpha", text)                                    ' Dirs/Drives
    Assert.DoesNotContain("notes.txt", text)                          ' filtered out
    Assert.Same(screen.Dialog, screen.PopupControl)
    Assert.False(console.CursorVisible)
    Assert.Equal("FileName", screen.Dialog.FocusedPart)
  End Sub

  <Fact>
  Public Sub FilesAndDirsAreSeparateLists()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, Me._root)

    screen.Run()
    Dim d = screen.Dialog

    Assert.Equal({"gorilla.bas", "Hello.vb"}, d.FileNames.ToArray())
    Assert.Equal({"..", "alpha", "beta"}, d.DirectoryNames.Take(3).ToArray())
    If OperatingSystem.IsWindows() Then
      Assert.Contains(d.DirectoryNames, Function(n) n.StartsWith("[-") AndAlso n.EndsWith("-]"))   ' drives, QBasic style
    End If
  End Sub

  <Fact>
  Public Sub TabAndShiftTabMoveBetweenTheParts()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, Me._root)

    Press(console, VirtualKey.VK_TAB)
    screen.Run()
    Assert.Equal("Files", screen.Dialog.FocusedPart)

    Press(console, VirtualKey.VK_TAB, VirtualKey.VK_TAB, VirtualKey.VK_TAB, VirtualKey.VK_TAB)
    RunAgain(screen)
    Assert.Equal("Help", screen.Dialog.FocusedPart)

    Press(console, VirtualKey.VK_TAB)   ' wraps round to the File Name box
    RunAgain(screen)
    Assert.Equal("FileName", screen.Dialog.FocusedPart)

    ShiftTab(console)
    RunAgain(screen)
    Assert.Equal("Help", screen.Dialog.FocusedPart)
  End Sub

  <Fact>
  Public Sub SelectingAFilePutsItsNameInTheFileNameBoxAndEnterAcceptsIt()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, Me._root)

    Press(console, VirtualKey.VK_DOWN)   ' gorilla.bas -> Hello.vb (arrows in the box move the Files selection)
    screen.Run()
    Assert.Equal("Hello.vb", screen.Dialog.SelectedFile)
    Assert.Equal("Hello.vb", screen.Dialog.TypedText)

    Press(console, VirtualKey.VK_RETURN)
    RunAgain(screen)

    Assert.Equal(Path.Combine(Me._root, "Hello.vb"), screen.Accepted)
    Assert.False(screen.Dialog.IsOpen)
    Assert.Null(screen.PopupControl)
    Assert.True(console.CursorVisible)
    Assert.DoesNotContain(" Open ", console.GetText())
  End Sub

  <Fact>
  Public Sub TabbingIntoTheFilesListSelectsItsFileSoEnterOpensIt()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, Me._root)

    Press(console, VirtualKey.VK_TAB, VirtualKey.VK_RETURN)
    screen.Run()

    Assert.Equal(Path.Combine(Me._root, "gorilla.bas"), screen.Accepted)
  End Sub

  <Fact>
  Public Sub EnterInTheDirsListOpensTheFolderAndBackspaceGoesUp()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, Me._root)

    Press(console, VirtualKey.VK_TAB, VirtualKey.VK_TAB, VirtualKey.VK_DOWN, VirtualKey.VK_RETURN)   ' Dirs: .. -> alpha, Enter
    screen.Run()
    Dim d = screen.Dialog

    Assert.Equal(Path.Combine(Me._root, "alpha"), d.CurrentDirectory)
    Assert.Equal({"inner.bas"}, d.FileNames.ToArray())
    Assert.Equal("Dirs", d.FocusedPart)
    Assert.Null(screen.Accepted)

    Press(console, VirtualKey.VK_BACK)
    RunAgain(screen)

    Assert.Equal(Me._root, d.CurrentDirectory)
    Assert.Equal("alpha", d.SelectedDirectory)   ' the folder we came from is selected
  End Sub

  <Fact>
  Public Sub TypingReplacesTheSelectedPatternAndSelectsByPrefix()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, Me._root)

    console.QueueText("hel")
    screen.Run()

    Assert.Equal("hel", screen.Dialog.TypedText)
    Assert.Equal("Hello.vb", screen.Dialog.SelectedFile)
    Assert.Contains("hel_", console.GetText())

    Press(console, VirtualKey.VK_BACK)
    RunAgain(screen)
    Assert.Equal("he", screen.Dialog.TypedText)
  End Sub

  <Fact>
  Public Sub TypingWhileAListHasTheFocusMovesBackToTheFileNameBox()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, Me._root)

    Press(console, VirtualKey.VK_TAB, VirtualKey.VK_TAB)   ' Dirs
    console.QueueText("gor")
    Press(console, VirtualKey.VK_RETURN)
    screen.Run()

    Assert.Equal(Path.Combine(Me._root, "gorilla.bas"), screen.Accepted)
  End Sub

  <Fact>
  Public Sub TypedPrefixOpensTheSelectedFileOnEnter()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, Me._root)

    console.QueueText("hel")
    Press(console, VirtualKey.VK_RETURN)
    screen.Run()

    Assert.Equal(Path.Combine(Me._root, "Hello.vb"), screen.Accepted)
  End Sub

  <Fact>
  Public Sub TypingAnUnknownNameShowsAMessage()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, Me._root)

    console.QueueText("zzz")
    Press(console, VirtualKey.VK_RETURN)
    screen.Run()

    Assert.True(screen.Dialog.IsOpen)
    Assert.Null(screen.Accepted)
    Assert.Contains("File not found.", console.GetText())
  End Sub

  <Fact>
  Public Sub AWildcardInTheFileNameBoxChangesTheFilesFilter()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, Me._root)

    console.QueueText("*.txt")
    Press(console, VirtualKey.VK_RETURN)
    screen.Run()

    Assert.Equal("*.txt", screen.Dialog.Pattern)
    Assert.Equal({"notes.txt"}, screen.Dialog.FileNames.ToArray())
    Assert.True(screen.Dialog.IsOpen)
    Assert.Contains("notes.txt", console.GetText())
    Assert.DoesNotContain("gorilla.bas", console.GetText())
  End Sub

  <Fact>
  Public Sub ATypedFolderNameOpensTheFolder()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, Me._root)

    console.QueueText("alpha")
    Press(console, VirtualKey.VK_RETURN)
    screen.Run()

    Assert.Equal(Path.Combine(Me._root, "alpha"), screen.Dialog.CurrentDirectory)
    Assert.True(screen.Dialog.IsOpen)
  End Sub

  <Fact>
  Public Sub SaveModeAcceptsANewNameWithTheDefaultExtension()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, Me._root)
    screen.Dialog.Mode = FileDialogMode.Save
    screen.Dialog.DefaultExtension = ".bas"

    console.QueueText("newprog")
    Press(console, VirtualKey.VK_RETURN)
    screen.Run()

    Assert.Equal(Path.Combine(Me._root, "newprog.bas"), screen.Accepted)
  End Sub

  <Fact>
  Public Sub EscapeCancelsAndOtherKeysAreSwallowedWhileOpen()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, Me._root)
    Dim leaked As Boolean = False
    AddHandler screen.BeforeKeyDown, Sub(sender, e) leaked = True

    Press(console, VirtualKey.VK_DOWN, VirtualKey.VK_ESCAPE)
    screen.Run()

    Assert.False(leaked)
    Assert.True(screen.Cancelled)
    Assert.Null(screen.Accepted)
    Assert.False(screen.Dialog.IsOpen)
  End Sub

  <Fact>
  Public Sub SpaceOrEnterPressesTheFocusedButton()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, Me._root)

    ' Tab: Files, Dirs, OK, Cancel. Space presses Cancel.
    Press(console, VirtualKey.VK_TAB, VirtualKey.VK_TAB, VirtualKey.VK_TAB, VirtualKey.VK_TAB, VirtualKey.VK_SPACE)
    screen.Run()

    Assert.True(screen.Cancelled)
    Assert.False(screen.Dialog.IsOpen)
  End Sub

  <Fact>
  Public Sub LeftAndRightMoveBetweenTheButtonsAndOkOpensTheFileNameBoxsFile()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, Me._root)

    Press(console, VirtualKey.VK_TAB, VirtualKey.VK_TAB, VirtualKey.VK_TAB)   ' OK
    screen.Run()
    Assert.Equal("Ok", screen.Dialog.FocusedPart)

    Press(console, VirtualKey.VK_RIGHT)
    RunAgain(screen)
    Assert.Equal("Cancel", screen.Dialog.FocusedPart)

    Press(console, VirtualKey.VK_LEFT, VirtualKey.VK_RETURN)   ' back to OK; Tab into Files had put gorilla.bas in the box
    RunAgain(screen)
    Assert.Equal(Path.Combine(Me._root, "gorilla.bas"), screen.Accepted)
  End Sub

  <Fact>
  Public Sub HelpButtonShowsAMessageAndKeepsTheDialogOpen()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, Me._root)

    screen.Run()
    Dim help = Find(console, "< Help >")
    console.QueueClick(help.Column + 2, help.Row)
    RunAgain(screen)

    Assert.True(screen.Dialog.IsOpen)
    Assert.Contains("Help is not available", console.GetText())
  End Sub

  <Fact>
  Public Sub ClickingCancelCancels()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, Me._root)

    screen.Run()
    Dim cancel = Find(console, "< Cancel >")
    console.QueueClick(cancel.Column + 2, cancel.Row)
    RunAgain(screen)

    Assert.True(screen.Cancelled)
    Assert.False(screen.Dialog.IsOpen)
  End Sub

  <Fact>
  Public Sub ClickingAFileSelectsItAndASecondClickOpensIt()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, Me._root)

    screen.Run()
    Dim hello = Find(console, "Hello.vb")
    console.QueueClick(hello.Column + 1, hello.Row)
    RunAgain(screen)
    Assert.Equal("Hello.vb", screen.Dialog.SelectedFile)
    Assert.Equal("Files", screen.Dialog.FocusedPart)
    Assert.Null(screen.Accepted)

    console.QueueClick(hello.Column + 1, hello.Row)
    RunAgain(screen)
    Assert.Equal(Path.Combine(Me._root, "Hello.vb"), screen.Accepted)
  End Sub

  <Fact>
  Public Sub ClickingAFolderSelectsItAndASecondClickEntersIt()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, Me._root)

    screen.Run()
    Dim beta = Find(console, "beta")
    console.QueueClick(beta.Column + 1, beta.Row)
    RunAgain(screen)
    Assert.Equal("beta", screen.Dialog.SelectedDirectory)
    Assert.Equal(Me._root, screen.Dialog.CurrentDirectory)

    console.QueueClick(beta.Column + 1, beta.Row)
    RunAgain(screen)
    Assert.Equal(Path.Combine(Me._root, "beta"), screen.Dialog.CurrentDirectory)
  End Sub

  <Fact>
  Public Sub FitsASmallWindowAndExplainsWhenItCannot()
    Dim fits As New HeadlessConsole(60, 20)
    Dim smallScreen As New DialogScreen(fits, Me._root)
    smallScreen.Run()
    Assert.Contains(" Open ", fits.GetText())
    Assert.Contains("< OK >", fits.GetText())
    Assert.Equal(3, smallScreen.Dialog.ListRows)

    Dim tiny As New HeadlessConsole(30, 8)
    Dim tinyScreen As New DialogScreen(tiny, Me._root)
    tinyScreen.Run()   ' must not throw
    Assert.Contains("too small", tiny.GetText())
  End Sub

  ' ---- the dialog scales with the window

  Private Function MakeFolderWithFiles(count As Integer, Optional prefix As String = "file", Optional extension As String = ".bas") As String
    Dim folder = Path.Combine(Me._root, "many" & Guid.NewGuid().ToString("N").Substring(0, 6))
    Directory.CreateDirectory(folder)
    For i = 1 To count
      File.WriteAllText(Path.Combine(folder, prefix & i.ToString("000") & extension), "")
    Next
    Return folder
  End Function

  <Fact>
  Public Sub AtEightyByTwentyFiveItIsExactlyQBasicsDialog()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, MakeFolderWithFiles(40))

    screen.Run()
    Dim d = screen.Dialog

    ' columns 6..72, rows 2..21, eight list rows
    Assert.Equal(6, d.Left)
    Assert.Equal(2, d.Top)
    Assert.Equal(67, d.Width)
    Assert.Equal(20, d.Height)
    Assert.Equal(8, d.ListRows)

    ' The file name box starts after "File Name:" and ends two columns before the frame.
    Assert.Equal("File Name:", console.RowText(4).Substring(8, 10))
    Assert.Equal(ChrW(&H250C), console.CharAt(19, 3))       ' box corner at column 19
    Assert.Equal(ChrW(&H2510), console.CharAt(70, 3))       ' ... to column 70
    ' headings and the two lists
    Assert.Equal("Files", console.RowText(7).Substring(8 + (45 - 5) \ 2, 5))
    Assert.Equal(ChrW(&H250C), console.CharAt(8, 8))        ' Files box
    Assert.Equal(ChrW(&H250C), console.CharAt(55, 8))       ' Dirs/Drives box
    ' separator and buttons spread evenly
    Assert.Equal(ChrW(&H251C), console.CharAt(6, 19))
    Dim ok = Find(console, "< OK >")
    Dim cancel = Find(console, "< Cancel >")
    Dim help = Find(console, "< Help >")
    Assert.Equal(20, ok.Row)
    ' the three buttons are centred 19 columns apart (the labels differ in length)
    Assert.Equal((cancel.Column + 5) - (ok.Column + 3), (help.Column + 4) - (cancel.Column + 5))
  End Sub

  <Fact>
  Public Sub FilesRunDownTheColumnsThenAcrossAndScrollSideways()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, MakeFolderWithFiles(100))

    screen.Run()
    Dim d = screen.Dialog

    ' names are 11 characters, so columns are 13 wide: three fit in the Files box
    Assert.Equal(3, d.VisibleFileColumns)
    Dim first = Find(console, "file001.bas")
    Assert.Equal("file002.bas", console.RowText(first.Row + 1).Substring(first.Column, 11))      ' down the column ...
    Assert.Equal("file009.bas", console.RowText(first.Row).Substring(first.Column + 13, 11))     ' ... then across (8 rows per column)
    Assert.Equal("file017.bas", console.RowText(first.Row).Substring(first.Column + 26, 11))
    Assert.DoesNotContain("file025.bas", console.GetText())                                       ' off to the right

    ' End goes to the last file and scrolls it into view
    console.QueueKey(VirtualKey.VK_END)
    screen.Run()
    Assert.Equal("file100.bas", d.SelectedFile)
    Assert.Contains("file100.bas", console.GetText())
    Assert.DoesNotContain("file001.bas", console.GetText())
  End Sub

  <Fact>
  Public Sub LeftAndRightMoveAWholeColumnInTheFilesList()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, MakeFolderWithFiles(40))

    console.QueueKey(VirtualKey.VK_TAB)    ' Files list
    console.QueueKey(VirtualKey.VK_RIGHT)
    screen.Run()
    Assert.Equal("file009.bas", screen.Dialog.SelectedFile)

    console.QueueKey(VirtualKey.VK_DOWN)
    console.QueueKey(VirtualKey.VK_RIGHT)
    screen.Run()
    Assert.Equal("file018.bas", screen.Dialog.SelectedFile)

    console.QueueKey(VirtualKey.VK_LEFT)
    console.QueueKey(VirtualKey.VK_LEFT)
    console.QueueKey(VirtualKey.VK_LEFT)   ' already in the first column: stays
    screen.Run()
    Assert.Equal("file002.bas", screen.Dialog.SelectedFile)
  End Sub

  <Fact>
  Public Sub ALargerWindowShowsMoreRowsAndColumns()
    Dim folder = MakeFolderWithFiles(100)

    Dim small As New HeadlessConsole(80, 25)
    Dim smallScreen As New DialogScreen(small, folder)
    smallScreen.Run()

    Dim large As New HeadlessConsole(160, 50)
    Dim largeScreen As New DialogScreen(large, folder)
    largeScreen.Run()

    Assert.Equal(8, smallScreen.Dialog.ListRows)
    Assert.Equal(33, largeScreen.Dialog.ListRows)                    ' 50 - 5 - 12
    Assert.Equal(147, largeScreen.Dialog.Width)                      ' 160 - 13
    Assert.True(largeScreen.Dialog.VisibleFileColumns > smallScreen.Dialog.VisibleFileColumns)
    ' 100 files fit in 33 rows x 3 columns, so the last one is on screen without scrolling
    Assert.Contains("file100.bas", large.GetText())
    Assert.DoesNotContain("file100.bas", small.GetText())
    ' the dialog is centred
    Assert.Equal((160 - 147) \ 2, largeScreen.Dialog.Left)
    Assert.Equal((50 - 45) \ 2, largeScreen.Dialog.Top)
  End Sub

  <Fact>
  Public Sub ItFollowsTheWindowWhenItIsResizedWhileOpen()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, MakeFolderWithFiles(100))

    screen.Run()
    Assert.Equal(8, screen.Dialog.ListRows)
    Assert.Equal(67, screen.Dialog.Width)

    console.QueueResize(120, 40)
    screen.Run()
    Assert.Equal(23, screen.Dialog.ListRows)
    Assert.Equal(107, screen.Dialog.Width)
    Assert.Contains("< Help >", console.GetText())
    Assert.True(screen.Dialog.IsOpen)

    console.QueueResize(80, 25)
    screen.Run()
    Assert.Equal(8, screen.Dialog.ListRows)
    Assert.Equal(67, screen.Dialog.Width)
    Assert.Contains("< Help >", console.GetText())
  End Sub

  <Fact>
  Public Sub LongFileNamesGetWideColumnsOnALargeWindowAndAnEllipsisOnASmallOne()
    Dim longName = "a_really_quite_long_program_name_for_testing"   ' 44 characters
    Dim folder = Path.Combine(Me._root, "long")
    Directory.CreateDirectory(folder)
    File.WriteAllText(Path.Combine(folder, longName & ".bas"), "")
    File.WriteAllText(Path.Combine(folder, "short.bas"), "")

    Dim wide As New HeadlessConsole(160, 40)
    Dim wideScreen As New DialogScreen(wide, folder)
    wideScreen.Run()
    Assert.Contains(longName & ".bas", wide.GetText())      ' whole name visible

    Dim narrow As New HeadlessConsole(80, 25)
    Dim narrowScreen As New DialogScreen(narrow, folder)
    narrowScreen.Run()
    Assert.DoesNotContain(longName & ".bas", narrow.GetText())
    Assert.Contains(longName.Substring(0, 20), narrow.GetText())
    Assert.Contains(ChrW(&H2026).ToString(), narrow.GetText())         ' ... shown cut with an ellipsis
    ' it can still be chosen
    narrow.QueueKey(VirtualKey.VK_TAB)
    narrow.QueueKey(VirtualKey.VK_RETURN, ChrW(13))
    narrowScreen.Run()
    Assert.Equal(Path.Combine(folder, longName & ".bas"), narrowScreen.Accepted)
  End Sub

  <Fact>
  Public Sub ClickingAFileInALaterColumnSelectsIt()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, MakeFolderWithFiles(40))

    screen.Run()
    Dim target = Find(console, "file010.bas")
    console.QueueClick(target.Column + 2, target.Row)
    screen.Run()

    Assert.Equal("file010.bas", screen.Dialog.SelectedFile)
    Assert.Equal("Files", screen.Dialog.FocusedPart)
  End Sub

  <Fact>
  Public Sub TheHorizontalScrollBarArrowsScrollTheFilesList()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DialogScreen(console, MakeFolderWithFiles(100))

    screen.Run()
    Dim d = screen.Dialog
    Dim left = ChrW(&H2190)   ' the arrows on the Files box's bottom row
    Dim row = 8 + 1 + 8       ' list box top (8) + 1 + rows
    Assert.Equal(left, console.CharAt(9, row))
    Assert.Equal(ChrW(&H2192), console.CharAt(51, row))

    Assert.DoesNotContain("file025.bas", console.GetText())
    console.QueueClick(51, row)             ' scroll right one column
    screen.Run()
    Assert.Contains("file025.bas", console.GetText())
    Assert.DoesNotContain("file001.bas", console.GetText())

    console.QueueClick(9, row)              ' and back
    screen.Run()
    Assert.Contains("file001.bas", console.GetText())
  End Sub

End Class
