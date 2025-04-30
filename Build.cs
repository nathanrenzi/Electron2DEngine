using Electron2D;
using Electron2D.Audio;
using Electron2D.Networking;
using Electron2D.Rendering;
using Riptide.Transports.Steam;
using Steamworks;
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
        bool isServer = false;
        SetBackgroundColor(isServer ? Color.FromArgb(255, 80, 80, 80) : Color.FromArgb(255, 80, 80, 255));
        NetworkManager.Instance.InitializeForNetwork();
        if(isServer) NetworkManager.Instance.Server.Start(10, 25565, "test!");
        if(NetworkManager.Instance.NetworkMode == NetworkMode.SteamP2P)
        {
            if (isServer)
            {
                NetworkManager.Instance.Client.Connect("127.0.0.1", 25565, "test!");
            }
        }
        else
        {
            NetworkManager.Instance.Client.Connect("127.0.0.1", 25565, "test!");
        }
        if(isServer) NetworkManager.Instance.Client.NetworkGameClassesLoaded += () => InitializeNetworkTest();
    }

    private NetworkAudioInstance instance;
    private void InitializeNetworkTest()
    {
        instance = new NetworkAudioInstance(ResourceManager.Instance.LoadAudioClip("Resources/Electron2D/Audio/TestAudio3.mp3"), 1f, 1, true, true);
        instance.Spawn();
        //instance.OnNetworkInitializedEvent += () =>
        //{
        //    Transform transform = new Transform();
        //    transform.Position = new Vector2(-500, 0);
        //    NetworkTransform networkTransform = new NetworkTransform(transform);
        //    networkTransform.Spawn("Transform");
        //    networkTransform.OnNetworkInitializedEvent += () =>
        //    {
        //        instance.Spatialize(networkTransform, true);
        //        instance.Play();
        //    };
        //};
        instance.Play();
    }

    // This is ran every frame
    protected override void Update()
    {
        if(instance != null && instance.IsOwner)
        {
            if(Input.GetKeyDown(GLFW.Keys.Space))
            {
                instance.Play();
            }
            else if(Input.GetKeyDown(GLFW.Keys.P))
            {
                if(instance.PlaybackState == PlaybackState.Playing) instance.Pause();
                else if(instance.PlaybackState == PlaybackState.Paused) instance.Unpause();
            }
            else if (Input.GetKeyDown(GLFW.Keys.S))
            {
                instance.Stop();
            }
        }

        if(Input.GetKeyDown(GLFW.Keys.M))
        {
            AudioSystem.MasterVolume = AudioSystem.MasterVolume == 1 ? 0 : 1;
            Debug.Log("Volume: " + AudioSystem.MasterVolume);
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