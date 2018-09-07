' Original version Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Imports System.ComponentModel.Composition
Imports System.ComponentModel.Composition.Hosting
Imports System.ComponentModel.Composition.Primitives
Imports System.Reflection

Public Class RoslynCompositionManager

  Private compositionContainer As CompositionContainer

  Private ReadOnly catalog As AggregateCatalog

  Sub New()

    Me.catalog = New AggregateCatalog()

    Dim roslynMefAssemblies = New String() {"Microsoft.CodeAnalysis.Workspaces",
                                            "Microsoft.CodeAnalysis.VisualBasic.Workspaces",
                                            "Microsoft.CodeAnalysis.CSharp.Workspaces"}

    For Each roslynMefAssembly In roslynMefAssemblies
      Me.Add(Assembly.Load(roslynMefAssembly))
    Next

  End Sub

  Public Sub Compose(ParamArray attributedParts As Object())
    Me.Container.ComposeParts(attributedParts)
  End Sub

  Public Function GetExport(Of T)() As T
    Return Me.Container.GetExport(Of T)().Value
  End Function

  Public ReadOnly Property Container As CompositionContainer
    Get

      If Me.compositionContainer Is Nothing Then
        Me.compositionContainer = New CompositionContainer(Me.catalog, isThreadSafe:=True)
      End If

      Return Me.compositionContainer

    End Get
  End Property

  Protected Function CreateAssemblyCatalog(assembly As Assembly) As ComposablePartCatalog

    'Dim result As ComposablePartCatalog = Nothing
    'result = New AssemblyCatalog(assembly)
    'Return result

    Return New AssemblyCatalog(assembly)

  End Function

  Protected Function CreateDirectoryCatalog(directory As String) As ComposablePartCatalog

    'Dim result As ComposablePartCatalog = Nothing
    'result = New DirectoryCatalog(directory)
    'Return result

    Return New DirectoryCatalog(directory)

  End Function

  Public Sub Add(assembly As Assembly)
    Me.catalog.Catalogs.Add(Me.CreateAssemblyCatalog(assembly))
  End Sub

End Class