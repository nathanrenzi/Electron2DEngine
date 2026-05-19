# Atlas2D

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![Platform](https://img.shields.io/badge/platform-Windows--x64-lightgrey)

A 2D game engine written in C#.

## Features

- 2D rigidbody physics powered by Box2D
- Built-in multiplayer via Riptide & Steam integration
- TrueType font rendering with FreeType
- Audio playback through NAudio
- OpenGL-based rendering pipeline

## Getting Started

### Installation

Atlas2D is currently used as a Git submodule. From the root of your game's repository:

```bash
git submodule add https://github.com/nathanrenzi/Atlas2D.git
git submodule update --init --recursive
```

Then reference the engine project from your game's `.csproj`:

```xml
<ItemGroup>
    <ProjectReference Include="Atlas2D\Atlas2D.csproj" />
</ItemGroup>
```

Adjust the path if you placed the submodule somewhere other than the repo root.

### Requirements

- .NET 8.0
- Windows x64

### 1. Create the Main Function

Create a `Program.cs` (or equivalent) file with a `Main` method, and add `[STAThread]` above the method. This method is the entry point of your application:

```csharp
public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Instantiate your custom game class here
        MyGame game = new MyGame();
        game.Run();
    }
}
```

### 2. Create a Game Class

Create a class that inherits from `Game`, which will contain your game logic:

```csharp
using Atlas2D;

public class MyGame : Game
{
    // This runs when the game is first initialized
    protected override void Initialize() { }

    // This runs when the game is ready to load content
    protected override void Load() { }

    // This runs every frame
    protected override void Update() { }

    // This runs every frame right before rendering
    protected override void Render() { }

    // This runs when the game is closing
    protected override void OnGameClose() { }
}
```

Once you have your `Main` method and custom game class set up, your project is ready to run. From here, you can start adding your own game logic and assets.

### 3. (Optional) Steam Networking Setup

If you want to use Steam features, you'll need to include the SteamAPI binary:

- Download the standalone version of [Steamworks.NET release 2024.8.0](https://github.com/rlabrecque/Steamworks.NET/releases/tag/2024.8.0).
- Locate the `steam_api64.dll` file in the **Windows-x64** folder of the downloaded package and copy it into the root of your game project.
- Add the following to your game project's `.csproj` to copy it to the build output:

```xml
<ItemGroup>
    <Content Include="steam_api64.dll">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
</ItemGroup>
```

- Create a file named `steam_appid.txt` containing only the number **480**, and place it in the build directory.

> ⚠️ **Important:** Remove `steam_appid.txt` before releasing your game. This file is only for development purposes as it overrides your application's Steam App ID; leaving it in the build can cause issues when distributing the game.

## Documentation (Work in Progress)

Full tutorials and guides for using the engine are coming soon.

## Built With

- [GL.cs](https://gist.githubusercontent.com/dcronqvist/8e0c594532748e8fc21133ac6e3e8514/raw/89a0bcbdbd9692790f95fd60143980482a12d817/GL.cs) - OpenGL C# bindings
- [GLFW](https://www.glfw.org/) - Input and window management
- [GLFW.NET](https://github.com/ForeverZer0/glfw-net) - C# wrapper for GLFW
- [Box2D.NetStandard](https://github.com/codingben/box2d-netstandard/tree/v2.4) - 2D physics backend
- [FreeTypeSharp](https://github.com/ryancheung/FreeTypeSharp) - Font rasterization
- [NAudio](https://github.com/naudio/NAudio) - Audio backend
- [RiptideNetworking](https://github.com/RiptideNetworking/Riptide) - Networking backend
- [Steamworks.NET](https://steamworks.github.io/) - C# wrapper for Steamworks API
- [DotnetNoise](https://github.com/cmsommer/DotnetNoise) - Noise generation

## Author

- [**Nathan Renzi**](https://github.com/nathanrenzi)

## Licensing

This project is licensed under the [MIT License](LICENSE.md).