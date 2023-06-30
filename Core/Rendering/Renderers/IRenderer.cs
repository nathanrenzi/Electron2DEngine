using Electron2D.Core.Rendering.Shaders;

namespace Electron2D.Core.Rendering
{
    public interface IRenderer
    {
        public void Load();
        public void Render();
        public void SetVertexValueAll(int _type, float _value);
        public float GetVertexValue(int _type, int _vertex = 0);
        public void SetSprite(int _spritesheetIndex, int _col, int _row);
        public Shader GetShader();

        /// <summary>
        /// This is true if the vertex data has been updated this frame
        /// </summary>
        public bool isDirty { get; set; }
        public bool isLoaded { get; set; }
    }
}
