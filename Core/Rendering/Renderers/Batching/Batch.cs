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

        private List<float> tempVertexBuffer = new List<float>();
        private List<uint> tempIndexBuffer = new List<uint>();

        private List<BufferUpdateItem> bufferUpdates = new List<BufferUpdateItem>();

        private VertexArray vertexArray;
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private BufferLayout layout;

        public Batch(Shader _shader, int _renderLayer)
        {
            shader = _shader;
            renderLayer = _renderLayer;

            Game.onUpdateEvent += OnUpdate;
            GameObjectManager.onLayerRendered += OnRender;

            vertexArray = new VertexArray();

            layout = new BufferLayout();
            layout.Add<float>(4); // Position
            layout.Add<float>(2); // UV
            layout.Add<float>(4); // Color
            layout.Add<float>(1); // Texture Index

            var textureSampleUniformLocation = shader.GetUniformLocation("u_Texture[0]");
            int[] samplers = new int[3] { 0, 1, 2 };
            glUniform1iv(textureSampleUniformLocation, samplers.Length, samplers);
        }

        private unsafe void OnUpdate()
        {
            // New buffers must be created every frame, since the vertices must be manually projected
            CreateNewBufferData();
            return;

            // If a renderer has been added or removed, new buffers will be created since buffers cannot be resized
            if (isDirty)
            {
                CreateNewBufferData();
                isDirty = false;
                for (int i = 0; i < renderers.Count; i++)
                {
                    renderers[i].isDirty = false;
                }
            }

            // Looping through every renderer to find any updated data
            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i].isDirty)
                {
                    bufferUpdates.Add(new BufferUpdateItem(i * layout.GetRawStride(), renderers[i].vertices));
                    renderers[i].isDirty = false;
                }
            }

            // Applying the updated data
            vertexBuffer.Bind();
            for (int i = 0; i < bufferUpdates.Count; i++)
            {
                BufferUpdateItem item = bufferUpdates[i];
                fixed (float* v = &item.data[0])
                    glBufferSubData(GL_ARRAY_BUFFER, item.offset, item.data.Length * sizeof(float), v);
            }
        }

        private void OnRender(int _layer)
        {
            if(_layer == renderLayer)
            {
                Render();
            }
        }

        private void CreateNewBufferData()
        {
            tempVertexBuffer.Clear();
            tempIndexBuffer.Clear();

            for (int i = 0; i < renderers.Count; i++)
            {
                // The model matrix is being added to the temp model list
                Matrix4x4 model = renderers[i].transform.GetScaleMatrix() * renderers[i].transform.GetRotationMatrix() * renderers[i].transform.GetPositionMatrix();
                Matrix4x4 projection = Camera2D.main.GetProjectionMatrix();

                Matrix4x4 finalM = projection * model;
                Vector4 m0 = new Vector4(finalM.M11, finalM.M21, finalM.M31, finalM.M41);
                Vector4 m1 = new Vector4(finalM.M12, finalM.M22, finalM.M32, finalM.M42);
                Vector4 m2 = new Vector4(finalM.M13, finalM.M23, finalM.M33, finalM.M43);
                Vector4 m3 = new Vector4(finalM.M14, finalM.M24, finalM.M34, finalM.M44);

                int loops = renderers[i].vertices.Length / layout.GetRawStride();
                for (int x = 0; x < loops; x++)
                {
                    int stride = x * layout.GetRawStride();
                    Vector4 pos = new Vector4(renderers[i].vertices[0 + stride], renderers[i].vertices[1 + stride], 0, 1);

                    Vector4 newPos = new Vector4();
                    newPos.X = m0.X * pos.X + m1.X * pos.Y + m2.X * pos.Z + m3.X * pos.W;
                    newPos.Y = m0.Y * pos.X + m1.Y * pos.Y + m2.Y * pos.Z + m3.Y * pos.W;
                    newPos.Z = m0.Z * pos.X + m1.Z * pos.Y + m2.Z * pos.Z + m3.Z * pos.W;
                    newPos.W = m0.W * pos.X + m1.W * pos.Y + m2.W * pos.Z + m3.W * pos.W;

                    tempVertexBuffer.Add(newPos.X);
                    tempVertexBuffer.Add(newPos.Y);
                    tempVertexBuffer.Add(newPos.Z);
                    tempVertexBuffer.Add(newPos.W);
                    for (int k = 4; k < layout.GetRawStride(); k++)
                    {
                        tempVertexBuffer.Add(renderers[i].vertices[k + stride]);
                    }
                }

                for (int z = 0; z < renderers[i].indices.Length; z++)
                {
                    tempIndexBuffer.Add(renderers[i].indices[z]);
                }
            }

            if (vertexBuffer == null || indexBuffer == null)
            {
                vertexBuffer = new VertexBuffer(tempVertexBuffer.ToArray());
                indexBuffer = new IndexBuffer(tempIndexBuffer.ToArray());

                vertexArray.AddBuffer(vertexBuffer, layout);
            }
            else
            {
                vertexBuffer.UpdateData(tempVertexBuffer.ToArray());
                indexBuffer.UpdateData(tempIndexBuffer.ToArray());
            }
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

            glDrawElements(GL_TRIANGLES, tempIndexBuffer.Count, GL_UNSIGNED_INT, (void*)0);
        }
    }
}
