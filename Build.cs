using Electron2D;
using Electron2D.Networking;
using Electron2D.Networking.Examples;
using System.Drawing;

public class Build : Game
{
    // This is ran when the game is first initialized
    protected override void Initialize()
    {
        ExampleNetworkGameClass.SetRegisterID(NetworkManager.RegisterNetworkGameClass(ExampleNetworkGameClass.FactoryMethod));
    }

    // This is ran when the game is ready to load content
    ExampleNetworkGameClass gameClass;
    protected override void Load()
    {
        SetBackgroundColor(Color.FromArgb(255, 80, 80, 80));
        NetworkManager.Instance.Initialize();
        NetworkManager.Instance.Server.SetAllowNonHostOwnership(true);
        try
        {
            NetworkManager.Instance.Server.Start(25565, 2);
        }
        catch
        {
            
        }
        NetworkManager.Instance.Client.Connect("127.0.0.1", 25565);
        NetworkManager.Instance.Client.ConnectionSuccess += CreateObject;
    }

    private void CreateObject()
    {
        if (NetworkManager.Instance.Client.IsConnected && NetworkManager.Instance.Server.IsRunning)
        {
            gameClass = new ExampleNetworkGameClass();
            gameClass.Spawn("test");
            gameClass.Value = 10;
        }
    }

    // This is ran every frame
    protected override void Update()
    {
        if(gameClass != null)
        {
            if(Input.GetKeyDown(GLFW.Keys.K))
            {
                gameClass.Value += 1;
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