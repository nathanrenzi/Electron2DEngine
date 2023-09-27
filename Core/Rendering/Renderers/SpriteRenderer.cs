using Electron2D.Core.GameObjects;
using Electron2D.Core.Management;
using Electron2D.Core.Management.Textures;
using Electron2D.Core.Rendering.Shaders;
using System.Numerics;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    /// <summary>
    /// A renderer specializing in rendering textures.
    /// </summary>
    public class SpriteRenderer : IRenderer
    {
        private readonly float[] vertices =
        {
            // Positions    UV            Color                     TexIndex
             1f,  1f,       1.0f, 1.0f,   1.0f, 1.0f, 1.0f, 1.0f,   0.0f,      // top right - red
             1f, -1f,       1.0f, 0.0f,   1.0f, 1.0f, 1.0f, 1.0f,   0.0f,      // bottom right - green
            -1f, -1f,       0.0f, 0.0f,   1.0f, 1.0f, 1.0f, 1.0f,   0.0f,      // bottom left - blue
            -1f,  1f,       0.0f, 1.0f,   1.0f, 1.0f, 1.0f, 1.0f,   0.0f,      // top left - white
        };

        private readonly float[] defaultUV =
        {
            1.0f, 1.0f,
            1.0f, 0.0f,
            0.0f, 0.0f,
            0.0f, 1.0f,
        };

        private readonly uint[] indices =
        {
            0, 1, 3, // Triangle 1
            1, 2, 3  // Triangle 2
        };

        private VertexBuffer vertexBuffer;
        private VertexArray vertexArray;
        private IndexBuffer indexBuffer;
        private BufferLayout layout;

        private Transform transform;
        private Shader shader;

        public bool IsDirty { get; set; } = false;
        public bool IsLoaded { get; set; } = false;
        public bool UseLinearFiltering { get; set; }

        public SpriteRenderer(Transform _transform, Shader _shader)
        {
            transform = _transform;
            shader = _shader;
        }

        public Shader GetShader() => shader;

        /// <summary>
        /// Loads all resources necessary for the renderer, such as the shader and all buffers.
        /// </summary>
        public void Load()
        {
            if (shader.compiled == false && !shader.CompileShader())
            {
                Console.WriteLine("Failed to compile shader.");
                return;
            }

            vertexArray = new VertexArray();
            vertexBuffer = new VertexBuffer(vertices);

            layout = new BufferLayout();
            layout.Add<float>(2);
            layout.Add<float>(2);
            layout.Add<float>(4);
            layout.Add<float>(1);

            vertexArray.AddBuffer(vertexBuffer, layout);
            shader.Use();
            indexBuffer = new IndexBuffer(indices);

            var textureSampleUniformLocation = shader.GetUniformLocation("u_Texture[0]");
            int[] samplers = new int[3] { 0, 1, 2 };
            glUniform1iv(textureSampleUniformLocation, samplers.Length, samplers);

            IsLoaded = true;
        }

        /// <summary>
        /// Sets a specific value in every vertex.
        /// </summary>
        /// <param name="_type">The type of attribute to edit.</param>
        /// <param name="_value">The value to set.</param>
        public void SetVertexValueAll(int _type, float _value)
        {
            if(!IsLoaded)
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
            IsDirty = true;
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
            IsDirty = true;
        }

        public unsafe void Render()
        {
            if (!IsLoaded || shader.compiled == false) return;

            if(IsDirty)
            {
                // Updating the vertex buffer
                vertexBuffer.UpdateData(vertices);
                IsDirty = false;
            }

            shader.Use();
            shader.SetMatrix4x4("model", transform.GetScaleMatrix() * transform.GetRotationMatrix() * transform.GetPositionMatrix()); // MUST BE IN ORDER
            vertexArray.Bind();
            indexBuffer.Bind();

            shader.SetMatrix4x4("projection", Camera2D.main.GetProjectionMatrix()); // MUST be set after Use is called

            RenderLayerManager.SetTextureFiltering(UseLinearFiltering);
            glDrawElements(GL_TRIANGLES, indices.Length, GL_UNSIGNED_INT, (void*)0);
        }
    }
    
    /// <summary>
    /// This enum corresponds to an attribute in a vertex for the sprite renderer.
    /// </summary>
    public enum SpriteVertexAttribute
    {
        PositionX = 0,
        PositionY = 1,
        UvX = 2,
        UvY = 3,
        ColorR = 4,
        ColorG = 5,
        ColorB = 6,
        ColorA = 7,
        TextureIndex = 8
    }
}
