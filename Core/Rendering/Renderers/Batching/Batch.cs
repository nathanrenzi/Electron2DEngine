using Electron2D.Core.Rendering.Shaders;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    public class Batch
    {
        public List<BatchedSpriteRenderer> renderers { get; private set; } = new List<BatchedSpriteRenderer>();
        public Shader shader;
        public bool isDirty;

        private List<float> tempVertexBuffer;
        private List<uint> tempIndexBuffer;

        private VertexArray vertexArray;
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private BufferLayout layout;

        public Batch(Shader _shader)
        {
            shader = _shader;
            Game.onUpdateEvent += OnUpdate;

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
        }

        private void CreateBuffers()
        {
            tempVertexBuffer.Clear();
            tempIndexBuffer.Clear();

            for (int i = 0; i < renderers.Count; i++)
            {
                for (int x = 0; x < renderers[i].vertices.Length; x++)
                {
                    tempVertexBuffer.Add(renderers[i].vertices[x]);
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
            glDrawElements(GL_TRIANGLES, tempIndexBuffer.Count, GL_UNSIGNED_INT, (void*)0);
        }
    }
}
