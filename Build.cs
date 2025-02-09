using Electron2D;
using Electron2D.Networking;
using Electron2D.Networking.Examples;
using System.Drawing;

public class Build : Game
{
    // This is ran when the game is first initialized
    protected override void Initialize()
    {
        ExampleNetworkGameClassPosition.SetRegisterID(NetworkManager.RegisterNetworkGameClass(ExampleNetworkGameClassPosition.FactoryMethod));
    }

    // This is ran when the game is ready to load content
    protected override void Load()
    {
        SetBackgroundColor(Color.FromArgb(255, 80, 80, 80));
        NetworkManager.Instance.InitializeForNetwork();
        NetworkManager.Instance.Server.Start(2, 25565, "test!");
        NetworkManager.Instance.Client.Connect("127.0.0.1", 25565, "test!");
        NetworkManager.Instance.Client.NetworkGameClassesLoaded += () => new ExampleNetworkGameClassPosition().Spawn("test2");
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