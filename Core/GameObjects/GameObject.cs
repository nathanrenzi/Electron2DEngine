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

        private int queuedSpriteIndex;
        private int queuedSpriteCol;
        private int queuedSpriteRow;
        private bool hasFinishedSetSprite = true;

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

        /// <summary>
        /// Safer to use than IRenderer.SetSprite(). Will queue until the renderer has loaded. 
        /// </summary>
        public void SetSprite(int _spritesheetIndex, int _col, int _row)
        {
            hasFinishedSetSprite = false;

            queuedSpriteIndex = _spritesheetIndex;
            queuedSpriteCol = _col;
            queuedSpriteRow = _row;

            if (renderer.isLoaded)
            {
                renderer.SetSprite(_spritesheetIndex, _col, _row);
                hasFinishedSetSprite = true;
            }
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
        /// Called automatically after Start if rendering is enabled. Initializes the mesh renderer and sets the sprite if it has not been set yet.
        /// </summary>
        public void InitializeMeshRenderer()
        {
            renderer.Load();
            if(!hasFinishedSetSprite)
            {
                renderer.SetSprite(queuedSpriteIndex, queuedSpriteCol, queuedSpriteRow);
                hasFinishedSetSprite = true;
            }
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