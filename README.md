# RayCaster
A ray casting game engine created in MonoGame targeting Windows.

## Building
Should just be a matter of installed MonoGame.  It was built with VS2013 Community.

## Credits
Most of the guts was based on the raycasting tutorial by [Lode Vandevenne](http://lodev.org/cgtutor/).  Judging by the suspiciously familiar code comments every other XNA raycaster I've found seems to be based on the same guide!

Another useful resource is [F. Permadi's tutorial](http://www.permadi.com/tutorial/raycast/index.html).

## Description
This is not really intended to be a complete game, more of tech demo for fun.  At some point I got frustrated by how slow it was and decided to change focus a bit.  I read somewhere that one should "make a game, not an engine."  I can see the wisdom in that since I was adding capabilities Because I Could without any focused motivation.  The result was an abysmally slow engine with features of dubious usefulness.  Is it all that useful to be able to specify unique textures for every side of a sector?  Did I really need to use HSV color at runtime and pay the *horrendous* speed penalty for it?

TODO: Describe current approach once part of it is, you know, implemented
