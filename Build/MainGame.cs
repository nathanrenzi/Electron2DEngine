using GLFW;
using Electron2D.Core;
using Electron2D.Core.Management;
using Electron2D.Build.Resources.Objects;
using Electron2D.Core.GameObjects;
using System.Numerics;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;
using System.Drawing;
using Electron2D.Core.Management.Textures;
using Electron2D.Core.Audio;
using Electron2D.Core.Physics;
using Electron2D.Core.UI;

namespace Electron2D.Build
{
    public class MainGame : Game
    {
        public MainGame(int _initialWindowWidth, int _initialWindowHeight, string _initialWindowTitle) : base(_initialWindowWidth, _initialWindowHeight, _initialWindowTitle)
        {

        }

        protected override void Initialize()
        {  
            Console.WriteLine("Game Started");
        }

        protected override void LoadContent()
        {
            // First spritesheet
            ResourceManager.Instance.LoadTexture("Build/Resources/Textures/boidSpritesheet.png");
            SpritesheetManager.Add(3, 1);

            UiComponent ui = new();
            ui.transform.position = Vector2.Zero;
            ui.sizeX = 200;
            ui.sizeY = 100;
            ui.anchor = new Vector2(1, 0);
            ui.GenerateUiMesh();
            for (int i = 0; i < ui.rendererReference.vertices.Length; i++)
            {
                Console.WriteLine(ui.rendererReference.vertices[i]);
            }
        }

        protected override void Update()
        {
            CameraMovement();
            if(Input.GetMouseButtonDown(MouseButton.Left))
            {
                SpawnNewPhysicsObj(Input.GetMouseWorldPosition());
            }
            if (Input.GetMouseButton(MouseButton.Right))
            {
                SpawnNewPhysicsObj(Input.GetMouseWorldPosition());
            }
        }

        private void SpawnNewPhysicsObj(Vector2 _position)
        {
            GameObject obj = new GameObject(-1, false);
            obj.renderer = new BatchedSpriteRenderer(obj.transform);
            obj.transform.position = _position;
            obj.SetSprite(0, 0, 0);

            VerletBody body = new VerletBody(obj.transform);
        }

        private void CameraMovement()
        {
            Camera2D.main.zoom += Input.scrollDelta;
            Camera2D.main.zoom = Math.Clamp(Camera2D.main.zoom, 1, 10);

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
