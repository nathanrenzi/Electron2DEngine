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
        TextFieldDef def = new TextFieldDef(2, 300, 200, new Vector4(0, 0, 0, 0),
            ResourceManager.Instance.LoadFont("Resources/Built-In/Fonts/Roboto-Regular.ttf", 30, 0),
            textMaterial, Color.White, Color.FromArgb(80, 255, 255, 255), backgroundMaterial, "", "", 100, 2, textAlignmentMode: TextAlignmentMode.Baseline,
            textHorizontalAlignment: TextAlignment.Center, textVerticalAlignment: TextAlignment.Center, textOverflowMode: TextOverflowMode.Word, waitForEnterKey: true);
        //field = new TextField(def);
        instance = AudioSystem.CreateInstance(ResourceManager.Instance.LoadAudioClip("Resources/Built-In/Audio/Electron2DRiff.mp3"));
        Panel panel = new Panel(Color.Red, -10, 300, 200);
        panel.Anchor = new Vector2(-1, -1);
        panel.Transform.Position = new Vector2(-1920 / 2 + 100, -1080 / 2 + 100);
        panel.Interactable = false;
        panel.SetLayoutGroup(new ListLayoutGroup(new Vector4(5, 5, 5, 5), 5, ListDirection.Vertical));
        TextField startServer = new TextField(def);
        startServer.Text = "Start Server";
        panel.ChildLayoutGroup.AddToLayout(startServer);
        panel.ChildLayoutGroup.AddToLayout(new Panel(Color.Transparent));
        TextField ipField = new TextField(def);
        ipField.PromptText = "Enter IP Address...";
        panel.ChildLayoutGroup.AddToLayout(ipField);
        TextField connectToServer = new TextField(def);
        connectToServer.Text = "Connect";
        panel.ChildLayoutGroup.AddToLayout(connectToServer);
        startServer.OnClickDown += instance.Play;
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