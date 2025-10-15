# Electron2D
A 2D game engine written in C#.

## Setup
To use Electron2D, follow these steps to prepare the required dependencies and files:

### 1. GLFW
Download a [pre-compiled GLFW binary](https://www.glfw.org/download) and place it in the build directory of your project. The build directory is created when you first build the project, for example: `Electron2DEngine/bin/Debug/net8.0`.

### 2. Steam Networking
To enable Steam networking, you must include the SteamAPI binary:
- Download the standalone version of [Steamworks.NET release 2024.8.0](https://github.com/rlabrecque/Steamworks.NET/releases/tag/2024.8.0).
- Locate the `steam_api64.dll` file in the **Windows-x64** folder of the downloaded package and copy it into the build directory.
- Create a file named `steam_appid.txt` containing only the number **480**, and place it in the build directory.

**Important:** Remove `steam_appid.txt` before releasing your game. This file is only for development purposes as it overrides your application's Steam App ID; leaving it in the build can cause issues when distributing the game.

### 3. Create the Main Function
Create a `Program.cs` (or equivalent) file with a `Main` method, and add `[STAThread]` above the method. This method is the starting point of your application:
```csharp
using Electron2D;

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

### 4. Create a Game Class
Create a class that inherits from `Game`, which will contain your game logic:
```csharp
using Electron2D;

public class MyGame : Game
{
  // This is ran when the game is first initialized
  protected override void Initialize()
  {

  }

  // This is ran when the game is ready to load content
  protected override void Load()
  {

  }


  // This is ran every frame
  protected override void Update()
  {

  }

  // This is ran every frame right before rendering
  protected unsafe override void Render()
  {

  }

  // This is ran when the game is closing
  protected override void OnGameClose()
  {

  }
}
```
Once you have your `Main` method and custom game class set up, your project is ready to run. From here, you can start adding your own game logic and assets.

## Documentation (WIP)
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
This project is licensed under the [MIT](LICENSE.md) License - see the [LICENSE.md](LICENSE.md) file
for details
