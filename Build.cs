using Electron2D;
using Electron2D.Audio;
using Electron2D.Rendering;
using Electron2D.Rendering.Shaders;
using Electron2D.Rendering.Text;
using Electron2D.UserInterface;
using System.Drawing;
using System.Numerics;

public class Build : Game
{
    // This is ran when the game is first initialized
    protected override void Initialize()
    {

    }

    // This is ran when the game is ready to load content
    TextField field;
    AudioInstance instance;
    protected override void Load()
    {
        SetBackgroundColor(Color.FromArgb(255, 80, 80, 80));
        Material textMaterial = Material.Create(new Shader(Shader.ParseShader("Resources/Built-In/Shaders/DefaultText.glsl")), Color.White);
        Material backgroundMaterial = Material.Create(new Shader(Shader.ParseShader("Resources/Built-In/Shaders/DefaultInterface.glsl")), Color.Gray);
        TextFieldDef def = new TextFieldDef(2, 300, 50, new Vector4(10, 10, 0, 15),
            ResourceManager.Instance.LoadFont("Resources/Built-In/Fonts/Roboto-Regular.ttf", 30, 0),
            textMaterial, Color.White, Color.FromArgb(80, 255, 255, 255), backgroundMaterial, "Test!", "Test!", 24, textAlignmentMode: TextAlignmentMode.Baseline,
            textHorizontalAlignment: TextAlignment.Left, textVerticalAlignment: TextAlignment.Bottom, textOverflowMode: TextOverflowMode.Disabled, waitForEnterKey: true);
        field = new TextField(def);
        instance = AudioSystem.CreateInstance(ResourceManager.Instance.LoadAudioClip("Resources/Built-In/Audio/Electron2DRiff.mp3"));
        field.OnTextEntered += (s) => instance.Play();
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