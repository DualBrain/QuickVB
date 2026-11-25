' Improvements Copyright (c) Cory Smith.  All Rights Reserved.

Option Explicit On
Option Strict On
Option Infer On

Namespace ConsoleGUI

  ''' <summary>
  ''' The console services a <see cref="Screen"/> needs: cell output, an event stream and cursor control.
  ''' <see cref="Win32Console"/> is the real implementation; <see cref="HeadlessConsole"/> lets screens run (and be tested) without a console.
  ''' </summary>
  Public Interface IConsoleDriver
    Inherits IDisposable

    Function GetBufferDimensions() As Dimensions

    ''' <summary>Writes the given spans (or everything when <paramref name="invalidSpans"/> is Nothing) of a width x height cell buffer.</summary>
    Sub WriteOutputBuffer(bufferChars() As Char, bufferFg() As ConsoleColor, bufferBg() As ConsoleColor, width As Integer, height As Integer, invalidSpans As List(Of RowSpan))

    Sub EnableInputEvents()

    ''' <summary>Blocks until the next event. Nothing means the input is finished and the I/O loop should end.</summary>
    Function GetNextEvent() As ConsoleEvent

    ''' <summary>Queues an event from any thread.</summary>
    Sub PostEvent([event] As ConsoleEvent)

    Sub SetCursorPosition(x As Integer, y As Integer)
    Sub SetCursorVisible(visible As Boolean)

    ''' <summary>Gives the terminal back (normal screen, normal input mode) so that another program can use it, for example while a program runs.</summary>
    Sub Suspend()

    ''' <summary>Takes the terminal over again after <see cref="Suspend"/>; the caller redraws the screen.</summary>
    Sub Reactivate()

  End Interface

End Namespace
