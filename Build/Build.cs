using GLFW;
using Electron2D.Core;
using Electron2D.Core.Management;
using System.Numerics;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.ECS;
using System.Drawing;
using Electron2D.Core.Misc;

namespace Electron2D.Build
{
    public class Build : Game
    {
        // Testing objects
        private Entity lightObj;
        private Tilemap tilemap;
        // ---------------

        public Build(int _initialWindowWidth, int _initialWindowHeight) : base(_initialWindowWidth, _initialWindowHeight, "Test Game!")
        {

        }

        protected override void Initialize()
        {  
            Console.WriteLine("Game Started");
        }

        protected override void Start()
        {
            Texture2D tex1 = ResourceManager.Instance.LoadTexture("Build/Resources/Textures/TestNormal.jpg", true);

            Shader diffuseShader = new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultLit.glsl"),
                _globalUniformTags: new string[] { "lights", "time" });

            tilemap = new Tilemap(Material.Create(diffuseShader, _normalTexture: tex1, _useLinearFiltering: true),
                new TileData[] { new TileData("Test", 0, 0) }, 100, 10, 10,
                new byte[]
              { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,});

            lightObj = new Entity();
            lightObj.AddComponent(new Transform());
            lightObj.AddComponent(new Light(Color.LightSalmon, 400, 4));
            lightObj.GetComponent<Transform>().Position = new Vector2(-400, 100);
        }

        protected override void Update()
        {
            CameraMovement();

            lightObj.GetComponent<Transform>().Position = new Vector2(MathF.Sin(Time.TotalElapsedSeconds * 3)
                * 300, MathF.Cos(Time.TotalElapsedSeconds * 3) * 300);
        }

        private void CameraMovement()
        {
            Camera2D.main.zoom += Input.scrollDelta;
            Camera2D.main.zoom = Math.Clamp(Camera2D.main.zoom, 0.5f, 2);

            float moveSpeed = 1000;
            if (Input.GetKey(Keys.W))
            {
                Camera2D.main.position += new Vector2(0, moveSpeed * Time.DeltaTime);
            }
            if (Input.GetKey(Keys.A))
            {
                Camera2D.main.position += new Vector2(-moveSpeed * Time.DeltaTime, 0);
            }
            if (Input.GetKey(Keys.S))
            {
                Camera2D.main.position += new Vector2(0, -moveSpeed * Time.DeltaTime);
            }
            if (Input.GetKey(Keys.D))
            {
                Camera2D.main.position += new Vector2(moveSpeed * Time.DeltaTime, 0);
            }
        }

        protected unsafe override void Render()
        {

        }
    }
}
