using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.Rendering.Text;
using System.Drawing;
using static Electron2D.OpenGL.GL;
using FreeTypeSharp.Native;
using static FreeTypeSharp.Native.FT;
using System.Runtime.InteropServices;
using System.Numerics;

namespace Electron2D.Core.Rendering.Renderers
{
    public class TextRenderer
    {
        public FontGlyphStore FontGlyphStore;
        public Shader TextShader;
        public int OutlineWidth => FontGlyphStore.Arguments.OutlineWidth;

        private uint VAO, VBO;

        public unsafe TextRenderer(FontGlyphStore _fontGlyphStore, Shader _shader)
        {
            FontGlyphStore = _fontGlyphStore;
            TextShader = _shader;

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

        public unsafe void Render(string _text, Vector2 _position, float _scale, Color _textColor, Color _outlineColor)
        {
            TextShader.Use();
            TextShader.SetColor("mainColor", _textColor);
            TextShader.SetColor("outlineColor", _outlineColor);
            glActiveTexture(GL_TEXTURE0);
            glBindVertexArray(VAO);

            uint previousIndex = 0;
            uint glyphIndex = 0;

            float _x = _position.X;
            for (int i = 0; i < _text.Length; i++)
            {
                Character ch = FontGlyphStore.Characters[_text[i]];
                glyphIndex = FT_Get_Char_Index(FontGlyphStore.Face, _text[i]);

                // Kerning (space between certain characters)
                if (FontGlyphStore.UseKerning)
                {
                    if(FT_Get_Kerning(FontGlyphStore.Face, previousIndex, glyphIndex, (uint)FT_Kerning_Mode.FT_KERNING_DEFAULT, out FT_Vector delta) == FT_Error.FT_Err_Ok)
                    {
                        long* temp = (long*)delta.x;
                        long res = *temp;
                        _x += res;
                    }
                    else
                    {
                        Debug.LogError($"FREETYPE: Unable to get kerning for font {FontGlyphStore.Arguments.FontName}");
                    }
                }

                float xpos = _x + ch.Bearing.X * _scale;
                float ypos = _position.Y - (ch.Size.Y - ch.Bearing.Y) * _scale; // Causes text to be slightly vertically offset by 1 pixel

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