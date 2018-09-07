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

    Public Overridable Function HitTest(x As Integer, y As Integer) As Control
      If x >= Me.Left AndAlso
         x <= Me.Right AndAlso
         y >= Me.Top AndAlso
         y <= Me.Bottom Then
        Return Me
      Else
        Return Nothing
      End If
    End Function

  End Class

End Namespace