using Electron2D.Core;
using Electron2D.Core.Management;
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
    ColorGradingPostProcess c;
    SliderSimple[] sliders = new SliderSimple[5];
    protected override void Load()
    {
        SetBackgroundColor(Color.FromArgb(255, 80, 80, 80));
        Sprite s = new Sprite(Material.Create(GlobalShaders.DefaultTexture,
            ResourceManager.Instance.LoadTexture("Build/Resources/Textures/MossyStone.jpg")), 0, 1920, 1080);
        PostProcessingStack stack = new PostProcessingStack(0);
        c = new ColorGradingPostProcess(Color.White, 0f, 0f, 0f, 0f, 0f);
        stack.Add(c);
        PostProcessor.Instance.Register(stack);

        for(int i = 0; i < sliders.Length; i++)
        {
            sliders[i] = new SliderSimple(Color.Black, Color.Red, Color.White,
                0, -1, i == 1 ? 4 : 1, 300, 20, 15, 30, 15, _ignorePostProcessing: false);
            sliders[i].Transform.Position = new System.Numerics.Vector2(0, (i * 50) - 480);
        }
    }

    // This is ran every frame
    protected override void Update()
    {
        c.HueShift = sliders[0].Value;
        c.Saturation = sliders[1].Value;
        c.Brightness = sliders[2].Value;
        c.Contrast = sliders[3].Value;
        c.Temperature = sliders[4].Value;
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