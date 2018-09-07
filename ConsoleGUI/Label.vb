' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

'Imports System.Text
'Imports System.Threading.Tasks

Namespace ConsoleGUI

  Public Class Label
    Inherits Control

    Public Sub New()
      Me.Height = 1
    End Sub

    Public Property Text() As String

    Public Overrides Sub Render(screen As Screen)

      If Not Me.Visible Then
        Return
      End If

      screen.DrawHorizontalLine(Me.Left,
                                Me.Right,
                                Me.Top,
                                " "c,
                                Me.ForegroundColor,
                                Me.BackgroundColor)

      screen.DrawString(Me.Left + Me.PaddingLeft,
                        Me.Top + Me.PaddingTop,
                        Me.Text.SubstringPadRight(0, Me.Width - Me.PaddingLeft - Me.PaddingRight, trim:=True),
                        Me.ForegroundColor,
                        Me.BackgroundColor)

    End Sub

  End Class

End Namespace