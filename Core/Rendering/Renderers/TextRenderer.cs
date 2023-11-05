using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.Rendering.Text;
using System.Drawing;
using static Electron2D.OpenGL.GL;
using FreeTypeSharp.Native;
using static FreeTypeSharp.Native.FT;
using System.Runtime.InteropServices;

namespace Electron2D.Core.Rendering.Renderers
{
    public class TextRenderer
    {
        private uint VAO, VBO;

        public unsafe TextRenderer()
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
            _shader.Use();
            _shader.SetColor("mainColor", _color);
            glActiveTexture(GL_TEXTURE0);
            glBindVertexArray(VAO);

            uint previousIndex = 0;
            uint glyphIndex = 0;

            for (int i = 0; i < _text.Length; i++)
            {
                Character ch = _fgh.Characters[_text[i]];
                glyphIndex = FT_Get_Char_Index(_fgh.Face, _text[i]);

                // Kerning (space between characters)
                if (_fgh.UseKerning)
                {
                    if(FT_Get_Kerning(_fgh.Face, previousIndex, glyphIndex, (uint)FT_Kerning_Mode.FT_KERNING_DEFAULT, out FT_Vector delta) == FT_Error.FT_Err_Ok)
                    {
                        long* temp = (long*)delta.x;
                        long res = *temp;
                        _x += res;
                    }
                    else
                    {
                        Debug.LogError($"FREETYPE: Unable to get kerning for font {_fgh.Arguments.FontName}");
                    }
                }

                float xpos = _x + ch.Bearing.X * _scale;
                float ypos = _y - (ch.Size.Y - ch.Bearing.Y) * _scale; // Causes text to be slightly vertically offset by 1 pixel

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

                _x += ch.Advance * _scale;

                previousIndex = glyphIndex;
            }

            glBindVertexArray(0);
            glBindTexture(GL_TEXTURE_2D, 0);
        }
    }
}