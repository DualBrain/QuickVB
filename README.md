# QuickVB

A MS-DOS QuickBASIC/QBasic-like IDE for Visual Basic code that's powered by the [.NET Compiler Platform ("Roslyn")](https://github.com/dotnet/roslyn/).

## Welcome

Welcome to the QuickVB project page! 

On the 50th anniversary of BASIC, the members of the Microsoft Visual Basic team released a fun sample - it looks like QuickBASIC/QBasic, but it's a .NET console app that lets you write modern Visual Basic code. QuickVB shows off several APIs from the [.NET Compiler Platform ("Roslyn")](https://github.com/dotnet/roslyn/), which is where they've reimplemented the Visual Basic compiler in Visual Basic itself.

Since the original post, they had enough feedback that they decided to release QuickVB as an open-source project! (This was done using CodePlex - which has since been "shutdown".)

There's a lot of work left to be done to turn this sample into an environment (IDE) where you can really spend some quality time! Some key features that are left: 

* Opening/saving of arbitrary files and projects - pretty important for an editor (WIP)
* Support for navigating around large projects and adding/removing code files
* General perf improvements when working on multi-file programs like the QuickVB solution itself
* QuickBasic compatibility library, to ease porting of classic programs like GORILLA.BAS (WIP)

This is where this GitHub project comes in; it serves to re-ignite and continue this effort...

* Move to latest release of "Roslyn"
* Move to newer version of .NET (currently set to 4.5)
* Migrate from being a "demo" to a full-featured (for a Console app) IDE
* Match menus and other UI elements to original QuickBASIC IDE (WIP)
* Improve ConsoleGUI to support "popup" windows; needed for additional functionality
* Improve ConsoleGUI to support activation of additional panels (help)
* Integrate "help" with online Microsoft documentation
* Enable "Roslyn" by default
* Actually port GORILLA.BAS as an included sample
* Remove "self-editing".

Currently it is necessary (due to the "old" version of "Roslyn"), it may be necessary to install the following in order to get running:

* [Microsoft Build Tools 2013](https://www.microsoft.com/en-US/download/details.aspx?id=40760) - required by the v0.7 version of "Roslyn".

## QuickVB Features

All of the features below are powered by the [.NET Compiler Platform ("Roslyn")](https://github.com/dotnet/roslyn/) APIs. To see them light up, click *Enable Roslyn* on the *Options* menu.

Semantic code colorization (powered by Roslyn's Classification API):

(insert picture)

Completion lists (powered by Roslyn's Recommendations API):

(insert picture)

Compiler diagnostics (powered by Roslyn's Diagnostics API):

(insert picture)

## Acknowledgements

Many thanks to the authors of the 2014 release:

* Alex Turner
* Ian Halliday
