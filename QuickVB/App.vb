' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

'Imports ConsoleGUI
Imports System.Reflection
Imports System.ComponentModel.Composition.Hosting
Imports System.IO
Imports System.Windows.Forms

Imports Microsoft.CodeAnalysis.Host.Mef

Module App

  Public TheWorkspace As QuickVBWorkspace

  Public CompositionContainer As CompositionContainer

  Private TheScreen As MyScreen

  ReadOnly untitledText As String = File.ReadAllText("TestProject/Untitled.vb")

  Public Sub NewUntitledWorkspace()

    Try
      TheWorkspace = New QuickVBWorkspace()
    Catch ex As ReflectionTypeLoadException
      Dim sb = New Text.StringBuilder()
      For Each exSub In ex.LoaderExceptions
        sb.AppendLine(exSub.Message)
        Dim exFileNotFound = TryCast(exSub, FileNotFoundException)
        If exFileNotFound IsNot Nothing Then
          If Not String.IsNullOrEmpty(exFileNotFound.FusionLog) Then
            sb.AppendLine("Fusion Log:")
            sb.AppendLine(exFileNotFound.FusionLog)
          End If
        End If
        sb.AppendLine()
      Next
      Dim errorMessage = sb.ToString()
      ' Display Or log the error based on your application.
      MsgBox(errorMessage)
    End Try

    If TheWorkspace IsNot Nothing Then
      Dim projectId = TheWorkspace.CreateProject("Untitled", "Untitled.exe")
      Dim documentId = TheWorkspace.CreateDocument(projectId, "Untitled.vb")
    End If

  End Sub

  Public Function OpenWorkspace() As String

    Dim ofd = New OpenFileDialog With {.Filter = "Basic files (*.vb;*.bas)|*.vb;*.bas|All files (*.*)|*.*",
                                       .FilterIndex = 1,
                                       .RestoreDirectory = False}

    If ofd.ShowDialog() <> DialogResult.OK Then Return Nothing

    TheWorkspace = New QuickVBWorkspace()

    Dim nameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(ofd.FileName)
    Dim nameWithExt = System.IO.Path.GetFileName(ofd.FileName)
    Dim projectId = TheWorkspace.CreateProject(nameWithoutExt, nameWithoutExt & ".exe")
    Dim documentId = TheWorkspace.CreateDocument(projectId, nameWithExt)

    Return ofd.FileName

  End Function

  Public Sub LoadSelfWorkspace()

    TheWorkspace = New QuickVBWorkspace()

    Dim appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)

    Dim slnFolder = Path.GetFullPath(Path.Combine(appPath, "..\..\.."))

    Do

      Dim slnPath = Path.Combine(slnFolder, "QuickVB.sln")

      If File.Exists(slnPath) Then
        TheWorkspace.LoadExistingSolution(slnPath)
        Exit Sub
      End If

      Dim newSlnFolder = Path.GetFullPath(Path.Combine(slnFolder, ".."))
      If newSlnFolder = slnFolder Then
        Exit Sub
      End If

      slnFolder = newSlnFolder

    Loop

  End Sub

  Sub Main()

    LoadComponents()

    TheScreen = New MyScreen

    TheScreen.Post(Sub()
                     NewUntitledWorkspace()
                     TheScreen.ViewDocument("Untitled", "Untitled.vb", untitledText)
                   End Sub)

    ConsoleGUI.Screen.NavigateTo(TheScreen)

  End Sub

  Public Sub LoadComponents(ParamArray attributedParts As Object())

    Dim compositionManager As New RoslynCompositionManager()

    compositionManager.Add(Assembly.Load("QuickVB"))
    compositionManager.Compose(attributedParts)

    CompositionContainer = compositionManager.Container

  End Sub

End Module