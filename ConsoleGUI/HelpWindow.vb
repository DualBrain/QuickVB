' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Imports System.Text.RegularExpressions

Namespace ConsoleGUI

  ''' <summary>
  ''' QuickBASIC's help window: a black pane titled "HELP: topic" with a link bar (◄Editing Keys► ◄Contents► ◄Index►), a rule, the text of the topic with
  ''' hyperlinks, and scroll bars. Tab / Shift+Tab (or a letter) move between hyperlinks, Enter follows one, Alt+F1 goes back, Ctrl+F1 / Shift+Ctrl+F1 go to the
  ''' next / previous topic, the arrow and page keys scroll, Ctrl+F10 maximizes and Esc closes. The owner positions it (Left, Top, Width, Height) and
  ''' supplies the topics with <see cref="Topics"/>; <see cref="ContentRows"/> says how tall the text is, so a short topic can have a short window.
  ''' </summary>
  Public Class HelpWindow
    Inherits Control

    Public Const LeftArrow As Char = ChrW(&H25C4)     ' ◄
    Public Const RightArrow As Char = ChrW(&H25BA)    ' ►

    Public Const StatusText As String = "<Shift+F1=Help> <F6=Window> <Esc=Cancel> <Ctrl+F1=Next> <Alt+F1=Back>"

    Private Enum LineKind
      Text
      Rule
      TitledRule
    End Enum

    Private NotInheritable Class Span
      Public Start As Integer
      Public Length As Integer
      Public IsLink As Boolean      ' otherwise emphasis
      Public LinkIndex As Integer
    End Class

    Private NotInheritable Class ParsedLine
      Public Kind As LineKind
      Public Text As String = ""
      Public Spans As New List(Of Span)()
    End Class

    Private NotInheritable Class LinkRef
      Public Text As String
      Public Target As String
      Public Line As Integer         ' -1: the link bar
      Public Start As Integer
      ' where it was last drawn (for mouse clicks)
      Public ScreenX, ScreenY, ScreenWidth As Integer
    End Class

    Private Shared ReadOnly MarkupPattern As New Regex("\[\[(?<text>[^|\]]+)\|(?<target>[^\]]+)\]\]|\{\{(?<emphasis>[^}]+)\}\}", RegexOptions.Compiled)

    Private _topic As HelpTopic
    Private _lines As New List(Of ParsedLine)()
    Private _links As New List(Of LinkRef)()
    Private _focusLink As Integer = -1
    Private _topLine As Integer
    Private _leftColumn As Integer
    Private ReadOnly _back As New Stack(Of String)()

    Public Sub New()
      Me.ForegroundColor = ConsoleColor.Gray
      Me.BackgroundColor = ConsoleColor.Black
      Me.Visible = False
      Me.TopicOrder = New List(Of String)()
    End Sub

    ''' <summary>Finds a topic by its id (Nothing when there is none).</summary>
    Public Property Topics As Func(Of String, HelpTopic)

    ''' <summary>The ids of the topics in the order Ctrl+F1 steps through them.</summary>
    Public Property TopicOrder As IReadOnlyList(Of String)

    ''' <summary>Called with the address of a hyperlink that is one (<c>[[text|https://...]]</c>): the topic stays as it is.</summary>
    Public Property OnOpenUrl As Action(Of String)

    ''' <summary>Called when the user presses Esc.</summary>
    Public Property OnClose As Action

    ''' <summary>Called when the window's height should be worked out again: another topic is showing, or it was maximized or restored.</summary>
    Public Property OnLayoutChanged As Action

    Private _maximized As Boolean

    Public Property Maximized As Boolean
      Get
        Return Me._maximized
      End Get
      Set(value As Boolean)
        If value <> Me._maximized Then
          Me._maximized = value
          Me.RaiseLayoutChanged()
        End If
      End Set
    End Property

    Private Sub RaiseLayoutChanged()
      If Me.OnLayoutChanged IsNot Nothing Then Me.OnLayoutChanged.Invoke()
    End Sub

    Public ReadOnly Property CurrentTopic As HelpTopic
      Get
        Return Me._topic
      End Get
    End Property

    ''' <summary>The number of lines of text in the topic.</summary>
    Public ReadOnly Property ContentRows As Integer
      Get
        Return Me._lines.Count
      End Get
    End Property

    ''' <summary>How many rows of text fit in the window at its current height.</summary>
    Public ReadOnly Property VisibleRows As Integer
      Get
        Return Math.Max(1, Me.Height - 4)
      End Get
    End Property

    Public ReadOnly Property TopLine As Integer
      Get
        Return Me._topLine
      End Get
    End Property

    ''' <summary>The text of the hyperlink that has the focus (Nothing: none).</summary>
    Public ReadOnly Property FocusedLinkText As String
      Get
        Return If(Me._focusLink >= 0 AndAlso Me._focusLink < Me._links.Count, Me._links(Me._focusLink).Text, Nothing)
      End Get
    End Property

    Public ReadOnly Property BackDepth As Integer
      Get
        Return Me._back.Count
      End Get
    End Property

    Public Overrides ReadOnly Property StatusHint As String
      Get
        Return StatusText
      End Get
    End Property

    ' ---- topics

    ''' <summary>Shows a topic and forgets where we came from.</summary>
    Public Function Open(id As String) As Boolean
      Me._back.Clear()
      Return Me.Show(id)
    End Function

    ''' <summary>Follows a hyperlink: shows the topic and remembers the current one for <see cref="GoBack"/> (20 deep).</summary>
    Public Function Follow(id As String) As Boolean
      ' an address leaves the help window: it is for the browser
      If id.StartsWith("https://", StringComparison.OrdinalIgnoreCase) Then
        If Me.OnOpenUrl IsNot Nothing Then Me.OnOpenUrl.Invoke(id)
        Return True
      End If
      Dim current = If(Me._topic Is Nothing, Nothing, Me._topic.Id)
      If Me.Show(id) Then
        If current IsNot Nothing AndAlso current <> id Then
          Me._back.Push(current)
          If Me._back.Count > 20 Then
            Dim items = Me._back.Reverse().Skip(1).ToList()     ' oldest first, drop the oldest
            Me._back.Clear()
            For Each item In items
              Me._back.Push(item)
            Next
          End If
        End If
        Return True
      End If
      Return False
    End Function

    Public Function GoBack() As Boolean
      If Me._back.Count = 0 Then Return False
      Return Me.Show(Me._back.Pop())
    End Function

    Public Function GoNext(forward As Boolean) As Boolean
      Dim order = Me.TopicOrder
      If Me._topic Is Nothing OrElse order Is Nothing OrElse order.Count = 0 Then Return False
      Dim index = order.ToList().IndexOf(Me._topic.Id)
      Dim target = order(((index + If(forward, 1, -1)) + order.Count) Mod order.Count)
      Return Me.Follow(target)
    End Function

    Private Function Show(id As String) As Boolean

      If Me.Topics Is Nothing Then Return False
      Dim topic = Me.Topics.Invoke(id)
      If topic Is Nothing Then Return False

      Me._topic = topic
      Me._topLine = 0
      Me._leftColumn = 0
      Me.Parse()
      Me._focusLink = If(Me._links.Count > 0, 0, -1)

      Me.RaiseLayoutChanged()

      Return True

    End Function

    Private Sub Parse()

      Me._lines.Clear()
      Me._links.Clear()

      ' the link bar: every entry but the topic itself is a hyperlink
      For Each item In Me._topic.Nav
        If Not item.IsCurrent Then
          Me._links.Add(New LinkRef With {.Text = item.Text, .Target = item.Target, .Line = -1})
        End If
      Next

      For lineIndex = 0 To Me._topic.Lines.Count - 1

        Dim raw = Me._topic.Lines(lineIndex)
        Dim parsed As New ParsedLine()

        If raw = "<hr>" Then
          parsed.Kind = LineKind.Rule
        ElseIf raw.StartsWith("<hr:", StringComparison.Ordinal) AndAlso raw.EndsWith(">", StringComparison.Ordinal) Then
          parsed.Kind = LineKind.TitledRule
          parsed.Text = raw.Substring(4, raw.Length - 5)
        Else

          Dim text As New System.Text.StringBuilder()
          Dim position = 0

          For Each m As Match In MarkupPattern.Matches(raw)

            text.Append(raw, position, m.Index - position)
            position = m.Index + m.Length

            If m.Groups("text").Success Then
              Dim label = LeftArrow & m.Groups("text").Value & RightArrow
              Dim link As New LinkRef With {.Text = m.Groups("text").Value, .Target = m.Groups("target").Value, .Line = Me._lines.Count, .Start = text.Length}
              Me._links.Add(link)
              parsed.Spans.Add(New Span With {.Start = text.Length, .Length = label.Length, .IsLink = True, .LinkIndex = Me._links.Count - 1})
              text.Append(label)
            Else
              Dim emphasis = m.Groups("emphasis").Value
              parsed.Spans.Add(New Span With {.Start = text.Length, .Length = emphasis.Length, .IsLink = False})
              text.Append(emphasis)
            End If

          Next

          text.Append(raw, position, raw.Length - position)
          parsed.Text = text.ToString()

        End If

        Me._lines.Add(parsed)

      Next

    End Sub

    ' ---- scrolling and focus

    Private Sub ClampScroll()
      Me._topLine = Math.Max(0, Math.Min(Me._topLine, Math.Max(0, Me._lines.Count - Me.VisibleRows)))
      Dim widest = If(Me._lines.Count = 0, 0, Me._lines.Max(Function(l) l.Text.Length))
      Me._leftColumn = Math.Max(0, Math.Min(Me._leftColumn, Math.Max(0, widest - Me.InnerWidth)))
    End Sub

    Private ReadOnly Property InnerWidth As Integer
      Get
        Return Math.Max(1, Me.Width - 3)
      End Get
    End Property

    Public Overrides Sub OnMouseWheel(sender As Object, e As MouseEvent)
      Me.ScrollTo(Me._topLine - Math.Sign(e.WheelDelta) * 3)
      Screen.GetScreen()?.Invalidate()
    End Sub

    Public Sub ScrollTo(topLine As Integer)
      Me._topLine = topLine
      Me.ClampScroll()
    End Sub

    Private Sub FocusLink(index As Integer)
      If Me._links.Count = 0 Then Return
      Me._focusLink = ((index Mod Me._links.Count) + Me._links.Count) Mod Me._links.Count
      Dim link = Me._links(Me._focusLink)
      If link.Line >= 0 Then
        ' bring an inline link into view
        If link.Line < Me._topLine Then Me._topLine = link.Line
        If link.Line >= Me._topLine + Me.VisibleRows Then Me._topLine = link.Line - Me.VisibleRows + 1
        Me.ClampScroll()
      End If
    End Sub

    Private Function FindLinkStartingWith(ch As Char, forward As Boolean) As Integer
      Dim count = Me._links.Count
      For step_ = 1 To count
        Dim index = ((Me._focusLink + If(forward, step_, -step_)) Mod count + count) Mod count
        If Me._links(index).Text.Length > 0 AndAlso Char.ToUpperInvariant(Me._links(index).Text(0)) = Char.ToUpperInvariant(ch) Then Return index
      Next
      Return -1
    End Function

    Private Sub FollowFocusedLink()
      If Me._focusLink >= 0 AndAlso Me._focusLink < Me._links.Count Then Me.Follow(Me._links(Me._focusLink).Target)
    End Sub

    ' ---- input

    Public Overrides Sub OnBeforeKeyDown(sender As Object, e As KeyEvent)

      If Not Me.Visible Then Return

      Dim ctrl = (e.ControlKeyState And (ControlKeyState.LEFT_CTRL_PRESSED Or ControlKeyState.RIGHT_CTRL_PRESSED)) <> 0
      Dim alt = (e.ControlKeyState And (ControlKeyState.LEFT_ALT_PRESSED Or ControlKeyState.RIGHT_ALT_PRESSED)) <> 0
      Dim shift = (e.ControlKeyState And ControlKeyState.SHIFT_PRESSED) <> 0

      Select Case e.VirtualKey

        Case VirtualKey.VK_ESCAPE
          e.Handled = True
          If Me.OnClose IsNot Nothing Then Me.OnClose.Invoke()

        Case VirtualKey.VK_F1
          If alt Then
            e.Handled = True
            Me.GoBack()
          ElseIf ctrl Then
            e.Handled = True
            Me.GoNext(Not shift)
          End If

        Case VirtualKey.VK_F10
          If ctrl Then
            e.Handled = True
            Me.Maximized = Not Me.Maximized
          End If

        Case VirtualKey.VK_TAB
          e.Handled = True
          Me.FocusLink(Me._focusLink + If(shift, -1, 1))

        Case VirtualKey.VK_RETURN
          e.Handled = True
          Me.FollowFocusedLink()

        Case VirtualKey.VK_UP : e.Handled = True : Me._topLine -= 1 : Me.ClampScroll()
        Case VirtualKey.VK_DOWN : e.Handled = True : Me._topLine += 1 : Me.ClampScroll()
        Case VirtualKey.VK_PRIOR : e.Handled = True : Me._topLine -= Me.VisibleRows : Me.ClampScroll()
        Case VirtualKey.VK_NEXT : e.Handled = True : Me._topLine += Me.VisibleRows : Me.ClampScroll()
        Case VirtualKey.VK_HOME : e.Handled = True : Me._topLine = 0 : Me._leftColumn = 0 : Me.ClampScroll()
        Case VirtualKey.VK_END : e.Handled = True : Me._topLine = Me._lines.Count : Me.ClampScroll()
        Case VirtualKey.VK_LEFT : e.Handled = True : Me._leftColumn -= 1 : Me.ClampScroll()
        Case VirtualKey.VK_RIGHT : e.Handled = True : Me._leftColumn += 1 : Me.ClampScroll()

        Case Else
          ' a letter jumps to the next hyperlink starting with it (the previous one with Shift)
          If Not ctrl AndAlso Not alt AndAlso Char.IsLetter(e.Character) Then
            Dim index = Me.FindLinkStartingWith(e.Character, Not Char.IsUpper(e.Character))
            If index >= 0 Then
              e.Handled = True
              Me.FocusLink(index)
            End If
          End If

      End Select

      If e.Handled Then Screen.GetScreen().Invalidate()

    End Sub

    Public Overrides Sub OnMouseDown(sender As Object, e As MouseEvent)

      Dim x = CInt(e.X)
      Dim y = CInt(e.Y)

      For i = 0 To Me._links.Count - 1
        Dim link = Me._links(i)
        If link.ScreenWidth > 0 AndAlso y = link.ScreenY AndAlso x >= link.ScreenX AndAlso x < link.ScreenX + link.ScreenWidth Then
          Me._focusLink = i
          Me.FollowFocusedLink()
          Screen.GetScreen().Invalidate()
          Return
        End If
      Next

      If x = Me.Right AndAlso y = Me.Top + 1 Then
        Me._topLine -= 1
      ElseIf x = Me.Right AndAlso y = Me.Bottom - 1 Then
        Me._topLine += 1
      ElseIf y = Me.Bottom AndAlso x = Me.Left Then
        Me._leftColumn -= 1
      ElseIf y = Me.Bottom AndAlso x = Me.Right Then
        Me._leftColumn += 1
      End If

      Me.ClampScroll()
      Screen.GetScreen().Invalidate()

    End Sub

    ' ---- drawing

    Public Overrides Sub PlaceCursor(screen As Screen)
      ' no text cursor in the help window
    End Sub

    Public Overrides Sub Render(screen As Screen)

      If Not Me.Visible OrElse Me._topic Is Nothing Then Return
      If Me.Width < 20 OrElse Me.Height < 6 Then Return

      Me.ClampScroll()

      Dim fg = Me.ForegroundColor
      Dim bg = Me.BackgroundColor

      screen.DrawBoxFill(Me.Left, Me.Top, Me.Right, Me.Bottom, fg, bg)
      screen.DrawBox(Me.Left, Me.Top, Me.Right, Me.Bottom, fg, bg)

      ' the title sits in the top border in reverse video
      Dim title = " HELP: " & Me._topic.Title & " "
      screen.DrawString(Me.Left + (Me.Width - title.Length) \ 2, Me.Top, title, bg, fg)

      Me.DrawLinkBar(screen)

      ' rule under the link bar
      screen.DrawLeftTee(Me.Left, Me.Top + 2, fg, bg)
      screen.DrawHorizontalLine(Me.Left + 1, Me.Right - 1, Me.Top + 2, fg, bg)
      screen.DrawRightTee(Me.Right, Me.Top + 2, fg, bg)
      If Not String.IsNullOrEmpty(Me._topic.RuleTitle) Then
        screen.DrawString(Me.Left + (Me.Width - Me._topic.RuleTitle.Length) \ 2, Me.Top + 2, Me._topic.RuleTitle, fg, bg)
      End If

      Me.DrawContent(screen)
      Me.DrawScrollBars(screen)

    End Sub

    Private Sub DrawLinkBar(screen As Screen)

      Dim fg = Me.ForegroundColor
      Dim bg = Me.BackgroundColor
      Dim x = Me.Left + 3
      Dim y = Me.Top + 1
      Dim linkIndex = 0

      For Each item In Me._topic.Nav

        Dim label = LeftArrow & item.Text & RightArrow
        If x + label.Length > Me.Right Then Exit For

        If item.IsCurrent Then
          screen.DrawString(x, y, label, ConsoleColor.White, bg)
        Else
          Dim focused = linkIndex = Me._focusLink
          screen.DrawString(x, y, LeftArrow.ToString(), ConsoleColor.Green, bg)
          screen.DrawString(x + 1, y, item.Text, If(focused, bg, fg), If(focused, fg, bg))
          screen.DrawString(x + label.Length - 1, y, RightArrow.ToString(), ConsoleColor.Green, bg)
          Dim link = Me._links(linkIndex)
          link.ScreenX = x
          link.ScreenY = y
          link.ScreenWidth = label.Length
          linkIndex += 1
        End If

        x += label.Length + 3

      Next

    End Sub

    Private Sub DrawContent(screen As Screen)

      Dim fg = Me.ForegroundColor
      Dim bg = Me.BackgroundColor
      Dim rows = Me.VisibleRows
      Dim left = Me.Left + 1

      For Each link In Me._links
        If link.Line >= 0 Then link.ScreenWidth = 0
      Next

      For r = 0 To rows - 1

        Dim index = Me._topLine + r
        If index >= Me._lines.Count Then Exit For

        Dim line = Me._lines(index)
        Dim y = Me.Top + 3 + r

        Select Case line.Kind

          Case LineKind.Rule
            screen.DrawLeftTee(Me.Left, y, fg, bg)
            screen.DrawHorizontalLine(Me.Left + 1, Me.Right - 1, y, fg, bg)
            screen.DrawRightTee(Me.Right, y, fg, bg)

          Case LineKind.TitledRule
            screen.DrawLeftTee(Me.Left, y, fg, bg)
            screen.DrawHorizontalLine(Me.Left + 1, Me.Right - 1, y, fg, bg)
            screen.DrawRightTee(Me.Right, y, fg, bg)
            screen.DrawString(Me.Left + (Me.Width - line.Text.Length) \ 2, y, line.Text, fg, bg)

          Case Else
            Me.DrawTextLine(screen, line, y, left)

        End Select

      Next

    End Sub

    Private Sub DrawTextLine(screen As Screen, line As ParsedLine, y As Integer, left As Integer)

      Dim bg = Me.BackgroundColor
      Dim width = Me.InnerWidth

      For column = 0 To width - 1

        Dim position = Me._leftColumn + column
        If position >= line.Text.Length Then Exit For

        Dim fg = Me.ForegroundColor
        Dim back = bg

        For Each span In line.Spans
          If position >= span.Start AndAlso position < span.Start + span.Length Then
            If span.IsLink Then
              Dim isArrow = position = span.Start OrElse position = span.Start + span.Length - 1
              Dim focused = span.LinkIndex = Me._focusLink
              If isArrow Then
                fg = ConsoleColor.Green
              ElseIf focused Then
                fg = bg
                back = Me.ForegroundColor
              End If
            Else
              fg = ConsoleColor.White
            End If
          End If
        Next

        screen.DrawString(left + column, y, line.Text(position).ToString(), fg, back)

      Next

      ' remember where inline links are, for mouse clicks
      For Each span In line.Spans
        If span.IsLink Then
          Dim link = Me._links(span.LinkIndex)
          Dim startColumn = span.Start - Me._leftColumn
          If startColumn >= 0 AndAlso startColumn + span.Length <= width Then
            link.ScreenX = left + startColumn
            link.ScreenY = y
            link.ScreenWidth = span.Length
          End If
        End If
      Next

    End Sub

    Private Sub DrawScrollBars(screen As Screen)

      Dim fg = Me.ForegroundColor
      Dim bg = Me.BackgroundColor

      ' vertical: the right border
      screen.DrawUpArrow(Me.Right, Me.Top + 1, fg, bg)
      screen.DrawDownArrow(Me.Right, Me.Bottom - 1, fg, bg)
      For y = Me.Top + 2 To Me.Bottom - 2
        screen.DrawLightShade(Me.Right, y, fg, bg)
      Next
      If Me.Bottom - 2 > Me.Top + 2 Then
        Dim span = Math.Max(1, Me._lines.Count - Me.VisibleRows)
        screen.DrawBlock(Me.Right, Utility.GetRangeValue(Me.Top + 2, Me.Bottom - 2, Me._topLine / CDbl(span)), fg, bg)
      End If

      ' horizontal: the bottom border
      screen.DrawLeftArrow(Me.Left, Me.Bottom, fg, bg)
      screen.DrawRightArrow(Me.Right, Me.Bottom, fg, bg)
      For x = Me.Left + 1 To Me.Right - 1
        screen.DrawLightShade(x, Me.Bottom, fg, bg)
      Next
      Dim widest = If(Me._lines.Count = 0, 0, Me._lines.Max(Function(l) l.Text.Length))
      Dim hspan = Math.Max(1, widest - Me.InnerWidth)
      screen.DrawBlock(Utility.GetRangeValue(Me.Left + 1, Me.Right - 1, Me._leftColumn / CDbl(hspan)), Me.Bottom, fg, bg)

    End Sub

  End Class

End Namespace
