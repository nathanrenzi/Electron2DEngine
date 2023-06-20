using static Electron2D.OpenGL.GL;
using GLFW;
using Electron2D.Core;
using Electron2D.Core.Management;
using Electron2D.Build.Resources.Objects;
using Electron2D.Core.GameObjects;
using System.Numerics;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;
using System.Drawing;

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
            BoidField b = new(3, 100);
            GameObject centerDisplay = new(false);
            centerDisplay.renderer.Load();
            centerDisplay.renderer.SetVertexValueAll((int)SpriteRendererAttribute.TextureIndex, 2);
            centerDisplay.renderer.SetVertexValueAll((int)SpriteRendererAttribute.ColorA, 1);
            centerDisplay.renderer.SetVertexValueAll((int)SpriteRendererAttribute.ColorR, 1);
            centerDisplay.renderer.SetVertexValueAll((int)SpriteRendererAttribute.ColorG, 1);
            centerDisplay.renderer.SetVertexValueAll((int)SpriteRendererAttribute.ColorB, 0.7f);

            GameObject testVertex = new(false, null);
            VertexRenderer renderer = new VertexRenderer(testVertex.transform, new Shader(Shader.ParseShader("Build/Resources/Shaders/DefaultVertex.glsl")));
            testVertex.renderer = renderer;

            // Diamond
            renderer.AddVertex(new Vector2(0.5f, 0), Color.Red);
            renderer.AddVertex(new Vector2(0, -1), Color.Red);
            renderer.AddVertex(new Vector2(-0.5f, 0), Color.Red);
            renderer.AddVertex(new Vector2(0, 1), Color.Red);
            renderer.AddTriangle(0, 1, 3, 0);
            renderer.AddTriangle(1, 2, 3, 0);

            // Multicolored diamond
            renderer.AddVertex(new Vector2(0.5f - 3, 0), Color.Red);
            renderer.AddVertex(new Vector2(0 - 3, -1), Color.Green);
            renderer.AddVertex(new Vector2(-0.5f - 3, 0), Color.Blue);
            renderer.AddVertex(new Vector2(0 - 3, 1), Color.White);
            renderer.AddTriangle(0, 1, 3, 4);
            renderer.AddTriangle(1, 2, 3, 4);

            // Finalizing the vertex renderer
            renderer.FinalizeVertices();
            renderer.ClearTempLists();
            renderer.Load();

            ResourceManager.Instance.LoadTexture("Build/Resources/Textures/boid1.png");
            ResourceManager.Instance.LoadTexture("Build/Resources/Textures/boid2.png");
            ResourceManager.Instance.LoadTexture("Build/Resources/Textures/mouseBorder.png");
        }

        protected override void Update()
        {
            Camera2D.main.zoom += Input.scrollDelta;
            Camera2D.main.zoom = Math.Clamp(Camera2D.main.zoom, 1, 3);

            float moveSpeed = 1000;
            if(Input.GetKey(Keys.W))
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
