Brazil
======

Game framework and 3D editor built in XNA 4.0.  Compatible with Win 8.1 providing Urho3D output.


Introduction
------------

Brazil is a project started in 2012 and was born from an XNA test application called Friendlier.
Friendlier was an attempt to build a 3D text editor and IDE in XNA/3D with some nice effects and a
pretty look and feel - it succeeded to do that but then Brazil was the re-imagining of the internals
of what it would take to make Friendlier work as a game development engine.   So Brazil was a rewrite
of some of core components of Friendlier - and then Friendlier rewritten in Brazil to take advantage
of it but also to allow it to build further Brazil apps within the Friendlier environment.

Brazil runs in two modes, inside the editor and in standalone mode.  Friendlier helps you build a game (say)
in much the same way that Unity3D (http://www.unity3d.com) does - but much simpler with a fraction of the
functionality.  You can then publish the game as an exe in Windows or also publish to Urho3D (a game library)
and through this to many other platforms including mobile.

Current Status
--------------

Brazil has not seen any development since mid 2013 but the code compiles cleanly in Visual Studio 2012 with 
XNA 4.0 and runs up the Friendlier editor and sample game.  At the point the last development was abandoned the
Urho3D scripted interface was starting to work i.e. we'd got Urho3D script output which could then compile into
an Android app.

Recommended next steps
----------------------

- Replace XNA with monogame (content pipeline potential gotchas)
- Xamarin/monodevelop port
- heap of more graphics components and effects need adding
- optimisation of the graphics routines
- further integration of third party libraries such as physics, sound
- refinement of Urho output or investigating alternatives for multiplatform output

Requirements and Installation
-----------------------------

- Visual Studio 2012 (tested with Ultimate but I'm sure other versions will work)
- XNA 4.0 Refresh (Visual Studio 2012) - https://msxna.codeplex.com/releases/view/117564

XNA 4.0 Refresh contains everything to you need to install to build Brazil and Friendlier.  Once these two
components are installed you can git clone:

$ git clone https://github.com/bownie/Brazil.git

and build the Friendlier project to compile all the dependencies and run up the editor.  For more information
on how Friendlier works you can see the pages here:

http://www.xyglo.com/friendlier

Contact and Licence
-------------------
Brazil and Friendlier are made available under the MIT Licence copyright (c) 2012 - 2014 Richard Bown.
See the LICENCE.txt for more details.

If you've got any questions then drop me a line at rich@xyglo.com or @rwbown on twitter.
