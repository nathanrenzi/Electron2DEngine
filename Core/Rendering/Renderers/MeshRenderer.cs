using Electron2D.Core.ECS;
using System.Numerics;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    public class MeshRendererSystem : BaseSystem<MeshRenderer> { }
    /// <summary>
    /// A multi-purpose mesh renderer.
    /// </summary>
    public class MeshRenderer : Component
    {
        public float[] vertices;
        public uint[] indices;

        public VertexBuffer vertexBuffer;
        public VertexArray vertexArray;
        public IndexBuffer indexBuffer;
        public BufferLayout layout;
        public Material Material;
        public int RenderLayer;

        private Transform transform;

        /// <summary>
        /// If enabled, the object will not move in world space, but will instead stay in one place in screen space.
        /// </summary>
        public bool UseUnscaledProjectionMatrix = false;
        public bool HasVertexData { get; private set; } = false;
        public bool IsDirty { get; set; } = false;
        public bool IsLoaded { get; set; } = false;

        public bool UseStencilBuffer { get; set; } = false;
        public bool StencilPreClearBuffer { get; set; } = false;
        public int StencilFail { get; set; } = GL_KEEP;
        public int StencilPass { get; set; } = GL_KEEP;
        public uint StencilMask { get; set; } = 0x00;
        public int StencilFunction { get; set; } = GL_ALWAYS;
        public int StencilReference { get; set; } = 1;
        public uint StencilFunctionMask { get; set; } = 0xFF;

        public MeshRenderer(Transform _transform, Material _material)
        {
            transform = _transform;
            Material = _material;

            MeshRendererSystem.Register(this);
        }

        protected override void OnDispose()
        {
            vertexBuffer.Dispose();
            vertexArray.Dispose();
            indexBuffer.Dispose();

            MeshRendererSystem.Unregister(this);
        }

        #region Materials
        public void SetMaterial(Material _material)
        {
            Material = _material;
        }

        public Material GetMaterial() => Material;
        #endregion

        #region Vertex Manipulation

        /// <summary>
        /// This must be called to initialize the renderer.
        /// </summary>
        /// <param name="_vertices"></param>
        /// <param name="_indices"></param>
        /// <param name="_loadOnSetArrays">Should the renderer be loaded when the arrays have been set (Should usually be left as true).</param>
        public void SetVertexArrays(float[] _vertices, uint[] _indices, bool _loadOnSetArrays = true, bool _setDirty = false)
        {
            vertices = _vertices;
            indices = _indices;
            //defaultUV = _defaultUV;

            HasVertexData = true;
            if (_loadOnSetArrays) Load();
            if (_setDirty) IsDirty = true;
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

        #endregion

        /// <summary>
        /// Loads all resources necessary for the renderer, such as the shader and buffers.
        /// </summary>
        public void Load()
        {
            if (!HasVertexData) return;

            vertexArray = new VertexArray();
            vertexBuffer = new VertexBuffer(vertices);
            indexBuffer = new IndexBuffer(indices);

            // Telling the vertex array how the vertices are structured
            layout = new BufferLayout();
            layout.Add<float>(2); // Position
            layout.Add<float>(2); // UV

            vertexArray.AddBuffer(vertexBuffer, layout);

            IsLoaded = true;
        }

        public unsafe void Render()
        {
            if (!HasVertexData) return;
            if (!IsLoaded || Material.Shader.Compiled == false) return;

            if (IsDirty)
            {
                // Huge memory leak when creating new buffer instead of updating data.
                vertexBuffer.UpdateData(vertices);
                // If index array also needs to be updated at some point, add a check here for that
                IsDirty = false;
            }

            if(UseStencilBuffer)
            {
                //https://learnopengl.com/Advanced-OpenGL/Stencil-testing
                if (StencilPreClearBuffer)
                    glClear(GL_STENCIL_BUFFER_BIT);

                glStencilOp(StencilFail, StencilPass, StencilPass);
                glStencilFunc(StencilFunction, StencilReference, StencilMask);
                glStencilMask(StencilMask);
            }
            else
            {
                // Don't update the buffer
                glStencilOp(GL_KEEP, GL_KEEP, GL_KEEP);
                glStencilFunc(GL_ALWAYS, 0, 0xFF);
                glStencilMask(0x00);
            }

            Material.Use();
            Material.Shader.SetMatrix4x4("model", transform.GetScaleMatrix() * transform.GetRotationMatrix() * transform.GetPositionMatrix()); // MUST BE IN ORDER
            vertexArray.Bind();
            indexBuffer.Bind();

            Material.Shader.SetMatrix4x4("projection", UseUnscaledProjectionMatrix ? Camera2D.main.GetUnscaledProjectionMatrix() : Camera2D.main.GetProjectionMatrix()); // MUST be set after Use is called

            glDrawElements(GL_TRIANGLES, indices.Length, GL_UNSIGNED_INT, (void*)0);
        }
    }

    /// <summary>
    /// This enum corresponds to an attribute in a vertex for the renderer.
    /// </summary>
    public enum MeshVertexAttribute
    {
        PositionX = 0,
        PositionY = 1,
        UvX = 2,
        UvY = 3,
    }
}