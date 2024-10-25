using Electron2D.Core;
using System.Drawing;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.Management;
using System.Numerics;

public class Build : Game
{
    private Tilemap tilemap;

    public Build(int _initialWindowWidth, int _initialWindowHeight) : base(_initialWindowWidth, _initialWindowHeight,
        $"Electron2D Build - {Program.BuildDate}", _vsync: false, _antialiasing: false, _physicsPositionIterations: 4, _physicsVelocityIterations: 8,
        _errorCheckingEnabled: true, _showElectronSplashscreen: true)
    { }


    // This is ran when the game is first initialized
    protected override void Initialize()
    {

    }

    // This is ran when the game is ready to load content
    protected override void Load()
    {
        SetBackgroundColor(Color.FromArgb(255, 80, 80, 80));

        int[] tiles = new int[100];
        Array.Fill(tiles, -1);
        StartCamera.Zoom = 4;
        tilemap = Tilemap.CreateSharedMaterial(Material.Create(GlobalShaders.DefaultTexture,
            ResourceManager.Instance.LoadTexture("Build/Resources/autotile.png")),
            new TileData[] {new TileData("Test", _ruleset: new DefaultTilemapRuleset()).SetAsCollider(new Electron2D.Core.PhysicsBox2D.RigidbodyStaticDef()),
                new TileData("Test2", _ruleset: new DefaultTilemapRuleset()) }, tiles,
            16, 10, 10);
    }

    // This is ran every frame
    protected override void Update()
    {
        if(Input.GetMouseButtonDown(GLFW.MouseButton.Left))
        {
            Vector2 pos = Input.GetMouseWorldPosition();
            if(pos.X <= 160 && pos.X >= 0 && pos.Y <= 160 && pos.Y >= 0)
            {
                tilemap.SetTileID((int)(pos.X / 16), (int)(pos.Y / 16), 0);
            }
        }
        if (Input.GetMouseButtonDown(GLFW.MouseButton.Right))
        {
            Vector2 pos = Input.GetMouseWorldPosition();
            if (pos.X <= 160 && pos.X >= 0 && pos.Y <= 160 && pos.Y >= 0)
            {
                tilemap.SetTileID((int)(pos.X / 16), (int)(pos.Y / 16), 1);
            }
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