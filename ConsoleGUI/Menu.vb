' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Namespace ConsoleGUI

  Public Class Menu
    Inherits Control

    Public Property DropDownMenu() As Boolean
    Public Property AllowNoSelection() As Boolean

    Private _menuItems As New List(Of MenuItem)()
    Private _active As Boolean
    Private _activator As Control

    Public Property ActiveMenuItem() As MenuItem

    Public ReadOnly Property VisibleMenuItems() As IEnumerable(Of MenuItem)
      Get
        Return Me._menuItems.Where(Function(mi) mi.Visible)
      End Get
    End Property

    Public Sub SetMenuItems(ParamArray menuItems() As MenuItem)
      Me._menuItems = menuItems.ToList()
      For Each menuItem In menuItems
        menuItem.ParentMenu = Me
      Next menuItem
      Me.ActiveMenuItem = Nothing
      Me.Layout()
    End Sub

    Public ReadOnly Property IsActive() As Boolean
      Get
        Return Me._active
      End Get
    End Property

    Public Sub Activate(screen As Screen, activator As Control)
      Me._activator = activator
      If Me.DropDownMenu AndAlso (Not screen.Controls.Contains(Me)) Then
        screen.Controls.Add(Me)
      End If
      If activator IsNot Nothing Then
        screen.ActiveControl = Me
      Else
        screen.PopupControl = Me
      End If
      If Me.DropDownMenu AndAlso (Not Me.AllowNoSelection) AndAlso Me._menuItems.Count > 0 Then
        Me.ActivateMenuItem(screen, Me._menuItems.First())
      End If
      screen.Invalidate()
      If activator IsNot Nothing Then
        screen.HideCursor()
      End If
    End Sub

    Public Sub ActivateMenuItem(screen As Screen, menuItem As MenuItem)
      If Me.ActiveMenuItem IsNot Nothing Then
        Me.ActiveMenuItem.Deactivate(screen)
      End If
      Me.ActiveMenuItem = menuItem
      If Me._activator IsNot Nothing Then
        menuItem.Activate(screen, Me)
      Else
        menuItem.Popup(screen)
      End If
    End Sub

    Public Sub Deactivate(screen As Screen)
      Me._active = False
      If Me.DropDownMenu Then
        screen.Controls.Remove(Me)
      End If
      If Me._activator IsNot Nothing Then
        screen.ActiveControl = Me._activator
      Else
        screen.PopupControl = Nothing
      End If
      screen.Invalidate()
      If Me._activator IsNot Nothing Then
        screen.ShowCursor()
      End If
    End Sub

    Friend Sub TerminateMenuMode()
      Me.Deactivate(Screen.GetScreen())
      Dim menuItem As MenuItem = TryCast(Screen.GetScreen().ActiveControl, MenuItem)
      If menuItem IsNot Nothing Then
        menuItem.TerminateMenuMode()
      End If
    End Sub

    Private Function HasFocus() As Boolean

      If Screen.GetScreen().ActiveControl Is Me OrElse
         Screen.GetScreen().PopupControl Is Me Then
        Return True
      End If

      For Each menuItem In Me.VisibleMenuItems
        If Screen.GetScreen().ActiveControl Is menuItem Then
          Return True
        End If
      Next

      Return False

    End Function

    Public Sub Layout()
      If Me.DropDownMenu Then
        Me.LayoutAsDropDownMenu()
      Else
        Me.LayoutAsMenuBar()
      End If
    End Sub

    Private Sub LayoutAsMenuBar()

      Dim runningLeft As Integer = Me.Left + 2

      For Each menuItem In Me.VisibleMenuItems

        If menuItem.AnchorRight Then
          Continue For
        End If

        menuItem.Width = menuItem.Name.Length + 2
        menuItem.Height = 1
        menuItem.Left = runningLeft
        menuItem.Top = Me.Top

        runningLeft += menuItem.Width

      Next

      Dim runningRight As Integer = Me.Left + Me.Width - 1

      For Each menuItem In Enumerable.Reverse(Me.VisibleMenuItems)

        If Not menuItem.AnchorRight Then
          Continue For
        End If

        menuItem.Width = menuItem.Name.Length + 2
        menuItem.Height = 1
        menuItem.Left = runningRight - menuItem.Width
        menuItem.Top = Me.Top

        runningRight -= menuItem.Width

      Next

    End Sub

    Private Sub LayoutAsDropDownMenu()

      Dim screen = ConsoleGUI.Screen.GetScreen()

      Dim runningTop As Integer = Me.Top + 1
      Dim maxWidth As Integer = 0

      For Each menuItem In Me.VisibleMenuItems

        If Not menuItem.Separator Then

          menuItem.Width = menuItem.Name.Length + 2

          If maxWidth < menuItem.Width Then
            maxWidth = menuItem.Width
          End If

        End If

        menuItem.Height = 1
        menuItem.Left = Me.Left + 1
        menuItem.Top = runningTop

        runningTop += 1

      Next

      Me.Width = maxWidth + 2
      Me.Height = runningTop - Me.Top + 1

      Dim dx As Integer = 0, dy As Integer = 0

      If screen IsNot Nothing Then
        If Me.ScreenBottom > (screen.Bottom - 2) Then
          dy -= (Me.ScreenBottom - (screen.Bottom - 2))
        End If
        If Me.ScreenRight > (screen.Right - 2) Then
          dx -= (Me.ScreenRight - (screen.Right - 2))
        End If
      End If

      Me.Top += dy
      Me.Left += dx

      For Each menuItem In Me.VisibleMenuItems
        menuItem.Left += dx
        menuItem.Top += dy
        If menuItem.Separator Then
          menuItem.Left -= 1
          menuItem.Width = maxWidth + 2
        Else
          menuItem.Width = maxWidth
        End If
      Next

    End Sub

    Public Sub SelectMenuItem()
      If Me.ActiveMenuItem IsNot Nothing Then
        Me.ActiveMenuItem.Select()
      End If
    End Sub

    Private Function MenuItemFromAccelerator(accelerator As Char) As MenuItem

      For Each menuItem In Me.VisibleMenuItems
        If Not menuItem.Separator AndAlso
           menuItem.IsAcceleratorMatch(accelerator) Then
          Return menuItem
        End If
      Next

      Return Nothing

    End Function

    Private Function MenuItemFromCursorKey(virtualKey As VirtualKey) As MenuItem

      Const NextMenuItem As Integer = 0
      Const PrevMenuItem As Integer = 1
      Const [Nothing] As Integer = 2

      Dim action As Integer = [Nothing]

      If Me.DropDownMenu Then
        If virtualKey = ConsoleGUI.VirtualKey.VK_UP Then
          action = PrevMenuItem
        ElseIf virtualKey = ConsoleGUI.VirtualKey.VK_DOWN Then
          action = NextMenuItem
        End If
      Else
        If virtualKey = ConsoleGUI.VirtualKey.VK_LEFT Then
          action = PrevMenuItem
        ElseIf virtualKey = ConsoleGUI.VirtualKey.VK_RIGHT Then
          action = NextMenuItem
        End If
      End If

      If action = [Nothing] OrElse (Not Me.VisibleMenuItems.Any()) Then
        Return Nothing
      End If

      Dim currentMenuItem = If(Me.ActiveMenuItem, Me.VisibleMenuItems.First())

      Dim index = Me._menuItems.IndexOf(currentMenuItem)
      Dim count = Me.VisibleMenuItems.Count()

      Do
        If action = NextMenuItem Then
          index = (index + 1) Mod count
        ElseIf action = PrevMenuItem Then
          index = (index - 1 + count) Mod count
        End If
      Loop While Me._menuItems(index).Separator

      Return Me._menuItems(index)

    End Function

    Public Overrides Sub OnBeforeKeyDown(sender As Object, e As KeyEvent)

      If (Not Me._active) AndAlso e.VirtualKey <> VirtualKey.VK_MENU Then
        ' Note we only do this when the VK is not VK_MENU so that we don't
        ' become active if alt key is held down and producing key repeats.
        Me._active = True
      End If

      If e.VirtualKey = VirtualKey.VK_ESCAPE Then
        Me.TerminateMenuMode()
      End If

      Dim menuItem = Me.MenuItemFromCursorKey(e.VirtualKey)
      Dim sendKeyDownToMenuItem As Boolean = False

      If menuItem Is Nothing Then
        menuItem = Me.MenuItemFromAccelerator(e.Character)
        If menuItem IsNot Nothing Then
          sendKeyDownToMenuItem = True
          e.Handled = True
        End If
      End If

      If menuItem IsNot Nothing Then
        Me.ActivateMenuItem(CType(sender, Screen), menuItem)
        If sendKeyDownToMenuItem Then
          menuItem.OnBeforeKeyDown(sender, e)
          menuItem.OnAfterKeyDown(sender, e)
        End If
        e.Handled = True
      End If

      If Not e.Handled Then
        If Me._activator Is Nothing Then
          If Me.ActiveMenuItem IsNot Nothing Then
            Me.ActiveMenuItem.OnBeforeKeyDown(sender, e)
            Me.ActiveMenuItem.OnAfterKeyDown(sender, e)
          End If
        Else
          If Me.DropDownMenu Then
            If e.VirtualKey = VirtualKey.VK_LEFT OrElse e.VirtualKey = VirtualKey.VK_RIGHT Then
              Me.Deactivate(Screen.GetScreen())
              Dim eDown = New KeyEvent(True, " "c, 1, VirtualKey.VK_DOWN, 0)
              Me._activator.OnBeforeKeyDown(sender, e)
              Screen.GetScreen().ActiveControl.OnBeforeKeyDown(sender, eDown)
              Me._activator.OnAfterKeyDown(sender, e)
              Screen.GetScreen().ActiveControl.OnAfterKeyDown(sender, eDown)
            End If
          End If
        End If
      End If

    End Sub

    Public Overrides Sub OnKeyUp(sender As Object, e As KeyEvent)
      If Not Me._active Then
        Me._active = True
        If Me.VisibleMenuItems.Any() AndAlso (Not Me.AllowNoSelection) Then
          Me.ActivateMenuItem(CType(sender, Screen), Me.VisibleMenuItems.First())
        End If
      Else
        If e.VirtualKey = VirtualKey.VK_MENU Then
          Me.Deactivate(CType(sender, Screen))
        End If
      End If
    End Sub

    Public Overrides Function HitTest(x As Integer, y As Integer) As Control

      For Each control In Me.VisibleMenuItems
        Dim hit = control.HitTest(x, y)
        If hit IsNot Nothing Then
          Return hit
        End If
      Next

      Return Nothing

    End Function

    Public Overrides Sub Render(screen As Screen)

      If Not Me.DropDownMenu Then
        screen.DrawHorizontalLine(Me.ScreenLeft, Me.ScreenRight, Me.ScreenTop, " "c, ConsoleColor.Black, ConsoleColor.Gray)
      Else
        screen.DrawShadow(Me.ScreenLeft + 2, Me.ScreenTop + 1, Me.ScreenRight + 2, Me.ScreenBottom + 1)
        screen.DrawBoxFill(Me.ScreenLeft, Me.ScreenTop, Me.ScreenRight, Me.ScreenBottom, ConsoleColor.Black, ConsoleColor.Gray)
      End If

      For Each menuItem In Me.VisibleMenuItems
        menuItem.HighlightAccelerator = Me.HasFocus()
        menuItem.Render(screen)
      Next

    End Sub

  End Class

End Namespace