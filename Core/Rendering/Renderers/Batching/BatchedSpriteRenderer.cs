using Electron2D.Core.GameObjects;
using Electron2D.Core.Rendering.Shaders;

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

        public BatchedSpriteRenderer(Transform _transform)
        {
            transform = _transform;
            BatchManager.spriteRenderBatch.AddRenderer(this);
        }

        public void Load() // not implemented
        {
            
        }

        public void Render() // not implemented
        {
            
        }

        public void SetVertexValueAll(int _type, float _value) // not implemented
        {
            
        }

        public float GetVertexValue(int _type, int _vertex = 0) // not implemented
        {
            return 0;
        }

        public void SetSprite(int _spritesheetIndex, int _col, int _row) // not implemented
        {
            
        }

        public Shader GetShader()
        {
            return BatchManager.spriteRenderBatch.shader;
        }
    }
}
