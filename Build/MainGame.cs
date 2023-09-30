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
    public class MainGame : Game
    {
        private List<GameObject> environmentTiles = new List<GameObject>();
        private SlicedUiComponent ui;

        public MainGame(int _initialWindowWidth, int _initialWindowHeight, string _initialWindowTitle) : base(_initialWindowWidth, _initialWindowHeight, _initialWindowTitle)
        {

        }

        protected override void Initialize()
        {  
            Console.WriteLine("Game Started");
        }

        protected override void Start()
        {
            // Environment Spritesheet
            Texture2D tex1 = ResourceManager.Instance.LoadTexture("Build/Resources/Textures/EnvironmentTiles.png");
            SpritesheetManager.Add(13, 11);
            //tex1.Use();

            // UI Spritesheet
            Texture2D tex2 = ResourceManager.Instance.LoadTexture("Build/Resources/Textures/UserInterfaceTextures.png");
            SpritesheetManager.Add(4, 4);
            //tex2.Use();

            // FOUND ISSUE - THERE IS NO TEXTURE.USE() SYSTEM IN PLACE
            // IMPLEMENT A TEXTURE SYSTEM WHERE RENDERABLES CAN USE TEXTURES
            // Note 2: Texture system works perfectly for sprite renderers... not UI for some reason?

            Random rand = new Random();
            int environmentScale = 50;
            int tiles = 1;
            for (int x = -tiles; x <= tiles; x++)
            {
                for (int y = -tiles; y <= tiles; y++)
                {
                    GameObject tile = new GameObject(-1);
                    tile.transform.position = new Vector2(x, y) * environmentScale;
                    tile.transform.scale = Vector2.One * environmentScale;
                    environmentTiles.Add(tile);

                    if (rand.Next(2) == 1)
                    {
                        //tile.SetSprite(0, 2, 9);
                        tile.SetSprite(1, 0, 1);
                    }
                    else
                    {
                        //tile.SetSprite(0, 3, 9);
                        tile.SetSprite(1, 0, 3);
                    }
                }
            }

            // NOTE - CAMERA POSITION IS NOT RELATIVE TO SCREEN SIZE - MOVES FASTER / SLOWER BASED ON SCREEN SIZE

            ui = new SlicedUiComponent(Color.White, 100, 100, 30, 30, 30, 30, 100);
            ui.renderer.UseLinearFiltering = true;
            ui.renderer.SetSprite(1, 0, 1);
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
