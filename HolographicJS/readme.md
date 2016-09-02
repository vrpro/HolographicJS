# HolographicJS

Because I couldn't think of a less generic name.

## Purpose

The purpose of HolographicJS is to provide a familiar environment for web developers who are interested in holographic development for Microsoft HoloLens.


## Approach

This project is very experimental, and as such I've taken some somewhat novel approaches to some difficult problems. Nothing provided here is recommended for
production environments at this point.

HolographicJS is built atop large open source efforts, including:

- [ANGLE](https://github.com/google/angle) by Google (and [adapted by Microsoft](https://github.com/microsoft/angle/tree/ms-holographic-experimental) for our
purposes)
- OpenGL by Khronos Group.
- [OpenGL.Net](https://github.com/luca-piccioni/OpenGL.Net) by [luca-piccioni](https://github.com/luca-piccioni)
- [JsBridge](https://github.com/lzyzsd/JsBridge) -- The ChakraBridge implementation is heavily borrowed from [@lzyzsd](https://github.com/lzyzsd)'s project  
which is an excellent demonstration of how to use ChakraCore in C#.

The goal of this project is to achieve rendering HoloLens holograms using Javascript with minimal or no adjustments to existing projects. Where required,
HoloLens specific solutions should be employed. One example of this is Microsoft's `ms-holographic-experimental` branch on their ANGLE GitHub page -- it
simply wouldn't make sense to try and fit that into this project.

## Instructions

Aquiring the necessary pieces to build this project is a bit of a task. You'll need a few things:

- Microsoft's HoloLens ANGLE branch: https://github.com/microsoft/angle/tree/ms-holographic-experimental
- My fork of OpenGL.Net which supports UWP applications: https://github.com/lwansbrough/OpenGL.Net

Follow the build instructions in those repositories.

Once you've built those projects, their outputs will be available to the HolographicJS project. 

The following DLLs are required by the HolographicJS project:

- `libEGL.dll`
- `libGLESv2.dll`
- `ChakraCore.dll`
- `OpenGL.Net.dll`

Provided you've managed to follow along thus far, you should be able to build the HolographicJS project. This project is a UWP and must be run using the
HoloLens emulator or device in order to utilize specific APIs required for generating holographic projections.

## More to follow...