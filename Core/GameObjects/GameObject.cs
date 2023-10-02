using Electron2D.Core.Rendering;

namespace Electron2D.Core.GameObjects
{
    public class GameObject : IRenderable // Remove GameObject from rendering and just use renderers in future
    {
        public Transform Transform = new Transform();
        public MeshRenderer Renderer;

        public int RenderLayer { get; private set; }

        public GameObject(Material _material, int _renderLayer = 0, MeshRenderer _customRenderer = null)
        {
            RenderLayer = _renderLayer;

            // Initializing the renderer
            if (_customRenderer != null)
            {
                Renderer = _customRenderer;
            }
            else
            {
                Renderer = new SpriteRenderer(Transform, _material);
            }

            RenderLayerManager.OrderRenderable(this);
            GameObjectManager.RegisterGameObject(this);
        }

        public void SetRenderLayer(int _renderLayer)
        {
            if (RenderLayer == _renderLayer) return;
            RenderLayerManager.OrderRenderable(this, true, RenderLayer, _renderLayer);
            RenderLayer = _renderLayer;
        }

        public int GetRenderLayer() => RenderLayer;

        public virtual void Start()
        {

        }

        public virtual void Render()
        {
            if (Renderer != null) Renderer.Render();
        }

        public virtual void Update()
        {

        }

        public void Destroy()
        {
            GameObjectManager.UnregisterGameObject(this);
            RenderLayerManager.RemoveRenderable(this);
            OnDestroy();
        }

        public virtual void OnDestroy()
        {

        }
    }
}