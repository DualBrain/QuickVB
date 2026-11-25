' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Namespace ConsoleGUI

  ''' <summary>One kind of text whose colours can be chosen (Normal Text, Current Statement, Breakpoint Lines).</summary>
  Public NotInheritable Class ColorItem

    Public Sub New(name As String, foreground As ConsoleColor, background As ConsoleColor)
      Me.Name = name
      Me.Foreground = foreground
      Me.Background = background
    End Sub

    Public ReadOnly Property Name As String
    Public Property Foreground As ConsoleColor
    Public Property Background As ConsoleColor

    Public Function Clone() As ColorItem
      Return New ColorItem(Me.Name, Me.Foreground, Me.Background)
    End Function

  End Class

  ''' <summary>
  ''' QBasic's Options &gt; Display dialog: a "Colors" box with a radio button and colour swatch for each <see cref="ColorItem"/> and
  ''' Foreground / Background colour lists, and a "Display Options" box with Scroll Bars and Tab Stops. It is 61 x 22 on an 80 x 25 screen and
  ''' grows with the window (more colour rows, wider swatches and lists).
  ''' Tab / Shift+Tab move between the parts; Up / Down change the item, colour or tab stops; Space toggles Scroll Bars; Enter is OK; Esc cancels.
  ''' The changes are made to the <see cref="Items"/>, <see cref="ScrollBars"/> and <see cref="TabStops"/> only when OK is chosen.
  ''' </summary>
  Public Class DisplayDialog
    Inherits Control

    ''' <summary>QBasic's names for the 16 colours, in <see cref="ConsoleColor"/> order.</summary>
    Public Shared ReadOnly ColorNames As String() = {"Black", "Blue", "Green", "Cyan", "Red", "Magenta", "Brown", "White",
                                                     "Gray", "Light Blue", "Light Green", "Light Cyan", "Light Red", "Light Magenta", "Yellow", "High-intensity White"}

    Public Const MinTabStops As Integer = 1
    Public Const MaxTabStops As Integer = 16

    Private Enum Area
      Items
      Foreground
      Background
      ScrollBarsBox
      TabStopsBox
      Ok
      Cancel
      Help
    End Enum

    Private Shared ReadOnly Order As Area() = {Area.Items, Area.Foreground, Area.Background, Area.ScrollBarsBox, Area.TabStopsBox, Area.Ok, Area.Cancel, Area.Help}

    Private Const OkLabel As String = "< OK >"
    Private Const CancelLabel As String = "< Cancel >"
    Private Const HelpLabel As String = "< Help >"

    ' 61 x 22 on 80 x 25: the colour lists have 8 rows, everything else is fixed
    Private Const BaseWidth As Integer = 61
    Private Const BaseListRows As Integer = 8
    Private Const FixedRows As Integer = 14

    Private _screen As Screen
    Private _previousPopup As Control
    Private _onOk As Action
    Private _onCancel As Action

    Private _working As New List(Of ColorItem)()
    Private _selected As Integer
    Private _workScrollBars As Boolean
    Private _tabStopsText As String = "4"
    Private _tabStopsSelected As Boolean
    Private _focus As Area = Area.Items
    Private _fgTop, _bgTop As Integer
    Private _listRows As Integer = BaseListRows

    ' screen positions (for mouse clicks), set while drawing
    Private _radioLeft, _radioFirstRow As Integer
    Private _swatchLeft, _swatchWidth As Integer
    Private _fgLeft, _fgRight, _bgLeft, _bgRight, _listTop As Integer
    Private _scrollLeft, _tabLeft, _optionsRow As Integer
    Private _buttonRow, _okLeft, _cancelLeft, _helpLeft As Integer

    Public Sub New()
      Me.ForegroundColor = ConsoleColor.Black
      Me.BackgroundColor = ConsoleColor.Gray
      Me.Items = New List(Of ColorItem)()
    End Sub

    ''' <summary>The colour items; the changes made in the dialog are written back to these when OK is chosen.</summary>
    Public Property Items As IList(Of ColorItem)
    Public Property ScrollBars As Boolean = True
    Public Property TabStops As Integer = 4

    Public ReadOnly Property IsOpen As Boolean
      Get
        Return Me._screen IsNot Nothing
      End Get
    End Property

    Public Overrides ReadOnly Property StatusHint As String
      Get
        Return Screen.DialogStatusHint
      End Get
    End Property

    ' ---- what the dialog is showing (for tests)

    Public ReadOnly Property SelectedItemIndex As Integer
      Get
        Return Me._selected
      End Get
    End Property

    ''' <summary>The colours as they currently stand in the dialog, before OK.</summary>
    Public ReadOnly Property WorkingItems As IReadOnlyList(Of ColorItem)
      Get
        Return Me._working
      End Get
    End Property

    Public ReadOnly Property WorkingScrollBars As Boolean
      Get
        Return Me._workScrollBars
      End Get
    End Property

    Public ReadOnly Property WorkingTabStopsText As String
      Get
        Return Me._tabStopsText
      End Get
    End Property

    Public ReadOnly Property FocusedPart As String
      Get
        Return Me._focus.ToString()
      End Get
    End Property

    Public ReadOnly Property ListRows As Integer
      Get
        Return Me._listRows
      End Get
    End Property

    Public Sub Show(screen As Screen, Optional onOk As Action = Nothing, Optional onCancel As Action = Nothing)

      Me._screen = screen
      Me._onOk = onOk
      Me._onCancel = onCancel
      Me._working = Me.Items.Select(Function(i) i.Clone()).ToList()
      Me._selected = 0
      Me._workScrollBars = Me.ScrollBars
      Me._tabStopsText = Me.TabStops.ToString()
      Me._tabStopsSelected = True
      Me._focus = Area.Items
      Me._fgTop = 0
      Me._bgTop = 0

      Me._previousPopup = screen.PopupControl
      screen.Controls.Add(Me)
      screen.PopupControl = Me
      screen.HideCursor()
      screen.Invalidate()

    End Sub

    Private Sub Close()
      Dim screen = Me._screen
      Me._screen = Nothing
      screen.Controls.Remove(Me)
      screen.PopupControl = Me._previousPopup
      screen.ShowCursor()
      screen.Invalidate()
    End Sub

    Private Sub Accept()

      For i = 0 To Math.Min(Me.Items.Count, Me._working.Count) - 1
        Me.Items(i).Foreground = Me._working(i).Foreground
        Me.Items(i).Background = Me._working(i).Background
      Next

      Me.ScrollBars = Me._workScrollBars

      Dim tabs As Integer
      If Not Integer.TryParse(Me._tabStopsText, tabs) Then tabs = Me.TabStops
      Me.TabStops = Math.Max(MinTabStops, Math.Min(MaxTabStops, tabs))

      Dim callback = Me._onOk
      Me.Close()
      If callback IsNot Nothing Then callback()

    End Sub

    Private Sub Cancel()
      Dim callback = Me._onCancel
      Me.Close()
      If callback IsNot Nothing Then callback()
    End Sub

    ' ---- behaviour

    Private ReadOnly Property Current As ColorItem
      Get
        Return If(Me._selected >= 0 AndAlso Me._selected < Me._working.Count, Me._working(Me._selected), Nothing)
      End Get
    End Property

    Private Sub MoveFocus(forward As Boolean)
      Dim i = Array.IndexOf(Order, Me._focus)
      Me._focus = Order(((i + If(forward, 1, Order.Length - 1)) Mod Order.Length))
      If Me._focus = Area.TabStopsBox Then Me._tabStopsSelected = True
    End Sub

    ''' <summary>Changes a colour of the current item by <paramref name="delta"/> list rows (or to an absolute index).</summary>
    Private Sub MoveColor(foreground As Boolean, key As VirtualKey)

      Dim item = Me.Current
      If item Is Nothing Then Return

      Dim index = CInt(If(foreground, item.Foreground, item.Background))
      Dim page = Math.Max(1, Me._listRows)

      Select Case key
        Case VirtualKey.VK_UP : index -= 1
        Case VirtualKey.VK_DOWN : index += 1
        Case VirtualKey.VK_PRIOR : index -= page
        Case VirtualKey.VK_NEXT : index += page
        Case VirtualKey.VK_HOME : index = 0
        Case VirtualKey.VK_END : index = 15
      End Select

      Me.SetColor(foreground, index)

    End Sub

    Private Sub SetColor(foreground As Boolean, index As Integer)
      Dim item = Me.Current
      If item Is Nothing Then Return
      Dim value = CType(Math.Max(0, Math.Min(15, index)), ConsoleColor)
      If foreground Then item.Foreground = value Else item.Background = value
    End Sub

    ''' <summary>Scrolls a list so that the item's current colour is visible.</summary>
    Private Sub KeepVisible(selectedIndex As Integer, ByRef top As Integer)
      If selectedIndex < top Then top = selectedIndex
      If selectedIndex >= top + Me._listRows Then top = selectedIndex - Me._listRows + 1
      top = Math.Max(0, Math.Min(top, Math.Max(0, 16 - Me._listRows)))
    End Sub

    Private Shared Function IsPrintable(e As KeyEvent) As Boolean
      Dim modified = (e.ControlKeyState And (ControlKeyState.LEFT_CTRL_PRESSED Or ControlKeyState.RIGHT_CTRL_PRESSED Or ControlKeyState.LEFT_ALT_PRESSED Or ControlKeyState.RIGHT_ALT_PRESSED)) <> 0
      Return Not modified AndAlso e.Character >= " "c AndAlso e.Character <> ChrW(127)
    End Function

    Private Sub Press(part As Area)
      Select Case part
        Case Area.Ok : Me.Accept()
        Case Area.Cancel : Me.Cancel()
        Case Area.Help : Me._screen.Invalidate()
      End Select
    End Sub

    Public Overrides Sub OnBeforeKeyDown(sender As Object, e As KeyEvent)

      ' Modal: nothing below the dialog sees keys while it is open.
      e.Handled = True

      If Me._screen Is Nothing Then Return

      Dim key = e.VirtualKey

      If key = VirtualKey.VK_ESCAPE Then
        Me.Cancel()
        Return
      End If

      If key = VirtualKey.VK_TAB Then

        Me.MoveFocus((e.ControlKeyState And ControlKeyState.SHIFT_PRESSED) = 0)

      ElseIf key = VirtualKey.VK_RETURN Then

        If Me._focus = Area.Cancel OrElse Me._focus = Area.Help Then Me.Press(Me._focus) Else Me.Accept()

      ElseIf Me._focus >= Area.Ok Then

        If key = VirtualKey.VK_SPACE Then
          Me.Press(Me._focus)
        ElseIf key = VirtualKey.VK_LEFT OrElse key = VirtualKey.VK_UP Then
          If Me._focus > Area.Ok Then Me._focus = CType(CInt(Me._focus) - 1, Area)
        ElseIf key = VirtualKey.VK_RIGHT OrElse key = VirtualKey.VK_DOWN Then
          If Me._focus < Area.Help Then Me._focus = CType(CInt(Me._focus) + 1, Area)
        ElseIf IsPrintable(e) Then
          Select Case Char.ToUpperInvariant(e.Character)
            Case "O"c : Me.Press(Area.Ok)
            Case "C"c : Me.Press(Area.Cancel)
            Case "H"c : Me.Press(Area.Help)
          End Select
        End If

      ElseIf Me._focus = Area.Items Then

        If key = VirtualKey.VK_UP OrElse key = VirtualKey.VK_LEFT Then
          Me._selected = Math.Max(0, Me._selected - 1)
        ElseIf key = VirtualKey.VK_DOWN OrElse key = VirtualKey.VK_RIGHT Then
          Me._selected = Math.Min(Me._working.Count - 1, Me._selected + 1)
        ElseIf key = VirtualKey.VK_HOME Then
          Me._selected = 0
        ElseIf key = VirtualKey.VK_END Then
          Me._selected = Me._working.Count - 1
        ElseIf IsPrintable(e) AndAlso Char.IsDigit(e.Character) Then
          ' the items are numbered 1, 2, 3
          Dim number = AscW(e.Character) - AscW("1"c)
          If number >= 0 AndAlso number < Me._working.Count Then Me._selected = number
        End If

      ElseIf Me._focus = Area.Foreground OrElse Me._focus = Area.Background Then

        Me.MoveColor(Me._focus = Area.Foreground, key)

      ElseIf Me._focus = Area.ScrollBarsBox Then

        If key = VirtualKey.VK_SPACE Then Me._workScrollBars = Not Me._workScrollBars

      ElseIf Me._focus = Area.TabStopsBox Then

        If key = VirtualKey.VK_BACK Then
          If Me._tabStopsSelected Then
            Me._tabStopsText = ""
            Me._tabStopsSelected = False
          ElseIf Me._tabStopsText.Length > 0 Then
            Me._tabStopsText = Me._tabStopsText.Substring(0, Me._tabStopsText.Length - 1)
          End If
        ElseIf key = VirtualKey.VK_UP OrElse key = VirtualKey.VK_DOWN Then
          Dim value As Integer
          If Not Integer.TryParse(Me._tabStopsText, value) Then value = Me.TabStops
          value = Math.Max(MinTabStops, Math.Min(MaxTabStops, value + If(key = VirtualKey.VK_UP, 1, -1)))
          Me._tabStopsText = value.ToString()
          Me._tabStopsSelected = True
        ElseIf IsPrintable(e) AndAlso Char.IsDigit(e.Character) Then
          If Me._tabStopsSelected Then
            Me._tabStopsText = ""
            Me._tabStopsSelected = False
          End If
          If Me._tabStopsText.Length < 2 Then Me._tabStopsText &= e.Character
        End If

      End If

      If Me._screen IsNot Nothing Then Me._screen.Invalidate()

    End Sub

    Public Overrides Sub OnKeyUp(sender As Object, e As KeyEvent)
      e.Handled = True
    End Sub

    Public Overrides Sub OnMouseDown(sender As Object, e As MouseEvent)

      If Me._screen Is Nothing Then Return

      Dim x = CInt(e.X)
      Dim y = CInt(e.Y)

      If y = Me._buttonRow Then

        If x >= Me._okLeft AndAlso x < Me._okLeft + OkLabel.Length Then
          Me._focus = Area.Ok
          Me.Press(Area.Ok)
        ElseIf x >= Me._cancelLeft AndAlso x < Me._cancelLeft + CancelLabel.Length Then
          Me._focus = Area.Cancel
          Me.Press(Area.Cancel)
        ElseIf x >= Me._helpLeft AndAlso x < Me._helpLeft + HelpLabel.Length Then
          Me._focus = Area.Help
          Me.Press(Area.Help)
        End If

      ElseIf y = Me._optionsRow Then

        If x >= Me._scrollLeft AndAlso x < Me._scrollLeft + 15 Then
          Me._focus = Area.ScrollBarsBox
          Me._workScrollBars = Not Me._workScrollBars
        ElseIf x >= Me._tabLeft Then
          Me._focus = Area.TabStopsBox
          Me._tabStopsSelected = True
        End If

      ElseIf y >= Me._listTop AndAlso y < Me._listTop + Me._listRows Then

        Dim row = y - Me._listTop

        If x >= Me._radioLeft AndAlso x < Me._fgLeft Then
          ' radio rows are every other row: 0, 2, 4
          If row Mod 2 = 0 AndAlso row \ 2 < Me._working.Count Then
            Me._focus = Area.Items
            Me._selected = row \ 2
          End If
        ElseIf x > Me._fgLeft AndAlso x < Me._fgRight Then
          Me._focus = Area.Foreground
          Me.SetColor(True, Me._fgTop + row)
        ElseIf x > Me._bgLeft AndAlso x < Me._bgRight Then
          Me._focus = Area.Background
          Me.SetColor(False, Me._bgTop + row)
        End If

      End If

      If Me._screen IsNot Nothing Then Me._screen.Invalidate()

    End Sub

    ' ---- drawing

    Private Shared Function Fit(text As String, width As Integer) As String
      If width <= 0 Then Return ""
      Return If(text.Length > width, text.Substring(0, width), text.PadRight(width))
    End Function

    Private Sub DrawGroupTitle(screen As Screen, left As Integer, right As Integer, row As Integer, title As String)
      Dim text = " " & title & " "
      screen.DrawString(left + (right - left + 1 - text.Length) \ 2, row, text, Me.ForegroundColor, Me.BackgroundColor)
    End Sub

    Private Sub DrawColorList(screen As Screen, left As Integer, right As Integer, selected As Integer, ByRef top As Integer, focused As Boolean)

      Dim fg = Me.ForegroundColor
      Dim bg = Me.BackgroundColor
      Dim rows = Me._listRows

      Me.KeepVisible(selected, top)

      screen.DrawBox(left, Me._listTop - 1, right, Me._listTop + rows, fg, bg)

      For r = 0 To rows - 1
        Dim index = top + r
        Dim y = Me._listTop + r
        Dim isSelected = index = selected
        ' a tall window has more rows than there are colours: the extra rows stay empty
        Dim name = If(index < ColorNames.Length, " " & ColorNames(index), "")
        screen.DrawString(left + 1, y, Fit(name, right - left - 1), If(isSelected, If(focused, ConsoleColor.White, ConsoleColor.Gray), fg), If(isSelected, ConsoleColor.Black, bg))
      Next

      ' the vertical scroll bar is the right border
      screen.DrawUpArrow(right, Me._listTop, fg, bg)
      screen.DrawDownArrow(right, Me._listTop + rows - 1, fg, bg)
      For y = Me._listTop + 1 To Me._listTop + rows - 2
        screen.DrawLightShade(right, y, fg, bg)
      Next
      If rows >= 3 Then
        screen.DrawBlock(right, Utility.GetRangeValue(Me._listTop + 1, Me._listTop + rows - 2, selected / 15.0), fg, bg)
      End If

    End Sub

    Public Overrides Sub Render(screen As Screen)

      If Not Me.Visible Then Return

      Dim extraRows = Math.Max(0, screen.Height - 25)
      Dim extraColumns = Math.Max(0, screen.Width - 80)

      Me._listRows = BaseListRows + extraRows
      Me.Width = BaseWidth + extraColumns
      Me.Height = Me._listRows + FixedRows
      Me.Left = (screen.Width - Me.Width) \ 2
      Me.Top = (screen.Height - Me.Height) \ 2

      ' Too big for the window (the keyboard still works): draw nothing.
      If Me.Left < 0 OrElse Me.Top < 0 OrElse Me.Right + 2 >= screen.Width OrElse Me.Bottom + 1 >= screen.Height Then Return

      Dim fg = Me.ForegroundColor
      Dim bg = Me.BackgroundColor
      Dim left = Me.Left
      Dim right = Me.Right
      Dim top = Me.Top

      screen.DrawShadow(left + 2, top + 1, right + 2, Me.Bottom + 1)
      screen.DrawBoxFill(left, top, right, Me.Bottom, fg, bg)
      screen.DrawBox(left, top, right, Me.Bottom, fg, bg)
      Me.DrawGroupTitle(screen, left, right, top, "Display")

      ' ---- Colors
      Dim groupLeft = left + 2
      Dim groupRight = right - 3
      Dim colorsBottom = top + 1 + Me._listRows + 5
      screen.DrawBox(groupLeft, top + 1, groupRight, colorsBottom, fg, bg)
      Me.DrawGroupTitle(screen, groupLeft, groupRight, top + 1, "Colors")

      Dim listWidth = 11 + extraColumns \ 3
      Me._swatchWidth = 20 + extraColumns \ 3
      Me._radioLeft = groupLeft + 2
      Me._swatchLeft = groupLeft + 9
      Me._fgLeft = Me._swatchLeft + Me._swatchWidth
      Me._fgRight = Me._fgLeft + listWidth - 1
      Me._bgRight = groupRight - 3
      Me._bgLeft = Me._bgRight - listWidth + 1
      Me._listTop = top + 4

      screen.DrawString(Me._fgLeft + (listWidth - 10) \ 2, top + 2, "Foreground", fg, bg)
      screen.DrawString(Me._bgLeft + (listWidth - 10) \ 2, top + 2, "Background", fg, bg)

      ' radio buttons with their colour swatches, every other row
      For i = 0 To Me._working.Count - 1
        Dim item = Me._working(i)
        Dim row = Me._listTop + i * 2
        If row >= Me._listTop + Me._listRows Then Exit For
        screen.DrawString(Me._radioLeft, row, "( ) " & (i + 1) & ".", fg, bg)
        If i = Me._selected Then
          Dim focused = Me._focus = Area.Items
          screen.DrawString(Me._radioLeft + 1, row, ChrW(&H2022).ToString(), If(focused, ConsoleColor.White, fg), If(focused, ConsoleColor.Black, bg))
        End If
        screen.DrawString(Me._swatchLeft, row, Fit(" " & item.Name, Me._swatchWidth), item.Foreground, item.Background)
      Next

      Dim current = Me.Current
      Dim fgIndex = If(current Is Nothing, 0, CInt(current.Foreground))
      Dim bgIndex = If(current Is Nothing, 0, CInt(current.Background))
      Me.DrawColorList(screen, Me._fgLeft, Me._fgRight, fgIndex, Me._fgTop, Me._focus = Area.Foreground)
      Me.DrawColorList(screen, Me._bgLeft, Me._bgRight, bgIndex, Me._bgTop, Me._focus = Area.Background)

      ' ---- Display Options
      Dim optionsTop = colorsBottom + 2
      screen.DrawBox(groupLeft, optionsTop, groupRight, optionsTop + 2, fg, bg)
      Me.DrawGroupTitle(screen, groupLeft, groupRight, optionsTop, "Display Options")
      Me._optionsRow = optionsTop + 1

      Me._scrollLeft = groupLeft + 4
      screen.DrawString(Me._scrollLeft, Me._optionsRow, "[" & If(Me._workScrollBars, "X", " ") & "] Scroll Bars", fg, bg)
      If Me._focus = Area.ScrollBarsBox Then
        screen.DrawString(Me._scrollLeft + 1, Me._optionsRow, If(Me._workScrollBars, "X", " "), ConsoleColor.White, ConsoleColor.Black)
      End If

      Me._tabLeft = left + CInt(Me.Width * 0.64)
      screen.DrawString(Me._tabLeft, Me._optionsRow, "Tab Stops: ", fg, bg)
      Dim valueLeft = Me._tabLeft + "Tab Stops: ".Length
      If Me._focus = Area.TabStopsBox Then
        If Me._tabStopsSelected AndAlso Me._tabStopsText.Length > 0 Then
          screen.DrawString(valueLeft, Me._optionsRow, Me._tabStopsText, ConsoleColor.White, ConsoleColor.Black)
          screen.DrawString(valueLeft + Me._tabStopsText.Length, Me._optionsRow, "_", fg, bg)
        Else
          screen.DrawString(valueLeft, Me._optionsRow, Me._tabStopsText & "_", fg, bg)
        End If
      Else
        screen.DrawString(valueLeft, Me._optionsRow, Me._tabStopsText, fg, bg)
      End If

      ' ---- separator and buttons
      Dim separatorRow = Me.Bottom - 2
      screen.DrawLeftTee(left, separatorRow, fg, bg)
      screen.DrawHorizontalLine(left + 1, right - 1, separatorRow, fg, bg)
      screen.DrawRightTee(right, separatorRow, fg, bg)

      Me._buttonRow = Me.Bottom - 1
      Dim centre = left + Me.Width \ 2
      Dim spread = (Me.Width - 10) \ 3 + 1
      Me._cancelLeft = centre - CancelLabel.Length \ 2
      Me._okLeft = centre - spread - OkLabel.Length \ 2
      Me._helpLeft = centre + spread - HelpLabel.Length \ 2
      Me.DrawButton(screen, Me._okLeft, OkLabel, Area.Ok)
      Me.DrawButton(screen, Me._cancelLeft, CancelLabel, Area.Cancel)
      Me.DrawButton(screen, Me._helpLeft, HelpLabel, Area.Help)

    End Sub

    Private Sub DrawButton(screen As Screen, left As Integer, text As String, part As Area)
      screen.DrawString(left, Me._buttonRow, text, Me.ForegroundColor, Me.BackgroundColor)
      ' the focused button, or OK while something else has the focus, has bright brackets
      Dim bright = If(Me._focus >= Area.Ok, Me._focus = part, part = Area.Ok)
      If bright Then
        screen.DrawString(left, Me._buttonRow, "<", ConsoleColor.White, Me.BackgroundColor)
        screen.DrawString(left + text.Length - 1, Me._buttonRow, ">", ConsoleColor.White, Me.BackgroundColor)
      End If
    End Sub

  End Class

End Namespace
