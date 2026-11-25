' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

'Imports System.Text

Namespace ConsoleGUI

  Public Structure Dimensions

    Public Property Width() As Integer
    Public Property Height() As Integer

    Public Sub New(width As Integer, height As Integer)
      'Me.New()
      Me.Width = width
      Me.Height = height
    End Sub

  End Structure

End Namespace