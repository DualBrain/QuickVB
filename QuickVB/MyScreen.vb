' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Imports ConsoleGUI
'Imports System.ComponentModel.Composition.Hosting
Imports System.Threading
Imports Microsoft.CodeAnalysis

Public Class MyScreen
  Inherits Screen

  Private WithEvents DocumentBuffer As DocumentTextBuffer
  Private WithEvents DocumentBufferView As DocumentTextBufferView
  Private WithEvents DocumentPane As Pane

  Private WithEvents DiagnosticsBuffer As TextBuffer
  Private WithEvents ErrorBufferView As TextBufferView
  Private WithEvents DiagnosticsPane As Pane

  Private ReadOnly MainMenu As Menu

  Private ReadOnly SystemMenuItem As MenuItem

  Private ReadOnly StatusTextLabel As Label
  Private ReadOnly StatusPositionLabel As Label

  Private RoslynEnabled As Boolean = False

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

    Console.Title = "QuickVB"

    Me.DocumentBuffer = New DocumentTextBuffer
    Me.DiagnosticsBuffer = New TextBuffer

    Me.DocumentBufferView = New DocumentTextBufferView With {.Buffer = Me.DocumentBuffer,
                                                             .BackgroundColor = ConsoleColor.DarkBlue,
                                                             .ForegroundColor = ConsoleColor.Gray}

    Me.DocumentPane = New Pane With {.Left = 0,
                                     .Top = 1,
                                     .Width = 80,
                                     .Height = 21,
                                     .PaddingBottom = 2,
                                     .BackgroundColor = ConsoleColor.DarkBlue,
                                     .ForegroundColor = ConsoleColor.Gray,
                                     .Title = "Loading..."}

    Me.DocumentPane.Controls.Add(Me.DocumentBufferView)

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

    Dim SystemMenu = New Menu With {.DropDownMenu = True}

    Dim FileMenu = New Menu With {.DropDownMenu = True}

    FileMenu.SetMenuItems(New MenuItem() {New MenuItem(Name:="&New Program", Action:=Sub()
                                                                                       Me.StatusPositionLabel.Visible = Not Me.StatusPositionLabel.Visible
                                                                                       'Dim p = New Pane With {.Left = 10,
                                                                                       '                       .Top = 10,
                                                                                       '                       .Width = 10,
                                                                                       '                       .Height = 10,
                                                                                       '                       .BackgroundColor = ConsoleColor.White,
                                                                                       '                       .ForegroundColor = ConsoleColor.Black,
                                                                                       '                       .Title = "Temp",
                                                                                       '                       .Visible = False,
                                                                                       '                       .Fill = True}
                                                                                       'Me.DocumentPane.Controls.Add(p)
                                                                                     End Sub),
                                          New MenuItem(Name:="&Open Program...", Action:=Sub()
                                                                                           Dim fn = App.OpenWorkspace()
                                                                                           If fn Is Nothing Then Return
                                                                                           Me.SetStatus(Nothing, False)
                                                                                           Dim txt = IO.File.ReadAllText(fn)
                                                                                           Me.ViewDocument(Me.DocumentBufferView, Me.DocumentPane, IO.Path.GetFileNameWithoutExtension(fn), IO.Path.GetFileName(fn), txt)
                                                                                         End Sub),
                                          New MenuItem(Name:="&Merge..."),
                                          New MenuItem(Name:="&Save..."),
                                          New MenuItem(Name:="Save &As..."),
                                          New MenuItem(Name:="Sa&ve All"),
                                          New MenuItem(Separator:=True),
                                          New MenuItem(Name:="&Create File..."),
                                          New MenuItem(Name:="&Load File..."),
                                          New MenuItem(Name:="&Unload File..."),
                                          New MenuItem(Separator:=True),
                                          New MenuItem(Name:="&Print..."),
                                          New MenuItem(Name:="&DOS Shell"),
                                          New MenuItem(Separator:=True),
                                          New MenuItem(Name:="E&xit", Action:=Sub() Environment.Exit(0))})
    'New MenuItem(Name:="Open &Self", Action:=Sub()
    '                                           App.LoadSelfWorkspace()
    '                                           Me.SetStatus(Nothing, False)
    '                                           Me.ViewDocument(Me.DocumentBufferView, Me.DocumentPane, "QuickVB", "MyScreen.vb")
    '                                         End Sub),

    Dim EditMenu = New Menu With {.DropDownMenu = True}

    EditMenu.SetMenuItems(New MenuItem() {New MenuItem(Name:="Undo   Alt+Backspace"),
                                          New MenuItem(Name:="Cu&t       Shift+Del"),
                                          New MenuItem(Name:="&Copy       Ctrl+Ins"),
                                          New MenuItem(Name:="&Paste     Shift+Ins"),
                                          New MenuItem(Name:="Clear            Del"),
                                          New MenuItem(Separator:=True),
                                          New MenuItem(Name:="New &SUB..."),
                                          New MenuItem(Name:="New &FUNCTION...")})

    Dim ViewMenu = New Menu With {.DropDownMenu = True}

    ViewMenu.SetMenuItems(New MenuItem() {New MenuItem(Name:="&SUBs...            F2"),
                                          New MenuItem(Name:="N&ext SUB      Shift+F2"),
                                          New MenuItem(Name:="S&plit"),
                                          New MenuItem(Separator:=True),
                                          New MenuItem(Name:="&Next Statement"),
                                          New MenuItem(Name:="O&utput Screen      F4"),
                                          New MenuItem(Separator:=True),
                                          New MenuItem(Name:="Included Files"),
                                          New MenuItem(Name:="Included &Lines"),
                                          New MenuItem(Separator:=True),
                                          New MenuItem(Name:="Switch to..."),
                                          New MenuItem(Name:="QuickVB\MyScreen.vb", Action:=Sub() Me.ViewDocument(Me.DocumentBufferView, Me.DocumentPane, "QuickVB", "MyScreen.vb")),
                                          New MenuItem(Name:="ConsoleGUI\Screen.cs", Action:=Sub() Me.ViewDocument(Me.DocumentBufferView, Me.DocumentPane, "ConsoleGUI", "Screen.cs"))})

    Dim SearchMenu = New Menu With {.DropDownMenu = True}

    SearchMenu.SetMenuItems(New MenuItem() {New MenuItem(Name:="&Find..."),
                                            New MenuItem(Name:="&Selected Text      Ctrl+\"),
                                            New MenuItem(Name:="&Repeat Last Find       F3"),
                                            New MenuItem(Name:="&Change..."),
                                            New MenuItem(Name:="&Label...")})


    Dim RunMenu = New Menu With {.DropDownMenu = True}

    RunMenu.SetMenuItems(New MenuItem() {New MenuItem(Name:="&Start      Shift+F5", Action:=Sub()
                                                                                              Me.RunProgram()
                                                                                            End Sub),
                                         New MenuItem(Name:="&Restart"),
                                         New MenuItem(Name:="Co&ntinue         F5"),
                                         New MenuItem(Name:="Modify &COMMAND$..."),
                                         New MenuItem(Separator:=True),
                                         New MenuItem(Name:="Make E&XE File...", Action:=Sub()
                                                                                           Me.BuildProgram()
                                                                                         End Sub),
                                         New MenuItem(Name:="Make &Library..."),
                                         New MenuItem(Separator:=True),
                                         New MenuItem(Name:="Set &Main Module...")})


    Dim DebugMenu = New Menu With {.DropDownMenu = True}

    DebugMenu.SetMenuItems(New MenuItem() {New MenuItem(Name:="&Add Watch..."),
                                           New MenuItem(Name:="&Instant Watch...   Shift+F9"),
                                           New MenuItem(Name:="&Watchpoint..."),
                                           New MenuItem(Name:="Delete Watch..."),
                                           New MenuItem(Name:="Delete All Watch"),
                                           New MenuItem(Separator:=True),
                                           New MenuItem(Name:="&Trace On"),
                                           New MenuItem(Name:="&History On"),
                                           New MenuItem(Separator:=True),
                                           New MenuItem(Name:="Toggle &Breakpoint        F9"),
                                           New MenuItem(Name:="&Clear All Breakpoints"),
                                           New MenuItem(Name:="Break on &Errors"),
                                           New MenuItem(Name:="Set Next Statement")})

    Dim CallsMenu = New Menu With {.DropDownMenu = True}

    CallsMenu.SetMenuItems(New MenuItem() {New MenuItem(Name:="&Untitled")})

    Dim OptionsMenu = New Menu With {.DropDownMenu = True}

    OptionsMenu.SetMenuItems(New MenuItem() {New MenuItem(Name:="&Display..."),
                                             New MenuItem(Name:="Set &Paths..."),
                                             New MenuItem(Name:="Right &Mouse..."),
                                             New MenuItem(Name:="&Syntax Checking"),
                                             New MenuItem(Name:="&Full Menus"),
                                             New MenuItem(Separator:=True),
                                             New MenuItem(Name:="Enable &Roslyn", Action:=AddressOf Me.EnableRoslyn)})

    Dim HelpMenu = New Menu With {.DropDownMenu = True}

    'HelpMenu.SetMenuItems(New MenuItem() {New MenuItem(Name:="&Alex Turner", Action:=Sub()
    '                                                                                 End Sub),
    '                                      New MenuItem(Name:="&Ian Halliday", Action:=Sub()
    '                                                                                  End Sub),
    '                                      New MenuItem(Name:="and &Roslyn!", Action:=Sub()
    '                                                                                 End Sub)})

    HelpMenu.SetMenuItems(New MenuItem() {New MenuItem(Name:="&Index", Action:=Sub()
                                                                               End Sub),
                                          New MenuItem(Name:="&Contents", Action:=Sub()
                                                                                  End Sub),
                                          New MenuItem(Name:="&Topic:                 F1", Action:=Sub()
                                                                                                   End Sub),
                                          New MenuItem(Name:="&Help on Help     Shift+F1", Action:=Sub()
                                                                                                   End Sub)})

    Me.MainMenu = New Menu With {.Left = 0,
                                 .Top = 0,
                                 .Width = Me.Width,
                                 .Height = 1}

    Me.SystemMenuItem = New MenuItem(Name:="&≡", DropDownMenu:=SystemMenu, Visible:=False)

    Me.MainMenu.SetMenuItems(New MenuItem() {Me.SystemMenuItem,
                                             New MenuItem(Name:="&File", DropDownMenu:=FileMenu),
                                             New MenuItem(Name:="&Edit", DropDownMenu:=EditMenu),
                                             New MenuItem(Name:="&View", DropDownMenu:=ViewMenu),
                                             New MenuItem(Name:="&Search", DropDownMenu:=SearchMenu),
                                             New MenuItem(Name:="&Run", DropDownMenu:=RunMenu),
                                             New MenuItem(Name:="&Debug", DropDownMenu:=DebugMenu),
                                             New MenuItem(Name:="&Calls", DropDownMenu:=CallsMenu),
                                             New MenuItem(Name:="&Options", DropDownMenu:=OptionsMenu),
                                             New MenuItem(Name:="&Help", AnchorRight:=True, DropDownMenu:=HelpMenu)})

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
    Me.Controls.Add(Me.DocumentPane)
    Me.Controls.Add(Me.MainMenu)
    Me.Controls.Add(Me.StatusTextLabel)
    Me.Controls.Add(Me.StatusPositionLabel)

    Me.ActiveControl = Me.DocumentBufferView
    Me.ActiveBufferView = Me.DocumentBufferView

  End Sub


  Private Sub MyScreen_BeforeRender(sender As Object, e As EventArgs) Handles Me.BeforeRender
    Dim oldPositionText = Me.StatusPositionLabel.Text
    Me.StatusPositionLabel.Text = "│       " + String.Format("{0:D5}:{1:D3}", Me.ActiveBufferView.CursorRow + 1, Me.ActiveBufferView.CursorColumn + 1)
    If Me.StatusPositionLabel.Text <> oldPositionText Then Me.Invalidate()
  End Sub

  Private Sub MyScreen_BeforeKeyDown(sender As Object, e As KeyEvent) Handles Me.BeforeKeyDown
    If (e.ControlKeyState And (ControlKeyState.LEFT_CTRL_PRESSED Or ControlKeyState.RIGHT_CTRL_PRESSED)) <> 0 Then
      Select Case e.VirtualKey
        Case VirtualKey.VK_SPACE
          Me.ActiveDocumentBufferView.MaybeTriggerCompletionList(Nothing)

          e.Handled = True
      End Select
    End If

    If e.Handled Then Exit Sub

    Select Case e.VirtualKey
      Case VirtualKey.VK_F5
        Me.RunProgram()
      Case VirtualKey.VK_F6
        Me.SwitchPane()
      Case VirtualKey.VK_MENU, VirtualKey.VK_F10
        If Not Me.MainMenu.IsActive() And Me.MainMenu IsNot Me.ActiveControl Then
          Me.MainMenu.Activate(Me, Me.ActiveControl)
        End If
    End Select
  End Sub

  Private Sub MyScreen_AfterKeyDown(sender As Object, e As KeyEvent) Handles Me.AfterKeyDown
    If Not Me.ActiveDocumentBufferView.IsCompletionListActive Then
      Dim document = Me.ActiveDocumentBufferView.Buffer.Document
      Dim text = document.GetTextAsync.Result
      If Me.ActiveDocumentBufferView.CursorRow >= text.Lines.Count Then Exit Sub
      Dim position = text.GetPositionFromLineAndColumn(Me.ActiveDocumentBufferView.CursorRow, Me.ActiveDocumentBufferView.CursorColumn - 1)

      If e.Character <> Nothing Then
        Me.ActiveDocumentBufferView.MaybeTriggerCompletionList(e.Character)
      End If
    End If
  End Sub

  Private Sub SwitchPane()
    Me.ActiveBufferView = If(Me.ActiveBufferView Is Me.DocumentBufferView, Me.ErrorBufferView, Me.DocumentBufferView)
    Me.ActiveControl = Me.ActiveBufferView
    Me.DocumentBufferView.ScrollBarsVisible = Me.ActiveBufferView Is Me.DocumentBufferView
    Me.Invalidate()
  End Sub

  Private Sub ViewDocument(bufferView As DocumentTextBufferView, pane As Pane, projectName As String, documentName As String, Optional newText As String = Nothing)
    Try
      bufferView.LoadDocument(projectName, documentName)
    Catch ex As Exception
      Me.SetStatus("NOTE: Please Open Self on the File menu first.", True)
      Exit Sub
    End Try

    If documentName.EndsWith(".vb") OrElse documentName.EndsWith(".bas") Then
      Console.Title = "QuickVB"
      Me.Theme = ControlTheme.Basic
      Me.DocumentBufferView.ForegroundColor = ConsoleColor.Gray
      Me.StatusPositionLabel.Visible = True
      Me.SystemMenuItem.Visible = False
      Me.MainMenu.Layout()
    Else
      Console.Title = "QuickVB - Anders mode"
      Me.Theme = ControlTheme.CSharp
      Me.DocumentBufferView.ForegroundColor = ConsoleColor.Yellow
      Me.StatusPositionLabel.Visible = False
      Me.SystemMenuItem.Visible = True
      Me.MainMenu.Layout()
    End If

    Me.SetStatus(Nothing, False)

    pane.Title = documentName

    If newText IsNot Nothing Then
      bufferView.Buffer.Text = newText
      Me.OnLineCommit(bufferView.Buffer)
    End If
  End Sub

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

  Public Sub ViewDocument(projectName As String, documentName As String, Optional newText As String = Nothing)
    Me.ViewDocument(Me.DocumentBufferView, Me.DocumentPane, projectName, documentName, newText)
  End Sub


  ' Compilation:

  Private _diagnostics As IReadOnlyList(Of Diagnostic)

  Private Sub RunProgram()
    If Not Me.RoslynEnabled Then
      Me.SetStatus("NOTE: Please enable Roslyn on the Options menu first.", True)
      Exit Sub
    End If

    App.TheWorkspace.RunProgram(Me)
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
      Dim diagnosticsString = String.Join(vbNewLine, diagnostics)
      Me.DiagnosticsBuffer.Text = diagnosticsString
      Me.DiagnosticsPane.Title = "Diagnostics"
    Else
      Me.DiagnosticsBuffer.Text = ""
      Me.DiagnosticsPane.Title = "Output"
    End If

    If diagnostics IsNot Nothing Then
      Me._diagnostics = diagnostics.ToList.AsReadOnly
      Me.ActiveDocumentBufferView.Buffer.Colorize(Me._diagnostics, cancel)
    End If

    Screen.GetScreen.Invalidate()
  End Sub


  ' Colorization/Formatting:

  Private _lineDirty As Boolean = False

  Private Sub DocumentBuffer_BufferChanged(sender As Object, e As EventArgs) Handles DocumentBuffer.BufferChanged
    If Not Me.RoslynEnabled Then Exit Sub

    Dim buffer = CType(sender, DocumentTextBuffer)

    Me._lineDirty = True

    Me.OnCharacterChanged(buffer)
  End Sub

  Private Sub DocumentBufferView_RowChanged(sender As Object, e As RowChangedEventArgs) Handles DocumentBufferView.RowChanged
    If Not Me.RoslynEnabled Then Exit Sub

    Dim bufferView = CType(sender, DocumentTextBufferView)

    If Me._lineDirty Then
      Me._lineDirty = False

      Me.OnLineCommit(bufferView.Buffer)
    End If
  End Sub

  Private Sub DocumentBufferView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles DocumentBufferView.SelectionChanged
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
    workspace.BackgroundCompile(
        Sub(diagnostics, cancel)
          Me.UpdateDiagnostics(diagnostics)
        End Sub)
  End Sub

  Private Sub RefreshDocument(buffer As DocumentTextBuffer)
    Me.OnCharacterChanged(buffer)
    Me.OnLineCommit(buffer)
  End Sub

  Private Sub EnableRoslyn()
    If Me.RoslynEnabled Then Exit Sub

    Me.SetStatus(Nothing, False)

    Me.RoslynEnabled = True
    Me.RefreshDocument(Me.ActiveDocumentBufferView.Buffer)
  End Sub
End Class
