using Electron2D.Core.Management.Textures;
using Electron2D.Core.Rendering;
using System.Numerics;

namespace Electron2D.Core
{
    /// <summary>
    /// Advanced version of <see cref="MeshRenderer"/> that allows for rendering sprites and automatically switching them.
    /// </summary>
    public class SpriteRenderer : MeshRenderer
    {
        private readonly float[] vertices =
        {
             1f,  1f,       1.0f, 1.0f,     0.0f,
             1f, -1f,       1.0f, 0.0f,     0.0f,
            -1f, -1f,       0.0f, 0.0f,     0.0f,
            -1f,  1f,       0.0f, 1.0f,     0.0f
        };

        private static readonly float[] defaultUV =
        {
            1.0f, 1.0f,
            1.0f, 0.0f,
            0.0f, 0.0f,
            0.0f, 1.0f,
        };

        private static readonly uint[] indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        /// <summary>
        /// Sprite switches per second.
        /// </summary>
        public float SpriteAnimationSpeed { get; private set; }

        private int currentSprite;
        private float time;

        public SpriteRenderer(Transform _transform, Material _material, int _renderLayer = 1) : base(_transform, _material)
        {
            RenderLayer = _renderLayer;

            // Must be called in order for HasVertexData to be true
            SetVertexArrays(vertices, indices);
        }

        protected override void CreateBufferLayout()
        {
            // Telling the vertex array how the vertices are structured
            layout = new BufferLayout();
            layout.Add<float>(2); // Position
            layout.Add<float>(2); // UV
            layout.Add<float>(1); // Texture Index
        }

        /// <summary>
        /// Returns the default texture UV associated with the vertex inputted.
        /// </summary>
        /// <param name="_vertex">The vertex to get the UV of.</param>
        /// <returns></returns>
        public Vector2 GetDefaultUV(int _vertex = 0)
        {
            return new Vector2(defaultUV[_vertex * 2], defaultUV[(_vertex * 2) + 1]);
        }

        public void SetSpriteAnimationSpeed(float _spritesPerSecond)
        {
            SpriteAnimationSpeed = _spritesPerSecond;
        }

        /// <summary>
        /// Switches the sprite to the next one in the texture.
        /// </summary>
        /// <param name="_allowWrapping">If the sprite can loop back to the beginning if there are no sprites left.</param>
        public void NextSprite(bool _allowWrapping = true)
        {
            currentSprite++;
            if (currentSprite >= Material.MainTexture.GetTextureLayers() && _allowWrapping)
            {
                currentSprite = 0;
            }
            else if(!_allowWrapping)
            {
                currentSprite = (int)MathF.Max(0, MathF.Min(Material.MainTexture.GetTextureLayers() - 1, currentSprite));
            }
        }

        /// <summary>
        /// Switches the sprite to the previous one in the texture.
        /// </summary>
        /// <param name="_allowWrapping">If the sprite can loop to the end if there are no sprites left.</param>
        public void PreviousSprite(bool _allowWrapping = true)
        {
            currentSprite--;
            if (currentSprite < 0 && _allowWrapping)
            {
                currentSprite = Material.MainTexture.GetTextureLayers() - 1;
            }
            else if (!_allowWrapping)
            {
                currentSprite = (int)MathF.Max(0, MathF.Min(Material.MainTexture.GetTextureLayers() - 1, currentSprite));
            }
        }

        public override void Update()
        {
            if (SpriteAnimationSpeed <= 0) return;

            time += Time.DeltaTime;
            if(time > 1f / SpriteAnimationSpeed)
            {
                currentSprite++;
                if (currentSprite >= Material.MainTexture.GetTextureLayers())
                    currentSprite = 0;

                time -= 1f / SpriteAnimationSpeed;

                SetVertexValueAll((int)SpriteVertexAttribute.SpriteIndex, currentSprite);
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
