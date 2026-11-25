' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Namespace ConsoleGUI

  Public MustInherit Class Control

    Public Sub New()
      Me.ForegroundColor = ConsoleColor.Gray
      Me.BackgroundColor = ConsoleColor.Black
      Me.Visible = True
    End Sub

    Public Property Left() As Integer
    Public Property Top() As Integer
    Public Property Width() As Integer
    Public Property Height() As Integer

    Public ReadOnly Property Right() As Integer
      Get
        Return Me.Left + Me.Width - 1
      End Get
    End Property

    Public ReadOnly Property Bottom() As Integer
      Get
        Return Me.Top + Me.Height - 1
      End Get
    End Property

    Public Property Visible() As Boolean

    Public Property Parent() As Control

    Public ReadOnly Property ScreenLeft() As Integer
      Get
        Return If(Me.Parent IsNot Nothing, Me.Parent.ScreenLeft, 0) + Me.Left
      End Get
    End Property

    Public ReadOnly Property ScreenTop() As Integer
      Get
        Return If(Me.Parent IsNot Nothing, Me.Parent.ScreenTop, 0) + Me.Top
      End Get
    End Property

    Public ReadOnly Property ScreenRight() As Integer
      Get
        Return Me.ScreenLeft + Me.Width - 1
      End Get
    End Property

    Public ReadOnly Property ScreenBottom() As Integer
      Get
        Return Me.ScreenTop + Me.Height - 1
      End Get
    End Property

    Public Property PaddingLeft() As Integer
    Public Property PaddingTop() As Integer
    Public Property PaddingRight() As Integer
    Public Property PaddingBottom() As Integer
    Public Property BackgroundColor() As ConsoleColor
    Public Property ForegroundColor() As ConsoleColor

    ''' <summary>Text for the status bar while this control is the active popup (Nothing: the screen's usual status).</summary>
    Public Overridable ReadOnly Property StatusHint As String
      Get
        Return Nothing
      End Get
    End Property

    Public Overridable Sub UpdateCursor(screen As Screen)
    End Sub

    Public MustOverride Sub Render(screen As Screen)

    Public Overridable Sub PlaceCursor(screen As Screen)
    End Sub

    Public Overridable Sub OnBeforeKeyDown(sender As Object, e As KeyEvent)
    End Sub
    Public Overridable Sub OnAfterKeyDown(sender As Object, e As KeyEvent)
    End Sub

    Public Overridable Sub OnKeyUp(sender As Object, e As KeyEvent)
    End Sub

    Public Overridable Sub OnMouseDown(sender As Object, e As MouseEvent)
    End Sub

    ''' <summary>The button that went down over this control was released (wherever the pointer is now).</summary>
    Public Overridable Sub OnMouseUp(sender As Object, e As MouseEvent)
    End Sub

    ''' <summary>The pointer moved while the button that went down over this control is still held.</summary>
    Public Overridable Sub OnMouseDrag(sender As Object, e As MouseEvent)
    End Sub

    ''' <summary>
    ''' The wheel turned over this control. Unless the control scrolls itself the wheel works like the Up / Down keys,
    ''' which scrolls lists and moves through menus.
    ''' </summary>
    Public Overridable Sub OnMouseWheel(sender As Object, e As MouseEvent)

      Dim key = If(e.WheelDelta > 0, VirtualKey.VK_UP, VirtualKey.VK_DOWN)
      Dim down As New KeyEvent(True, ChrW(0), 1, key, 0)

      Me.OnBeforeKeyDown(sender, down)
      If Not down.Handled Then Me.OnAfterKeyDown(sender, down)

      Screen.GetScreen()?.Invalidate()

    End Sub

    Public Overridable Function HitTest(x As Integer, y As Integer) As Control
      If Me.Visible AndAlso
         x >= Me.ScreenLeft AndAlso
         x <= Me.ScreenRight AndAlso
         y >= Me.ScreenTop AndAlso
         y <= Me.ScreenBottom Then
        Return Me
      Else
        Return Nothing
      End If
    End Function

  End Class

End Namespace