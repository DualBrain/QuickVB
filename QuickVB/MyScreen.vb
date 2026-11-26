' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Imports ConsoleGUI
'Imports System.ComponentModel.Composition.Hosting
Imports System.Threading
Imports Microsoft.CodeAnalysis

Partial Public Class MyScreen
  Inherits Screen

  Private WithEvents DiagnosticsBuffer As TextBuffer
  Private WithEvents ErrorBufferView As TextBufferView
  Private WithEvents DiagnosticsPane As Pane

  Private MainMenu As Menu


  Private StatusTextLabel As Label
  Private StatusPositionLabel As Label

  ' Roslyn is switched on (see EnableRoslyn) once the first document is loaded; the Options menu toggles it.
  Private RoslynEnabled As Boolean = False
  Private OptionsMenu As Menu
  Private RoslynMenuItem As MenuItem

  Private _activeBufferView As TextBufferView
  Private _activeDocumentBufferView As DocumentTextBufferView

  Property ActiveBufferView As TextBufferView
    Get
      Return Me._activeBufferView
    End Get
    Set(value As TextBufferView)
      Me._activeBufferView = value
      If TypeOf Me._activeBufferView Is DocumentTextBufferView Then
        Me._activeDocumentBufferView = CType(Me._activeBufferView, DocumentTextBufferView)
      End If
    End Set
  End Property

  ReadOnly Property ActiveDocumentBufferView As DocumentTextBufferView
    Get
      Return Me._activeDocumentBufferView
    End Get
  End Property

  Public Sub New()
    MyBase.New()
    Console.Title = "QuickVB"
    Me.Initialize()
  End Sub

  ''' <summary>Runs QuickVB on any console driver (a <see cref="HeadlessConsole"/> in tests).</summary>
  Public Sub New(driver As IConsoleDriver)
    MyBase.New(driver)
    Me.Initialize()
  End Sub

  Private Sub Initialize()

    Me.CreateEditorWindows()
    Me.DiagnosticsBuffer = New TextBuffer

    Me.ErrorBufferView = New TextBufferView With {.Buffer = Me.DiagnosticsBuffer,
                                                  .Left = 1,
                                                  .Top = 17,
                                                  .Width = 78,
                                                  .Height = 5,
                                                  .ScrollBars = False,
                                                  .BackgroundColor = ConsoleColor.DarkBlue,
                                                  .ForegroundColor = ConsoleColor.Gray}

    Me.DiagnosticsPane = New Pane With {.Left = 0,
                                        .Top = 21,
                                        .Width = 80,
                                        .Height = 4,
                                        .PaddingBottom = 1,
                                        .BackgroundColor = ConsoleColor.DarkBlue,
                                        .ForegroundColor = ConsoleColor.Gray,
                                        .Title = "Output"}

    Me.DiagnosticsPane.Controls.Add(Me.ErrorBufferView)

    Dim FileMenu = New Menu With {.DropDownMenu = True}

    FileMenu.SetMenuItems(New MenuItem() {New MenuItem(Name:="&New Program", Action:=Sub() Me.Post(AddressOf Me.NewProgram)),
                                          New MenuItem(Name:="&Open Program...", Action:=Sub() Me.Post(AddressOf Me.ShowOpenDialog)),
                                          New MenuItem(Name:="Sampl&es...", Action:=Sub() Me.Post(AddressOf Me.ShowSamplesDialog)),
                                          New MenuItem(Name:="&Merge...", Action:=Sub() Me.Post(AddressOf Me.ShowMergeDialog)),
                                          New MenuItem(Name:="&Save", Action:=Sub() Me.Post(Sub() Me.SaveProgram())),
                                          New MenuItem(Name:="Save &As...", Action:=Sub() Me.Post(AddressOf Me.ShowSaveAsDialog)),
                                          New MenuItem(Name:="Save As Pro&ject...", Action:=Sub() Me.Post(AddressOf Me.ShowSaveAsProjectDialog)),
                                          New MenuItem(Name:="Sa&ve All", Action:=Sub() Me.Post(Sub() Me.SaveAll())),
                                          New MenuItem(Separator:=True),
                                          New MenuItem(Name:="&Create File...", Action:=Sub() Me.Post(AddressOf Me.ShowCreateFileDialog)),
                                          New MenuItem(Name:="&Load File...", Action:=Sub() Me.Post(AddressOf Me.ShowLoadFileDialog)),
                                          New MenuItem(Name:="&Unload File...", Action:=Sub() Me.Post(AddressOf Me.ShowUnloadFileDialog)),
                                          New MenuItem(Separator:=True),
                                          New MenuItem(Name:="&Print...", Action:=Sub() Me.Post(AddressOf Me.ShowPrintDialog)),
                                          New MenuItem(Name:="&DOS Shell", Action:=Sub() Me.Post(AddressOf Me.RunShell)),
                                          New MenuItem(Separator:=True),
                                          New MenuItem(Name:="E&xit", Action:=Sub() Me.Post(AddressOf Me.RequestExit))})

    Dim EditMenu = New Menu With {.DropDownMenu = True}

    EditMenu.SetMenuItems(New MenuItem() {New MenuItem(Name:="&Undo   Alt+Backspace", Action:=Sub() Me.Post(Sub() Me.ActiveBufferView.Undo())),
                                          New MenuItem(Name:="Cu&t       Shift+Del", Action:=Sub() Me.Post(Sub() Me.ActiveBufferView.Edit(AddressOf Me.ActiveBufferView.CutSelectedText))),
                                          New MenuItem(Name:="&Copy       Ctrl+Ins", Action:=Sub() Me.Post(Sub() Me.ActiveBufferView.CopySelectedText())),
                                          New MenuItem(Name:="&Paste     Shift+Ins", Action:=Sub() Me.Post(Sub() Me.ActiveBufferView.Edit(AddressOf Me.ActiveBufferView.PasteFromClipboard))),
                                          New MenuItem(Name:="Clear            Del", Action:=Sub() Me.Post(Sub() Me.ActiveBufferView.Edit(AddressOf Me.ActiveBufferView.DeleteSelectedText))),
                                          New MenuItem(Separator:=True),
                                          New MenuItem(Name:="New &SUB...", Action:=Sub() Me.Post(Sub() Me.ShowNewProcedureDialog("Sub"))),
                                          New MenuItem(Name:="New &FUNCTION...", Action:=Sub() Me.Post(Sub() Me.ShowNewProcedureDialog("Function")))})

    Dim ViewMenu = New Menu With {.DropDownMenu = True}

    ViewMenu.SetMenuItems(New MenuItem() {New MenuItem(Name:="&SUBs...            F2", Action:=Sub() Me.Post(AddressOf Me.ShowSubsDialog)),
                                          New MenuItem(Name:="N&ext SUB      Shift+F2", Action:=Sub() Me.Post(Sub() Me.GoToProcedure(True))),
                                          New MenuItem(Name:="S&plit", Action:=Sub() Me.Post(AddressOf Me.ToggleSplit)),
                                          New MenuItem(Separator:=True),
                                          New MenuItem(Name:="&Next Statement", Action:=Sub() Me.Post(AddressOf Me.ShowNextStatement)),
                                          New MenuItem(Name:="O&utput Screen      F4", Action:=Sub() Me.Post(AddressOf Me.ShowOutputScreen)),
                                          New MenuItem(Separator:=True),
                                          New MenuItem(Name:="Included Files"),
                                          New MenuItem(Name:="Included &Lines"),
                                          New MenuItem(Separator:=True),
                                          New MenuItem(Name:="S&witch to...", Action:=Sub() Me.Post(AddressOf Me.ShowSwitchToDialog))})

    Dim SearchMenu = New Menu With {.DropDownMenu = True}

    SearchMenu.SetMenuItems(New MenuItem() {New MenuItem(Name:="&Find...", Action:=Sub() Me.Post(AddressOf Me.ShowFindDialog)),
                                            New MenuItem(Name:="&Selected Text      Ctrl+\", Action:=Sub() Me.Post(AddressOf Me.FindSelectedText)),
                                            New MenuItem(Name:="&Repeat Last Find       F3", Action:=Sub() Me.Post(AddressOf Me.RepeatFind)),
                                            New MenuItem(Name:="&Change...", Action:=Sub() Me.Post(AddressOf Me.ShowChangeDialog)),
                                            New MenuItem(Name:="&Label...")})


    Dim RunMenu = New Menu With {.DropDownMenu = True}

    RunMenu.SetMenuItems(New MenuItem() {New MenuItem(Name:="&Start      Shift+F5", Action:=Sub() Me.Post(AddressOf Me.StartFromTheBeginning)),
                                         New MenuItem(Name:="&Restart", Action:=Sub() Me.Post(AddressOf Me.RestartProgram)),
                                         New MenuItem(Name:="Co&ntinue         F5", Action:=Sub() Me.Post(AddressOf Me.ContinueOrRun)),
                                         New MenuItem(Name:="Modify &COMMAND$...", Action:=Sub() Me.Post(AddressOf Me.ShowCommandDialog)),
                                         New MenuItem(Separator:=True),
                                         New MenuItem(Name:="Make E&XE File...", Action:=Sub() Me.Post(AddressOf Me.ShowMakeExeDialog)),
                                         New MenuItem(Name:="Make &Library...", Action:=Sub() Me.Post(AddressOf Me.ShowMakeLibraryDialog)),
                                         New MenuItem(Separator:=True),
                                         New MenuItem(Name:="Set &Main Module...", Action:=Sub() Me.Post(AddressOf Me.ShowSetMainModuleDialog))})


    Me._syntaxCheckingItem = New MenuItem(Name:="&Syntax Checking    " & ChrW(&H221A), Action:=Sub() Me.Post(AddressOf Me.ToggleSyntaxChecking))
    Me._fullMenusItem = New MenuItem(Name:="&Full Menus    " & ChrW(&H221A), Action:=Sub() Me.Post(AddressOf Me.ToggleFullMenus))
    Me._breakOnErrorsItem = New MenuItem(Name:="Break on &Errors", Action:=Sub() Me.Post(AddressOf Me.ToggleBreakOnErrors))

    Dim DebugMenu = New Menu With {.DropDownMenu = True}

    DebugMenu.SetMenuItems(New MenuItem() {New MenuItem(Name:="&Add Watch...", Action:=Sub() Me.Post(AddressOf Me.ShowAddWatchDialog)),
                                           New MenuItem(Name:="&Instant Watch...   Shift+F9", Action:=Sub() Me.Post(AddressOf Me.ShowInstantWatch)),
                                           New MenuItem(Name:="&Watchpoint...", Action:=Sub() Me.Post(AddressOf Me.ShowWatchpointDialog)),
                                           New MenuItem(Name:="&Delete Watch...", Action:=Sub() Me.Post(AddressOf Me.ShowDeleteWatchDialog)),
                                           New MenuItem(Name:="Delete All W&atch", Action:=Sub() Me.Post(AddressOf Me.DeleteAllWatches)),
                                           New MenuItem(Separator:=True),
                                           New MenuItem(Name:="&Trace On"),
                                           New MenuItem(Name:="&History On"),
                                           New MenuItem(Separator:=True),
                                           New MenuItem(Name:="Toggle &Breakpoint        F9", Action:=Sub() Me.Post(AddressOf Me.ToggleBreakpoint)),
                                           New MenuItem(Name:="&Clear All Breakpoints", Action:=Sub() Me.Post(AddressOf Me.ClearAllBreakpoints)),
                                           Me._breakOnErrorsItem,
                                           New MenuItem(Name:="Set Next Statement")})

    Me._callsMenu = New Menu With {.DropDownMenu = True}
    Dim CallsMenu = Me._callsMenu

    CallsMenu.SetMenuItems(New MenuItem() {New MenuItem(Name:="&Untitled")})

    Dim OptionsMenu = New Menu With {.DropDownMenu = True}

    Me.OptionsMenu = OptionsMenu
    Me.RoslynMenuItem = New MenuItem(Name:="Disable &Roslyn", Action:=AddressOf Me.ToggleRoslyn)


    OptionsMenu.SetMenuItems(New MenuItem() {New MenuItem(Name:="&Display...", Action:=Sub() Me.Post(AddressOf Me.ShowDisplayDialog)),
                                             New MenuItem(Name:="Set &Paths...", Action:=Sub() Me.Post(AddressOf Me.ShowSetPathsDialog)),
                                             New MenuItem(Name:="Right &Mouse...", Action:=Sub() Me.Post(AddressOf Me.ShowRightMouseDialog)),
                                             Me._syntaxCheckingItem,
                                             Me._fullMenusItem,
                                             New MenuItem(Separator:=True),
                                             Me.RoslynMenuItem})

    Dim HelpMenu = New Menu With {.DropDownMenu = True}

    'HelpMenu.SetMenuItems(New MenuItem() {New MenuItem(Name:="&Alex Turner", Action:=Sub()
    '                                                                                 End Sub),
    '                                      New MenuItem(Name:="&Ian Halliday", Action:=Sub()
    '                                                                                  End Sub),
    '                                      New MenuItem(Name:="and &Roslyn!", Action:=Sub()
    '                                                                                 End Sub)})

    HelpMenu.SetMenuItems(New MenuItem() {New MenuItem(Name:="&Index", Action:=Sub() Me.Post(Sub() Me.ShowHelp(HelpContent.IndexId))),
                                          New MenuItem(Name:="&Contents", Action:=Sub() Me.Post(Sub() Me.ShowHelp(HelpContent.ContentsId))),
                                          New MenuItem(Name:="&Topic:                 F1", Action:=Sub() Me.Post(Sub() Me.ShowHelp(HelpContent.ContentsId))),
                                          New MenuItem(Name:="&Help on Help     Shift+F1", Action:=Sub() Me.Post(Sub() Me.ShowHelp(HelpContent.HelpOnHelpId))),
                                          New MenuItem(Separator:=True),
                                          New MenuItem(Name:="&About", Action:=Sub() Me.Post(AddressOf Me.ShowAbout))})

    Me.MainMenu = New Menu With {.Left = 0,
                                 .Top = 0,
                                 .Width = Me.Width,
                                 .Height = 1}

    Me.MainMenu.SetMenuItems(New MenuItem() {New MenuItem(Name:="&File", DropDownMenu:=FileMenu),
                                             New MenuItem(Name:="&Edit", DropDownMenu:=EditMenu),
                                             New MenuItem(Name:="&View", DropDownMenu:=ViewMenu),
                                             New MenuItem(Name:="&Search", DropDownMenu:=SearchMenu),
                                             New MenuItem(Name:="&Run", DropDownMenu:=RunMenu),
                                             New MenuItem(Name:="&Debug", DropDownMenu:=DebugMenu),
                                             New MenuItem(Name:="&Calls", DropDownMenu:=CallsMenu),
                                             New MenuItem(Name:="&Options", DropDownMenu:=OptionsMenu),
                                             New MenuItem(Name:="&Help", AnchorRight:=True, DropDownMenu:=HelpMenu)})

    For Each topItem In Me.MainMenu.MenuItems
      If topItem.DropDownMenu Is Nothing Then Continue For
      For Each item In topItem.DropDownMenu.MenuItems
        If item.Action Is Nothing AndAlso item.DropDownMenu Is Nothing AndAlso Not item.Separator Then
          item.Action = Sub() Me.Post(AddressOf Me.ShowNotImplemented)
          item.Dimmed = True
        End If
      Next
    Next

    Me.BackgroundColor = ConsoleColor.DarkBlue

    Me.StatusTextLabel = New Label With {.Top = 24,
                                         .Left = 0,
                                         .Width = 80,
                                         .PaddingLeft = 1,
                                         .PaddingRight = 1,
                                         .BackgroundColor = ConsoleColor.DarkCyan,
                                         .ForegroundColor = ConsoleColor.White,
                                         .Text = "<F6=Window> <F5=Run>"}

    Me.StatusPositionLabel = New Label With {.Top = 24,
                                             .Left = 80 - 18,
                                             .Width = 18,
                                             .PaddingRight = 1,
                                             .BackgroundColor = ConsoleColor.DarkCyan,
                                             .ForegroundColor = ConsoleColor.Black,
                                             .Text = " "}

    Me.Controls.Add(Me.DiagnosticsPane)
    Me.Controls.Add(Me._mainPane)
    Me.Controls.Add(Me.HelpPane)
    Me.Controls.Add(Me.MainMenu)
    Me.Controls.Add(Me.StatusTextLabel)
    Me.Controls.Add(Me.StatusPositionLabel)

    Me.ActiveControl = Me.DocumentBufferView
    Me.ActiveBufferView = Me.DocumentBufferView

    Me.InitializeHelp()
    Me.LayoutChrome()

  End Sub


  ' How tall the Immediate/Output window is (Alt+Plus / Alt+Minus change it).
  Private _outputRows As Integer = 4

  Private Sub ResizeActiveWindow(grow As Integer)

    ' with two editor windows Plus gives the active one a row of the other
    If Me._splitVisible AndAlso TypeOf Me.ActiveBufferView Is DocumentTextBufferView Then
      Me._splitShift += If(Me.IsSplitFocused(), -grow, grow)
      Me._splitShift = Math.Max(-Me.Height, Math.Min(Me.Height, Me._splitShift))
      Me.LayoutChrome()
      Me.Invalidate()
      Return
    End If

    ' the editor grows when the output window shrinks, and the other way round
    Dim outputActive = Me.ActiveBufferView Is Me.ErrorBufferView
    Dim delta = If(outputActive, grow, -grow)

    Me._outputRows = Math.Max(2, Math.Min(Math.Max(2, Me.Height - 6), Me._outputRows + delta))
    Me.LayoutChrome()
    Me.Invalidate()

  End Sub

  ''' <summary>Shift+F2 (next) / Ctrl+F2 (previous): moves to another SUB or FUNCTION, wrapping around the program.</summary>
  Public Sub GoToProcedure(forward As Boolean)

    Dim procedures = Me.FindProcedures()
    If procedures.Count = 0 Then Return

    Dim row = Me.ActiveDocumentBufferView.CursorRow
    Dim target As ProcedureInfo

    If forward Then
      target = If(procedures.FirstOrDefault(Function(p) p.Row > row), procedures.First())
    Else
      target = If(procedures.LastOrDefault(Function(p) p.Row < row), procedures.Last())
    End If

    Me.ActiveDocumentBufferView.MoveCursor(target.Row, 0)
    Me.Invalidate()

  End Sub

  ''' <summary>Sizes the menu bar, panes and status line to the current window; the diagnostics pane keeps its height and the editor takes the rest.</summary>
  Private Sub LayoutChrome()

    Dim w = Math.Max(1, Me.Width)
    Dim h = Math.Max(1, Me.Height)
    ' With help open the Immediate/Output pane shrinks to its borders, as in QuickBASIC, to leave room for the help text.
    Dim diagnosticsHeight = If(Me.HelpPane.Visible, Math.Min(2, Math.Max(1, h - 3)), Math.Min(Me._outputRows, Math.Max(1, h - 3)))

    Me.MainMenu.Width = w
    Me.MainMenu.Layout()

    ' The help window (when open) is as tall as its text needs, up to what leaves the editor a few rows; Ctrl+F10 maximizes it.
    Dim helpHeight = 0
    If Me.HelpPane.Visible Then
      Dim most = Math.Max(6, h - diagnosticsHeight - 3)
      helpHeight = If(Me.HelpPane.Maximized, Math.Max(6, h - 2), Math.Min(Math.Max(Me.HelpPane.ContentRows + 4, 8), most))
      Me.HelpPane.Left = 0
      Me.HelpPane.Top = 1
      Me.HelpPane.Width = w
      Me.HelpPane.Height = helpHeight
    End If

    ' A maximized help window covers the panes, so they keep their usual places behind it.
    Dim stacked = If(Me.HelpPane.Maximized, 0, helpHeight)

    ' the rows from the top of the first editor window to the top border of the Output window (the windows share a border line)
    Dim total = Math.Max(3, h - diagnosticsHeight - stacked)

    Me._mainPane.Left = 0
    Me._mainPane.Top = 1 + stacked
    Me._mainPane.Width = w

    If Me._splitVisible Then
      Dim firstHeight = Math.Max(3, Math.Min(total - 2, (total + 1) \ 2 + Me._splitShift))
      Me._mainPane.Height = firstHeight
      Me._splitPane.Left = 0
      Me._splitPane.Top = Me._mainPane.Top + firstHeight - 1
      Me._splitPane.Width = w
      Me._splitPane.Height = Math.Max(3, total - firstHeight + 1)
      Me.DiagnosticsPane.Top = Me._splitPane.Top + Me._splitPane.Height - 1
      Me._splitPane.LayoutChildren()
    Else
      Me._mainPane.Height = total
      Me.DiagnosticsPane.Top = Me._mainPane.Top + Me._mainPane.Height - 1
    End If

    Me.DiagnosticsPane.Left = 0
    Me.DiagnosticsPane.Width = w
    Me.DiagnosticsPane.Height = diagnosticsHeight

    Me._mainPane.LayoutChildren()
    Me.DiagnosticsPane.LayoutChildren()

    Me.StatusTextLabel.Top = h - 1
    Me.StatusTextLabel.Width = w

    Me.StatusPositionLabel.Top = h - 1
    Me.StatusPositionLabel.Left = Math.Max(0, w - Me.StatusPositionLabel.Width)

  End Sub

  Private Sub MyScreen_Resize(sender As Object, e As BufferSizeEvent) Handles Me.Resize
    Me.LayoutChrome()
    Me.Invalidate()
  End Sub

  Private _statusHint As String

  ' While a dialog is open the status bar shows its hint; afterwards the usual status comes back
  ' (unless something has put a message there in the meantime).
  Private Sub UpdateStatusForDialogs()

    Dim popup = Me.PopupControl
    Dim popupHint = If(popup IsNot Nothing, popup.StatusHint, Nothing)
    Dim hint = popupHint
    If hint Is Nothing AndAlso Me.HelpPane.Visible AndAlso Me.ActiveControl Is Me.HelpPane Then hint = Me.HelpPane.StatusHint

    If String.Equals(hint, Me._statusHint) Then Return

    Dim previous = Me._statusHint
    Me._statusHint = hint

    ' A dialog's hint takes the whole bar, like QBasic's (no line:column readout beside it).
    Me.StatusPositionLabel.Visible = (hint Is Nothing)

    If hint IsNot Nothing Then
      Me.SetStatus(hint, False)
    ElseIf Me.StatusTextLabel.Text = previous Then
      Me.SetStatus(Nothing, False)
    End If

    Me.Invalidate()

  End Sub

  Private Sub MyScreen_BeforeRender(sender As Object, e As EventArgs) Handles Me.BeforeRender
    Me.UpdateStatusForDialogs()
    Dim oldPositionText = Me.StatusPositionLabel.Text
    Me.StatusPositionLabel.Text = "│       " + String.Format("{0:D5}:{1:D3}", Me.ActiveBufferView.CursorRow + 1, Me.ActiveBufferView.CursorColumn + 1)
    If Me.StatusPositionLabel.Text <> oldPositionText Then Me.Invalidate()
  End Sub

  Private Sub MyScreen_BeforeKeyDown(sender As Object, e As KeyEvent) Handles Me.BeforeKeyDown
    If e.VirtualKey <> VirtualKey.VK_F3 AndAlso e.VirtualKey <> VirtualKey.VK_MENU Then Me.ClearFindHighlight()
    If (e.ControlKeyState And (ControlKeyState.LEFT_CTRL_PRESSED Or ControlKeyState.RIGHT_CTRL_PRESSED)) <> 0 Then
      Select Case e.VirtualKey
        Case VirtualKey.VK_SPACE
          Me.ActiveDocumentBufferView.MaybeTriggerCompletionList(Nothing)

          e.Handled = True
        Case VirtualKey.VK_OEM_5
          Me.FindSelectedText()
          e.Handled = True
      End Select

      ' terminals send Ctrl+\ as a control character of its own
      If e.Character = ChrW(28) Then
        Me.FindSelectedText()
        e.Handled = True
      End If
    End If

    ' Alt+Plus / Alt+Minus: a larger / smaller active window
    If (e.ControlKeyState And (ControlKeyState.LEFT_ALT_PRESSED Or ControlKeyState.RIGHT_ALT_PRESSED)) <> 0 AndAlso Me.PopupControl Is Nothing Then
      Dim grow As Integer = 0
      Select Case e.VirtualKey
        Case VirtualKey.VK_OEM_PLUS, VirtualKey.VK_ADD : grow = 1
        Case VirtualKey.VK_OEM_MINUS, VirtualKey.VK_SUBTRACT : grow = -1
        Case Else
          If e.Character = "+"c OrElse e.Character = "="c Then grow = 1
          If e.Character = "-"c Then grow = -1
      End Select
      If grow <> 0 Then
        Me.ResizeActiveWindow(grow)
        e.Handled = True
      End If
    End If

    If e.Handled Then Exit Sub

    Select Case e.VirtualKey
      Case VirtualKey.VK_F1
        Me.HandleF1(e)
      Case VirtualKey.VK_F2
        If (e.ControlKeyState And ControlKeyState.SHIFT_PRESSED) <> 0 Then
          Me.GoToProcedure(True)
        ElseIf (e.ControlKeyState And (ControlKeyState.LEFT_CTRL_PRESSED Or ControlKeyState.RIGHT_CTRL_PRESSED)) <> 0 Then
          Me.GoToProcedure(False)
        Else
          Me.ShowSubsDialog()
        End If
      Case VirtualKey.VK_F3
        Me.RepeatFind()
      Case VirtualKey.VK_F9
        If (e.ControlKeyState And ControlKeyState.SHIFT_PRESSED) <> 0 Then
          Me.ShowInstantWatch()
        Else
          Me.ToggleBreakpoint()
        End If
      Case VirtualKey.VK_F4
        Me.ShowOutputScreen()
      Case VirtualKey.VK_F7
        Me.RunToCursor()
      Case VirtualKey.VK_F8
        If (e.ControlKeyState And ControlKeyState.SHIFT_PRESSED) <> 0 Then Me.ShowNotImplemented() Else Me.StepProgram("into")
      Case VirtualKey.VK_F5
        If (e.ControlKeyState And ControlKeyState.SHIFT_PRESSED) <> 0 Then Me.StartFromTheBeginning() Else Me.ContinueOrRun()
      Case VirtualKey.VK_F6
        Me.SwitchPane((e.ControlKeyState And ControlKeyState.SHIFT_PRESSED) <> 0)
      Case VirtualKey.VK_F10
        If Me._paused AndAlso (e.ControlKeyState And ControlKeyState.SHIFT_PRESSED) = 0 Then
          Me.StepProgram("over")
        ElseIf (e.ControlKeyState And ControlKeyState.SHIFT_PRESSED) <> 0 Then
          Me.ShowNotImplemented()
        Else
          Me.OpenMenuFromKey(e)
        End If
      Case VirtualKey.VK_MENU
        Me.OpenMenuFromKey(e)
    End Select
  End Sub

  Private Sub OpenMenuFromKey(e As KeyEvent)
    Select Case e.VirtualKey
      Case VirtualKey.VK_MENU, VirtualKey.VK_F10
        ' Ctrl+F10 is not the menu key: it maximizes the help window.
        Dim ctrl = (e.ControlKeyState And (ControlKeyState.LEFT_CTRL_PRESSED Or ControlKeyState.RIGHT_CTRL_PRESSED)) <> 0
        If Not ctrl AndAlso Not Me.MainMenu.IsActive() AndAlso Me.MainMenu IsNot Me.ActiveControl Then
          Me.MainMenu.Activate(Me, Me.ActiveControl)
        End If
    End Select
  End Sub

  ' F1: help; Shift+F1: help on help. (Ctrl+F1 / Alt+F1 are the help window's own next / back keys.)
  Private Sub HandleF1(e As KeyEvent)

    Dim ctrl = (e.ControlKeyState And (ControlKeyState.LEFT_CTRL_PRESSED Or ControlKeyState.RIGHT_CTRL_PRESSED Or ControlKeyState.LEFT_ALT_PRESSED Or ControlKeyState.RIGHT_ALT_PRESSED)) <> 0
    If ctrl Then Exit Sub

    e.Handled = True

    If (e.ControlKeyState And ControlKeyState.SHIFT_PRESSED) <> 0 OrElse (Me.HelpPane.Visible AndAlso Me.ActiveControl Is Me.HelpPane) Then
      Me.ShowHelp(HelpContent.HelpOnHelpId)
    Else
      Me.ShowHelp(HelpContent.ContentsId)
    End If

  End Sub

  Private Sub MyScreen_AfterKeyDown(sender As Object, e As KeyEvent) Handles Me.AfterKeyDown

    ' Completion needs Roslyn and a loaded document (a key can arrive before the first document exists).
    If Not Me.RoslynEnabled OrElse Me.ActiveDocumentBufferView.IsCompletionListActive Then Exit Sub

    Dim document = Me.ActiveDocumentBufferView.Buffer.Document
    If document Is Nothing Then Exit Sub

    If e.Character <> Nothing Then
      Me.ActiveDocumentBufferView.MaybeTriggerCompletionList(e.Character)
    End If

  End Sub

  ''' <summary>Ctrl+\: searches for the selected text (or the word at the cursor).</summary>
  Private Sub FindSelectedText()

    If Me.PopupControl IsNot Nothing Then Return

    Dim view = Me.ActiveDocumentBufferView
    Dim text = If(view.HasTextSelection AndAlso Not view.SelectedText.Contains(vbLf), view.SelectedText, Me.WordAtCursor())

    If text.Length = 0 Then
      Me.ShowFindDialog()
    Else
      Me.FindText(text, False, False)
    End If

  End Sub

  ' The right mouse button asks for help (F1) and, with Shift, for an Instant Watch (Shift+F9), as in QuickBASIC.
  Private Sub MyScreen_MouseDown(sender As Object, e As MouseEvent) Handles Me.MouseDown

    If Me.PopupControl IsNot Nothing Then Return

    If (e.ButtonState And ButtonState.FROM_LEFT_1ST_BUTTON_PRESSED) <> 0 Then Me.FocusClickedWindow(e.X, e.Y)

    If (e.ButtonState And ButtonState.RIGHTMOST_BUTTON_PRESSED) = 0 Then Return

    If (e.ControlKeyState And ControlKeyState.SHIFT_PRESSED) <> 0 Then
      Me.ShowInstantWatch()
    ElseIf Me._rightMouse = "cursor" AndAlso Object.ReferenceEquals(Me.HitTest(e.X, e.Y), Me.DocumentBufferView) Then
      Me.DocumentBufferView.PlaceCursorAt(e.X, e.Y)
      Me.RunToCursor()
    Else
      Me.HandleF1(New KeyEvent(True, ChrW(0), 1, VirtualKey.VK_F1, 0))
    End If

  End Sub

  Private Sub SwitchPane(Optional backwards As Boolean = False)

    ' help window (when open) -> editor -> immediate/output -> help window ...
    Dim windows As New List(Of Control)()
    If Me.HelpPane.Visible Then windows.Add(Me.HelpPane)
    windows.Add(Me._mainView)
    If Me._splitVisible Then windows.Add(Me._splitView)
    windows.Add(Me.ErrorBufferView)

    Dim current As Control = If(Me.ActiveControl Is Me.HelpPane, CType(Me.HelpPane, Control), CType(Me.ActiveBufferView, Control))
    Dim [next] = windows((windows.IndexOf(current) + If(backwards, windows.Count - 1, 1)) Mod windows.Count)

    If [next] Is Me.HelpPane Then
      Me.FocusHelp()
    Else
      Me.ReleaseHelpFocus()
      Me.ActiveBufferView = DirectCast([next], TextBufferView)
      Me.ActiveControl = Me.ActiveBufferView
    End If

    Me.UpdateScrollBars()
    Me.Invalidate()

  End Sub

  Private Sub ViewDocument(bufferView As DocumentTextBufferView, pane As Pane, projectName As String, documentName As String, Optional newText As String = Nothing, Optional title As String = Nothing)
    Dim file = Me.RegisterFile(documentName, title)

    Try
      bufferView.LoadDocument(projectName, documentName)
    Catch ex As Exception
      Me.SetStatus("Unable to load " & documentName & ": " & ex.Message, True)
      Exit Sub
    End Try

    Me._active = file

    Me.SetStatus(Nothing, False)

    pane.Title = Me.TitleOf(file)

    If newText IsNot Nothing Then
      bufferView.Buffer.Text = newText
      Me.OnLineCommit(bufferView.Buffer)
    End If

    Me._savedText = Me.DocumentBuffer.Text
    bufferView.ClearUndo()
    Me.ClearAllMarks()
    Me.ApplyMarks()
  End Sub

  Private ReadOnly OpenDialog As New FileDialog With {.Title = "Open", .Pattern = "*.bas;*.vb;*.vbproj;*.sln;*.slnx", .OnHelp = Sub() Me.ShowDialogHelp()}

  Private _lastOpenDirectory As String

  Private Sub ShowOpenDialog()
    Me.OpenDialog.Show(Me, Me._lastOpenDirectory, AddressOf Me.OpenProgram)
  End Sub

  Public Sub OpenProgram(fileName As String)

    If ProjectFiles.IsProjectFile(fileName) Then
      Me.OpenProjectFile(fileName)
      Return
    End If

    Dim text As String

    Try
      text = IO.File.ReadAllText(fileName)
    Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
      Me.SetStatus("Unable to open " & IO.Path.GetFileName(fileName) & ": " & ex.Message, True)
      Return
    End Try

    Me._lastOpenDirectory = IO.Path.GetDirectoryName(fileName)

    App.OpenWorkspace(fileName)
    Me.ResetFiles()
    Me._files.Add(New ProgramFile With {.Name = IO.Path.GetFileName(fileName), .Path = fileName})
    Me.SetStatus(Nothing, False)
    Me.ViewDocument(Me.DocumentBufferView, Me.DocumentPane, IO.Path.GetFileNameWithoutExtension(fileName), IO.Path.GetFileName(fileName), text)

  End Sub

  Private ReadOnly WelcomeDialog As New NoticeDialog With {.EnterText = "Press Enter to see the Survival Guide",
                                                           .EscapeText = "Press ESC to clear this dialog box"}

  Private Shared Function PlatformName() As String
    If OperatingSystem.IsWindows() Then Return "Windows"
    If OperatingSystem.IsLinux() Then Return "Linux"
    If OperatingSystem.IsMacOS() Then Return "macOS"
    Return "your platform"
  End Function

  ''' <summary>The box QBasic shows at start-up.</summary>
  Public Sub ShowWelcome()

    Me.WelcomeDialog.Lines = New List(Of String) From {"Welcome to QuickVB for " & PlatformName(),
                                                       "",
                                                       "Copyright (C) Dartmouth Didn't, 1964-2026.",
                                                       "All rights reserved."}

    Me.WelcomeDialog.Show(Me, onEnter:=Sub() Me.SetStatus("The Survival Guide is not available yet.", True))

  End Sub

  ' for tests (QuickVB.Tests can see Friend members)
  Friend ReadOnly Property Editor As DocumentTextBuffer
    Get
      Return Me.DocumentBuffer
    End Get
  End Property

  Friend ReadOnly Property EditorView As DocumentTextBufferView
    Get
      Return Me.DocumentBufferView
    End Get
  End Property

  Friend ReadOnly Property CurrentPath As String
    Get
      Return Me._currentPath
    End Get
  End Property

  ' ==========================================================================================
  ' QBasic style dialogs, built on ConsoleGUI.FormDialog / FileDialog
  ' ==========================================================================================

  ' ---- messages -----------------------------------------------------------------------

  ''' <summary>What a menu command or hotkey says while it has nothing behind it yet.</summary>
  Public Sub ShowNotImplemented()
    If Me.PopupControl Is Nothing Then Me.ShowMessage("Not implemented.")
  End Sub

  ''' <summary>A plain message with an OK and a Help button (QBasic's "Syntax error", "Match not found" ...).</summary>
  Public Sub ShowMessage(text As String, Optional afterClose As Action = Nothing)
    Dim d As New FormDialog() With {.MinimumWidth = Math.Max(25, text.Length + 6)}
    d.AddText(text)
    d.AddButton("<  OK  >", "O"c, afterClose, isDefault:=True, isCancel:=True)
    d.AddButton("< Help >", "H"c, Sub() Me.ShowDialogHelp(), closes:=False)
    d.Show(Me)
  End Sub

  ''' <summary>Help &gt; About.</summary>
  Public Sub ShowAbout()
    Dim version = GetType(MyScreen).Assembly.GetName().Version
    Dim d As New FormDialog() With {.MinimumWidth = 49}
    d.AddText("QuickVB for " & PlatformName())
    d.AddText("Version " & version.Major & "." & version.Minor)
    d.AddText("Copyright (C) Dartmouth Didn't, 1964-2026.")
    d.AddButton("<  OK  >", "O"c, isDefault:=True, isCancel:=True)
    d.Show(Me)
  End Sub

  ' ---- unsaved changes, saving, exit ---------------------------------------------------

  Private ReadOnly SaveDialog As New FileDialog With {.Mode = FileDialogMode.Save, .Pattern = "*.bas;*.vb", .DefaultExtension = ".vb", .OnHelp = Sub() Me.ShowDialogHelp()}

  ' Runs once the program has been saved (File > Exit saves first when asked to).
  Private _afterSave As Action

  ''' <summary>What File &gt; Exit does once any unsaved program has been settled; tests replace it.</summary>
  Public Property ExitAction As Action = Sub()
                                           Me.EndSession()
                                           Environment.Exit(0)
                                         End Sub

  ''' <summary>File &gt; Exit: leave, after asking what to do about a program that has not been saved.</summary>
  Public Sub RequestExit()

    If Not Me.HasUnsavedChanges Then
      Me.ExitAction.Invoke()
      Return
    End If

    Dim d As New FormDialog()
    d.AddText("Loaded file is not saved. Save it now?")
    d.AddButton("< Yes >", "Y"c, Sub() Me.SaveAll(Me.ExitAction), isDefault:=True)
    d.AddButton("< No >", "N"c, Sub() Me.ExitAction.Invoke())
    d.AddButton("<Cancel>", "C"c, isCancel:=True)
    d.AddButton("< Help >", "H"c, Sub() Me.ShowDialogHelp(), closes:=False)
    d.Show(Me)

  End Sub

  ''' <summary>File &gt; Save: write the program to its file, asking for a name first when it has none.</summary>
  Public Sub SaveProgram(Optional afterSave As Action = Nothing)
    If Me._currentPath Is Nothing Then
      Me.ShowSaveDialog("Save", afterSave)
    Else
      Me._afterSave = afterSave
      Me.WriteProgram(Me._currentPath)
    End If
  End Sub

  ''' <summary>File &gt; Save As...</summary>
  Public Sub ShowSaveAsDialog()
    Me.ShowSaveDialog("Save As")
  End Sub

  Private Sub ShowSaveDialog(title As String, Optional afterSave As Action = Nothing)
    Me._afterSave = afterSave
    Me.SaveDialog.Title = title
    Me.SaveDialog.Show(Me, If(Me._currentPath IsNot Nothing, IO.Path.GetDirectoryName(Me._currentPath), Me._lastOpenDirectory),
                       AddressOf Me.ConfirmSave, Sub() Me._afterSave = Nothing)
  End Sub

  ' An existing file other than the one being edited is only overwritten when the user agrees.
  Private Sub ConfirmSave(path As String)

    If IO.File.Exists(path) AndAlso Not String.Equals(path, Me._currentPath, StringComparison.OrdinalIgnoreCase) Then
      Dim d As New FormDialog()
      d.AddText(IO.Path.GetFileName(path) & " already exists.")
      d.AddText("Do you want to replace it?")
      d.AddButton("< Yes >", "Y"c, Sub() Me.WriteProgram(path), isDefault:=True)
      d.AddButton("< No >", "N"c, Sub() Me._afterSave = Nothing, isCancel:=True)
      d.Show(Me)
    Else
      Me.WriteProgram(path)
    End If

  End Sub

  Private Sub WriteProgram(path As String)

    Try
      IO.File.WriteAllText(path, Me.DocumentBuffer.Text, New System.Text.UTF8Encoding(False))
    Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
      Me._afterSave = Nothing
      Me.SetStatus("Unable to save " & IO.Path.GetFileName(path) & ": " & ex.Message, True)
      Return
    End Try

    Me._currentPath = path
    Me._savedText = Me.DocumentBuffer.Text
    Me._lastOpenDirectory = IO.Path.GetDirectoryName(path)
    Me.DocumentPane.Title = IO.Path.GetFileName(path)
    Me.SetStatus("Saved " & IO.Path.GetFileName(path), False)
    Me.Invalidate()

    Dim continuation = Me._afterSave
    Me._afterSave = Nothing
    If continuation IsNot Nothing Then continuation()

  End Sub

  ' ---- Run: syntax errors --------------------------------------------------------------

  ' QBasic stops at the first error with a "Syntax error" box and puts the cursor on it.
  Private Sub ShowSyntaxError(errorDiagnostic As Diagnostic)

    Me.ShowMessage("Syntax error",
                   Sub()
                     Me.ShowFileOf(errorDiagnostic)
                     If errorDiagnostic.Location.IsInSource Then
                       Dim position = errorDiagnostic.Location.GetLineSpan().StartLinePosition
                       Me.ActiveDocumentBufferView.MoveCursor(position.Line, position.Character)
                     End If
                     Me.SetStatus(errorDiagnostic.GetMessage(), True)
                     Me.Invalidate()
                   End Sub)

  End Sub

  ' ---- New SUB / New FUNCTION ----------------------------------------------------------

  Private Shared Function IsValidIdentifier(name As String) As Boolean
    Return System.Text.RegularExpressions.Regex.IsMatch(name, "^[A-Za-z_][A-Za-z0-9_]*$")
  End Function

  Public Sub ShowNewProcedureDialog(kind As String)

    Dim d As New FormDialog() With {.Title = "New " & kind.ToUpperInvariant(), .Size = FormDialogSize.Half, .TopPadding = 0}
    Dim name = d.AddInput("Name:", "")

    d.AddButton("< OK >", "O"c,
                Sub() Me.InsertProcedure(kind, name.Text.Trim()),
                Function()
                  If Not IsValidIdentifier(name.Text.Trim()) Then
                    d.Message = "Identifier expected"
                    Return False
                  End If
                  Return True
                End Function,
                isDefault:=True)
    d.AddButton("< Cancel >", "C"c, isCancel:=True)
    d.AddButton("< Help >", "H"c, Sub() Me.ShowDialogHelp(), closes:=False)
    d.Show(Me)

  End Sub

  ' Adds an empty SUB or FUNCTION at the end of the module and puts the cursor in its body.
  Private Sub InsertProcedure(kind As String, name As String)

    Dim text = Me.DocumentBuffer.Text
    Dim at = text.LastIndexOf("End Module", StringComparison.OrdinalIgnoreCase)
    Dim prefix = ""

    If at < 0 Then
      at = text.Length
      If text.Length > 0 AndAlso Not text.EndsWith(vbLf) Then prefix = vbCrLf
    End If

    Dim block As String
    Dim bodyColumn As Integer

    If kind = "Sub" Then
      block = "  Sub " & name & "()" & vbCrLf & "    " & vbCrLf & "  End Sub" & vbCrLf & vbCrLf
      bodyColumn = 4
    Else
      block = "  Function " & name & "() As Integer" & vbCrLf & "    Return 0" & vbCrLf & "  End Function" & vbCrLf & vbCrLf
      bodyColumn = 4
    End If

    Dim headerRow = text.Substring(0, at).Split(vbLf).Length - 1 + If(prefix.Length > 0, 1, 0)

    Me.DocumentBuffer.Text = text.Insert(at, prefix & block)
    Me.ActiveDocumentBufferView.MoveCursor(headerRow + 1, bodyColumn)
    Me.SetStatus("Added " & kind.ToUpperInvariant() & " " & name, False)
    Me.Invalidate()

  End Sub

  ' ---- the program's SUBs and FUNCTIONs -------------------------------------------------

  Private Sub ConfirmDeleteProcedure(procedure As ProcedureInfo)
    Dim d As New FormDialog()
    d.AddText("Delete " & procedure.Kind & " " & procedure.Name & "?")
    d.AddButton("< Yes >", "Y"c,
                Sub()
                  Me.ActivateFile(procedure.File)
                  Me.DeleteProcedure(procedure)
                End Sub, isDefault:=True)
    d.AddButton("< No >", "N"c, isCancel:=True)
    d.Show(Me)
  End Sub

  Private Sub DeleteProcedure(procedure As ProcedureInfo)

    Dim lines = Me.DocumentBuffer.Lines
    Dim endRow = procedure.Row

    Do While endRow < lines.Count AndAlso Not System.Text.RegularExpressions.Regex.IsMatch(lines(endRow).Text, "^\s*End\s+(Sub|Function)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
      endRow += 1
    Loop

    If endRow >= lines.Count Then endRow = lines.Count - 1

    ' the declaration through "End SUB", plus one blank line after it
    Dim last = If(endRow + 1 < lines.Count AndAlso lines(endRow + 1).Text.Trim().Length = 0, endRow + 1, endRow)

    Me.ActiveDocumentBufferView.Edit(Sub()
                                       For row = last To procedure.Row Step -1
                                         Me.DocumentBuffer.RemoveLine(row)
                                       Next

                                       Me.ActiveDocumentBufferView.MoveCursor(Math.Max(0, Math.Min(procedure.Row, Me.DocumentBuffer.Lines.Count - 1)), 0)
                                     End Sub)
    Me.SetStatus("Deleted " & procedure.Kind & " " & procedure.Name, False)
    Me.Invalidate()

  End Sub

  ' ---- Find / Change --------------------------------------------------------------------

  Private _findText As String = ""
  Private _findMatchCase As Boolean
  Private _findWholeWord As Boolean

  ' the found text is shown with a highlight that goes away at the next key
  Private _highlightLine As TextBufferLine
  Private _highlightSpan As ColorSpan

  Private Sub ClearFindHighlight()
    If Me._highlightLine IsNot Nothing Then
      Me._highlightLine.ColorSpans.Remove(Me._highlightSpan)
      Me._highlightLine = Nothing
      Me._highlightSpan = Nothing
      Me.Invalidate()
    End If
  End Sub

  Private Shared Function IsWordCharacter(ch As Char) As Boolean
    Return Char.IsLetterOrDigit(ch) OrElse ch = "_"c
  End Function

  ''' <summary>The word the cursor is on (what Find and Change start with), or "".</summary>
  Private Function WordAtCursor() As String
    Dim lines = Me.DocumentBuffer.Lines
    Dim view = Me.ActiveDocumentBufferView
    If view.CursorRow < 0 OrElse view.CursorRow >= lines.Count Then Return ""
    Dim text = lines(view.CursorRow).Text
    Dim start = Math.Min(view.CursorColumn, text.Length)
    Dim [end] = start
    Do While start > 0 AndAlso IsWordCharacter(text(start - 1))
      start -= 1
    Loop
    Do While [end] < text.Length AndAlso IsWordCharacter(text([end]))
      [end] += 1
    Loop
    Return text.Substring(start, [end] - start)
  End Function

  Private Shared Function IndexOfMatch(line As String, startColumn As Integer, find As String, matchCase As Boolean, wholeWord As Boolean) As Integer

    Dim comparison = If(matchCase, StringComparison.Ordinal, StringComparison.OrdinalIgnoreCase)
    Dim index = startColumn

    Do While index <= line.Length - find.Length

      index = line.IndexOf(find, index, comparison)
      If index < 0 Then Return -1

      Dim before = index = 0 OrElse Not IsWordCharacter(line(index - 1))
      Dim after = index + find.Length >= line.Length OrElse Not IsWordCharacter(line(index + find.Length))

      If Not wholeWord OrElse (before AndAlso after) Then Return index

      index += 1

    Loop

    Return -1

  End Function

  ''' <summary>Moves the cursor to the next match (wrapping round the program unless told not to) and highlights it.</summary>
  Private Function FindNext(skipCurrent As Boolean, Optional wrap As Boolean = True) As Boolean

    Me.ClearFindHighlight()

    If Me._findText.Length = 0 Then Return False

    Dim lines = Me.DocumentBuffer.Lines
    Dim count = lines.Count
    Dim view = Me.ActiveDocumentBufferView
    Dim row = view.CursorRow
    Dim column = view.CursorColumn + If(skipCurrent, 1, 0)
    Dim passes = If(wrap, count, count - row - 1)

    For i = 0 To passes
      Dim r = (row + i) Mod count
      Dim index = IndexOfMatch(lines(r).Text, If(i = 0, column, 0), Me._findText, Me._findMatchCase, Me._findWholeWord)
      If index >= 0 Then
        view.MoveCursor(r, index)
        Me._highlightLine = lines(r)
        Me._highlightSpan = New ColorSpan With {.Start = index, .Length = Me._findText.Length, .ForegroundColor = ConsoleColor.Black,
                                                .BackgroundColor = ConsoleColor.Gray, .IsSticky = True}
        Me._highlightLine.ColorSpans.Add(Me._highlightSpan)
        Me.Invalidate()
        Return True
      End If
    Next

    Return False

  End Function

  Private Sub FindText(text As String, matchCase As Boolean, wholeWord As Boolean)
    Me._findText = text
    Me._findMatchCase = matchCase
    Me._findWholeWord = wholeWord
    If Not Me.FindNext(False) Then Me.ShowMessage("Match not found")
  End Sub

  ''' <summary>Search &gt; Repeat Last Find (F3).</summary>
  Public Sub RepeatFind()
    If Me._findText.Length = 0 Then
      Me.ShowFindDialog()
    ElseIf Not Me.FindNext(True) Then
      Me.ShowMessage("Match not found")
    End If
  End Sub

  Public Sub ShowFindDialog()

    Dim word = Me.WordAtCursor()
    Dim d As New FormDialog() With {.Title = "Find", .Size = FormDialogSize.Medium, .TopPadding = 0}
    Dim what = d.AddInput("Find What:", If(word.Length > 0, word, Me._findText))
    d.AddText("")
    d.AddText("")
    Dim checks = d.AddChecks("Match Upper/Lowercase", "Whole Word")
    checks.Items(0).Checked = Me._findMatchCase
    checks.Items(1).Checked = Me._findWholeWord

    d.AddButton("< OK >", "O"c,
                Sub() Me.FindText(what.Text, checks.Items(0).Checked, checks.Items(1).Checked),
                Function()
                  If what.Text.Length = 0 Then
                    d.Message = "Enter the text to find"
                    Return False
                  End If
                  Return True
                End Function,
                isDefault:=True)
    d.AddButton("< Cancel >", "C"c, isCancel:=True)
    d.AddButton("< Help >", "H"c, Sub() Me.ShowDialogHelp(), closes:=False)
    d.Show(Me)

  End Sub

  Public Sub ShowChangeDialog()

    Dim word = Me.WordAtCursor()
    Dim d As New FormDialog() With {.Title = "Change", .Size = FormDialogSize.Medium, .TopPadding = 0}
    Dim what = d.AddInput("Find What:", If(word.Length > 0, word, Me._findText))
    Dim changeTo = d.AddInput("Change To:", "")
    d.AddText("")
    d.AddText("")
    Dim checks = d.AddChecks("Match Upper/Lowercase", "Whole Word")
    checks.Items(0).Checked = Me._findMatchCase
    checks.Items(1).Checked = Me._findWholeWord

    Dim validate = Function() As Boolean
                     If what.Text.Length = 0 Then
                       d.Message = "Enter the text to find"
                       Return False
                     End If
                     Return True
                   End Function

    d.AddButton("< Find and Verify >", "F"c,
                Sub() Me.StartVerify(what.Text, changeTo.Text, checks.Items(0).Checked, checks.Items(1).Checked),
                validate, isDefault:=True)
    d.AddButton("< Change All >", "A"c,
                Sub() Me.ChangeAll(what.Text, changeTo.Text, checks.Items(0).Checked, checks.Items(1).Checked),
                validate)
    d.AddButton("< Cancel >", "C"c, isCancel:=True)
    d.AddButton("< Help >", "H"c, Sub() Me.ShowDialogHelp(), closes:=False)
    d.Show(Me)

  End Sub

  ''' <summary>Replaces every match in the program and says how many there were.</summary>
  Private Sub ChangeAll(find As String, replacement As String, matchCase As Boolean, wholeWord As Boolean)

    Me._findText = find
    Me._findMatchCase = matchCase
    Me._findWholeWord = wholeWord
    Me.ClearFindHighlight()

    Dim lines = Me.DocumentBuffer.Lines.Select(Function(l) l.Text).ToList()
    Dim changed = 0

    For i = 0 To lines.Count - 1
      Dim sb As New System.Text.StringBuilder()
      Dim line = lines(i)
      Dim position = 0
      Do
        Dim index = IndexOfMatch(line, position, find, matchCase, wholeWord)
        If index < 0 Then Exit Do
        sb.Append(line, position, index - position).Append(replacement)
        position = index + find.Length
        changed += 1
      Loop
      If position > 0 Then
        sb.Append(line, position, line.Length - position)
        lines(i) = sb.ToString()
      End If
    Next

    If changed = 0 Then
      Me.ShowMessage("Match not found")
      Return
    End If

    Dim row = Me.ActiveDocumentBufferView.CursorRow
    Dim column = Me.ActiveDocumentBufferView.CursorColumn
    Me.ActiveDocumentBufferView.Edit(Sub()
                                       Me.DocumentBuffer.Text = String.Join(vbCrLf, lines)
                                       Me.ActiveDocumentBufferView.MoveCursor(Math.Min(row, lines.Count - 1), column)
                                     End Sub)
    Me.SetStatus(changed & If(changed = 1, " occurrence", " occurrences") & " changed", False)
    Me.Invalidate()

  End Sub

  Private Sub StartVerify(find As String, replacement As String, matchCase As Boolean, wholeWord As Boolean)
    Me._findText = find
    Me._findMatchCase = matchCase
    Me._findWholeWord = wholeWord
    Me.VerifyNext(replacement, False)
  End Sub

  ' Finds the next match and asks whether to change it; carries on until the end of the program.
  Private Sub VerifyNext(replacement As String, skipCurrent As Boolean)

    If Not Me.FindNext(skipCurrent, wrap:=False) Then
      Me.ShowMessage("Change complete")
      Return
    End If

    Dim view = Me.ActiveDocumentBufferView
    Dim row = view.CursorRow
    Dim column = view.CursorColumn
    Dim length = Me._findText.Length

    Dim d As New FormDialog()
    d.AddText("Change this occurrence?")
    d.AddButton("< Change >", "C"c,
                Sub()
                  Me.ClearFindHighlight()
                  view.Edit(Sub()
                              Me.DocumentBuffer.RemoveText(row, column, length)
                              Me.DocumentBuffer.InsertText(row, column, replacement)
                              view.MoveCursor(row, column + replacement.Length)
                            End Sub)
                  Me.VerifyNext(replacement, False)
                End Sub,
                isDefault:=True)
    d.AddButton("< Skip >", "S"c, Sub() Me.VerifyNext(replacement, True))
    d.AddButton("<Cancel>", ControlChars.NullChar, Sub() Me.ClearFindHighlight(), isCancel:=True)
    d.Show(Me)

  End Sub

  ' ---- Print, Help Path -----------------------------------------------------------------

  Public Sub ShowPrintDialog()
    Dim d As New FormDialog() With {.Title = "Print"}
    Dim what = d.AddRadios(2, {"Selected Text Only", "Current Window", "Entire Program"})
    d.AddButton("< OK >", "O"c, Sub() Me.SetStatus("Printing is not available yet.", True), isDefault:=True)
    d.AddButton("< Cancel >", "C"c, isCancel:=True)
    d.AddButton("< Help >", "H"c, Sub() Me.ShowDialogHelp(), closes:=False)
    d.Show(Me)
  End Sub

  ' ---- Options > Display ------------------------------------------------------------------

  Private ReadOnly _normalColors As New ColorItem("Normal Text", ConsoleColor.Gray, ConsoleColor.DarkBlue)
  Private ReadOnly _statementColors As New ColorItem("Current Statement", ConsoleColor.White, ConsoleColor.DarkBlue)
  Private ReadOnly _breakpointColors As New ColorItem("Breakpoint Lines", ConsoleColor.Gray, ConsoleColor.DarkRed)
  Private _showScrollBars As Boolean = True
  Private _tabStops As Integer = 4

  ''' <summary>The spaces of one level of indentation: the Tab Stops of the Display dialog (what the formatter indents with, too).</summary>
  Public ReadOnly Property IndentSize As Integer
    Get
      Return Me._tabStops
    End Get
  End Property

  ' Where the Options > Display choices are kept between runs (Nothing: not saved, which is what the tests want).
  Public Property SettingsPath As String

  Private Shared Function ParseColors(text As String, item As ColorItem) As Boolean
    Dim parts = text.Split(","c)
    Dim fg, bg As ConsoleColor
    If parts.Length = 2 AndAlso [Enum].TryParse(parts(0).Trim(), True, fg) AndAlso [Enum].TryParse(parts(1).Trim(), True, bg) Then
      item.Foreground = fg
      item.Background = bg
      Return True
    End If
    Return False
  End Function

  ''' <summary>Reads <see cref="SettingsPath"/> (if it exists) and applies it.</summary>
  Public Sub LoadSettings()

    If Me.SettingsPath Is Nothing OrElse Not IO.File.Exists(Me.SettingsPath) Then Return

    Try
      For Each line In IO.File.ReadAllLines(Me.SettingsPath)
        Dim at = line.IndexOf("="c)
        If at <= 0 Then Continue For
        Dim key = line.Substring(0, at).Trim().ToLowerInvariant()
        Dim value = line.Substring(at + 1).Trim()
        Dim number As Integer
        Select Case key
          Case "normal" : ParseColors(value, Me._normalColors)
          Case "statement" : ParseColors(value, Me._statementColors)
          Case "breakpoint" : ParseColors(value, Me._breakpointColors)
          Case "scrollbars" : Boolean.TryParse(value, Me._showScrollBars)
          Case "syntaxchecking" : Boolean.TryParse(value, Me._syntaxChecking)
          Case "fullmenus" : Boolean.TryParse(value, Me._fullMenus)
          Case "rightmouse" : If value = "help" OrElse value = "cursor" Then Me._rightMouse = value
          Case "tabstops" : If Integer.TryParse(value, number) AndAlso number >= DisplayDialog.MinTabStops AndAlso number <= DisplayDialog.MaxTabStops Then Me._tabStops = number
        End Select
      Next
    Catch ex As System.IO.IOException
      ' unreadable settings: the defaults stay
    End Try

    Me.ApplyDisplaySettings()
    Me.ApplyMenuSettings()

  End Sub

  Private Sub SaveSettings()

    If Me.SettingsPath Is Nothing Then Return

    Try
      IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(Me.SettingsPath))
      IO.File.WriteAllLines(Me.SettingsPath, {"normal=" & Me._normalColors.Foreground & "," & Me._normalColors.Background,
                                              "statement=" & Me._statementColors.Foreground & "," & Me._statementColors.Background,
                                              "breakpoint=" & Me._breakpointColors.Foreground & "," & Me._breakpointColors.Background,
                                              "scrollbars=" & Me._showScrollBars,
                                              "rightmouse=" & Me._rightMouse,
                                              "syntaxchecking=" & Me._syntaxChecking,
                                              "fullmenus=" & Me._fullMenus,
                                              "tabstops=" & Me._tabStops})
    Catch ex As Exception When TypeOf ex Is System.IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
      Me.SetStatus("The settings could not be saved: " & ex.Message, True)
    End Try

  End Sub

  Public Sub ShowDisplayDialog()

    Dim d As New DisplayDialog() With {.ScrollBars = Me._showScrollBars, .TabStops = Me._tabStops}
    d.Items.Add(Me._normalColors)
    d.Items.Add(Me._statementColors)
    d.Items.Add(Me._breakpointColors)

    d.Show(Me, Sub()
                 Me._showScrollBars = d.ScrollBars
                 Me._tabStops = d.TabStops
                 Me.ApplyDisplaySettings()
                 Me.SaveSettings()
               End Sub)

  End Sub

  ''' <summary>Puts the Normal Text colours, scroll bars and tab stops on the editor and the output pane.</summary>
  Private Sub ApplyDisplaySettings()

    Dim foreground = Me._normalColors.Foreground
    Dim background = Me._normalColors.Background

    Me.ForegroundColor = foreground
    Me.BackgroundColor = background

    Me.ErrorBufferView.ForegroundColor = foreground
    Me.ErrorBufferView.BackgroundColor = background
    Me.ErrorBufferView.TabStops = Me._tabStops

    For Each pane In {Me._mainPane, Me.DiagnosticsPane}
      pane.ForegroundColor = foreground
      pane.BackgroundColor = background
    Next

    Me.ApplyViewColors()

    ' the syntax colours sit on the new background
    Me._mainBuffer.EditorBackground = background
    If Me.RoslynEnabled Then
      Me.OnCharacterChanged(Me._mainBuffer)
      If Me._splitVisible Then Me.OnCharacterChanged(Me._splitBuffer)
    End If

    Me.Invalidate()

  End Sub

  ' ---- Options > Set Paths ------------------------------------------------------------------

  Private ReadOnly _searchPaths As String() = {"", "", "", ""}   ' executable, include, library, help

  Public Sub ShowSetPathsDialog()

    Dim d As New FormDialog() With {.Title = "Set Paths", .Size = FormDialogSize.Large, .LabelIndent = 3, .TopPadding = 0, .BottomPadding = 0}
    Dim header = d.AddText("Search Path For:", centered:=False)
    Dim executable = d.AddInput("Executable files:", Me._searchPaths(0), "(.EXE, .COM)")
    d.AddText("")
    Dim includes = d.AddInput("Include files:", Me._searchPaths(1), "(.BI, .BAS)")
    d.AddText("")
    Dim libraries = d.AddInput("Library files:", Me._searchPaths(2), "(.LIB, .QLB)")
    d.AddText("")
    Dim help = d.AddInput("Help file:", Me._searchPaths(3), "(.HLP)")

    Dim fields = {executable, includes, libraries, help}

    d.AddButton("< OK >", "O"c,
                Sub()
                  For i = 0 To 3
                    Me._searchPaths(i) = fields(i).Text.Trim()
                  Next
                  Me.SetStatus("Search paths set", False)
                End Sub,
                Function()
                  For Each field In fields
                    Dim value = field.Text.Trim()
                    If value.Length > 0 AndAlso Not IO.Directory.Exists(value) Then
                      header.Text = "Path not found: " & value
                      Return False
                    End If
                  Next
                  Return True
                End Function,
                isDefault:=True)
    d.AddButton("< Cancel >", "C"c, isCancel:=True)
    d.AddButton("< Help >", "H"c, Sub() Me.ShowDialogHelp(), closes:=False)
    d.Show(Me)

  End Sub

  ' ---- Debug: Add Watch, Watchpoint, Instant Watch ------------------------------------------

  Private ReadOnly _watches As New List(Of String)()
  Private ReadOnly _watchpoints As New List(Of String)()

  ' One line of text, one input box and OK / Cancel / Help: Add Watch, Watchpoint, Modify COMMAND$.
  Private Sub ShowExpressionDialog(title As String, prompt As String, initial As String, onOk As Action(Of String), Optional allowEmpty As Boolean = False)

    Dim d As New FormDialog() With {.Title = title, .Size = FormDialogSize.Medium, .BottomPadding = 0}
    d.AddText(prompt)
    Dim note = d.AddText("")
    Dim expression = d.AddInput("", initial)

    d.AddButton("< OK >", "O"c,
                Sub() onOk(expression.Text.Trim()),
                Function()
                  If Not allowEmpty AndAlso expression.Text.Trim().Length = 0 Then
                    note.Text = "Enter an expression"
                    Return False
                  End If
                  Return True
                End Function,
                isDefault:=True)
    d.AddButton("< Cancel >", "C"c, isCancel:=True)
    d.AddButton("< Help >", "H"c, Sub() Me.ShowDialogHelp(), closes:=False)
    d.Show(Me)

  End Sub

  Public Sub ShowAddWatchDialog()
    Me.ShowExpressionDialog("Add Watch", "Enter expression to be added to Watch window:", Me.WordAtCursor(),
                            Sub(text)
                              Me._watches.Add(text)
                              Me.RefreshWatchWindow()
                              Me.SetStatus("Watch added: " & text, False)
                            End Sub)
  End Sub

  Public Sub ShowWatchpointDialog()
    Me.ShowExpressionDialog("Watchpoint", "Enter expression (will break when TRUE):", "",
                            Sub(text)
                              Me._watchpoints.Add(text)
                              Me.SetStatus("Watchpoint added: " & text, False)
                            End Sub)
  End Sub

  ' ---- Run: Modify COMMAND$, Make EXE File, Make Library ----------------------------------------

  ' The text the running program receives as its command line.
  Private _commandLine As String = ""

  Public Sub ShowCommandDialog()
    Me.ShowExpressionDialog("Modify COMMAND$", "Enter text for COMMAND$:", Me._commandLine,
                            Sub(text)
                              Me._commandLine = text
                              Me.SetStatus("COMMAND$ set", False)
                            End Sub, allowEmpty:=True)
  End Sub

  Private Function ResolveOutputPath(name As String, extension As String) As String
    Dim path = If(IO.Path.HasExtension(name), name, name & extension)
    If IO.Path.IsPathRooted(path) Then Return path
    Return IO.Path.Combine(If(Me._currentPath IsNot Nothing, IO.Path.GetDirectoryName(Me._currentPath), If(Me._lastOpenDirectory, IO.Directory.GetCurrentDirectory())), path)
  End Function

  Private Function DefaultOutputName() As String
    If Me._currentPath IsNot Nothing Then Return IO.Path.GetFileNameWithoutExtension(Me._currentPath)
    Return ""
  End Function

  Public Sub ShowMakeExeDialog()

    If Not Me.RoslynEnabled Then
      Me.SetStatus("NOTE: Please enable Roslyn on the Options menu first.", True)
      Return
    End If

    Dim d As New FormDialog() With {.Title = "Make EXE File", .Size = FormDialogSize.Large, .TopPadding = 0, .BottomPadding = 0}
    Dim name = d.AddInput("EXE File Name:", Me.DefaultOutputName())
    d.AddText("")
    Dim options = d.AddOptions("Produce Debug Code", "Produce:", 0, {"EXE Requiring .NET Runtime", "Stand-Alone EXE File"})

    Dim validate = Function() As Boolean
                     Return name.Text.Trim().Length > 0
                   End Function

    d.AddButton("< Make EXE >", "M"c, Sub() Me.MakeExe(name.Text.Trim(), options.Check.Checked, options.Radios.SelectedIndex = 1, False), validate, isDefault:=True)
    d.AddButton("< Make EXE and Exit >", "X"c, Sub() Me.MakeExe(name.Text.Trim(), options.Check.Checked, options.Radios.SelectedIndex = 1, True), validate)
    d.AddButton("< Cancel >", "C"c, isCancel:=True)
    d.AddButton("< Help >", "H"c, Sub() Me.ShowDialogHelp(), closes:=False)
    d.Show(Me)

  End Sub

  Private Sub MakeExe(name As String, debugCode As Boolean, standAlone As Boolean, thenExit As Boolean)

    If standAlone Then
      Me.ShowMessage("Stand-alone EXE files are not available yet")
      Return
    End If

    Me.UpdateMainModule()

    Dim path = Me.ResolveOutputPath(name, QuickVBWorkspace.ExecutableSuffix)
    Dim diagnostics As List(Of Diagnostic)

    Try
      diagnostics = App.TheWorkspace.MakeExecutable(path, debugCode)
    Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
      Me.SetStatus("Unable to make " & IO.Path.GetFileName(path) & ": " & ex.Message, True)
      Return
    End Try

    Dim firstError = diagnostics.FirstOrDefault(Function(x) x.Severity = DiagnosticSeverity.Error)
    If firstError IsNot Nothing Then
      Me.UpdateDiagnostics(diagnostics)
      Me.ShowSyntaxError(firstError)
      Return
    End If

    Me.SetStatus("Made " & path, False)
    If thenExit Then Me.ExitAction.Invoke()

  End Sub

  Public Sub ShowMakeLibraryDialog()

    If Not Me.RoslynEnabled Then
      Me.SetStatus("NOTE: Please enable Roslyn on the Options menu first.", True)
      Return
    End If

    Dim d As New FormDialog() With {.Title = "Make Library", .Size = FormDialogSize.Large, .TopPadding = 0, .BottomPadding = 0}
    Dim name = d.AddInput("Library File Name:", Me.DefaultOutputName())
    d.AddText("")
    Dim checks = d.AddChecks("Produce Debug Code")

    Dim validate = Function() As Boolean
                     Return name.Text.Trim().Length > 0
                   End Function

    d.AddButton("< Make Library >", "M"c, Sub() Me.MakeLibrary(name.Text.Trim(), checks.Items(0).Checked, False), validate, isDefault:=True)
    d.AddButton("< Make Library and Exit >", "X"c, Sub() Me.MakeLibrary(name.Text.Trim(), checks.Items(0).Checked, True), validate)
    d.AddButton("< Cancel >", "C"c, isCancel:=True)
    d.AddButton("< Help >", "H"c, Sub() Me.ShowDialogHelp(), closes:=False)
    d.Show(Me)

  End Sub

  Private Sub MakeLibrary(name As String, debugCode As Boolean, thenExit As Boolean)

    Dim path = Me.ResolveOutputPath(name, ".dll")
    Dim diagnostics As List(Of Diagnostic)

    Try
      diagnostics = App.TheWorkspace.MakeLibrary(path, debugCode)
    Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
      Me.SetStatus("Unable to make " & IO.Path.GetFileName(path) & ": " & ex.Message, True)
      Return
    End Try

    Dim firstError = diagnostics.FirstOrDefault(Function(x) x.Severity = DiagnosticSeverity.Error)
    If firstError IsNot Nothing Then
      Me.UpdateDiagnostics(diagnostics)
      Me.ShowSyntaxError(firstError)
      Return
    End If

    Me.SetStatus("Made " & path, False)
    If thenExit Then Me.ExitAction.Invoke()

  End Sub

  ' ---- the Help window ---------------------------------------------------------------------

  Private ReadOnly HelpPane As New HelpWindow()

  ' where the focus goes back to when the help window closes
  Private _helpReturnTo As Control
  Private _cursorHiddenForHelp As Boolean

  Private Sub InitializeHelp()
    Me.HelpPane.Topics = AddressOf HelpContent.Find
    Me.HelpPane.TopicOrder = HelpContent.Order
    Me.HelpPane.OnClose = AddressOf Me.CloseHelp
    Me.HelpPane.OnLayoutChanged = Sub()
                                    Me.LayoutChrome()
                                    Me.Invalidate()
                                  End Sub
  End Sub

  ''' <summary>Shows a help topic in the window above the editor (the Contents when none is named).</summary>
  Public Sub ShowHelp(Optional topicId As String = HelpContent.ContentsId)

    If Not Me.HelpPane.Visible Then
      Me._helpReturnTo = Me.ActiveControl
      Me.HelpPane.Visible = True
    End If

    Me.HelpPane.Open(topicId)
    Me.FocusHelp()
    Me.LayoutChrome()
    Me.Invalidate()

  End Sub

  Public Sub CloseHelp()

    If Not Me.HelpPane.Visible Then Return

    Me.HelpPane.Visible = False
    Me.HelpPane.Maximized = False
    Me.ReleaseHelpFocus()
    Me.ActiveControl = If(Me._helpReturnTo, CType(Me.DocumentBufferView, Control))
    Me.ActiveBufferView = If(TryCast(Me.ActiveControl, TextBufferView), CType(Me.DocumentBufferView, TextBufferView))
    Me.LayoutChrome()
    Me.Invalidate()

  End Sub

  Private Sub FocusHelp()
    Me.ActiveControl = Me.HelpPane
    If Not Me._cursorHiddenForHelp Then
      Me._cursorHiddenForHelp = True
      Me.HideCursor()
    End If
  End Sub

  Private Sub ReleaseHelpFocus()
    If Me._cursorHiddenForHelp Then
      Me._cursorHiddenForHelp = False
      Me.ShowCursor()
    End If
  End Sub

  Friend ReadOnly Property Help As HelpWindow
    Get
      Return Me.HelpPane
    End Get
  End Property

  Public Sub SetStatus(message As String, highlight As Boolean)
    Dim defaultStatus As String
    Dim background As ConsoleColor
    Dim foreground As ConsoleColor

    If Me.ActiveDocumentBufferView.Buffer.Document IsNot Nothing AndAlso Me.ActiveDocumentBufferView.Buffer.Document.Name.EndsWith(".vb") Then
      defaultStatus = "<F6=Window> <F5=Run>"
      background = ConsoleColor.DarkCyan
      foreground = ConsoleColor.White
    Else
      defaultStatus = "F6 Window  F5 Run  F10 Menu"
      background = ConsoleColor.Gray
      foreground = ConsoleColor.Black
    End If

    If message IsNot Nothing Then
      Me.StatusTextLabel.Text = message
    Else
      Me.StatusTextLabel.Text = defaultStatus
    End If

    If Not highlight Then
      Me.StatusTextLabel.BackgroundColor = background
      Me.StatusTextLabel.ForegroundColor = foreground
    Else
      Me.StatusTextLabel.BackgroundColor = foreground
      Me.StatusTextLabel.ForegroundColor = background
    End If
  End Sub

  Public Sub ViewDocument(projectName As String, documentName As String, Optional newText As String = Nothing, Optional title As String = Nothing)
    Me.ViewDocument(Me.DocumentBufferView, Me.DocumentPane, projectName, documentName, newText, title)
  End Sub


  ' Compilation:

  Private _diagnostics As IReadOnlyList(Of Diagnostic)

  ''' <summary>Run &gt; Start (also /RUN on the command line).</summary>
  Public Sub StartProgram()
    Me.RunProgram()
  End Sub

  ''' <summary>The text the running program receives as COMMAND$ (Run &gt; Modify COMMAND$, or /CMD on the command line).</summary>
  Public Property CommandLineText As String
    Get
      Return Me._commandLine
    End Get
    Set(value As String)
      Me._commandLine = If(value, "")
    End Set
  End Property

  Private Sub RunProgram()
    If Not Me.RoslynEnabled Then
      Me.SetStatus("NOTE: Please enable Roslyn on the Options menu first.", True)
      Exit Sub
    End If

    Me.ClearFindHighlight()
    Me.PushEditorTextToWorkspace()
    Me.UpdateMainModule()
    Me.PrepareToRun()

    Dim diagnostics As List(Of Diagnostic)

    Try
      Dim name = Me.ProgramName()
      diagnostics = App.TheWorkspace.RunProgram(Me, Me._commandLine,
                                                Sub(exitCode)
                                                  Me.SetStatus(name & " ended" & If(exitCode <> 0, " (exit code " & exitCode & ")", "") & ".", exitCode <> 0)
                                                  Me.Invalidate()
                                                End Sub)
      If App.TheWorkspace.IsWindowedProgram AndAlso OperatingSystem.IsWindows() AndAlso Not diagnostics.Any(Function(d) d.Severity = DiagnosticSeverity.Error) Then
        Me.SetStatus(name & " is running in its own window. Close the window to come back to it here.", False)
      End If
    Catch ex As InvalidOperationException
      Me.SetStatus(ex.Message, True)
      Me.ShowMessage(ex.Message)
      Return
    End Try

    Dim firstError = diagnostics.FirstOrDefault(Function(d) d.Severity = DiagnosticSeverity.Error)

    If firstError IsNot Nothing Then
      Me.UpdateDiagnostics(diagnostics)
      Me.ShowSyntaxError(firstError)
    End If
  End Sub

  Private Sub BuildProgram()
    If Not Me.RoslynEnabled Then
      Me.SetStatus("NOTE: Please enable Roslyn on the Options menu first.", True)
      Exit Sub
    End If

    Dim diagnostics = App.TheWorkspace.BuildProgram()
    Me.UpdateDiagnostics(diagnostics)
  End Sub

  Private Sub UpdateDiagnostics(diagnostics As IEnumerable(Of Diagnostic), Optional cancel As CancellationToken = Nothing)
    If diagnostics IsNot Nothing AndAlso diagnostics.Any Then
      Dim diagnosticsString = String.Join(Environment.NewLine, diagnostics)
      Me.DiagnosticsBuffer.Text = diagnosticsString
      Me.DiagnosticsPane.Title = "Diagnostics"
    Else
      Me.DiagnosticsBuffer.Text = ""
      Me.DiagnosticsPane.Title = "Output"
    End If

    If diagnostics IsNot Nothing Then
      Me._diagnostics = diagnostics.ToList.AsReadOnly
      For Each slot In Me.EditorSlots()
        slot.Item1.Buffer.Colorize(Me._diagnostics, cancel)
      Next
    End If

    Screen.GetScreen.Invalidate()
  End Sub


  ' Colorization/Formatting:

  Private _lineDirty As Boolean = False

  Private Sub DocumentBuffer_BufferChanged(sender As Object, e As EventArgs)
    If Me._syncingViews Then Exit Sub

    Me.SyncOtherView(CType(sender, DocumentTextBuffer))

    If Not Me.RoslynEnabled Then Exit Sub

    Dim buffer = CType(sender, DocumentTextBuffer)

    Me._lineDirty = True

    Me.OnCharacterChanged(buffer)
  End Sub

  Private Sub DocumentBufferView_RowChanged(sender As Object, e As RowChangedEventArgs)
    If Not Me.RoslynEnabled Then Exit Sub

    Dim bufferView = CType(sender, DocumentTextBufferView)

    If Me._lineDirty Then
      Me._lineDirty = False

      bufferView.AddMissingParentheses(e.OldRow)
      Me._lineDirty = False

      Me.OnLineCommit(bufferView.Buffer)
    End If
  End Sub

  Private Sub DocumentBufferView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
    If Not Me.RoslynEnabled Then Exit Sub

    Dim bufferView = CType(sender, DocumentTextBufferView)

    bufferView.UpdateDiagnostics(Me._diagnostics)
  End Sub

  Private Sub OnCharacterChanged(buffer As DocumentTextBuffer)
    If Not Me.RoslynEnabled Then Exit Sub

    ' TODO: Do this conversion in a background task?
    App.TheWorkspace.ChangedDocumentTextInternal(buffer.DocumentId, buffer.Text)

    buffer.Colorize()
  End Sub

  Private Sub OnLineCommit(buffer As DocumentTextBuffer)
    If Not Me.RoslynEnabled Then Exit Sub

    Dim workspace = App.TheWorkspace

    buffer.Format()
    Me.UpdateMainModule()
    workspace.BackgroundCompile(Me,
        Sub(diagnostics, cancel)
          ' with Syntax Checking off nothing is flagged until the program is run
          ' (and nothing is said about a program that has not been started: an empty editor is not an error)
          Me.UpdateDiagnostics(If(Me._syntaxChecking AndAlso Not Me.IsProgramBlank(), diagnostics, Nothing))
          Me.AddMissingQbImports()
          Me.ApplyNameCasing()
          If Me._settleSavedText Then
            Me._settleSavedText = False
            Me._savedText = Me.DocumentBuffer.Text
          End If
        End Sub)
  End Sub

  Private Sub RefreshDocument(buffer As DocumentTextBuffer)
    Me.OnCharacterChanged(buffer)
    Me.OnLineCommit(buffer)
  End Sub

  Public Sub EnableRoslyn()
    If Me.RoslynEnabled Then Exit Sub

    Me.SetStatus(Nothing, False)

    Me.RoslynEnabled = True
    Me.RoslynMenuItem.Name = "Disable &Roslyn"
    Me.OptionsMenu.Layout()

    ' Roslyn formats the program when it takes over; that is not a change the user made.
    Dim wasSaved = Not Me.HasUnsavedChanges
    Me.RefreshDocument(Me.ActiveDocumentBufferView.Buffer)
    If wasSaved Then Me._savedText = Me.DocumentBuffer.Text
  End Sub

  Public Sub DisableRoslyn()
    If Not Me.RoslynEnabled Then Exit Sub

    Me.RoslynEnabled = False
    Me.RoslynMenuItem.Name = "Enable &Roslyn"
    Me.OptionsMenu.Layout()

    ' Back to plain text: no semantic colours, diagnostics or error highlights.
    Me._diagnostics = Nothing
    Me.UpdateDiagnostics(Nothing)
    For Each line In Me.DocumentBuffer.Lines
      line.ColorSpans.Clear()
    Next
    Me.Invalidate()
  End Sub

  Private Sub ToggleRoslyn()
    If Me.RoslynEnabled Then
      Me.DisableRoslyn()
    Else
      Me.EnableRoslyn()
    End If
  End Sub
End Class
