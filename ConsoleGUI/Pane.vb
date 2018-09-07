' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

'Imports System.Text
'Imports System.Threading.Tasks

Namespace ConsoleGUI

  Public Class Pane
    Inherits ContainerControl

    Public Sub New()
      Me.Fill = True
      Me.PaddingLeft = 1
      Me.PaddingTop = 1
      Me.PaddingRight = 1
      Me.PaddingBottom = 1
    End Sub

    Public Property Title() As String

    Public Overrides Sub Render(screen As Screen)

      screen.DrawTopLeftCorner(Me.ScreenLeft, Me.ScreenTop, Me.ForegroundColor, Me.BackgroundColor)
      screen.DrawTopRightCorner(Me.ScreenRight, Me.ScreenTop, Me.ForegroundColor, Me.BackgroundColor)

      If Me.ScreenLeft + 1 <= Me.ScreenRight - 1 Then
        screen.DrawHorizontalLine(Me.ScreenLeft + 1, Me.ScreenRight - 1, Me.ScreenTop, Me.ForegroundColor, Me.BackgroundColor)
      End If

      If Me.ScreenTop + 1 <= Me.ScreenBottom - 1 Then
        screen.DrawVerticalLine(Me.ScreenLeft, Me.ScreenTop + 1, Me.ScreenBottom - 1, Me.ForegroundColor, Me.BackgroundColor)
        screen.DrawVerticalLine(Me.ScreenRight, Me.ScreenTop + 1, Me.ScreenBottom - 1, Me.ForegroundColor, Me.BackgroundColor)
      End If

      screen.DrawLeftTee(Me.ScreenLeft, Me.ScreenBottom, Me.ForegroundColor, Me.BackgroundColor)
      screen.DrawRightTee(Me.ScreenRight, Me.ScreenBottom, Me.ForegroundColor, Me.BackgroundColor)

      Dim titleString = " " & Me.Title & " "

      Dim titleLeft = (Me.ScreenLeft + (Me.ScreenRight - Me.ScreenLeft) \ 2) - (titleString.Length \ 2)

      Dim fg, bg As ConsoleColor

      If screen.ActiveControl.Parent Is Me AndAlso screen.Theme = ControlTheme.Basic Then
        fg = Me.BackgroundColor
        bg = Me.ForegroundColor
      Else
        fg = Me.ForegroundColor
        bg = Me.BackgroundColor
      End If

      screen.DrawString(titleLeft, Me.ScreenTop, titleString, fg, bg)

      MyBase.Render(screen)

    End Sub

  End Class

End Namespace