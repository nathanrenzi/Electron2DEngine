using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;

namespace Electron2D.Core.GameObjects
{
    public class GameObject
    {
        public Transform transform = new Transform();
        public IRenderer renderer;
        public int renderLayer { get; private set; }
        public bool useAutoInitialization { get; private set; }

        public GameObject(int _renderOrder = 0, bool _useAutoRendererInitialization = true, IRenderer _customRenderer = null)
        {
            renderLayer = _renderOrder;
            useAutoInitialization = _useAutoRendererInitialization;

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

        public void SetRenderOrder(int _renderOrder)
        {
            if (renderLayer == _renderOrder) return;
            GameObjectManager.OrderGameObject(this, true, renderLayer, _renderOrder);
            renderLayer = _renderOrder;
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