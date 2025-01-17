using Electron2D;
using Electron2D.Core;
using Electron2D.Core.Management;
using Electron2D.Core.Misc;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.PostProcessing;
using Electron2D.Core.Rendering.Shaders;
using System.Drawing;

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