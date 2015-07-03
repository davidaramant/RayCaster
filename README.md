# RayCaster
A ray casting game engine created in MonoGame targeting Windows.

## Building
Should just be a matter of installed MonoGame 3.4.  It was built with VS2013 Community targetting .NET 4.

## Credits
Most of the guts was based on the raycasting tutorial by [Lode Vandevenne](http://lodev.org/cgtutor/).

Another useful resource is [F. Permadi's tutorial](http://www.permadi.com/tutorial/raycast/index.html).

[The 8Bit Pimps Pixel Mine](https://the8bitpimp.wordpress.com) is also a gold mine of neat snippets and ideas.

Other XNA/MonoGame ray casters I've found:
* https://wolf3dx.codeplex.com
* http://raycasterx.codeplex.com
* https://github.com/JoshuaSmyth/XnaRaycaster

## Description
This is not really intended to be a complete game, more of tech demo for fun.  At some point I got frustrated by how slow it was and decided to change focus a bit.  I read somewhere that one should "make a game, not an engine."  I can see the wisdom in that since I was adding capabilities Because I Could without any focused motivation.  The result was an abysmally slow engine with features of dubious usefulness.  Is it all that useful to be able to specify unique textures for every side of a sector?  Did I really need to use HSV color at runtime and pay the *horrendous* speed penalty for it?

TODO: Describe current approach once part of it is, you know, implemented
