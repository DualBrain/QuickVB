Option Explicit On
Option Strict On
Option Infer On

Imports ConsoleGUI

' View > Split: a second editor window under the first, so that two files (or two places of one file) can be looked at together.
' The two windows are two views with a buffer each; when both show the same file an edit in one is copied to the other.
Partial Public Class MyScreen

  Private _mainBuffer As DocumentTextBuffer
  Private _mainView As DocumentTextBufferView
  Private _mainPane As Pane

  Private _splitBuffer As DocumentTextBuffer
  Private _splitView As DocumentTextBufferView
  Private _splitPane As Pane

  Private _splitVisible As Boolean

  ' how many rows the upper window has gained (or lost) with Alt+Plus / Alt+Minus
  Private _splitShift As Integer

  Private _activeMain As ProgramFile
  Private _activeSplit As ProgramFile

  Private _syncingViews As Boolean

  ' what the editing code works on: the window that has (or last had) the focus
  Private ReadOnly Property DocumentBufferView As DocumentTextBufferView
    Get
      Return If(Me._activeDocumentBufferView, Me._mainView)
    End Get
  End Property

  Private ReadOnly Property DocumentBuffer As DocumentTextBuffer
    Get
      Return Me.DocumentBufferView.Buffer
    End Get
  End Property

  Private ReadOnly Property DocumentPane As Pane
    Get
      Return If(Me.IsSplitFocused(), Me._splitPane, Me._mainPane)
    End Get
  End Property

  Private Function IsSplitFocused() As Boolean
    Return Me._splitVisible AndAlso Object.ReferenceEquals(Me.DocumentBufferView, Me._splitView)
  End Function

  ' the file shown in the focused window
  Private Property _active As ProgramFile
    Get
      Return If(Me.IsSplitFocused(), Me._activeSplit, Me._activeMain)
    End Get
    Set(value As ProgramFile)
      If Me.IsSplitFocused() Then
        Me._activeSplit = value
      Else
        Me._activeMain = value
      End If
    End Set
  End Property

  ''' <summary>The editor windows that are showing, each with the file it shows.</summary>
  Private Function EditorSlots() As List(Of Tuple(Of DocumentTextBufferView, ProgramFile))
    Dim slots As New List(Of Tuple(Of DocumentTextBufferView, ProgramFile))()
    slots.Add(Tuple.Create(Me._mainView, Me._activeMain))
    If Me._splitVisible Then slots.Add(Tuple.Create(Me._splitView, Me._activeSplit))
    Return slots
  End Function

  Friend ReadOnly Property IsSplit As Boolean
    Get
      Return Me._splitVisible
    End Get
  End Property

  ''' <summary>The file shown in the lower window (Nothing when the window is not split).</summary>
  Friend ReadOnly Property SplitFileName As String
    Get
      Return If(Me._splitVisible AndAlso Me._activeSplit IsNot Nothing, Me._activeSplit.Name, Nothing)
    End Get
  End Property

  ''' <summary>The upper window, whichever has the focus (for tests).</summary>
  Friend ReadOnly Property UpperView As DocumentTextBufferView
    Get
      Return Me._mainView
    End Get
  End Property

  Friend ReadOnly Property SplitView As DocumentTextBufferView
    Get
      Return Me._splitView
    End Get
  End Property

  Friend ReadOnly Property SplitEditor As DocumentTextBuffer
    Get
      Return Me._splitBuffer
    End Get
  End Property

  ' ---- creating the two windows

  Private Sub CreateEditorWindows()

    Me._mainBuffer = New DocumentTextBuffer
    Me._mainView = New DocumentTextBufferView With {.Buffer = Me._mainBuffer, .BackgroundColor = ConsoleColor.DarkBlue, .ForegroundColor = ConsoleColor.Gray}
    Me._mainPane = New Pane With {.Left = 0, .Top = 1, .Width = 80, .Height = 21, .PaddingBottom = 2,
                                  .BackgroundColor = ConsoleColor.DarkBlue, .ForegroundColor = ConsoleColor.Gray, .Title = "Loading..."}
    Me._mainPane.Controls.Add(Me._mainView)

    Me._splitBuffer = New DocumentTextBuffer
    Me._splitView = New DocumentTextBufferView With {.Buffer = Me._splitBuffer, .BackgroundColor = ConsoleColor.DarkBlue, .ForegroundColor = ConsoleColor.Gray}
    Me._splitPane = New Pane With {.Left = 0, .Top = 12, .Width = 80, .Height = 10, .PaddingBottom = 2,
                                   .BackgroundColor = ConsoleColor.DarkBlue, .ForegroundColor = ConsoleColor.Gray, .Title = ""}
    Me._splitPane.Controls.Add(Me._splitView)

    For Each buffer In {Me._mainBuffer, Me._splitBuffer}
      AddHandler buffer.BufferChanged, AddressOf Me.DocumentBuffer_BufferChanged
    Next

    For Each view In {Me._mainView, Me._splitView}
      AddHandler view.RowChanged, AddressOf Me.DocumentBufferView_RowChanged
      AddHandler view.SelectionChanged, AddressOf Me.DocumentBufferView_SelectionChanged
    Next

  End Sub

  ' ---- View > Split

  ''' <summary>View &gt; Split: a second window showing the same file; choose another file in it with View &gt; Switch to. Again, it closes.</summary>
  Public Sub ToggleSplit()

    If Me._splitVisible Then
      Me.CloseSplit()
    Else
      Me.OpenSplit()
    End If

  End Sub

  Private Sub OpenSplit()

    If Me._activeMain Is Nothing Then Return

    Dim file = Me._activeMain

    Me.PushEditorTextToWorkspace()

    Me._splitVisible = True
    Me.Controls.Insert(Me.Controls.IndexOf(Me._mainPane) + 1, Me._splitPane)

    Try
      Me._splitView.LoadDocument(Me.ProgramName(), file.Name)
    Catch ex As Exception
      Me._splitVisible = False
      Me.Controls.Remove(Me._splitPane)
      Me.SetStatus("Unable to split the window: " & ex.Message, True)
      Return
    End Try

    Me._activeSplit = file
    Me._splitPane.Title = Me.TitleOf(file)
    Me._splitView.ClearUndo()
    Me._splitView.MoveCursor(Me._mainView.CursorRow, Me._mainView.CursorColumn)
    Me._splitView.IsReadOnly = Me._mainView.IsReadOnly

    Me.ApplyViewColors()
    Me.ApplyMarks()
    If Me.RoslynEnabled Then Me._splitBuffer.Colorize(Me._diagnostics)

    Me._splitShift = 0
    Me.LayoutChrome()
    Me.UpdateScrollBars()
    Me.Invalidate()

  End Sub

  Private Sub CloseSplit()

    If Not Me._splitVisible Then Return

    ' the window that has the focus is the one that stays, in the upper position
    If Me.IsSplitFocused() AndAlso Me._activeSplit IsNot Nothing AndAlso Not Object.ReferenceEquals(Me._activeSplit, Me._activeMain) Then
      Dim keep = Me._activeSplit
      Dim row = Me._splitView.CursorRow
      Dim column = Me._splitView.CursorColumn
      Me.InView(Me._mainView, Sub() Me.ActivateFile(keep))
      Me._mainView.MoveCursor(row, column)
    End If

    Me.SyncBreakpointsFromEditor()
    Me.PushEditorTextToWorkspace()

    Dim documentId = Me._splitBuffer.DocumentId
    If documentId IsNot Nothing AndAlso App.TheWorkspace IsNot Nothing Then App.TheWorkspace.UnregisterTextBuffer(documentId, Me._splitBuffer)

    Me.Controls.Remove(Me._splitPane)
    Me._splitVisible = False
    Me._activeSplit = Nothing

    Me.ActiveBufferView = Me._mainView
    Me.ActiveControl = Me._mainView

    Me.LayoutChrome()
    Me.UpdateScrollBars()
    Me.Invalidate()

  End Sub

  ''' <summary>Runs <paramref name="action"/> as if <paramref name="view"/> were the focused window (the focus itself stays where it is).</summary>
  Private Sub InView(view As DocumentTextBufferView, action As Action)

    Dim previous = Me._activeDocumentBufferView
    Me._activeDocumentBufferView = view

    Try
      action()
    Finally
      Me._activeDocumentBufferView = previous
    End Try

  End Sub

  ' ---- keeping the windows in step

  ''' <summary>When both windows show the same file, what is typed in one appears in the other.</summary>
  Private Sub SyncOtherView(changed As DocumentTextBuffer)

    If Not Me._splitVisible OrElse Me._syncingViews Then Return
    If Me._activeMain Is Nothing OrElse Not Object.ReferenceEquals(Me._activeMain, Me._activeSplit) Then Return

    Dim other = If(Object.ReferenceEquals(changed, Me._mainBuffer), Me._splitBuffer, Me._mainBuffer)
    Dim text = changed.Text

    If String.Equals(other.Text, text, StringComparison.Ordinal) Then Return

    Me._syncingViews = True
    Try
      other.Text = text
    Finally
      Me._syncingViews = False
    End Try

    If Me.RoslynEnabled Then other.Colorize(Me._diagnostics)

  End Sub

  Private Sub UpdateScrollBars()
    Me._mainView.ScrollBarsVisible = Object.ReferenceEquals(Me.ActiveControl, Me._mainView)
    Me._splitView.ScrollBarsVisible = Object.ReferenceEquals(Me.ActiveControl, Me._splitView)
  End Sub

  ''' <summary>Gives the focus to a window that was clicked.</summary>
  Private Sub FocusClickedWindow(x As Integer, y As Integer)

    Dim hit = TryCast(Me.HitTest(x, y), TextBufferView)

    If hit Is Nothing OrElse Object.ReferenceEquals(hit, Me.ActiveControl) Then Return
    If Not (Object.ReferenceEquals(hit, Me._mainView) OrElse Object.ReferenceEquals(hit, Me._splitView) OrElse Object.ReferenceEquals(hit, Me.ErrorBufferView)) Then Return

    ' the file that was being edited keeps its place
    If TypeOf Me.ActiveControl Is DocumentTextBufferView AndAlso Me._active IsNot Nothing Then
      Me._active.Row = Me.DocumentBufferView.CursorRow
      Me._active.Column = Me.DocumentBufferView.CursorColumn
    End If

    Me.ReleaseHelpFocus()
    Me.ActiveBufferView = hit
    Me.ActiveControl = hit
    Me.UpdateScrollBars()
    Me.Invalidate()

  End Sub

  ''' <summary>The colours of the display settings, for both editor windows.</summary>
  Private Sub ApplyViewColors()
    For Each view In {Me._mainView, Me._splitView}
      view.ForegroundColor = Me._normalColors.Foreground
      view.BackgroundColor = Me._normalColors.Background
      view.TabStops = Me._tabStops
      view.BreakpointForeground = Me._breakpointColors.Foreground
      view.BreakpointBackground = Me._breakpointColors.Background
      view.StatementForeground = Me._statementColors.Foreground
      view.StatementBackground = Me._statementColors.Background
      view.ScrollBars = Me._showScrollBars
    Next
    Me._splitPane.ForegroundColor = Me._normalColors.Foreground
    Me._splitPane.BackgroundColor = Me._normalColors.Background
    Me._splitBuffer.EditorBackground = Me._normalColors.Background
  End Sub

End Class
