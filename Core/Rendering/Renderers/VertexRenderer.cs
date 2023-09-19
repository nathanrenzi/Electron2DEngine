using Electron2D.Core.GameObjects;
using Electron2D.Core.Rendering.Shaders;
using System.Drawing;
using System.Numerics;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    /// <summary>
    /// A renderer that specializes in custom shapes.
    /// </summary>
    public class VertexRenderer : IRenderer
    {
        public float[] vertices { get; private set; }
        public uint[] indices { get; private set; }

        private List<float> tempVertices = new List<float>();
        private List<uint> tempIndices = new List<uint>();

        private VertexBuffer vertexBuffer;
        private VertexArray vertexArray;
        private IndexBuffer indexBuffer;
        private BufferLayout layout;

        private Transform transform;
        private Shader shader;

        /// <summary>
        /// If enabled, the object will not move in world space, but will instead stay in one place in screen space.
        /// </summary>
        public bool useUnscaledProjectionMatrix = false;
        public bool IsDirty { get; set; } = false;
        public bool IsLoaded { get; set; } = false;

        public VertexRenderer(Transform _transform, Shader _shader)
        {
            transform = _transform;
            shader = _shader;
        }

        public Shader GetShader() => shader;

        /// <summary>
        /// Clears the temp vertices and indices lists.
        /// </summary>
        public void ClearTempLists()
        {
            tempVertices.Clear();
            tempIndices.Clear();
        }

        /// <summary>
        /// Adds a vertex to the temp vertex list, which will be used when FinalizeVertices() is called.
        /// </summary>
        /// <param name="_position">The position of the vertex, from 1,1 to -1,-1.</param>
        /// <param name="_color">The color of the vertex.</param>
        public void AddVertex(Vector2 _position, Color _color)
        {
            tempVertices.Add(_position.X);
            tempVertices.Add(_position.Y);
            // Must divide the colors by 255 to normalize them
            tempVertices.Add(_color.R / 255f);
            tempVertices.Add(_color.G / 255f);
            tempVertices.Add(_color.B / 255f);
            tempVertices.Add(_color.A / 255f);
        }

        /// <summary>
        /// Adds a triangle to the temp indices list, which will be used when FinalizeVertices() is called.
        /// </summary>
        /// <param name="_vert1">The first vertex in the triangle.</param>
        /// <param name="_vert2">The second vertex in the triangle.</param>
        /// <param name="_vert3">The third vertex in the triangle.</param>
        /// <param name="_offset">The offset which is added to the previous indices to specify which vertices are being referenced.
        /// EX: 0, 1, 2 with an offset of 10 would be 10, 11, 12 in the vertex array.</param>
        public void AddTriangle(uint _vert1, uint _vert2, uint _vert3, uint _offset)
        {
            tempIndices.Add(_vert1 + _offset);
            tempIndices.Add(_vert2 + _offset);
            tempIndices.Add(_vert3 + _offset);
        }

        /// <summary>
        /// Sends the current temp vertices and indices to their respective arrays, ready for loading.
        /// </summary>
        public void FinalizeVertices()
        {
            vertices = tempVertices.ToArray();
            indices = tempIndices.ToArray();
            IsDirty = true;
        }

        /// <summary>
        /// Loads all resources necessary for the renderer, such as the shader and buffers.
        /// </summary>
        public void Load()
        {
            if (!shader.CompileShader())
            {
                Console.WriteLine("Failed to compile shader.");
                return;
            }

            vertexArray = new VertexArray();
            vertexBuffer = new VertexBuffer(vertices);

            layout = new BufferLayout();
            layout.Add<float>(2); // Position
            layout.Add<float>(4); // Color

            vertexArray.AddBuffer(vertexBuffer, layout);
            shader.Use();
            indexBuffer = new IndexBuffer(indices);

            IsLoaded = true;
        }

        /// <summary>
        /// Sets a specific value in every vertex.
        /// </summary>
        /// <param name="_type">The type of attribute to edit.</param>
        /// <param name="_value">The value to set.</param>
        public void SetVertexValueAll(int _type, float _value)
        {
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

        public void SetSprite(int _spritesheetIndex, int _col, int _row) => Console.WriteLine("Trying to set sprite with a vertex renderer does not do anything.");

        public unsafe void Render()
        {
            if (!IsLoaded || shader.compiled == false) return;

            if (IsDirty)
            {
                // Setting a new vertex buffer if the vertices have been updated
                indexBuffer = new IndexBuffer(indices);
                vertexBuffer = new VertexBuffer(vertices);
                vertexArray.AddBuffer(vertexBuffer, layout);
                IsDirty = false;
            }

            shader.Use();
            shader.SetMatrix4x4("model", transform.GetScaleMatrix() * transform.GetRotationMatrix() * transform.GetPositionMatrix()); // MUST BE IN ORDER
            vertexArray.Bind();
            indexBuffer.Bind();

            shader.SetMatrix4x4("projection", useUnscaledProjectionMatrix ? Camera2D.main.GetUnscaledProjectionMatrix() : Camera2D.main.GetProjectionMatrix()); // MUST be set after Use is called

            glDrawElements(GL_TRIANGLES, indices.Length, GL_UNSIGNED_INT, (void*)0);
        }
    }

    /// <summary>
    /// This enum corresponds to an attribute in a vertex for the vertex renderer.
    /// </summary>
    public enum VertexAttribute
    {
        PositionX = 0,
        PositionY = 1,
        ColorR = 2,
        ColorG = 3,
        ColorB = 4,
        ColorA = 5
    }
}