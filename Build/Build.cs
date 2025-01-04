using Electron2D.Core;
using Electron2D.Core.Management;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.PostProcessing;
using Electron2D.Core.Rendering.Shaders;
using System.Drawing;

public class Build : Game
{
    public Build(int _initialWindowWidth, int _initialWindowHeight) : base(_initialWindowWidth, _initialWindowHeight,
        $"Electron2D Build - {Program.BuildDate}", _vsync: false, _antialiasing: false, _physicsPositionIterations: 4, _physicsVelocityIterations: 8,
        _errorCheckingEnabled: true, _enablePostProcessing: true, _showElectronSplashscreen: false)
    { }


    // This is ran when the game is first initialized
    protected override void Initialize()
    {

    }

    ChromaticAberrationPostProcess c;
    // This is ran when the game is ready to load content
    protected override void Load()
    {
        SetBackgroundColor(Color.FromArgb(255, 80, 80, 80));
        Sprite s = new Sprite(Material.Create(GlobalShaders.DefaultTexture,
            ResourceManager.Instance.LoadTexture("Build/Resources/Textures/MossyStone.jpg")), 0, 1920, 1080);
        PostProcessingStack stack = new PostProcessingStack(0);
        c = new ChromaticAberrationPostProcess(1);
        stack.Add(c);
        PostProcessor.Instance.Register(stack);
    }

    // This is ran every frame
    protected override void Update()
    {
        if(Input.GetKey(GLFW.Keys.W))
        {
            c.Intensity += Time.DeltaTime;
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