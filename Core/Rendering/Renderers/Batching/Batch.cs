using Electron2D.Core.GameObjects;
using Electron2D.Core.Rendering.Shaders;
using System.Numerics;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    /// <summary>
    /// Allows for multiple objects to be rendered with one draw call. Can only support one shader and one render layer per batch.
    /// </summary>
    public class Batch : IRenderable
    {
        public int renderLayer;

        public List<BatchedSpriteRenderer> renderers { get; private set; } = new List<BatchedSpriteRenderer>();
        public Shader shader;
        public bool isDirty { get; set; }

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

            vertexArray = new VertexArray();

            layout = new BufferLayout();
            layout.Add<float>(4); // Position
            layout.Add<float>(2); // UV
            layout.Add<float>(4); // Color
            layout.Add<float>(1); // Texture Index

            var textureSampleUniformLocation = shader.GetUniformLocation("u_Texture[0]");
            int[] samplers = new int[3] { 0, 1, 2 };
            glUniform1iv(textureSampleUniformLocation, samplers.Length, samplers);

            RenderLayerManager.OrderRenderable(this);
        }

        ~Batch()
        {
            RenderLayerManager.RemoveRenderable(this);
        }

        private unsafe void OnUpdate()
        {
            //CreateNewBufferData();
            //return;

            // If a renderer has been added or removed, new buffers will be created since buffers cannot be resized
            if (isDirty)
            {
                CreateNewBufferData();
                isDirty = false;
                for (int i = 0; i < renderers.Count; i++)
                {
                    renderers[i].isDirty = false;
                }

                return;
            }

            // Looping through every renderer to find any updated data
            bool foundDirty = false;
            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i].isDirty || renderers[i].transform.isDirty)
                {
                    //bufferUpdates.Add(new BufferUpdateItem(i * layout.GetRawStride(), renderers[i].vertices));
                    renderers[i].isDirty = false;
                    renderers[i].transform.UnflagDirty();
                    foundDirty = true;
                }
            }

            // Only if there was a renderer who's data was updated, update the data -- TEMP DISABLED BECAUSE OF ERRORS WITH SUBBING DATA
            if (foundDirty)
            {
                //// Applying the updated data
                //vertexBuffer.Bind();
                //for (int i = 0; i < bufferUpdates.Count; i++)
                //{
                //    BufferUpdateItem item = bufferUpdates[i];
                //    fixed (float* v = &item.data[0])
                //        glBufferSubData(GL_ARRAY_BUFFER, item.offset, item.data.Length * sizeof(float), v);
                //}
                CreateNewBufferData();
            }
        }

        public int GetRenderLayer() => renderLayer;

        private void CreateNewBufferData()
        {
            tempVertexBuffer.Clear();
            tempIndexBuffer.Clear();

            // Add renderer culling so that if all vertices are off screen and not off screen in opposing directions

            for (int i = 0; i < renderers.Count; i++)
            {
                int loops = renderers[i].vertices.Length / layout.GetRawStride();
                for (int x = 0; x < loops; x++)
                {
                    int stride = x * layout.GetRawStride();
                    Vector2 localPos = new Vector2(renderers[i].vertices[0 + stride], renderers[i].vertices[1 + stride]);
                    Vector2 scale = new Vector2(Program.game.currentWindowWidth * (renderers[i].transform.scale.X / 100f), Program.game.currentWindowWidth * (renderers[i].transform.scale.Y / 100f));

                    // Rotation
                    Vector2 pivotPoint = renderers[i].transform.pivotPoint;
                    float rot = renderers[i].transform.rotation * (MathF.PI / 180);
                    Vector2 rotationVector = RotateVertexAroundLocalPoint(rot, renderers[i].transform, localPos, pivotPoint);
                    localPos = rotationVector;
                    // -------------

                    // Scale
                    localPos.X *= scale.X;
                    localPos.Y *= scale.Y;
                    // -------------

                    // Position
                    Vector2 currentPos = new Vector2(renderers[i].transform.position.X * Game.WINDOW_SCALE, renderers[i].transform.position.Y * Game.WINDOW_SCALE);
                    localPos += currentPos;
                    // -------------

                    tempVertexBuffer.Add(localPos.X);
                    tempVertexBuffer.Add(localPos.Y);
                    tempVertexBuffer.Add(0);
                    tempVertexBuffer.Add(1);
                    for (int k = 4; k < layout.GetRawStride(); k++)
                    {
                        tempVertexBuffer.Add(renderers[i].vertices[k + stride]);
                    }
                }

                for (int z = 0; z < renderers[i].indices.Length; z++)
                {
                    tempIndexBuffer.Add((uint)(i * 4) + renderers[i].indices[z]);
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
            shader.SetMatrix4x4("projection", Camera2D.main.GetProjectionMatrix());
            vertexArray.Bind();
            indexBuffer.Bind();

            glDrawElements(GL_TRIANGLES, tempIndexBuffer.Count, GL_UNSIGNED_INT, (void*)0);
        }

        private Vector2 RotateVertexAroundLocalPoint(float _radians, Transform _transform, Vector2 _position, Vector2 _pivotPoint)
        {
            // Using the negative of radians to get positive clockwise rotation
            float s = (float)Math.Sin(-_radians);
            float c = (float)Math.Cos(-_radians);

            Vector2 localPivot = (_transform.up * _pivotPoint.Y) + (_transform.right * _pivotPoint.X);

            // Calculating new coordinate values
            float xnew = c * (_position.X - localPivot.X) - s * (_position.Y - localPivot.Y) + localPivot.X;
            float ynew = s * (_position.X - localPivot.X) + c * (_position.Y - localPivot.Y) + localPivot.Y;

            return new Vector2(xnew, ynew);
        }
    }
}
