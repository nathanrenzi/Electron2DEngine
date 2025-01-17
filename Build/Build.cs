using Electron2D.Core;
using System.Drawing;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;
using Electron2D;
using Electron2D.Core.Management;

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
        if (Input.GetMouseButtonDown(GLFW.MouseButton.Left))
        {
            Cursor.SetType(GLFW.CursorType.Arrow);
        }
        if (Input.GetMouseButtonDown(GLFW.MouseButton.Right))
        {
            Cursor.SetType(GLFW.CursorType.Hand);
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