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
    protected override void Load()
    {
        SetBackgroundColor(Color.FromArgb(255, 80, 80, 80));
        AudioInstance instance = AudioSystem.CreateInstance(ResourceManager.Instance.LoadAudioClip("Resources/Built-In/Audio/Electron2DRiff.mp3"));

        Panel panel = new Panel(Color.Red, -10, 300, 200);
        panel.Anchor = new Vector2(-1, -1);
        panel.Transform.Position = new Vector2(-1920 / 2 + 100, -1080 / 2 + 100);
        panel.Interactable = false;
        panel.SetLayoutGroup(new ListLayoutGroup(new Vector4(5, 5, 5, 5), 5, ListDirection.Vertical));

        Material textMaterial = Material.Create(new Shader(Shader.ParseShader("Resources/Built-In/Shaders/DefaultText.glsl")), Color.White);
        Material backgroundMaterial = Material.Create(new Shader(Shader.ParseShader("Resources/Built-In/Shaders/DefaultInterface.glsl")), Color.FromArgb(150, 150, 100));

        TextLabelDef labelDef = new TextLabelDef("", 300, 200, ResourceManager.Instance.LoadFont("Resources/Built-In/Fonts/Roboto-Regular.ttf", 30, 0), textMaterial, Color.White,
            TextAlignment.Center, TextAlignment.Center, TextAlignmentMode.Baseline, TextOverflowMode.Disabled);

        TextLabel startServerTextLabel = new TextLabel(labelDef);
        startServerTextLabel.Text = "Start Server";
        startServerTextLabel.Interactable = false;
        SlicedPanel startServerPanel = new SlicedPanel(new SlicedPanelDef(0.5f, 0.5f, 0.5f, 0.5f, 10), Material.CreateCircle(Color.FromArgb(150, 150, 100), true), 100, 100, -1);
        startServerPanel.SetLayoutGroup(new ContainLayoutGroup(Vector4.Zero));
        startServerPanel.ChildLayoutGroup.AddToLayout(startServerTextLabel);
        startServerPanel.OnClickDown += instance.Play;
        startServerPanel.SetHoverCursorType(GLFW.CursorType.Hand);
        panel.ChildLayoutGroup.AddToLayout(startServerPanel);

        panel.ChildLayoutGroup.AddToLayout(new Panel(Color.Transparent));

        TextFieldDef fieldDef = new TextFieldDef(2, 300, 200, new Vector4(0, 0, 0, 0),
            ResourceManager.Instance.LoadFont("Resources/Built-In/Fonts/Roboto-Regular.ttf", 30, 0),
            textMaterial, Color.White, Color.FromArgb(50, 255, 255, 255), Material.CreateCircle(Color.FromArgb(150, 150, 100), true), "", "", 100, 2, textAlignmentMode: TextAlignmentMode.Baseline,
            textHorizontalAlignment: TextAlignment.Center, textVerticalAlignment: TextAlignment.Center, textOverflowMode: TextOverflowMode.Word, waitForEnterKey: true,
            backgroundPanelDef: new SlicedPanelDef(0.5f, 0.5f, 0.5f, 0.5f, 10));
        TextField ipField = new TextField(fieldDef);
        ipField.PromptText = "Enter IP Address...";
        panel.ChildLayoutGroup.AddToLayout(ipField);

        TextLabel connectTextLabel = new TextLabel(labelDef);
        connectTextLabel.Text = "Connect";
        connectTextLabel.Interactable = false;
        SlicedPanel connectPanel = new SlicedPanel(new SlicedPanelDef(0.5f, 0.5f, 0.5f, 0.5f, 10), Material.CreateCircle(Color.FromArgb(150, 150, 100), true), 100, 100, -1);
        connectPanel.SetLayoutGroup(new ContainLayoutGroup(Vector4.Zero));
        connectPanel.ChildLayoutGroup.AddToLayout(connectTextLabel);
        connectPanel.OnClickDown += instance.Play;
        connectPanel.SetHoverCursorType(GLFW.CursorType.Hand);
        panel.ChildLayoutGroup.AddToLayout(connectPanel);
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