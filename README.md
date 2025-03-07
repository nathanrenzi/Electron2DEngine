# Electron2D

A 2D game engine written in C#. This project is a self-study meant to give me more experience with large projects and to gain a better understanding of how game engines work.

## Setup

In order to download and use this respository, a [GLFW pre-compiled binary](https://www.glfw.org/download) must be placed in the build directory (ex. Electron2DEngine/bin/Debug/net6.0).
In addition to this, Steamworks (if Steam networking is going to be used) requires a **steam_api64.dll** located within the standalone zip of this Steamworks.NET release in the Windows-x64 folder [Steamworks.NET Release](https://github.com/rlabrecque/Steamworks.NET/releases/tag/2024.8.0), and this must be placed in the build directory of the project.

## Author

  - [**Nathan Renzi**](https://github.com/nathanrenzi)

## Built With

  - [GL.cs](https://gist.githubusercontent.com/dcronqvist/8e0c594532748e8fc21133ac6e3e8514/raw/89a0bcbdbd9692790f95fd60143980482a12d817/GL.cs) - OpenGL C# bindings
  - [GLFW](https://www.glfw.org/) - Input and window management
  - [GLFW.NET](https://github.com/ForeverZer0/glfw-net) - GLFW C# bindings
  - [Box2D.NetStandard](https://github.com/codingben/box2d-netstandard/tree/v2.4) - 2D physics backend
  - [FreeTypeSharp](https://github.com/ryancheung/FreeTypeSharp) - Font rasterization
  - [NAudio](https://github.com/naudio/NAudio) - Audio backend
  - [RiptideNetworking](https://github.com/RiptideNetworking/Riptide) - Networking backend
  - [DotnetNoise](https://github.com/cmsommer/DotnetNoise) - Noise generation

## Licensing

This project is licensed under the [MIT](LICENSE.md) License - see the [LICENSE.md](LICENSE.md) file
for details
