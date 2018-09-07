' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

'Imports System.Text

Namespace ConsoleGUI

  Public Class ConsoleEvent
  End Class

  Public Class NotImplementedEvent
    Inherits ConsoleEvent

    Private ReadOnly _message As String

    Public Sub New(message As String)
      Me._message = message
    End Sub

    Public Overrides Function ToString() As String
      Return "NotImplementedEvent: " & Me._message
    End Function

  End Class

  Public Class KeyEvent
    Inherits ConsoleEvent

    Public Property KeyDown() As Boolean
    Public Property Character() As Char
    Public Property RepeatCount() As UShort
    Public Property VirtualKey() As VirtualKey
    Public Property ControlKeyState() As ControlKeyState

    Public Property Handled() As Boolean

    Public Sub New(keyDown As Boolean, character As Char, repeatCount As UShort, virtualKey As VirtualKey, controlKeyState As ControlKeyState)
      Me.KeyDown = keyDown
      Me.Character = character
      Me.RepeatCount = repeatCount
      Me.VirtualKey = virtualKey
      Me.ControlKeyState = controlKeyState
      Me.Handled = False
    End Sub

  End Class

  Public Class MouseEvent
    Inherits ConsoleEvent

    Public Property X() As Short
    Public Property Y() As Short
    Public Property ButtonState() As ButtonState
    Public Property ControlKeyState() As ControlKeyState
    Public Property MouseEventFlags() As MouseEventFlags

    Public Sub New(x As Short, y As Short, buttonState As ButtonState, controlKeyState As ControlKeyState, mouseEventFlags As MouseEventFlags)
      Me.X = x
      Me.Y = y
      Me.ButtonState = buttonState
      Me.ControlKeyState = controlKeyState
      Me.MouseEventFlags = mouseEventFlags
    End Sub

  End Class

  Public Class BufferSizeEvent
    Inherits ConsoleEvent

    Public Sub New(width As Integer, height As Integer)
      Me.Dimensions = New Dimensions(width, height)
    End Sub

    Public Property Dimensions() As Dimensions

  End Class

End Namespace