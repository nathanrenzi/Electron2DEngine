using Electron2D.Core;
using System.Drawing;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;
using System.Numerics;
using Electron2D.Core.PhysicsBox2D;

public class Build : Game
{
    // This is ran when the game is first initialized
    protected override void Initialize()
    {

    }

    // This is ran when the game is ready to load content
    protected override void Load()
    {
        SetBackgroundColor(Color.FromArgb(255, 80, 80, 80));

        Sprite s = new Sprite(Material.Create(GlobalShaders.DefaultTexture, Color.Red));
    }

    // This is ran every frame
    protected override void Update()
    {
        if(Input.GetKeyDown(GLFW.Keys.Alpha1))
        {
            Display.SetWindowMode(WindowMode.Fullscreen);
        }
        else if(Input.GetKeyDown(GLFW.Keys.Alpha2))
        {
            Display.SetWindowMode(WindowMode.Windowed);
        }
        else if(Input.GetKeyDown(GLFW.Keys.Alpha3))
        {
            Display.SetWindowMode(WindowMode.BorderlessWindow);
        }
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