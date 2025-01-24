using Electron2D.Rendering;
using System.Numerics;

namespace Electron2D
{
    /// <summary>
    /// Advanced version of <see cref="MeshRenderer"/> that allows for rendering sprites and automatically switching them.
    /// </summary>
    public class SpriteRenderer : MeshRenderer
    {
        private readonly float[] _vertices =
        {
             1f,  1f,       1.0f, 1.0f,     0.0f,
             1f, -1f,       1.0f, 0.0f,     0.0f,
            -1f, -1f,       0.0f, 0.0f,     0.0f,
            -1f,  1f,       0.0f, 1.0f,     0.0f
        };

        private static readonly float[] _defaultUV =
        {
            1.0f, 1.0f,
            1.0f, 0.0f,
            0.0f, 0.0f,
            0.0f, 1.0f,
        };

        private static readonly uint[] _indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        /// <summary>
        /// Sprite switches per second.
        /// </summary>
        public float SpriteAnimationSpeed { get; set; }

        /// <summary>
        /// The sprites queued up to be displayed.
        /// </summary>
        public Queue<Texture2DArray> QueuedSprites { get; set; } = new Queue<Texture2DArray>();
        public Queue<bool> QueuedFinishFlags { get; set; } = new Queue<bool>();
        public Action OnCompletedSpriteLoop { get; set; }
        public bool MustFinishLoopFlag { get; set; }

        private int _spriteIndex;
        private float _time;

        public SpriteRenderer(Transform transform, Material material, int renderLayer = 1) : base(transform, material)
        {
            RenderLayer = renderLayer;

            // Must be called in order for HasVertexData to be true
            SetVertexArrays(_vertices, _indices);
        }

        protected override void CreateBufferLayout()
        {
            // Telling the vertex array how the vertices are structured
            Layout = new BufferLayout();
            Layout.Add<float>(2); // Position
            Layout.Add<float>(2); // UV
            Layout.Add<float>(1); // Texture Index
        }

        /// <summary>
        /// Returns the default texture UV associated with the vertex inputted.
        /// </summary>
        /// <param name="vertex">The vertex to get the UV of.</param>
        /// <returns></returns>
        public Vector2 GetDefaultUV(int vertex = 0)
        {
            return new Vector2(_defaultUV[vertex * 2], _defaultUV[(vertex * 2) + 1]);
        }

        /// <summary>
        /// Manually switches the sprite to the next one in the texture.
        /// </summary>
        /// <param name="allowWrapping">If the sprite can loop back to the beginning if there are no sprites left.</param>
        public void NextSprite(bool allowWrapping = true)
        {
            _spriteIndex++;
            if (_spriteIndex >= Material.MainTexture.GetTextureLayers() && allowWrapping)
            {
                _spriteIndex = 0;
            }
            else if(!allowWrapping)
            {
                _spriteIndex = (int)MathF.Max(0, MathF.Min(Material.MainTexture.GetTextureLayers() - 1, _spriteIndex));
            }
        }

        /// <summary>
        /// Manually switches the sprite to the previous one in the texture.
        /// </summary>
        /// <param name="allowWrapping">If the sprite can loop to the end if there are no sprites left.</param>
        public void PreviousSprite(bool allowWrapping = true)
        {
            _spriteIndex--;
            if (_spriteIndex < 0 && allowWrapping)
            {
                _spriteIndex = Material.MainTexture.GetTextureLayers() - 1;
            }
            else if (!allowWrapping)
            {
                _spriteIndex = (int)MathF.Max(0, MathF.Min(Material.MainTexture.GetTextureLayers() - 1, _spriteIndex));
            }
        }

        /// <summary>
        /// Clears all currently running animations so that a new texture can be set instantly.
        /// </summary>
        public void ClearAnimation(Texture2DArray newTexture = null)
        {
            if(newTexture != null)
            {
                Material.MainTexture = newTexture;
            }

            QueuedSprites.Clear();
            QueuedFinishFlags.Clear();
            _spriteIndex = 0;
            _time = 0;
            SetVertexValueAll((int)SpriteVertexAttribute.SpriteIndex, _spriteIndex);
        }

        public new void Update()
        {
            if (SpriteAnimationSpeed <= 0) return;

            _time += Time.DeltaTime;
            if(_time > 1f / SpriteAnimationSpeed)
            {
                // Resetting the time
                _time -= 1f / SpriteAnimationSpeed;

                // Checking to see if there is a new texture queued
                if (QueuedSprites.Count > 0 && !MustFinishLoopFlag)
                {
                    Material.MainTexture = QueuedSprites.Dequeue();
                    MustFinishLoopFlag = QueuedFinishFlags.Dequeue();

                    _spriteIndex = 0;
                    SetVertexValueAll((int)SpriteVertexAttribute.SpriteIndex, _spriteIndex);
                    return;
                }

                // If there are no sprites queued, 
                _spriteIndex++;
                if (_spriteIndex >= Material.MainTexture.GetTextureLayers())
                {
                    // Loop is finished
                    OnCompletedSpriteLoop?.Invoke();

                    // Checking to see if there is a new texture queued
                    if (QueuedSprites.Count > 0)
                    {
                        Material.MainTexture = QueuedSprites.Dequeue();
                        MustFinishLoopFlag = QueuedFinishFlags.Dequeue();

                        _spriteIndex = 0;
                        SetVertexValueAll((int)SpriteVertexAttribute.SpriteIndex, _spriteIndex);
                        return;
                    }

                    _spriteIndex = 0;
                }
                SetVertexValueAll((int)SpriteVertexAttribute.SpriteIndex, _spriteIndex);
            }
        }
    }

    /// <summary>
    /// This enum corresponds to an attribute in a vertex for the renderer.
    /// </summary>
    public enum SpriteVertexAttribute
    {
        PositionX = 0,
        PositionY = 1,
        UvX = 2,
        UvY = 3,
        SpriteIndex = 4
    }
}
