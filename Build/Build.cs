using Electron2D.Core;
using Electron2D.Core.Management;
using Electron2D.Core.Rendering;
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
        StartCamera.Zoom = 5;
        TileData[] data = new TileData[]
        {
            new TileData() {Material = Material.Create(GlobalShaders.DefaultTexture, ResourceManager.Instance.LoadTexture("Build/Resources/testimage.jpg")), Name = "Test"},
            new TileData() {Material = Material.Create(GlobalShaders.DefaultTexture), Name = "Test2"}
        };

        int[] tiles = new int[]
        {
            0,0,1,0,-1,1,1,1,0
        };

        Tilemap tilemap = new Tilemap(data, tiles, 16, 3, 3);
        tilemap.GetComponent<Transform>().Position = new System.Numerics.Vector2(16 * -5, 16 * -5);
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