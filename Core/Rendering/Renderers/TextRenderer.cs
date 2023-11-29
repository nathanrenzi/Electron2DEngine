using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.Rendering.Text;
using System.Drawing;
using static Electron2D.OpenGL.GL;
using FreeTypeSharp.Native;
using static FreeTypeSharp.Native.FT;
using System.Runtime.InteropServices;
using System.Numerics;
using System.Text;

namespace Electron2D.Core.Rendering.Renderers
{
    public class TextRenderer
    {
        public FontGlyphStore FontGlyphStore;
        public Shader TextShader;
        public int OutlineWidth => FontGlyphStore.Arguments.OutlineWidth;
        public float LineHeightMultiplier = 1.35f;
        public TextAlignment HorizontalAlignment;
        public TextAlignment VerticalAlignment;
        public Vector2 Position;
        public Rectangle Bounds;
        private Sprite s1, s2, s3, s4; // testing sprites

        private uint VAO, VBO;
        private List<int> xOffsets = new List<int>(); // Stores the pixel distance between the end of the line and the right bound

        public unsafe TextRenderer(FontGlyphStore _fontGlyphStore, Shader _shader, Vector2 _position, Rectangle _bounds, TextAlignment _horizontalAlignment = TextAlignment.Left, TextAlignment _verticalAlignment = TextAlignment.Top)
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

            Position = _position;
            Bounds = _bounds;
            HorizontalAlignment = _horizontalAlignment;
            VerticalAlignment = _verticalAlignment;

            Shader shader = new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultTexture.glsl"));
            s1 = new Sprite(Material.Create(shader, Color.Black));
            s1.Renderer.UseUnscaledProjectionMatrix = true;
            s2 = new Sprite(Material.Create(shader, Color.Black));
            s2.Renderer.UseUnscaledProjectionMatrix = true;
            s3 = new Sprite(Material.Create(shader, Color.Black));
            s3.Renderer.UseUnscaledProjectionMatrix = true;
            s4 = new Sprite(Material.Create(shader, Color.Black));
            s4.Renderer.UseUnscaledProjectionMatrix = true;
            s1.Transform.Position = new Vector2(Position.X, Position.Y);
            s1.Transform.Scale = new Vector2(4);
            s2.Transform.Position = new Vector2(Position.X + Bounds.Size.Width, Position.Y);
            s2.Transform.Scale = new Vector2(4);
            s3.Transform.Position = new Vector2(Position.X + Bounds.Size.Width, Position.Y - Bounds.Size.Height);
            s3.Transform.Scale = new Vector2(4);
            s4.Transform.Position = new Vector2(Position.X, Position.Y - Bounds.Size.Height);
            s4.Transform.Scale = new Vector2(4);
        }

        #region Text Formatting
        private unsafe string GetTextFormatting(string _text, float _scale)
        {
            StringBuilder builder = new StringBuilder();
            xOffsets.Clear();

            uint previousIndex = 0;
            uint glyphIndex = 0;

            float _x = Bounds.X;
            float _y = Bounds.Y;

            for (int i = 0; i < _text.Length; i++)
            {
                Character ch = FontGlyphStore.Characters[_text[i]];
                glyphIndex = FT_Get_Char_Index(FontGlyphStore.Face, _text[i]);
                builder.Append(_text[i]);

                // Kerning (space between certain characters)
                if (FontGlyphStore.UseKerning)
                {
                    if (FT_Get_Kerning(FontGlyphStore.Face, previousIndex, glyphIndex, (uint)FT_Kerning_Mode.FT_KERNING_DEFAULT, out FT_Vector delta) == FT_Error.FT_Err_Ok)
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

                // Moves to next character position
                _x += ch.Advance * _scale;

                // Newline check -- CHANGE TO MAKE WHOLE WORDS MOVE TO NEW LINE
                bool outsideBounds = !Bounds.Contains(new Rectangle((int)_x, (int)-_y, (int)(ch.Size.X + ch.Bearing.X), (int)ch.Size.Y));
                if (_text[i] == '\n' || outsideBounds)
                {
                    xOffsets.Add((Bounds.Width + Bounds.X) - (int)_x);
                    if(outsideBounds) builder.Append('\n');

                    _x = Bounds.X;
                    _y -= FontGlyphStore.Arguments.FontSize * LineHeightMultiplier;
                    previousIndex = glyphIndex;
                    continue;
                }
                else if (i + 1 == _text.Length)
                {
                    xOffsets.Add((Bounds.Width + Bounds.X) - (int)_x);
                    break;
                }
            }

            return builder.ToString();
        }

        private int GetXOffsetPosition(int _iteration)
        {
            if (xOffsets.Count > _iteration)
            {
                switch (HorizontalAlignment)
                {
                    case TextAlignment.Left:
                        return 0;
                    case TextAlignment.Center:
                        return xOffsets[_iteration] / 2;
                    case TextAlignment.Right:
                        return xOffsets[_iteration];
                    default: return 0;
                }
            }
            else
            {
                return 0;
            }
        }
        #endregion

        #region Rendering
        public unsafe void Render(string _text, float _scale, Color _textColor, Color _outlineColor)
        {
            TextShader.Use();
            TextShader.SetColor("mainColor", _textColor);
            TextShader.SetColor("outlineColor", _outlineColor);
            glActiveTexture(GL_TEXTURE0);
            glBindVertexArray(VAO);

            uint previousIndex = 0;
            uint glyphIndex = 0;

            string renderText = GetTextFormatting(_text, _scale);

            int newlineIterations = 0;
            float x = GetXOffsetPosition(newlineIterations) + Position.X;

            float _x = x;
            float _y = Bounds.Y;

            // Rendering loop
            for (int i = 0; i < renderText.Length; i++)
            {
                Character ch = FontGlyphStore.Characters[renderText[i]];
                glyphIndex = FT_Get_Char_Index(FontGlyphStore.Face, renderText[i]);

                if (renderText[i] == '\n')
                {
                    newlineIterations++;
                    _x = GetXOffsetPosition(newlineIterations) + Position.X;

                    _y -= FontGlyphStore.Arguments.FontSize * LineHeightMultiplier;

                    previousIndex = glyphIndex;
                    continue;
                }

                // Kerning (special spacing between certain characters)
                if (FontGlyphStore.UseKerning)
                {
                    if (FT_Get_Kerning(FontGlyphStore.Face, previousIndex, glyphIndex, (uint)FT_Kerning_Mode.FT_KERNING_DEFAULT, out FT_Vector delta) == FT_Error.FT_Err_Ok)
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
                float ypos = _y - (ch.Size.Y - ch.Bearing.Y) * _scale; // Causes text to be slightly vertically offset by 1 pixel

                float w = ch.Size.X * _scale;
                float h = ch.Size.Y * _scale;

                float[,] vertices = new float[6, 4] {
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

            // REMOVE
            s1.Transform.Position = new Vector2(Position.X, Position.Y);
            s1.Transform.Scale = new Vector2(4);
            s2.Transform.Position = new Vector2(Position.X + Bounds.Size.Width, Position.Y);
            s2.Transform.Scale = new Vector2(4);
            s3.Transform.Position = new Vector2(Position.X + Bounds.Size.Width, Position.Y - Bounds.Size.Height);
            s3.Transform.Scale = new Vector2(4);
            s4.Transform.Position = new Vector2(Position.X, Position.Y - Bounds.Size.Height);
            s4.Transform.Scale = new Vector2(4);
        }
        #endregion
    }
}