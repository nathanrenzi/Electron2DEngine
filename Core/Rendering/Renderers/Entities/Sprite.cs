using Electron2D.Core.ECS;
using Electron2D.Core.Rendering;

namespace Electron2D.Core
{
    public class Sprite : Entity, IRenderable
    {
        public Transform Transform;
        public SpriteRenderer Renderer;
        public int RenderLayer;

        /// <param name="_material">The material to use for the sprite. This includes the textures.</param>
        /// <param name="_spritesPerSecond">The number of sprite switches per second.</param>
        /// <param name="_sizeX"></param>
        /// <param name="_sizeY"></param>
        /// <param name="_renderLayer"></param>
        public Sprite(Material _material, float _spritesPerSecond = 0, int _sizeX = 100, int _sizeY = 100, int _renderLayer = 1)
        {
            RenderLayer = _renderLayer;

            Transform = new Transform();
            AddComponent(Transform);
            Transform.Scale = new System.Numerics.Vector2(_sizeX, _sizeY);

            Renderer = new SpriteRenderer(Transform, _material, _renderLayer);
            AddComponent(Renderer);
            Renderer.SpriteAnimationSpeed = _spritesPerSecond;

            RenderLayerManager.OrderRenderable(this);
        }

        public void ForceTexture(Texture2DArray _texture, bool _mustFinishLoop)
        {
            Renderer.ClearAnimation(_texture);
            Renderer.MustFinishLoopFlag = _mustFinishLoop;
        }
        public void AddTextureToQueue(Texture2DArray _texture, bool _mustFinishLoop)
        {
            Renderer.QueuedSprites.Enqueue(_texture);
            Renderer.QueuedFinishFlags.Enqueue(_mustFinishLoop);
        }
        public void NextSprite() { Renderer.NextSprite(); }
        public void PreviousSprite() { Renderer.PreviousSprite(); }
        public void SetSpriteAnimationSpeed(float _spritesPerSecond) { Renderer.SpriteAnimationSpeed = _spritesPerSecond; }

        ~Sprite()
        {
            RenderLayerManager.RemoveRenderable(this);
        }

        public int GetRenderLayer() => RenderLayer;

        public void Render()
        {
            Renderer.Render();
        }

        public bool ShouldIgnorePostProcessing()
        {
            return false;
        }
    }
}
