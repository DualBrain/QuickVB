' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Namespace ConsoleGUI

  ''' <summary>One entry of a help topic's link bar ("Editing Keys", "Contents", "Index"): the topic it leads to, or the topic itself when <see cref="IsCurrent"/>.</summary>
  Public NotInheritable Class HelpNavItem

    Public Sub New(text As String, target As String, Optional isCurrent As Boolean = False)
      Me.Text = text
      Me.Target = target
      Me.IsCurrent = isCurrent
    End Sub

    Public ReadOnly Property Text As String
    Public ReadOnly Property Target As String
    Public ReadOnly Property IsCurrent As Boolean

  End Class

  ''' <summary>
  ''' A page of help. <see cref="Lines"/> are plain text, except that a line of "&lt;hr&gt;" draws a horizontal rule, "&lt;hr:Title&gt;" a rule with a title in it,
  ''' and "[[text|topic]]" anywhere in a line is a hyperlink to another topic.
  ''' </summary>
  Public NotInheritable Class HelpTopic

    Public Sub New(id As String, title As String, nav As IEnumerable(Of HelpNavItem), lines As IEnumerable(Of String), Optional ruleTitle As String = Nothing)
      Me.Id = id
      Me.RuleTitle = ruleTitle
      Me.Title = title
      Me.Nav = nav.ToList()
      Me.Lines = lines.ToList()
    End Sub

    Public ReadOnly Property Id As String

    ''' <summary>The window title is "HELP: " followed by this.</summary>
    Public ReadOnly Property Title As String

    ''' <summary>A title drawn in the rule under the link bar (as in the Search Keys topic), or Nothing for a plain rule.</summary>
    Public ReadOnly Property RuleTitle As String

    Public ReadOnly Property Nav As IReadOnlyList(Of HelpNavItem)
    Public ReadOnly Property Lines As IReadOnlyList(Of String)

  End Class

End Namespace
