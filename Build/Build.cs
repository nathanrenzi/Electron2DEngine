using Electron2D;
using Electron2D.Core;
using Electron2D.Core.Management;
using Electron2D.Core.Misc;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.PostProcessing;
using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.UserInterface;
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

    // This is ran when the game is ready to load content
    GaussianBlurPostProcess c;
    Slider[] sliders = new Slider[2];
    protected override void Load()
    {
        SetBackgroundColor(Color.FromArgb(255, 80, 80, 80));
        Sprite s = new Sprite(Material.Create(GlobalShaders.DefaultTexture,
            ResourceManager.Instance.LoadTexture("Build/Resources/Textures/MossyStone.jpg")), 0, 1920, 1080);
        PostProcessingStack stack = new PostProcessingStack(0);
        c = new GaussianBlurPostProcess(1, 0.7f, 32);
        stack.Add(c);
        PostProcessor.Instance.Register(stack);

        SlicedPanelDef slicedDef1 = new SlicedPanelDef(0.5f, 0.5f, 0.5f, 0.5f, 15);
        SlicedPanelDef slicedDef2 = new SlicedPanelDef(0.5f, 0.5f, 0.5f, 0.5f, 7.5f);

        SliderDef def1 = new SliderDef(
            Material.CreateCircle(Color.Black),
            Material.CreateCircle(Color.Red),
            Material.CreateCircle(Color.White),
            300, 30, 15, 30, slicedDef1, slicedDef2, slicedDef1,
            0, 50, 0, 15);

        for (int i = 0; i < sliders.Length; i++)
        {
            sliders[i] = new Slider(def1, _ignorePostProcessing: true);
            sliders[i].Transform.Position = new System.Numerics.Vector2(0, (i * 50) - 480);
        }
        sliders[1].MinValue = 0.1f;
        sliders[1].MaxValue = 8f;
        sliders[1].Value = 0.1f;
    }

    // This is ran every frame
    protected override void Update()
    {
        c.BlurRadius = sliders[0].Value;
        c.Sigma = sliders[1].Value;
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