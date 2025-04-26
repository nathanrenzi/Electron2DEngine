using Electron2D;
using Electron2D.Networking;
using Electron2D.Rendering;
using System.Drawing;
using System.Numerics;

public class Build : Game
{
    // This is ran when the game is first initialized
    protected override void Initialize()
    {
        
    }

    // This is ran when the game is ready to load content
    NetworkTransform networkTransform;
    Sprite s;
    protected override void Load()
    {
        bool isServer = false;
        SetBackgroundColor(isServer ? Color.FromArgb(255, 80, 80, 80) : Color.FromArgb(255, 80, 80, 255));
        NetworkManager.Instance.InitializeForNetwork();
        if(isServer) NetworkManager.Instance.Server.Start(2, 25565, "test!");
        if(NetworkManager.Instance.NetworkMode == NetworkMode.SteamP2P)
        {
            NetworkManager.Instance.Client.Connect(isServer ? "127.0.0.1" : "steamid", 25565, "test!");
        }
        else
        {
            NetworkManager.Instance.Client.Connect("127.0.0.1", 25565, "test!");
        }
        if(isServer)
        {
            NetworkValueSettings networkValueSettings = new NetworkValueSettings(true, 0.05f, Riptide.MessageSendMode.Unreliable, true);
            networkTransform = new NetworkTransform(new Transform(), networkValueSettings, networkValueSettings, networkValueSettings);
            NetworkManager.Instance.Client.NetworkGameClassesLoaded += () => networkTransform.Spawn("test");
            networkTransform.OnNetworkInitializedEvent += () => s = new Sprite(Material.Create(Color.Red), transform: networkTransform.Transform);
        }
    }

    // This is ran every frame
    bool found = false;
    protected override void Update()
    {
        if (networkTransform != null)
        {
            networkTransform.Position = Input.GetOffsetMousePosition();
            networkTransform.Scale += Vector2.One * Input.ScrollDelta * 50;
            if(Input.GetKey(GLFW.Keys.Right))
            {
                networkTransform.Rotation += Time.DeltaTime * 10;
            }
        }
        else
        {
            if (found) return;
            NetworkTransform transform = (NetworkTransform)NetworkManager.Instance.Client.GetNetworkGameClass("test");
            if(transform != null)
            {
                Sprite s = new Sprite(Material.Create(Color.Red), transform: transform.Transform, useCustomTransformScale: true);
                found = true;
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