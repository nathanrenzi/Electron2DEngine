using Electron2D;
using Electron2D.Audio;
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
        AudioInstance instance = new AudioInstance(
            ResourceManager.Instance.LoadAudioClip("Resources/Built-In/Audio/TestAudio2.mp3"),
            1, 1, true);
        instance.Pitch = 1f;
        ReverbEffect r = new ReverbEffect(ReverbFilterSettings.LargeRoom, 1, 40);
        instance.AddEffect(r);
        instance.Play();
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