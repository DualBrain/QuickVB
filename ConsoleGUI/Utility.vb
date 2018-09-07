' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

'Imports System.Text
'Imports System.Threading.Tasks

Namespace ConsoleGUI
  Friend Module Utility

    <System.Runtime.CompilerServices.Extension>
    Public Function IsControlPressed(state As ControlKeyState) As Boolean
      Return (state And (ControlKeyState.LEFT_CTRL_PRESSED Or ControlKeyState.RIGHT_CTRL_PRESSED)) <> 0
    End Function

    <System.Runtime.CompilerServices.Extension>
    Public Function SubstringPadRight(s As String, startIndex As Integer, length As Integer, totalWidth As Integer, Optional trim As Boolean = False) As String

      Dim [sub] As String

      Dim l = Math.Min(length, s.Length - startIndex)

      If startIndex > s.Length Then
        [sub] = ""
      Else
        [sub] = s.Substring(startIndex, l)
      End If

      [sub] = [sub].PadRight(totalWidth)

      If trim AndAlso [sub].Length > totalWidth Then
        [sub] = [sub].Substring(0, totalWidth)
      End If

      Return [sub]

    End Function

    <System.Runtime.CompilerServices.Extension>
    Public Function SubstringPadRight(s As String, startIndex As Integer, totalWidth As Integer, Optional trim As Boolean = False) As String
      Return s.SubstringPadRight(startIndex, s.Length, totalWidth, trim)
    End Function

    Public Function GetRangeValue([from] As Integer, [to] As Integer, progress As Double) As Integer

      Dim value = CInt(Fix(Math.Floor([from] + (progress * ([to] - [from])))))

      If value < from Then
        value = [from]
      ElseIf value > [to] Then
        value = [to]
      End If

      Return value

    End Function

  End Module

End Namespace