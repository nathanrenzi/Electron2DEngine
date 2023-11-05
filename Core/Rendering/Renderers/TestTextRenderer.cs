using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.Rendering.Text;
using System.Drawing;
using System.Numerics;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering.Renderers
{
    public class TestTextRenderer
    {
        public Material Material;
        private uint VAO, VBO;

        public unsafe TestTextRenderer()
        {
            VAO = glGenVertexArray();
            VBO = glGenBuffer();
            glBindVertexArray(VAO);
            glBindBuffer(GL_ARRAY_BUFFER, VBO);
            glBufferData(GL_ARRAY_BUFFER, sizeof(float) * 6 * 4, NULL, GL_DYNAMIC_DRAW);
            glEnableVertexAttribArray(0);
            glVertexAttribPointer(0, 4, GL_FLOAT, false, 4 * sizeof(float), (void*)0);
            glBindBuffer(GL_ARRAY_BUFFER, 0);
            glBindVertexArray(0);
        }

        public unsafe void Render(FontGlyphStore _fgh, Shader _shader, string _text, float _x, float _y, float _scale, Color _color)
        {
            float nx = _x;
            _shader.Use();
            _shader.SetColor("mainColor", _color);
            glActiveTexture(GL_TEXTURE0);
            glBindVertexArray(VAO);

            for (int i = 0; i < _text.Length; i++)
            {
                Character ch = _fgh.Characters[_text[i]];

                float xpos = nx + ch.Bearing.X * _scale;
                float ypos = _y - (ch.Size.Y - ch.Bearing.Y) * _scale;

                float w = ch.Size.X * _scale;
                float h = ch.Size.Y * _scale;

                float[,] vertices = new float[6,4] {
                    { xpos,     ypos + h,   0.0f, 0.0f },
                    { xpos,     ypos,       0.0f, 1.0f },
                    { xpos + w, ypos,       1.0f, 1.0f },

                    { xpos,     ypos + h,   0.0f, 0.0f },
                    { xpos + w, ypos,       1.0f, 1.0f },
                    { xpos + w, ypos + h,   1.0f, 0.0f }
                };

                glBindTexture(GL_TEXTURE_2D, ch.TextureHandle);
                glBindBuffer(GL_ARRAY_BUFFER, VBO);
                fixed (float* ptr = vertices)
                    glBufferSubData(GL_ARRAY_BUFFER, 0, sizeof(float) * 6 * 4, ptr);
                glBindBuffer(GL_ARRAY_BUFFER, 0);
                glDrawArrays(GL_TRIANGLES, 0, 6);

                nx += ch.Advance * _scale;
            }

            glBindVertexArray(0);
            glBindTexture(GL_TEXTURE_2D, 0);
        }
    }
}
