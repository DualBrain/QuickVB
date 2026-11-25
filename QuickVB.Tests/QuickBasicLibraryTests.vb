Option Explicit On
Option Strict On
Option Infer On

Imports System.Reflection
Imports Xunit

Public Class QuickBasicLibraryTests

  <Fact>
  Public Sub ACopyOfQuickBasicIsEmbeddedInQuickVB()
    Assert.Contains("QuickBasic.dll", GetType(QuickBasicLibrary).Assembly.GetManifestResourceNames())
  End Sub

  <Fact>
  Public Sub TheEmbeddedCopyIsTheLibraryQuickVBUses()
    Dim embedded = Assembly.Load(QuickBasicLibrary.Image())
    Assert.Equal("QuickBasic", embedded.GetName().Name)
    Assert.NotNull(embedded.GetType("QuickBasic.Compatibility"))
    Assert.NotNull(embedded.GetType("QuickBasic.DebugHost"))
  End Sub

End Class
