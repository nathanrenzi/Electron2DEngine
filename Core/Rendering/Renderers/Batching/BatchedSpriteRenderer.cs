using Electron2D.Core.GameObjects;
using Electron2D.Core.Management.Textures;
using Electron2D.Core.Rendering.Shaders;
using System.Numerics;

namespace Electron2D.Core.Rendering
{
    public class BatchedSpriteRenderer : IRenderer
    {
        public readonly float[] vertices =
        {
            // Positions    UV            Color                     TexIndex
             1f,  1f,       1.0f, 1.0f,   1.0f, 1.0f, 1.0f, 1.0f,   0.0f,      // top right
             1f, -1f,       1.0f, 0.0f,   1.0f, 1.0f, 1.0f, 1.0f,   0.0f,      // bottom right
            -1f, -1f,       0.0f, 0.0f,   1.0f, 1.0f, 1.0f, 1.0f,   0.0f,      // bottom left
            -1f,  1f,       0.0f, 1.0f,   1.0f, 1.0f, 1.0f, 1.0f,   0.0f,      // top left
        };

        public readonly float[] defaultUV =
        {
            1.0f, 1.0f,
            1.0f, 0.0f,
            0.0f, 0.0f,
            0.0f, 1.0f,
        };

        public readonly uint[] indices =
        {
            0, 1, 3, // Triangle 1
            1, 2, 3  // Triangle 2
        };

        public Transform transform { get; private set; }
        public bool isDirty { get; set; }
        public bool isLoaded { get; set; }

        private BufferLayout layout;

        public BatchedSpriteRenderer(Transform _transform)
        {
            transform = _transform;
            BatchManager.staticObjectBatch.AddRenderer(this);

            layout = new BufferLayout();
            layout.Add<float>(2);
            layout.Add<float>(2);
            layout.Add<float>(4);
            layout.Add<float>(1);
        }

        public void Load()
        {
            // This renderer does not need to load, as all rendering is done in the batch object
        }

        public void Render()
        {
            // This renderer does not need to render anything itself
        }

        /// <summary>
        /// Sets a specific value in every vertex.
        /// </summary>
        /// <param name="_type">The type of attribute to edit.</param>
        /// <param name="_value">The value to set.</param>
        public void SetVertexValueAll(int _type, float _value)
        {
            if (!isLoaded)
            {
                Console.WriteLine("Trying to set vertex data when renderer has not been initialized yet.");
                return;
            }

            int loops = vertices.Length / layout.GetRawStride();

            // Setting the value for each vertex
            for (int i = 0; i < loops; i++)
            {
                vertices[(i * layout.GetRawStride()) + _type] = _value;
            }

            // The vertex buffer will be updated before rendering
            isDirty = true;
        }

        /// <summary>
        /// Returns the vertex value of the specified type. Samples from the first vertex by default.
        /// </summary>
        /// <param name="_type">The type of vertex data to return.</param>
        /// <returns></returns>
        public float GetVertexValue(int _type, int _vertex = 0)
        {
            return vertices[(_vertex * layout.GetRawStride()) + _type];
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

        /// <summary>
        /// Sets the UV's of each vertex to display a certain sprite on a spritesheet.
        /// </summary>
        /// <param name="_spritesheet">The spritesheet to get the sprite from.</param>
        /// <param name="_col">The column of the desired sprite (Left to Right).</param>
        /// <param name="_row">The row of the desired sprite (Bottom to Top)</param>
        public void SetSprite(int _spritesheetIndex, int _col, int _row)
        {
            int loops = vertices.Length / layout.GetRawStride();
            Vector2 newUV;
            for (int i = 0; i < loops; i++)
            {
                // Getting the new UV from the spritesheet
                newUV = SpritesheetManager.GetVertexUV(_spritesheetIndex, _col, _row, GetDefaultUV(i));

                // Setting the new UV
                vertices[(i * layout.GetRawStride()) + (int)SpriteVertexAttribute.UvX] = newUV.X;
                vertices[(i * layout.GetRawStride()) + (int)SpriteVertexAttribute.UvY] = newUV.Y;
            }

            // Setting the texture index
            SetVertexValueAll((int)SpriteVertexAttribute.TextureIndex, _spritesheetIndex);
            isDirty = true;
        }

        public Shader GetShader()
        {
            return BatchManager.staticObjectBatch.shader;
        }
    }
}
