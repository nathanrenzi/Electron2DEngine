using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;

namespace Electron2D.Core.GameObjects
{
    public class GameObject
    {
        public Transform transform = new Transform();
        public SpriteRenderer renderer;
        public Shader shader;
        public bool useRendering { get; private set; }

        public GameObject(bool _useRendering = true)
        {
            useRendering = _useRendering;

            // Initializing the shader and transform to their default values
            shader = new Shader(Shader.ParseShader("Build/Resources/Shaders/Default.glsl"), true);
            renderer = new SpriteRenderer(transform, shader);

            GameObjectManager.RegisterGameObject(this);
        }

        public virtual void Start()
        {

        }

        /// <summary>
        /// Called automatically after Start if rendering is enabled. Initializes the mesh renderer and inputs
        /// </summary>
        public void InitializeMeshRenderer()
        {
            renderer.Load();
        }

        public virtual void Render()
        {
            if (renderer != null) renderer.Render();
        }

        public virtual void Update()
        {

        }

        public void Destroy()
        {
            GameObjectManager.UnregisterGameObject(this);
            OnDestroy();
        }

        public virtual void OnDestroy()
        {

        }
    }
}