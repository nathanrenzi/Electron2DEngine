using GLFW;
using Electron2D.Core;
using Electron2D.Core.Management;
using System.Numerics;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.ECS;
using System.Drawing;
using Electron2D.Core.Misc;
using Electron2D.Core.Management.Textures;

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
            Texture2D tex2 = ResourceManager.Instance.LoadTexture("Build/Resources/Textures/EnvironmentTiles.png");
            SpritesheetManager.Add(tex2, 13, 11);

            Shader diffuseShader = new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultLit.glsl"),
                _globalUniformTags: new string[] { "lights" });

            //tilemap = new Tilemap(Material.Create(diffuseShader, _mainTexture: tex2, _useLinearFiltering: false, _normalScale: 0),
            //    new TileData[] { new TileData("Grass1", 1, 7), new TileData("Grass2", 2, 9), new TileData("Pebble", 4, 2) }, 108, 10, 10,
            //    new byte[]
            //  { 0, 1, 0, 0, 1, 1, 0, 0, 2, 0,
            //    2, 0, 1, 1, 0, 1, 1, 0, 1, 1,
            //    1, 1, 0, 0, 0, 1, 0, 2, 0, 0,
            //    1, 0, 1, 1, 0, 1, 0, 0, 1, 1,
            //    1, 1, 0, 1, 0, 1, 0, 2, 0, 1,
            //    0, 1, 0, 0, 1, 0, 0, 1, 0, 0,
            //    0, 0, 2, 0, 0, 1, 1, 0, 0, 2,
            //    0, 1, 1, 1, 0, 2, 1, 1, 1, 1,
            //    1, 0, 1, 0, 0, 1, 0, 0, 0, 1,
            //    0, 1, 1, 0, 1, 0, 0, 1, 1, 0,});

            //tilemap.GetComponent<Transform>().Position = new Vector2(-540, -540);

            //StreamWriter writer = new StreamWriter("C:/Users/Nathan/source/repos/Electron2D/Build/Resources/map.txt");
            //Console.WriteLine(tilemap.ToJson());
            //writer.Write(tilemap.ToJson());
            //writer.Close();

            tilemap = Tilemap.FromJson("C:/Users/Nathan/source/repos/Electron2D/Build/Resources/map.txt");

            lightObj = new Entity();
            lightObj.AddComponent(new Transform());
            lightObj.AddComponent(new Light(Color.White, 400, 4, Light.LightType.Point, 2));
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
