using Electron2D.Core.GameObjects;
using Electron2D.Core.Rendering.Shaders;
using System;
using System.Numerics;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    /// <summary>
    /// Allows for multiple objects to be rendered with one draw call. Can only support one shader and one render layer per batch.
    /// </summary>
    public class Batch
    {
        public int renderLayer;

        public List<BatchedSpriteRenderer> renderers { get; private set; } = new List<BatchedSpriteRenderer>();
        public Shader shader;
        public bool isDirty;

        private List<float> tempVertexBuffer;
        private List<uint> tempIndexBuffer;
        private List<Matrix4x4> tempModelList;

        private VertexArray vertexArray;
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private BufferLayout layout;

        public Batch(Shader _shader)
        {
            shader = _shader;
            Game.onUpdateEvent += OnUpdate;
            GameObjectManager.onLayerRendered += OnRender;

            vertexArray = new VertexArray();

            layout = new BufferLayout();
            layout.Add<float>(2); // Position
            layout.Add<float>(2); // UV
            layout.Add<float>(4); // Color
            layout.Add<float>(1); // Texture Index
            layout.Add<float>(1); // Matrix Index
        }

        private void OnUpdate()
        {
            if (isDirty)
            {
                isDirty = false;
                CreateBuffers();
            }

            // use an SSBO to store all object matrices
        }

        private void OnRender(int _layer)
        {
            if(_layer == renderLayer)
            {
                Render();
            }
        }

        private void CreateBuffers()
        {
            tempVertexBuffer.Clear();
            tempIndexBuffer.Clear();
            tempModelList.Clear();

            for (int i = 0; i < renderers.Count; i++)
            {
                // The model matrix is being added to the temp model list
                tempModelList.Add(renderers[i].transform.GetScaleMatrix() * renderers[i].transform.GetRotationMatrix() * renderers[i].transform.GetPositionMatrix());

                for (int x = 0; x < renderers[i].vertices.Length; x++)
                {
                    tempVertexBuffer.Add(renderers[i].vertices[x]);
                    tempVertexBuffer.Add(i); // This is the model matrix index
                }

                for (int z = 0; z < renderers[i].indices.Length; z++)
                {
                    tempIndexBuffer.Add(renderers[i].indices[z]);
                }
            }

            vertexBuffer = new VertexBuffer(tempVertexBuffer.ToArray());
            indexBuffer = new IndexBuffer(tempIndexBuffer.ToArray());

            vertexArray.AddBuffer(vertexBuffer, layout);
        }

        public void AddRenderer(BatchedSpriteRenderer _renderer)
        {
            if (renderers.Contains(_renderer))
            {
                Console.WriteLine("Renderer is already in this batch, cannot add.");
                return;
            }

            isDirty = true;
            renderers.Add(_renderer);
        }

        public void RemoveRenderer(BatchedSpriteRenderer _renderer)
        {
            if (!renderers.Remove(_renderer))
            {
                Console.WriteLine("Renderer is not a part of this batch, cannot remove.");
                return;
            }

            isDirty = true;
        }

        public unsafe void Render()
        {
            shader.Use();
            vertexArray.Bind();
            indexBuffer.Bind();

            shader.SetMatrix4x4("projection", Camera2D.main.GetProjectionMatrix());

            glDrawElements(GL_TRIANGLES, tempIndexBuffer.Count, GL_UNSIGNED_INT, (void*)0);
        }
    }
}
