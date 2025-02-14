using Electron2D.Rendering;

namespace Electron2D
{
    public class Sprite : IRenderable
    {
        public Transform Transform { get; private set; }
        public SpriteRenderer Renderer { get; private set; }
        public int RenderLayer;

        /// <param name="material">The material to use for the sprite. This includes the textures.</param>
        /// <param name="spritesPerSecond">The number of sprite switches per second.</param>
        /// <param name="sizeX"></param>
        /// <param name="sizeY"></param>
        /// <param name="renderLayer"></param>
        /// <param name="transform">Sets a custom Transform object for the Sprite to use.</param> 
        public Sprite(Material material, int sizeX = 100, int sizeY = 100, float spritesPerSecond = 0, int renderLayer = 1, Transform transform = null, bool useCustomTransformScale = false)
        {
            RenderLayer = renderLayer;

            if(transform == null)
            {
                Transform = new Transform();
                Transform.Scale = new System.Numerics.Vector2(sizeX, sizeY);
            }
            else
            {
                Transform = transform;
                if(!useCustomTransformScale) Transform.Scale = new System.Numerics.Vector2(sizeX, sizeY);
            }

            Renderer = new SpriteRenderer(Transform, material, renderLayer);
            Renderer.SpriteAnimationSpeed = spritesPerSecond;

            RenderLayerManager.OrderRenderable(this);
        }

        ~Sprite()
        {
            Dispose();
        }

        public void Dispose()
        {
            RenderLayerManager.RemoveRenderable(this);
            Renderer.Dispose();
            GC.SuppressFinalize(this);
        }

        public void ForceTexture(Texture2DArray texture, bool mustFinishLoop)
        {
            Renderer.ClearAnimation(texture);
            Renderer.MustFinishLoopFlag = mustFinishLoop;
        }
        public void AddTextureToQueue(Texture2DArray texture, bool mustFinishLoop)
        {
            Renderer.QueuedSprites.Enqueue(texture);
            Renderer.QueuedFinishFlags.Enqueue(mustFinishLoop);
        }
        public void NextSprite() { Renderer.NextSprite(); }
        public void PreviousSprite() { Renderer.PreviousSprite(); }
        public void SetSpriteAnimationSpeed(float spritesPerSecond) { Renderer.SpriteAnimationSpeed = spritesPerSecond; }

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
