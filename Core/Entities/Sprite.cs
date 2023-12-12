using Electron2D.Core.ECS;
using Electron2D.Core.Rendering;

namespace Electron2D.Core
{
    public class Sprite : Entity
    {
        public Transform Transform;
        public SpriteRenderer Renderer;

        public Sprite(Material _material, int _sizeX = 100, int _sizeY = 100, int _renderLayer = 1)
        {
            Transform = new Transform();
            AddComponent(Transform);
            Transform.Scale = new System.Numerics.Vector2(_sizeX, _sizeY);

            Renderer = new SpriteRenderer(Transform, _material, _renderLayer);
            AddComponent(Renderer);
        }
    }
}
