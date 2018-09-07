# QuickVB

A MS-DOS QuickBASIC/QBasic-like IDE for Visual Basic code that's powered by the  [https://github.com/dotnet/roslyn/](.NET Compiler Platform ("Roslyn")).

[AddressOf](http://addressof.com)

(NOTE: The following needs to be updated.)

# Welcome

Welcome to the QuickVB project page! On the 50th anniversary of BASIC, we released a fun sample we'd written on the Visual Basic team - it looks like QuickBasic, but it's a .NET console app that lets you write modern Visual Basic code. QuickVB shows off APIs from the .NET Compiler Platform ("Roslyn"), where we've been reimplementing the Visual Basic compiler in Visual Basic itself.

Since the post, we've seen such a great reaction that we've decided to develop QuickVB further as an open-source project!

There's a lot of work left to be done to turn this sample into an environment where you can really spend some quality time! Some key features that are left: 

* Opening/saving of arbitrary files and projects - pretty important for an editor :)
* Support for navigating around large projects and adding/removing code files
* General perf improvements when working on multi-file programs like the QuickVB solution itself
* QuickBasic compatibility library, to ease porting of classic programs like GORILLA.BAS

# Getting Started

The easiest way to get started is to download the source code from the Source Code tab and then build and run it from Visual Studio 2013. You don’t need any Roslyn Previews installed, as NuGet package restore should pull down the required packages upon build.

Or, just clone the Git repository:

    git clone https://git01.codeplex.com/quickvb

# QuickVB Features

All of the features below are powered by the .NET Compiler Platform ("Roslyn") APIs. To see them light up, click Enable Roslyn on the Options menu.

Semantic code colorization (powered by Roslyn's Classification API):

(insert picture)

Completion lists (powered by Roslyn's Recommendations API):

(insert picture)

Compiler diagnostics (powered by Roslyn's Diagnostics API):

(insert picture)
