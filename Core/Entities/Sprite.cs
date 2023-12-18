using Electron2D.Core.ECS;
using Electron2D.Core.Rendering;

namespace Electron2D.Core
{
    public class Sprite : Entity, IRenderable
    {
        public Transform Transform;
        public SpriteRenderer Renderer;
        public int RenderLayer;

        public Sprite(Material _material, int _sizeX = 100, int _sizeY = 100, int _renderLayer = 1)
        {
            RenderLayer = _renderLayer;

            Transform = new Transform();
            AddComponent(Transform);
            Transform.Scale = new System.Numerics.Vector2(_sizeX, _sizeY);

            Renderer = new SpriteRenderer(Transform, _material, _renderLayer);
            AddComponent(Renderer);

            RenderLayerManager.OrderRenderable(this);
        }

        ~Sprite()
        {
            RenderLayerManager.RemoveRenderable(this);
        }

        public int GetRenderLayer() => RenderLayer;

        public void Render()
        {
            Renderer.Render();
        }
    }
}
