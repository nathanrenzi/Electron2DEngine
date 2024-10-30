using Electron2D.Core;
using System.Drawing;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;
using System.Numerics;
using Electron2D.Core.PhysicsBox2D;
using Electron2D;
using Box2D.NetStandard.Dynamics.Joints;

public class Build : Game
{
    private Sprite s;

    public Build(int _initialWindowWidth, int _initialWindowHeight) : base(_initialWindowWidth, _initialWindowHeight,
        $"Electron2D Build - {Program.BuildDate}", _vsync: false, _antialiasing: false, _physicsPositionIterations: 4, _physicsVelocityIterations: 8,
        _errorCheckingEnabled: true, _showElectronSplashscreen: true)
    { }


    // This is ran when the game is first initialized
    protected override void Initialize()
    {

    }

    // This is ran when the game is ready to load content
    protected override void Load()
    {
        SetBackgroundColor(Color.FromArgb(255, 80, 80, 80));

        s = new Sprite(Material.Create(GlobalShaders.DefaultTexture, Color.Red), 0, 20, 20);

        Sprite entity = new Sprite(Material.Create(GlobalShaders.DefaultTexture), 0, 40, 40);
        entity.GetComponent<Transform>().Position = new Vector2(0, 100);
        entity.GetComponent<Transform>().Scale = new Vector2(40, 40);
        entity.AddComponent(Rigidbody.CreateKinematic(
            new RigidbodyKinematicDef()
            {
                Shape = RigidbodyShape.Box
            }));

        Sprite entity2 = new Sprite(Material.Create(GlobalShaders.DefaultTexture), 0, 40, 40);
        entity2.GetComponent<Transform>().Position = Vector2.Zero;
        entity2.GetComponent<Transform>().Scale = new Vector2(40, 40);
        entity2.AddComponent(Rigidbody.CreateDynamic(
            new RigidbodyDynamicDef()
            {
                Shape = RigidbodyShape.Box,
                MassData = new Box2D.NetStandard.Collision.Shapes.MassData() { mass = 1 },
            }));

        entity2.GetComponent<Rigidbody>().CreateJoint(new RigidbodyDistanceJointDef()
        {
            RigidbodyA = entity.GetComponent<Rigidbody>(),
            RigidbodyB = entity2.GetComponent<Rigidbody>(),
            Length = 50
        });
    }

    // This is ran every frame
    protected override void Update()
    {
        if(Input.GetMouseButtonDown(GLFW.MouseButton.Left))
        {
            Sprite entity = new Sprite(Material.Create(GlobalShaders.DefaultTexture), 0, 40, 40);
            entity.GetComponent<Transform>().Position = Input.GetMouseWorldPosition();
            entity.GetComponent<Transform>().Scale = new Vector2(40, 40);
            entity.AddComponent(Rigidbody.CreateDynamic(
                new RigidbodyDynamicDef()
                {
                    Shape = RigidbodyShape.Box,
                    MassData = new Box2D.NetStandard.Collision.Shapes.MassData() { mass = 1 },
                }));
        }

        RaycastHit hit;
        Physics.Raycast(new Vector2(-1920 / 2, 0), new Vector2(1, 0), 1920, out hit);
        if(hit.Hit)
        {
            s.Transform.Position = hit.Point;
        }
        else
        {
            s.Transform.Position = new Vector2(-1000, 0);
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