using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;

namespace Electron2D.Core.GameObjects
{
    public class GameObject
    {
        public Transform transform = new Transform();
        public IRenderer renderer;
        public bool useAutoInitialization { get; private set; }

        public GameObject(bool _useAutoInitialization = true, IRenderer _customRenderer = null)
        {
            useAutoInitialization = _useAutoInitialization;

            // Initializing the renderer
            if (_customRenderer != null)
            {
                renderer = _customRenderer;
            }
            else
            {
                renderer = new SpriteRenderer(transform, new Shader(Shader.ParseShader("Build/Resources/Shaders/Default.glsl"), false));
            }

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