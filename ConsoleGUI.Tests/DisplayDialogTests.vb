Option Strict On
Option Infer On

Imports ConsoleGUI
Imports Xunit

Public Class DisplayDialogTests

  Private Class DisplayScreen
    Inherits Screen

    Public ReadOnly Property Dialog As New DisplayDialog()
    Public Property OkCount As Integer
    Public Property CancelCount As Integer

    ' shown by the first queued event, so keys queued afterwards reach the dialog
    Public Sub New(driver As IConsoleDriver)
      MyBase.New(driver)
      Me.Dialog.Items.Add(New ColorItem("Normal Text", ConsoleColor.Gray, ConsoleColor.DarkBlue))
      Me.Dialog.Items.Add(New ColorItem("Current Statement", ConsoleColor.White, ConsoleColor.DarkBlue))
      Me.Dialog.Items.Add(New ColorItem("Breakpoint Lines", ConsoleColor.Gray, ConsoleColor.DarkRed))
      Me.Dialog.ScrollBars = True
      Me.Dialog.TabStops = 4
      Me.Post(Sub() Me.Dialog.Show(Me, Sub() Me.OkCount += 1, Sub() Me.CancelCount += 1))
    End Sub

    Public Sub Run()
      NavigateTo(Me)
    End Sub

  End Class

  Private Shared Function Find(console As HeadlessConsole, text As String, Optional occurrence As Integer = 0) As (Column As Integer, Row As Integer)
    Dim seen = 0
    For row = 0 To console.Height - 1
      Dim line = console.RowText(row)
      Dim column = line.IndexOf(text, StringComparison.Ordinal)
      Do While column >= 0
        If seen = occurrence Then Return (column, row)
        seen += 1
        column = line.IndexOf(text, column + 1, StringComparison.Ordinal)
      Loop
    Next
    Throw New InvalidOperationException("'" & text & "' is not on the screen:" & Environment.NewLine & console.GetText())
  End Function

  <Fact>
  Public Sub LooksLikeQBasicsDisplayDialog()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DisplayScreen(console)

    screen.Run()
    Dim d = screen.Dialog

    Assert.Equal(61, d.Width)
    Assert.Equal(22, d.Height)
    Assert.Equal(9, d.Left)
    Assert.Equal(1, d.Top)
    Assert.Equal(8, d.ListRows)

    Dim text = console.GetText()
    Assert.Contains(" Display ", text)
    Assert.Contains(" Colors ", text)
    Assert.Contains("Foreground", text)
    Assert.Contains("Background", text)
    Assert.Contains(" Display Options ", text)
    Assert.Contains("(" & ChrW(&H2022) & ") 1.", text)
    Assert.Contains("( ) 2.", text)
    Assert.Contains("( ) 3.", text)
    Assert.Contains("Normal Text", text)
    Assert.Contains("Current Statement", text)
    Assert.Contains("Breakpoint Lines", text)
    Assert.Contains("[X] Scroll Bars", text)
    Assert.Contains("Tab Stops: 4", text)
    Assert.Contains("< OK >", text)
    Assert.Contains("< Cancel >", text)
    Assert.Contains("< Help >", text)
    For Each name In {"Black", "Blue", "Green", "Cyan", "Red", "Magenta", "Brown", "White"}
      Assert.Contains(name, text)
    Next
    Assert.Equal(ConsoleGUI.Screen.DialogStatusHint, d.StatusHint)
  End Sub

  <Fact>
  Public Sub SwatchesShowEachItemsColours()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DisplayScreen(console)

    screen.Run()

    Dim normal = Find(console, "Normal Text")
    Assert.Equal(ConsoleColor.Gray, console.ForegroundAt(normal.Column, normal.Row))
    Assert.Equal(ConsoleColor.DarkBlue, console.BackgroundAt(normal.Column, normal.Row))
    Dim statement = Find(console, "Current Statement")
    Assert.Equal(ConsoleColor.White, console.ForegroundAt(statement.Column, statement.Row))
    Dim breakpoint = Find(console, "Breakpoint Lines")
    Assert.Equal(ConsoleColor.DarkRed, console.BackgroundAt(breakpoint.Column, breakpoint.Row))
  End Sub

  <Fact>
  Public Sub TheListsShowTheSelectedItemsColours()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DisplayScreen(console)

    screen.Run()

    ' Normal Text is Gray ("White" in QBasic's names) on DarkBlue ("Blue"): both are the selected rows, drawn on black
    Dim fgWhite = Find(console, "White", 0)
    Assert.Equal(ConsoleColor.Black, console.BackgroundAt(fgWhite.Column, fgWhite.Row))
    Dim bgBlue = Find(console, "Blue", 1)
    Assert.Equal(ConsoleColor.Black, console.BackgroundAt(bgBlue.Column, bgBlue.Row))
    Dim fgBlue = Find(console, "Blue", 0)
    Assert.Equal(ConsoleColor.Gray, console.BackgroundAt(fgBlue.Column, fgBlue.Row))      ' not selected there
  End Sub

  <Fact>
  Public Sub ChoosingAnotherItemMovesTheListsToItsColours()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DisplayScreen(console)

    console.QueueKey(VirtualKey.VK_DOWN)       ' Current Statement: bright white on blue
    screen.Run()

    Assert.Equal(1, screen.Dialog.SelectedItemIndex)
    Assert.Contains("( ) 1.", console.GetText())
    Assert.Contains("(" & ChrW(&H2022) & ") 2.", console.GetText())
    Assert.Contains("High-int", console.GetText())        ' the Foreground list scrolled to its last entry
  End Sub

  <Fact>
  Public Sub DigitsPickTheItem()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DisplayScreen(console)

    console.QueueText("3")
    screen.Run()

    Assert.Equal(2, screen.Dialog.SelectedItemIndex)
  End Sub

  <Fact>
  Public Sub TheColourListsChangeTheSwatchAsYouMove()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DisplayScreen(console)

    console.QueueKey(VirtualKey.VK_TAB)          ' Foreground list
    console.QueueKey(VirtualKey.VK_DOWN)         ' Gray -> Dark gray
    console.QueueKey(VirtualKey.VK_TAB)          ' Background list
    console.QueueKey(VirtualKey.VK_UP)           ' Dark blue -> Black
    screen.Run()

    Dim normal = Find(console, "Normal Text")
    Assert.Equal(ConsoleColor.DarkGray, console.ForegroundAt(normal.Column, normal.Row))
    Assert.Equal(ConsoleColor.Black, console.BackgroundAt(normal.Column, normal.Row))
    Assert.Equal(ConsoleColor.DarkGray, screen.Dialog.WorkingItems(0).Foreground)
    ' the real items are untouched until OK
    Assert.Equal(ConsoleColor.Gray, screen.Dialog.Items(0).Foreground)
  End Sub

  <Fact>
  Public Sub OkWritesTheChangesBackAndCancelDoesNot()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DisplayScreen(console)

    console.QueueKey(VirtualKey.VK_TAB)
    console.QueueKey(VirtualKey.VK_END)          ' foreground: High-intensity white
    console.QueueKey(VirtualKey.VK_TAB)
    console.QueueKey(VirtualKey.VK_NEXT)         ' background: a page down
    console.QueueKey(VirtualKey.VK_TAB)          ' Scroll Bars
    console.QueueKey(VirtualKey.VK_SPACE)        ' off
    console.QueueKey(VirtualKey.VK_TAB)          ' Tab Stops
    console.QueueText("8")
    console.QueueKey(VirtualKey.VK_RETURN, ChrW(13))
    screen.Run()

    Assert.Equal(1, screen.OkCount)
    Assert.False(screen.Dialog.IsOpen)
    Assert.Equal(ConsoleColor.White, screen.Dialog.Items(0).Foreground)
    Assert.Equal(CType(1 + 8, ConsoleColor), screen.Dialog.Items(0).Background)    ' DarkBlue + a page of 8
    Assert.False(screen.Dialog.ScrollBars)
    Assert.Equal(8, screen.Dialog.TabStops)

    ' a second dialog that is cancelled leaves everything as it was
    Dim console2 As New HeadlessConsole(80, 25)
    Dim screen2 As New DisplayScreen(console2)
    console2.QueueKey(VirtualKey.VK_TAB)
    console2.QueueKey(VirtualKey.VK_END)
    console2.QueueKey(VirtualKey.VK_ESCAPE)
    screen2.Run()
    Assert.Equal(1, screen2.CancelCount)
    Assert.Equal(ConsoleColor.Gray, screen2.Dialog.Items(0).Foreground)
  End Sub

  <Fact>
  Public Sub ScrollBarsIsACheckBox()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DisplayScreen(console)

    console.QueueKey(VirtualKey.VK_TAB)
    console.QueueKey(VirtualKey.VK_TAB)
    console.QueueKey(VirtualKey.VK_TAB)
    console.QueueKey(VirtualKey.VK_SPACE)
    screen.Run()

    Assert.Equal("ScrollBarsBox", screen.Dialog.FocusedPart)
    Assert.Contains("[ ] Scroll Bars", console.GetText())
    Assert.False(screen.Dialog.WorkingScrollBars)
  End Sub

  <Theory>
  <InlineData("8", 8)>
  <InlineData("12", 12)>
  <InlineData("99", 16)>
  <InlineData("0", 1)>
  Public Sub TabStopsAreTypedAndClamped(typed As String, expected As Integer)
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DisplayScreen(console)

    For i = 1 To 4
      console.QueueKey(VirtualKey.VK_TAB)
    Next
    console.QueueText(typed)
    console.QueueKey(VirtualKey.VK_RETURN, ChrW(13))
    screen.Run()

    Assert.Equal(expected, screen.Dialog.TabStops)
  End Sub

  <Fact>
  Public Sub TabStopsCanBeEditedWithBackspaceAndTheArrows()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DisplayScreen(console)

    For i = 1 To 4
      console.QueueKey(VirtualKey.VK_TAB)
    Next
    console.QueueKey(VirtualKey.VK_UP)            ' 4 -> 5
    console.QueueKey(VirtualKey.VK_UP)            ' 6
    screen.Run()
    Assert.Equal("6", screen.Dialog.WorkingTabStopsText)
    Assert.Contains("Tab Stops: 6", console.GetText())

    console.QueueKey(VirtualKey.VK_DOWN)
    screen.Run()
    Assert.Equal("5", screen.Dialog.WorkingTabStopsText)
  End Sub

  <Fact>
  Public Sub TabWrapsRoundAllTheParts()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DisplayScreen(console)

    screen.Run()
    Dim seen As New List(Of String) From {screen.Dialog.FocusedPart}
    For i = 1 To 8
      console.QueueKey(VirtualKey.VK_TAB)
      screen.Run()
      seen.Add(screen.Dialog.FocusedPart)
    Next

    Assert.Equal({"Items", "Foreground", "Background", "ScrollBarsBox", "TabStopsBox", "Ok", "Cancel", "Help", "Items"}, seen.ToArray())
  End Sub

  <Fact>
  Public Sub TheMouseChoosesItemsColoursAndOptions()
    Dim console As New HeadlessConsole(80, 25)
    Dim screen As New DisplayScreen(console)

    screen.Run()

    Dim third = Find(console, "( ) 3.")
    console.QueueClick(third.Column + 1, third.Row)
    screen.Run()
    Assert.Equal(2, screen.Dialog.SelectedItemIndex)

    Dim green = Find(console, "Green", 0)                  ' the Foreground list
    console.QueueClick(green.Column + 1, green.Row)
    screen.Run()
    Assert.Equal(ConsoleColor.DarkGreen, screen.Dialog.WorkingItems(2).Foreground)

    Dim scroll = Find(console, "[X] Scroll Bars")
    console.QueueClick(scroll.Column + 1, scroll.Row)
    screen.Run()
    Assert.False(screen.Dialog.WorkingScrollBars)

    Dim ok = Find(console, "< OK >")
    console.QueueClick(ok.Column + 2, ok.Row)
    screen.Run()
    Assert.Equal(1, screen.OkCount)
    Assert.Equal(ConsoleColor.DarkGreen, screen.Dialog.Items(2).Foreground)
  End Sub

  <Fact>
  Public Sub ALargerWindowGivesMoreColourRowsAndWidth()
    Dim console As New HeadlessConsole(120, 40)
    Dim screen As New DisplayScreen(console)

    screen.Run()
    Dim d = screen.Dialog

    Assert.Equal(8 + 15, d.ListRows)
    Assert.Equal(61 + 40, d.Width)
    Assert.Equal(d.ListRows + 14, d.Height)
    ' all sixteen colours are visible now
    Assert.Contains("Light Mag", console.GetText())
    Assert.Contains("Yellow", console.GetText())
  End Sub

  <Fact>
  Public Sub ADialogTooBigForTheWindowIsHiddenButStillWorks()
    Dim console As New HeadlessConsole(40, 12)
    Dim screen As New DisplayScreen(console)

    console.QueueKey(VirtualKey.VK_ESCAPE)
    screen.Run()    ' must not throw

    Assert.Equal(1, screen.CancelCount)
  End Sub

End Class
