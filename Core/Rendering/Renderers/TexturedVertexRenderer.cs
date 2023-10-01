using Electron2D.Core.GameObjects;
using Electron2D.Core.Management.Textures;
using System.Numerics;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    /// <summary>
    /// A renderer that specializes in rendering textured vertex objects.
    /// </summary>
    public class TexturedVertexRenderer : IRenderer
    {
        private float[] vertices;
        private uint[] indices;
        private float[] defaultUV;

        private VertexBuffer vertexBuffer;
        private VertexArray vertexArray;
        private IndexBuffer indexBuffer;
        private BufferLayout layout;
        private Transform transform;

        private Material material;

        /// <summary>
        /// If enabled, the object will not move in world space, but will instead stay in one place in screen space.
        /// </summary>
        public bool UseUnscaledProjectionMatrix = true;
        public bool HasVertexData { get; private set; } = false;
        public bool IsDirty { get; set; } = false;
        public bool IsLoaded { get; set; } = false;
        public int SpriteCol { get; private set; }
        public int SpriteRow { get; private set; }
        public int SpriteIndex { get; private set; } = -1;

        public TexturedVertexRenderer(Transform _transform, Material _material)
        {
            transform = _transform;
            material = _material;
        }

        #region Materials
        public void SetMaterial(Material _material)
        {
            material = _material;
        }

        public Material GetMaterial() => material;
        #endregion

        #region Vertex Manipulation

        /// <summary>
        /// This must be called to initialize the renderer.
        /// </summary>
        /// <param name="_vertices"></param>
        /// <param name="_indices"></param>
        /// <param name="_defaultUV"></param>
        /// <param name="_loadOnSetArrays">Should the renderer be loaded when the arrays have been set (Should usually be left as true).</param>
        public void SetVertexArrays(float[] _vertices, uint[] _indices, float[] _defaultUV, bool _loadOnSetArrays = true)
        {
            vertices = _vertices;
            indices = _indices;
            defaultUV = _defaultUV;

            HasVertexData = true;
            if (_loadOnSetArrays) Load();
        }

        /// <summary>
        /// Sets a specific value in every vertex.
        /// </summary>
        /// <param name="_type">The type of attribute to edit.</param>
        /// <param name="_value">The value to set.</param>
        public void SetVertexValueAll(int _type, float _value)
        {
            if (!HasVertexData) return;
            int loops = vertices.Length / layout.GetRawStride();

            // Setting the value for each vertex
            for (int i = 0; i < loops; i++)
            {
                vertices[(i * layout.GetRawStride()) + _type] = _value;
            }

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

        // Will likely be deprecated once new texture system is set up
        public void SetSprite(int _spritesheetIndex, int _col, int _row)
        {
            if (!HasVertexData) return;

            SpriteCol = _col;
            SpriteRow = _row;
            SpriteIndex = _spritesheetIndex;

            int loops = vertices.Length / layout.GetRawStride();
            Vector2 newUV;
            for (int i = 0; i < loops; i++)
            {
                // Getting the new UV from the spritesheet
                newUV = SpritesheetManager.GetVertexUV(_spritesheetIndex, _col, _row, GetDefaultUV(i));

                // Setting the new UV
                vertices[(i * layout.GetRawStride()) + (int)TexturedVertexAttribute.UvX] = newUV.X;
                vertices[(i * layout.GetRawStride()) + (int)TexturedVertexAttribute.UvY] = newUV.Y;
            }

            // Setting the texture index - Removed for now since textures are bound based on materials.
            //SetVertexValueAll((int)TexturedVertexAttribute.TextureIndex, _spritesheetIndex);
            IsDirty = true;
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

        #endregion

        /// <summary>
        /// Loads all resources necessary for the renderer, such as the shader and buffers.
        /// </summary>
        public void Load()
        {
            if (!HasVertexData) return;
            if (!material.Shader.compiled && !material.Shader.CompileShader())
            {
                Console.WriteLine("Failed to compile shader.");
                return;
            }

            vertexArray = new VertexArray();
            vertexBuffer = new VertexBuffer(vertices);
            indexBuffer = new IndexBuffer(indices);

            // Telling the vertex array how the vertices are structured
            layout = new BufferLayout();
            layout.Add<float>(2); // Position
            layout.Add<float>(2); // UV
            layout.Add<float>(4); // Color
            layout.Add<float>(1); // Texture Index

            vertexArray.AddBuffer(vertexBuffer, layout);

            IsLoaded = true;
        }

        public unsafe void Render()
        {
            if (!HasVertexData) return;
            if (!IsLoaded || material.Shader.compiled == false) return;

            if (IsDirty)
            {
                // Huge memory leak when creating new buffer instead of updating data.
                vertexBuffer.UpdateData(vertices);
                // If index array also needs to be updated at some point, add a check here for that
                IsDirty = false;
            }

            material.Use();
            material.Shader.SetMatrix4x4("model", transform.GetScaleMatrix() * transform.GetRotationMatrix() * transform.GetPositionMatrix()); // MUST BE IN ORDER
            vertexArray.Bind();
            indexBuffer.Bind();

            material.Shader.SetMatrix4x4("projection", UseUnscaledProjectionMatrix ? Camera2D.main.GetUnscaledProjectionMatrix() : Camera2D.main.GetProjectionMatrix()); // MUST be set after Use is called

            glDrawElements(GL_TRIANGLES, indices.Length, GL_UNSIGNED_INT, (void*)0);
        }
    }

    /// <summary>
    /// This enum corresponds to an attribute in a vertex for the renderer.
    /// </summary>
    public enum TexturedVertexAttribute
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