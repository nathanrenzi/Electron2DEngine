﻿using Electron2D;
using Electron2D.Core;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.PostProcessing;
using Electron2D.Core.Rendering.Shaders;
using System.Drawing;

public class Build : Game
{
    public Build(int _initialWindowWidth, int _initialWindowHeight) : base(_initialWindowWidth, _initialWindowHeight,
        $"Electron2D Build - {Program.BuildDate}", _vsync: false, _antialiasing: false, _physicsPositionIterations: 4, _physicsVelocityIterations: 8,
        _errorCheckingEnabled: true, _showElectronSplashscreen: false)
    { }


    // This is ran when the game is first initialized
    protected override void Initialize()
    {

    }

    // This is ran when the game is ready to load content
    protected override void Load()
    {
        SetBackgroundColor(Color.FromArgb(255, 80, 80, 80));
        // Forcing the post processor to initialize
        // This should be moved to Game.cs with a parameter in the constructor to enable or disable it
        PostProcessor.Instance.Initialize();
        Sprite s = new Sprite(Material.Create(GlobalShaders.DefaultTexture, Color.Red), _sizeX: 100, _sizeY: 100);
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