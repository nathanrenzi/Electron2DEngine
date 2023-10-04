using Electron2D.Core.ECS;
using Electron2D.Core.Rendering;

namespace Electron2D.Core
{
    public class Sprite : Entity
    {
        public Transform Transform;
        public SpriteRenderer Renderer;

        public Sprite(Material _material, int _renderLayer = 1)
        {
            Transform = new Transform();
            AddComponent(Transform);

            Renderer = new SpriteRenderer(Transform, _material, _renderLayer);
            AddComponent(Renderer);
        }
    }
}
