Option Explicit On
Option Strict On
Option Infer On

Imports System.IO
Imports System.Reflection

''' <summary>
''' QuickBasic.dll, the library programs written in QuickVB use (PRINT and the rest). A copy is embedded in QuickVB so that it is there
''' however QuickVB was installed: a single file has no QuickBasic.dll beside it to take it from.
''' </summary>
Public Module QuickBasicLibrary

  Private _image As Byte()

  ''' <summary>The bytes of QuickBasic.dll.</summary>
  Public Function Image() As Byte()

    If _image Is Nothing Then

      Using stream = GetType(QuickBasicLibrary).Assembly.GetManifestResourceStream("QuickBasic.dll")

        If stream IsNot Nothing Then
          Using buffer As New MemoryStream()
            stream.CopyTo(buffer)
            _image = buffer.ToArray()
          End Using
        Else
          ' no embedded copy (it should always be there): the library QuickVB itself is using
          _image = File.ReadAllBytes(GetType(QuickBasic.Compatibility).Assembly.Location)
        End If

      End Using

    End If

    Return _image

  End Function

End Module
