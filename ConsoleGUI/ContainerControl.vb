' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Imports System.Collections.ObjectModel
'Imports System.Collections.Specialized

Namespace ConsoleGUI

  Public MustInherit Class ContainerControl
    Inherits Control

    Protected Sub New()
      Me.Controls = New ObservableCollection(Of Control)()
      AddHandler Me.Controls.CollectionChanged, AddressOf Me.Controls_CollectionChanged
    End Sub

    Private Sub Controls_CollectionChanged(sender As Object, e As System.Collections.Specialized.NotifyCollectionChangedEventArgs)

      If e.OldItems IsNot Nothing Then
        For Each control In e.OldItems.Cast(Of Control)()
          control.Parent = Nothing
        Next
      End If

      If e.NewItems IsNot Nothing Then

        For Each control In e.NewItems.Cast(Of Control)()

          control.Parent = Me

          Me.ApplyFill(control)

        Next

      End If

    End Sub

    Private Sub ApplyFill(control As Control)
      If Me.Fill Then
        control.Left = Me.PaddingLeft
        control.Top = Me.PaddingTop
        control.Width = Math.Max(0, Me.Width - Me.PaddingLeft - Me.PaddingRight)
        control.Height = Math.Max(0, Me.Height - Me.PaddingTop - Me.PaddingBottom)
      End If
    End Sub

    ''' <summary>Re-applies the fill layout to the children; call after changing this container's size.</summary>
    Public Sub LayoutChildren()
      For Each control In Me.Controls
        Me.ApplyFill(control)
      Next
    End Sub

    Public Property Controls() As ObservableCollection(Of Control)
    Public Property Fill() As Boolean

    Public Overrides Sub Render(screen As Screen)
      For Each control In Me.Controls
        control.Render(screen)
      Next
    End Sub

    Public Overrides Function HitTest(x As Integer, y As Integer) As Control

      For Each control In Me.Controls.Reverse()
        Dim hit = control.HitTest(x, y)
        If hit IsNot Nothing Then
          Return hit
        End If
      Next

      Return Nothing

    End Function

  End Class

End Namespace