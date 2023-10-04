using GLFW;
using Electron2D.Core;
using Electron2D.Core.Management;
using System.Numerics;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.Management.Textures;
using Electron2D.Core.UserInterface;
using Electron2D.Core.ECS;
using System.Drawing;

namespace Electron2D.Build
{
    public class Build : Game
    {
        // Testing objects
        private SlicedUiComponent ui;
        private Texture2D tex1, tex2;
        private Entity lightObj;
        private Entity lightObj2;
        private Entity lightObj3;
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
            // Environment Spritesheet
            tex1 = ResourceManager.Instance.LoadTexture("Build/Resources/Textures/EnvironmentTiles.png");
            SpritesheetManager.Add(tex1, 13, 11);

            Shader diffuseShader = new Shader(Shader.ParseShader("Core/Rendering/Shaders/LitShader.glsl"), _useLightData: true);
            Random rand = new Random();
            int environmentScale = 50;
            int tiles = 10;
            for (int x = -tiles; x <= tiles; x++)
            {
                for (int y = -tiles; y <= tiles; y++)
                {
                    Sprite tile = new Sprite(Material.Create(diffuseShader), -1);
                    tile.Transform.Position = new Vector2(x, y) * environmentScale;
                    tile.Transform.Scale = Vector2.One * environmentScale;
                }
            }

            lightObj = new Entity();
            lightObj.AddComponent(new Transform());
            lightObj.AddComponent(new Light(lightObj.GetComponent<Transform>(), System.Drawing.Color.LightSalmon, 400));
            lightObj.GetComponent<Transform>().Position = new Vector2(-400, 100);

            lightObj2 = new Entity();
            lightObj2.AddComponent(new Transform());
            lightObj2.AddComponent(new Light(lightObj2.GetComponent<Transform>(), System.Drawing.Color.FloralWhite, 200));
            lightObj2.GetComponent<Transform>().Position = new Vector2(300, 300);

            lightObj3 = new Entity();
            lightObj3.AddComponent(new Transform());
            lightObj3.AddComponent(new Light(lightObj3.GetComponent<Transform>(), System.Drawing.Color.MediumPurple, 200));
            lightObj3.GetComponent<Transform>().Position = new Vector2(100, -400);
        }

        protected override void Update()
        {
            CameraMovement();
            lightObj.GetComponent<Transform>().Position = new Vector2(MathF.Sin(Time.totalElapsedSeconds * 3)
                * 300, MathF.Cos(Time.totalElapsedSeconds * 3) * 300);
        }

        private void CameraMovement()
        {
            Camera2D.main.zoom += Input.scrollDelta;
            Camera2D.main.zoom = Math.Clamp(Camera2D.main.zoom, 0.5f, 2);

            float moveSpeed = 1000;
            if (Input.GetKey(Keys.W))
            {
                Camera2D.main.position += new Vector2(0, moveSpeed * Time.deltaTime);
            }
            if (Input.GetKey(Keys.A))
            {
                Camera2D.main.position += new Vector2(-moveSpeed * Time.deltaTime, 0);
            }
            if (Input.GetKey(Keys.S))
            {
                Camera2D.main.position += new Vector2(0, -moveSpeed * Time.deltaTime);
            }
            if (Input.GetKey(Keys.D))
            {
                Camera2D.main.position += new Vector2(moveSpeed * Time.deltaTime, 0);
            }
        }

        protected unsafe override void Render()
        {

        }
    }
}
