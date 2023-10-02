using GLFW;
using Electron2D.Core;
using Electron2D.Core.Management;
using Electron2D.Core.GameObjects;
using System.Numerics;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;
using System.Drawing;
using Electron2D.Core.Management.Textures;
using Electron2D.Core.Audio;
using Electron2D.Core.Physics;
using Electron2D.Core.UI;
using Electron2D.Core.Misc;
using Electron2D.Core.UserInterface;

namespace Electron2D.Build
{
    public class Build : Game
    {
        // Testing objects
        private List<GameObject> environmentTiles = new List<GameObject>();
        private SlicedUiComponent ui;
        private Texture2D tex1, tex2;
        // ---------------

        public Build(int _initialWindowWidth, int _initialWindowHeight, string _initialWindowTitle) : base(_initialWindowWidth, _initialWindowHeight, _initialWindowTitle)
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

            // UI Spritesheet
            tex2 = ResourceManager.Instance.LoadTexture("Build/Resources/Textures/UserInterfaceTextures.png");
            SpritesheetManager.Add(tex2, 4, 4);

            Random rand = new Random();
            int environmentScale = 50;
            int tiles = 10;
            for (int x = -tiles; x <= tiles; x++)
            {
                for (int y = -tiles; y <= tiles; y++)
                {
                    GameObject tile = new GameObject(Material.Create(GlobalShaders.DefaultTexture, tex1, false), -1);
                    tile.Transform.Position = new Vector2(x, y) * environmentScale;
                    tile.Transform.Scale = Vector2.One * environmentScale;
                    environmentTiles.Add(tile);

                    if (rand.Next(2) == 1)
                    {
                        tile.Renderer.SetMaterial(Material.Create(GlobalShaders.DefaultTexture, tex2, true));
                    }
                }
            }

            // NOTE - CAMERA POSITION IS NOT RELATIVE TO SCREEN SIZE - MOVES FASTER / SLOWER BASED ON SCREEN SIZE

            ui = new SlicedUiComponent(Color.White, 100, 100, 30, 30, 30, 30, 100);
            ui.Renderer.SetMaterial(Material.Create(GlobalShaders.DefaultTexture, tex1, false));
        }

        protected override void Update()
        {
            CameraMovement();
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
