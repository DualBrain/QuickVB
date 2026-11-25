Option Strict On
Option Infer On

Imports ConsoleGUI
Imports Xunit

Public Class FormDialogTests

  Private Class FormScreen
    Inherits Screen

    Public ReadOnly Property Dialog As New FormDialog()
    Public ReadOnly Property Log As New List(Of String)()

    ' shown by the first queued event, so keys queued afterwards reach the dialog
    Public Sub New(driver As IConsoleDriver, Optional build As Action(Of FormScreen) = Nothing)
      MyBase.New(driver)
      If build IsNot Nothing Then build(Me)
      Me.Post(Sub() Me.Dialog.Show(Me))
    End Sub

    Public Sub Run()
      NavigateTo(Me)
    End Sub

  End Class

  Private Shared Function Find(console As HeadlessConsole, text As String) As (Column As Integer, Row As Integer)
    For row = 0 To console.Height - 1
      Dim column = console.RowText(row).IndexOf(text, StringComparison.Ordinal)
      If column >= 0 Then Return (column, row)
    Next
    Throw New InvalidOperationException("'" & text & "' is not on the screen:" & Environment.NewLine & console.GetText())
  End Function

  ' ---- a message with buttons (the "Loaded file is not saved" prompt)

  Private Shared Sub BuildPrompt(s As FormScreen)
    Dim d = s.Dialog
    d.AddText("Loaded file is not saved. Save it now?")
    d.AddButton("< Yes >", "Y"c, Sub() s.Log.Add("yes"), isDefault:=True)
    d.AddButton("< No >", "N"c, Sub() s.Log.Add("no"))
    d.AddButton("<Cancel>", "C"c, Sub() s.Log.Add("cancel"), isCancel:=True)
    d.AddButton("< Help >", "H"c, Sub() s.Log.Add("help"), closes:=False)
  End Sub

  <Fact>
  Public Sub APromptIsAsWideAsItsTextAndSevenRowsTall()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New FormScreen(console, AddressOf BuildPrompt)

    screen.Run()
    Dim d = screen.Dialog

    Assert.Equal(41, d.Width)                    ' "Loaded file is not saved. Save it now?" is 38 characters
    Assert.Equal(7, d.Height)                    ' border, blank, text, blank, separator, buttons, border
    Assert.Equal((80 - 41) \ 2, d.Left)
    Assert.Equal((25 - 7) \ 2, d.Top)

    Dim text = console.GetText()
    Assert.Contains("Loaded file is not saved. Save it now?", text)
    Assert.Contains("< Yes >", text)
    Assert.Contains("< No >", text)
    Assert.Contains("<Cancel>", text)
    Assert.Contains("< Help >", text)
    Assert.Equal(ChrW(&H250C), console.CharAt(d.Left, d.Top))
    Assert.Equal(ChrW(&H251C), console.CharAt(d.Left, d.Top + 4))       ' separator meets the frame
    Assert.Equal(ChrW(&H2524), console.CharAt(d.Right, d.Top + 4))
  End Sub

  <Fact>
  Public Sub ButtonsAreSpreadInEqualSlots()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New FormScreen(console, AddressOf BuildPrompt)

    screen.Run()

    Dim centres = {"< Yes >", "< No >", "<Cancel>", "< Help >"}.Select(Function(t) Find(console, t).Column + t.Length / 2.0).ToArray()
    Dim gaps = {centres(1) - centres(0), centres(2) - centres(1), centres(3) - centres(2)}
    ' four slots of (41 - 2) / 4 = 9.75 columns: centres about 9.75 apart (whole columns, so within one)
    For Each gap In gaps
      Assert.InRange(gap, 8.5, 11)
    Next
  End Sub

  <Fact>
  Public Sub TheDefaultButtonHasBrightBracketsWhichFollowTheFocus()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New FormScreen(console, AddressOf BuildPrompt)

    screen.Run()
    Dim yes = Find(console, "< Yes >")
    Dim no = Find(console, "< No >")
    Assert.Equal(ConsoleColor.White, console.ForegroundAt(yes.Column, yes.Row))
    Assert.Equal(ConsoleColor.Black, console.ForegroundAt(yes.Column + 2, yes.Row))
    Assert.Equal(ConsoleColor.Black, console.ForegroundAt(no.Column, no.Row))
    Assert.Equal("< Yes >", screen.Dialog.FocusedPart)

    console.QueueKey(VirtualKey.VK_TAB)
    screen.Run()
    Assert.Equal("< No >", screen.Dialog.FocusedPart)
    Assert.Equal(ConsoleColor.White, console.ForegroundAt(no.Column, no.Row))
    Assert.Equal(ConsoleColor.Black, console.ForegroundAt(yes.Column, yes.Row))
  End Sub

  <Fact>
  Public Sub EnterPressesTheFocusedButtonAndClosesTheDialogFirst()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New FormScreen(console, Sub(s)
                                            BuildPrompt(s)
                                            s.Dialog.AddButton("< Extra >", afterClose:=Sub() s.Log.Add("popup=" & If(s.PopupControl Is Nothing, "none", "open")))
                                          End Sub)

    console.QueueKey(VirtualKey.VK_TAB)      ' No
    console.QueueKey(VirtualKey.VK_RETURN, ChrW(13))
    screen.Run()

    Assert.Equal({"no"}, screen.Log.ToArray())
    Assert.False(screen.Dialog.IsOpen)
    Assert.Null(screen.PopupControl)
    Assert.DoesNotContain("Loaded file", console.GetText())
  End Sub

  <Fact>
  Public Sub EnterOnTheFirstButtonIsTheDefaultAction()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New FormScreen(console, AddressOf BuildPrompt)

    console.QueueKey(VirtualKey.VK_RETURN, ChrW(13))
    screen.Run()

    Assert.Equal({"yes"}, screen.Log.ToArray())
  End Sub

  <Fact>
  Public Sub EscapePressesTheCancelButton()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New FormScreen(console, AddressOf BuildPrompt)

    console.QueueKey(VirtualKey.VK_ESCAPE)
    screen.Run()

    Assert.Equal({"cancel"}, screen.Log.ToArray())
    Assert.False(screen.Dialog.IsOpen)
  End Sub

  <Fact>
  Public Sub HotKeysPressButtonsWhenNothingIsBeingTyped()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New FormScreen(console, AddressOf BuildPrompt)

    console.QueueText("n")
    screen.Run()

    Assert.Equal({"no"}, screen.Log.ToArray())
  End Sub

  <Fact>
  Public Sub AButtonThatDoesNotCloseLeavesTheDialogOpen()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New FormScreen(console, AddressOf BuildPrompt)

    console.QueueText("h")
    screen.Run()

    Assert.Equal({"help"}, screen.Log.ToArray())
    Assert.True(screen.Dialog.IsOpen)
  End Sub

  <Fact>
  Public Sub LeftAndRightMoveBetweenButtonsButStayAmongThem()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New FormScreen(console, AddressOf BuildPrompt)

    console.QueueKey(VirtualKey.VK_LEFT)       ' already on the first button
    screen.Run()
    Assert.Equal("< Yes >", screen.Dialog.FocusedPart)

    console.QueueKey(VirtualKey.VK_RIGHT)
    console.QueueKey(VirtualKey.VK_RIGHT)
    screen.Run()
    Assert.Equal("<Cancel>", screen.Dialog.FocusedPart)

    console.QueueKey(VirtualKey.VK_RIGHT)
    console.QueueKey(VirtualKey.VK_RIGHT)      ' stays on the last
    screen.Run()
    Assert.Equal("< Help >", screen.Dialog.FocusedPart)
  End Sub

  <Fact>
  Public Sub ClickingAButtonPressesIt()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New FormScreen(console, AddressOf BuildPrompt)

    screen.Run()
    Dim cancel = Find(console, "<Cancel>")
    console.QueueClick(cancel.Column + 3, cancel.Row)
    screen.Run()

    Assert.Equal({"cancel"}, screen.Log.ToArray())
  End Sub

  <Fact>
  Public Sub OtherKeysAreSwallowedAndTheStatusHintIsTheDialogOne()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New FormScreen(console, AddressOf BuildPrompt)
    Dim leaked As Boolean = False
    AddHandler screen.BeforeKeyDown, Sub(sender, e) leaked = True

    console.QueueText("x")
    screen.Run()

    Assert.False(leaked)
    Assert.True(screen.Dialog.IsOpen)
    Assert.Equal(ConsoleGUI.Screen.DialogStatusHint, screen.Dialog.StatusHint)
  End Sub

  ' ---- an input box (New SUB, Help Path)

  Private Shared Sub BuildNewSub(s As FormScreen)
    Dim d = s.Dialog
    d.Title = "New SUB"
    d.Size = FormDialogSize.Half
    d.TopPadding = 0
    Dim name = d.AddInput("Name:", "")
    d.AddButton("< OK >", "O"c, Sub() s.Log.Add("ok:" & name.Text), validate:=Function()
                                                                              If name.Text.Length = 0 Then
                                                                                d.Message = "Identifier expected"
                                                                                Return False
                                                                              End If
                                                                              Return True
                                                                            End Function, isDefault:=True)
    d.AddButton("< Cancel >", "C"c, Sub() s.Log.Add("cancel"), isCancel:=True)
    d.AddButton("< Help >", "H"c, Sub() s.Log.Add("help"), closes:=False)
  End Sub

  <Fact>
  Public Sub AnInputDialogIsHalfTheWindowAndEightRowsTall()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New FormScreen(console, AddressOf BuildNewSub)

    screen.Run()
    Dim d = screen.Dialog

    Assert.Equal(39, d.Width)
    Assert.Equal(8, d.Height)
    Assert.Contains(" New SUB ", console.GetText())
    Assert.Contains("Name:", console.GetText())
    ' the box starts 8 columns in (after the label) and ends two columns before the frame, three rows tall
    Assert.Equal(ChrW(&H250C), console.CharAt(d.Left + 8, d.Top + 1))
    Assert.Equal(ChrW(&H2510), console.CharAt(d.Right - 2, d.Top + 1))
    Assert.Equal(ChrW(&H2514), console.CharAt(d.Left + 8, d.Top + 3))
    Assert.Equal("Name:", console.RowText(d.Top + 2).Substring(d.Left + 2, 5))
    ' the empty box shows the cursor
    Assert.Equal("_"c, console.CharAt(d.Left + 9, d.Top + 2))
    Assert.Equal("Input", screen.Dialog.FocusedPart)
  End Sub

  <Fact>
  Public Sub TypingFillsTheBoxAndOkReturnsTheText()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New FormScreen(console, AddressOf BuildNewSub)

    console.QueueText("Hello")
    console.QueueKey(VirtualKey.VK_BACK)
    console.QueueText("p")
    screen.Run()
    Assert.Contains("Hellp_", console.GetText())

    console.QueueKey(VirtualKey.VK_RETURN, ChrW(13))     ' Enter in the box presses the default button
    screen.Run()
    Assert.Equal({"ok:Hellp"}, screen.Log.ToArray())
    Assert.False(screen.Dialog.IsOpen)
  End Sub

  <Fact>
  Public Sub ValidationKeepsTheDialogOpenAndShowsItsMessage()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New FormScreen(console, AddressOf BuildNewSub)

    console.QueueKey(VirtualKey.VK_RETURN, ChrW(13))     ' empty name
    screen.Run()

    Assert.True(screen.Dialog.IsOpen)
    Assert.Empty(screen.Log)
    Assert.Contains("Identifier expected", console.GetText())

    console.QueueText("a")                               ' the message goes away with the next key
    screen.Run()
    Assert.DoesNotContain("Identifier expected", console.GetText())
  End Sub

  <Fact>
  Public Sub InitialTextIsSelectedAndTheFirstKeyReplacesIt()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New FormScreen(console, Sub(s)
                                            s.Dialog.Size = FormDialogSize.Half
                                            s.Dialog.TopPadding = 0
                                            s.Dialog.AddInput("Name:", "testdddd")
                                            s.Dialog.AddButton("< OK >", isDefault:=True)
                                          End Sub)

    screen.Run()
    Dim d = screen.Dialog
    Dim text = Find(console, "testdddd")
    Assert.Equal(ConsoleColor.White, console.ForegroundAt(text.Column, text.Row))        ' selected: white on black
    Assert.Equal(ConsoleColor.Black, console.BackgroundAt(text.Column, text.Row))
    Assert.Equal("_"c, console.CharAt(text.Column + 8, text.Row))                         ' cursor after it

    console.QueueText("x")
    screen.Run()
    Assert.Contains("x_", console.GetText())
    Assert.DoesNotContain("testdddd", console.GetText())
  End Sub

  <Fact>
  Public Sub AnInputWithoutALabelSpansTheDialog()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New FormScreen(console, Sub(s)
                                            s.Dialog.Title = "Help Path"
                                            s.Dialog.Size = FormDialogSize.Medium
                                            s.Dialog.BottomPadding = 0
                                            s.Dialog.AddText("Location (path) of the help files:")
                                            s.Dialog.AddText("")
                                            s.Dialog.AddInput("", "")
                                            s.Dialog.AddButton("< OK >", isDefault:=True)
                                            s.Dialog.AddButton("< Cancel >", isCancel:=True)
                                            s.Dialog.AddButton("< Help >", closes:=False)
                                          End Sub)

    screen.Run()
    Dim d = screen.Dialog

    Assert.Equal(60, d.Width)
    Assert.Equal(10, d.Height)
    ' the box runs from 2 columns in to 2 columns before the frame
    Assert.Equal(ChrW(&H250C), console.CharAt(d.Left + 3, d.Top + 4))
    Assert.Equal(ChrW(&H2510), console.CharAt(d.Right - 2, d.Top + 4))
  End Sub

  ' ---- check boxes (Find, Change)

  Private Shared Sub BuildFind(s As FormScreen)
    Dim d = s.Dialog
    d.Title = "Find"
    d.Size = FormDialogSize.Medium
    d.TopPadding = 0
    d.AddInput("Find What:", "testdddd")
    Dim checks = d.AddChecks("Match Upper/Lowercase", "Whole Word")
    d.AddButton("< OK >", "O"c, Sub() s.Log.Add("ok:" & checks.Items(0).Checked & ":" & checks.Items(1).Checked), isDefault:=True)
    d.AddButton("< Cancel >", "C"c, isCancel:=True)
    d.AddButton("< Help >", "H"c, closes:=False)
  End Sub

  <Fact>
  Public Sub CheckBoxesSitSideBySideAndSpaceTogglesThem()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New FormScreen(console, AddressOf BuildFind)

    screen.Run()
    Dim d = screen.Dialog

    Assert.Equal(60, d.Width)
    Dim first = Find(console, "[ ] Match Upper/Lowercase")
    Dim second = Find(console, "[ ] Whole Word")
    Assert.Equal(first.Row, second.Row)
    Assert.Equal(d.Left + 5, first.Column)
    Assert.InRange(second.Column - d.Left, 36, 39)      ' about 63% across, as in QBasic

    console.QueueKey(VirtualKey.VK_TAB)                  ' first check box
    console.QueueKey(VirtualKey.VK_SPACE, " "c)
    screen.Run()
    Assert.Equal("Check:Match Upper/Lowercase", screen.Dialog.FocusedPart)
    Assert.Contains("[X] Match Upper/Lowercase", console.GetText())
    Assert.Contains("[ ] Whole Word", console.GetText())

    console.QueueKey(VirtualKey.VK_TAB)
    console.QueueKey(VirtualKey.VK_SPACE, " "c)
    console.QueueKey(VirtualKey.VK_RETURN, ChrW(13))     ' Enter presses the default button, whatever has the focus
    screen.Run()
    Assert.Equal({"ok:True:True"}, screen.Log.ToArray())
  End Sub

  <Fact>
  Public Sub TabVisitsTheInputThenTheCheckBoxesThenTheButtons()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New FormScreen(console, AddressOf BuildFind)

    screen.Run()
    Dim seen As New List(Of String) From {screen.Dialog.FocusedPart}
    For i = 1 To 6
      console.QueueKey(VirtualKey.VK_TAB)
      screen.Run()
      seen.Add(screen.Dialog.FocusedPart)
    Next

    Assert.Equal({"Input", "Check:Match Upper/Lowercase", "Check:Whole Word", "< OK >", "< Cancel >", "< Help >", "Input"}, seen.ToArray())
  End Sub

  <Fact>
  Public Sub ClickingACheckBoxTogglesIt()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New FormScreen(console, AddressOf BuildFind)

    screen.Run()
    Dim whole = Find(console, "[ ] Whole Word")
    console.QueueClick(whole.Column + 1, whole.Row)
    screen.Run()

    Assert.Contains("[X] Whole Word", console.GetText())
  End Sub

  ' ---- a list (SUBs)

  <Fact>
  Public Sub AListDialogIsWideAndTheListGrowsWithTheWindow()
    Dim build As Action(Of FormScreen) =
      Sub(s)
        Dim d = s.Dialog
        d.Title = "SUBs"
        d.Size = FormDialogSize.Wide
        d.BottomPadding = 0
        d.AddText("Choose program item to edit", centered:=False)
        Dim list = d.AddList({"Untitled", "Alpha", "Beta"})
        Dim info = d.AddText("Untitled is the Main Module", centered:=False)
        list.SelectionChanged = Sub() info.Text = list.SelectedText & " is selected"
        d.AddButton("< Edit in Active >", "E"c, isDefault:=True)
        d.AddButton("< Delete >", "D"c)
        d.AddButton("< Cancel >", "C"c, isCancel:=True)
        d.AddButton("< Help >", "H"c, closes:=False)
      End Sub

    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New FormScreen(console, build)
    screen.Run()
    Dim d1 = screen.Dialog

    Assert.Equal(73, d1.Width)
    Assert.Equal(18, d1.Height)
    Assert.Equal(3, d1.Left)
    Assert.Equal((25 - 18) \ 2, d1.Top)
    Assert.Contains("Choose program item to edit", console.GetText())
    Assert.Equal(d1.Left + 2, Find(console, "Choose program").Column)      ' left aligned
    Assert.Equal(d1.Left + 2, Find(console, "Untitled is the Main").Column)
    Dim row = Find(console, "Untitled ").Row
    Assert.Equal(ConsoleColor.White, console.ForegroundAt(d1.Left + 4, row))   ' selected item: white on black
    Assert.Equal(ConsoleColor.Black, console.BackgroundAt(d1.Left + 4, row))

    ' Up / Down change the selection and tell the dialog
    console.QueueKey(VirtualKey.VK_DOWN)
    screen.Run()
    Assert.Contains("Alpha is selected", console.GetText())

    ' ... and a taller window gives the list the extra rows
    Dim tall As New HeadlessConsole(80, 40)
    Dim tallScreen As New FormScreen(tall, build)
    tallScreen.Run()
    Assert.Equal(33, tallScreen.Dialog.Height)       ' 18 + (40 - 25)
  End Sub

  <Fact>
  Public Sub ClickingAListItemSelectsIt()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New FormScreen(console, Sub(s)
                                            s.Dialog.Size = FormDialogSize.Wide
                                            s.Dialog.AddList({"Untitled", "Alpha", "Beta"})
                                            s.Dialog.AddButton("< OK >", isDefault:=True)
                                          End Sub)

    screen.Run()
    Dim beta = Find(console, "Beta")
    console.QueueClick(beta.Column + 1, beta.Row)
    screen.Run()

    Assert.Equal("List", screen.Dialog.FocusedPart)
    Dim bar = console.ForegroundAt(beta.Column, beta.Row)
    Assert.Equal(ConsoleColor.White, bar)
  End Sub

  ' ---- radio buttons (Print)

  <Fact>
  Public Sub RadioButtonsChooseOneOption()
    Dim chosen As String = Nothing
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New FormScreen(console, Sub(s)
                                            s.Dialog.Title = "Print"
                                            Dim radios = s.Dialog.AddRadios(2, {"Selected Text Only", "Current Window", "Entire Program"})
                                            s.Dialog.AddButton("< OK >", isDefault:=True, afterClose:=Sub() chosen = radios.SelectedText)
                                            s.Dialog.AddButton("< Cancel >", isCancel:=True)
                                            s.Dialog.AddButton("< Help >", closes:=False)
                                          End Sub)

    screen.Run()
    Dim d = screen.Dialog
    Assert.Equal(34, d.Width)
    Assert.Equal(9, d.Height)
    Assert.Contains("( ) Selected Text Only", console.GetText())
    Assert.Contains("( ) Current Window", console.GetText())
    Assert.Contains("(" & ChrW(&H2022) & ") Entire Program", console.GetText())

    console.QueueKey(VirtualKey.VK_UP)
    console.QueueKey(VirtualKey.VK_RETURN, ChrW(13))
    screen.Run()
    Assert.Equal("Current Window", chosen)
  End Sub

  ' ---- a plain message (About, Syntax error)

  <Fact>
  Public Sub AMessageWithOneButtonShowsLinesCentred()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New FormScreen(console, Sub(s)
                                            s.Dialog.AddText("QuickVB for Windows")
                                            s.Dialog.AddText("Version 1.0")
                                            s.Dialog.AddText("Copyright (C) Dartmouth Didn't, 1964-2026.")
                                            s.Dialog.AddButton("<  OK  >", "O"c, isDefault:=True, isCancel:=True)
                                          End Sub)

    screen.Run()
    Dim d = screen.Dialog

    Assert.Equal("Copyright (C) Dartmouth Didn't, 1964-2026.".Length + 3, d.Width)
    Assert.Equal(3 + 1 + 1 + 4, d.Height)                           ' three lines, a blank row above and below, separator, buttons, frame
    Dim version = Find(console, "Version 1.0")
    Assert.Equal(d.Left + (d.Width - "Version 1.0".Length) \ 2, version.Column)
    Assert.Contains("<  OK  >", console.GetText())

    console.QueueKey(VirtualKey.VK_ESCAPE)
    screen.Run()
    Assert.False(screen.Dialog.IsOpen)
  End Sub

  <Fact>
  Public Sub ADialogTooBigForTheWindowIsHiddenButStillWorks()
    Dim console As New HeadlessConsole(30, 6)
    Dim screen As New FormScreen(console, AddressOf BuildPrompt)

    console.QueueKey(VirtualKey.VK_ESCAPE)
    screen.Run()    ' must not throw

    Assert.Equal({"cancel"}, screen.Log.ToArray())
  End Sub

End Class
