' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Namespace ConsoleGUI

  Public Class MenuItem
    Inherits Control

    Public Property AnchorRight() As Boolean
    Public Property Separator() As Boolean
    Public Property DropDownMenu() As Menu
    Public Property HighlightAccelerator() As Boolean
    Public Property Action() As Action
    Public Property ParentMenu() As Menu

    Private _name As String
    Private _accelerator? As Char
    Private _acceleratorCharIndex As Integer
    Private _active As Boolean
    Private _activator As Menu

    Public Sub New(Optional Name As String = Nothing,
                   Optional Action As Action = Nothing,
                   Optional DropDownMenu As Menu = Nothing,
                   Optional Visible As Boolean = True,
                   Optional Separator As Boolean = False,
                   Optional AnchorRight As Boolean = False)
      Me.Name = Name
      Me.Action = Action
      Me.DropDownMenu = DropDownMenu
      Me.Visible = Visible
      Me.Separator = Separator
      Me.AnchorRight = AnchorRight
    End Sub

    Public Property Name() As String
      Get
        Return Me._name
      End Get
      Set(value As String)
        Me._accelerator = Nothing
        If value IsNot Nothing Then
          Me._acceleratorCharIndex = value.IndexOf("&"c)
          Me._name = value
          If Me._acceleratorCharIndex <> -1 Then
            Me._name = Me._name.Remove(Me._acceleratorCharIndex, 1)
            Me._accelerator = Me._name.Chars(Me._acceleratorCharIndex)
          End If
        Else
          Me._name = Nothing
        End If
      End Set
    End Property

    Public ReadOnly Property Accelerator() As Char?
      Get
        Return Me._accelerator
      End Get
    End Property

    Public Sub Activate(screen As Screen, activator As Menu)
      Me._active = True
      screen.ActiveControl = Me
      Me._activator = activator
      screen.Invalidate()
    End Sub

    Public Sub Popup(screen As Screen)
      Me._active = True
      Me._activator = Nothing
      screen.Invalidate()
    End Sub

    Public Sub Deactivate(screen As Screen)
      Me._active = False
      If Me._activator IsNot Nothing Then
        screen.ActiveControl = Me._activator
        Me._activator = Nothing
      End If
      screen.Invalidate()
    End Sub

    Private Sub ActivateDropDownMenu(sender As Object)
      Me.DropDownMenu.Left = Me.Left
      Me.DropDownMenu.Top = Me.Top + 1
      Me.DropDownMenu.Layout()
      If Me.AnchorRight Then
        Me.DropDownMenu.Left = Me.Right - Me.DropDownMenu.Width
        Me.DropDownMenu.Layout()
      End If
      Me.DropDownMenu.Activate(CType(sender, Screen), Me)
    End Sub

    Friend Sub TerminateMenuMode()
      Me.Deactivate(Screen.GetScreen())
      Dim parent As Menu = TryCast(Screen.GetScreen().ActiveControl, Menu)
      If parent IsNot Nothing Then
        parent.TerminateMenuMode()
      End If
    End Sub

    Public Overrides Sub OnBeforeKeyDown(sender As Object, e As KeyEvent)

      ' Really should just check for enter, down & dropdown, and accelerator,
      ' then execute action delegate.

      If Me.DropDownMenu IsNot Nothing Then
        If e.VirtualKey = VirtualKey.VK_DOWN OrElse Me.IsAcceleratorMatch(e.Character) Then
          Me.ActivateDropDownMenu(sender)
          e.Handled = True
          Return
        End If
      ElseIf Me.Action IsNot Nothing Then
        If e.VirtualKey = VirtualKey.VK_RETURN OrElse Me.IsAcceleratorMatch(e.Character) Then
          Me.[Select]()
          e.Handled = True
          Return
        End If
      End If

      If e.VirtualKey = VirtualKey.VK_ESCAPE Then
        Me.TerminateMenuMode()
        e.Handled = True
        Return
      End If

      If Me._activator IsNot Nothing AndAlso (Not Me.IsAcceleratorMatch(e.Character)) Then
        Dim activator = Me._activator
        activator.OnBeforeKeyDown(sender, e)
        If Screen.GetScreen().ActiveControl IsNot Me Then
          Me._active = False
        End If
        activator.OnAfterKeyDown(sender, e)
      End If

    End Sub

    Public Overrides Sub OnMouseDown(sender As Object, e As MouseEvent)

      Dim screen = CType(sender, Screen)
      Dim activator = screen.ActiveControl

      Dim theParentMenu = Me.ParentMenu

      If theParentMenu Is Nothing Then
        Return
      End If

      If Me.DropDownMenu IsNot Nothing Then
        Dim previousMenuItem = TryCast(activator, MenuItem)
        If previousMenuItem Is Nothing Then
          theParentMenu.Activate(screen, activator)
        Else
          previousMenuItem.ParentMenu.Deactivate(screen)
        End If
        theParentMenu.ActivateMenuItem(screen, Me)
        Me.ActivateDropDownMenu(screen)
      ElseIf Me.Action IsNot Nothing Then
        theParentMenu.ActivateMenuItem(screen, Me)
        Me.Select()
      End If

    End Sub

    Public Sub [Select]()
      If Me.Action IsNot Nothing Then
        Me.TerminateMenuMode()
        Me.Action()()
      End If
    End Sub

    Public Overrides Sub OnKeyUp(sender As Object, e As KeyEvent)
      If e.VirtualKey = VirtualKey.VK_MENU Then
        Me.Deactivate(CType(sender, Screen))
        CType(sender, Screen).ActiveControl.OnKeyUp(sender, e)
      End If
    End Sub

    Friend Function IsAcceleratorMatch(character As Char) As Boolean

      If Me.Accelerator Is Nothing Then
        Return False
      End If

      Return Char.ToLowerInvariant(CChar(Me.Accelerator)) = Char.ToLowerInvariant(character)

    End Function

    Public Overrides Sub Render(screen As Screen)

      If Not Me.Visible Then
        Return
      End If

      Dim fg, bg, acceleratorColor As ConsoleColor

      If screen.Theme = ControlTheme.CSharp Then
        fg = If(Me._active, ConsoleColor.Black, ConsoleColor.Black)
        bg = If(Me._active, ConsoleColor.DarkGreen, ConsoleColor.Gray)
        acceleratorColor = ConsoleColor.DarkRed
      Else
        fg = If(Me._active, ConsoleColor.Gray, ConsoleColor.Black)
        bg = If(Me._active, ConsoleColor.Black, ConsoleColor.Gray)
        acceleratorColor = ConsoleColor.White
      End If

      Dim grayedOut = Me.DropDownMenu Is Nothing AndAlso
                      Me.Action Is Nothing AndAlso
                      Not Me.Separator

      If grayedOut Then
        fg = ConsoleColor.DarkGray
      End If

      If Me.Separator Then
        screen.DrawLeftTee(Me.ScreenLeft, Me.ScreenTop, fg, bg)
        screen.DrawRightTee(Me.ScreenRight, Me.ScreenTop, fg, bg)
        screen.DrawHorizontalLine(Me.ScreenLeft + 1, Me.ScreenRight - 1, Me.ScreenTop, fg, bg)
      ElseIf (Not Me.HighlightAccelerator AndAlso screen.Theme <> ControlTheme.CSharp) OrElse grayedOut Then
        screen.DrawHorizontalLine(Me.ScreenLeft, Me.ScreenRight, Me.ScreenTop, " "c, fg, bg)
        screen.DrawString(Me.ScreenLeft + 1, Me.ScreenTop, Me.Name, fg, bg)
      Else

        screen.DrawHorizontalLine(Me.ScreenLeft, Me.ScreenRight, Me.ScreenTop, " "c, fg, bg)

        screen.DrawString(Me.ScreenLeft + 1, Me.ScreenTop, Me.Name, fg, bg)

        If Me._acceleratorCharIndex <> -1 Then
          screen.DrawString(Me.ScreenLeft + 1 + Me._acceleratorCharIndex, Me.ScreenTop, Me.Name.Substring(Me._acceleratorCharIndex, 1), acceleratorColor, bg)
        End If

      End If

    End Sub

  End Class

  Public Class MenuItem(Of TValue)
    Inherits MenuItem

    Public Overloads Property Value() As TValue

    Public Sub New(Optional Name As String = Nothing,
                   Optional Value As TValue = Nothing,
                   Optional Action As Action = Nothing,
                   Optional DropDownMenu As Menu = Nothing,
                   Optional Visible As Boolean = True,
                   Optional Separator As Boolean = False,
                   Optional AnchorRight As Boolean = False)
      MyBase.New(Name, Action, DropDownMenu, Visible, Separator, AnchorRight)
      Me.Value = Value
    End Sub

  End Class

End Namespace