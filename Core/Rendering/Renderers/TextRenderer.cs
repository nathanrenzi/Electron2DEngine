using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.Rendering.Text;
using System.Drawing;
using static Electron2D.OpenGL.GL;
using FreeTypeSharp.Native;
using static FreeTypeSharp.Native.FT;
using System.Runtime.InteropServices;
using System.Numerics;
using System.Text;
using static System.Formats.Asn1.AsnWriter;
using System.Text.RegularExpressions;

namespace Electron2D.Core.Rendering.Renderers
{
    public class TextRenderer
    {
        public FontGlyphStore FontGlyphStore;
        public Shader TextShader;
        public int OutlineWidth => FontGlyphStore.Arguments.OutlineWidth;
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                UpdateTextFormatting(value);
            }
        }
        private string text;
        public float Scale = 1;
        public float LineHeightMultiplier = 1.35f;
        public TextAlignment HorizontalAlignment;
        public TextAlignment VerticalAlignment;
        public TextAlignmentMode AlignmentMode;
        public Color TextColor;
        public Color OutlineColor;
        public Vector2 Position;
        public Rectangle Bounds;
        private Sprite s1, s2, s3, s4; // testing sprites, remove these

        private List<int> lineOffsets = new List<int>(); // Stores the pixel distance between the end of the line and the right bound
        private string formattedText;
        private float totalYHeight;
        private float firstLineMaxHeight = 0;
        private float lastLineMinHeight = 0;
        private uint VAO, VBO;

        public unsafe TextRenderer(FontGlyphStore _fontGlyphStore, Shader _shader, string _text, Vector2 _position,
            Color _textColor, Color _outlineColor, Rectangle _bounds,
            TextAlignment _horizontalAlignment = TextAlignment.Left,
            TextAlignment _verticalAlignment = TextAlignment.Top,
            TextAlignmentMode _alignmentMode = TextAlignmentMode.Baseline)
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
            TextColor = _textColor;
            OutlineColor = _outlineColor;
            Bounds = _bounds;
            Text = _text;
            HorizontalAlignment = _horizontalAlignment;
            VerticalAlignment = _verticalAlignment;
            AlignmentMode = _alignmentMode;

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
        private unsafe void UpdateTextFormatting(string _inputText)
        {
            // Split input text into substrings
            string[] words = Regex.Split(_inputText, @"(\s)");

            // Loop through all words and check if they overlap with boundary
            StringBuilder builder = new StringBuilder();
            float _x = Bounds.X;
            float _y = Bounds.Y;
            uint previousIndex = 0;
            uint glyphIndex = 0;
            float maxHeight = 0;
            float minHeight = 0;
            int newlineCount = 0;
            for (int w = 0; w < words.Length; w++)
            {
                previousIndex = FT_Get_Char_Index(FontGlyphStore.Face, ' ');
                for (int i = 0; i < words[w].Length; i++)
                {
                    Character ch = FontGlyphStore.Characters[words[w][i]];
                    glyphIndex = FT_Get_Char_Index(FontGlyphStore.Face, words[w][i]);

                    // If first line, record tallest character
                    if (newlineCount == 0)
                    {
                        if (ch.Bearing.Y > maxHeight) maxHeight = ch.Bearing.Y;
                    }

                    // Set min if the lowest point is lower in position (greater in value) than the current min
                    if (ch.Size.Y - ch.Bearing.Y > minHeight) minHeight = ch.Size.Y - ch.Bearing.Y;

                    // If word is a newline character, handle it separately
                    // DOES NOT WORK CURRENLY, NEWLINES ARE NOT SEPARATE WORDS
                    if (words[w] == "\n")
                    {
                        _x = Bounds.X;
                        _y -= FontGlyphStore.Arguments.FontSize * LineHeightMultiplier;
                        newlineCount++;
                        lineOffsets.Add((Bounds.Width + Bounds.X) - (int)_x);

                        // Reset minimum height since a new line was created
                        minHeight = 0;
                        break;
                    }

                    // Kerning
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

                    // If the character overlaps the bounds, move to new line
                    if (!Bounds.Contains(new Rectangle((int)_x, (int)_y, (int)(ch.Size.X + ch.Bearing.X), (int)ch.Size.Y)))
                    {
                        _x = Bounds.X;
                        _y -= FontGlyphStore.Arguments.FontSize * LineHeightMultiplier;
                        newlineCount++;
                        lineOffsets.Add(Bounds.Width - (int)_x);

                        minHeight = 0;
                        builder.Append('\n');
                        break;
                    }

                    // Advancing x position
                    _x += ch.Advance * Scale;

                    previousIndex = glyphIndex;
                }

                builder.Append(words[w]);
            }

            totalYHeight = Math.Abs(_y);
            firstLineMaxHeight = AlignmentMode == TextAlignmentMode.Geometry ? maxHeight : 0;
            lastLineMinHeight = AlignmentMode == TextAlignmentMode.Geometry ? minHeight : 0;
            formattedText = builder.ToString();

            Debug.Log(totalYHeight + " " + firstLineMaxHeight + " " + lastLineMinHeight + " " + formattedText);
        }

        private int GetXOffset(int _iteration)
        {
            if (lineOffsets.Count > _iteration)
            {
                switch (HorizontalAlignment)
                {
                    case TextAlignment.Left:
                        return 0;
                    case TextAlignment.Center:
                        return lineOffsets[_iteration] / 2;
                    case TextAlignment.Right:
                        return lineOffsets[_iteration];
                    default: return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        private int GetYOffset()
        {
            switch (VerticalAlignment)
            {
                case TextAlignment.Top:
                    return 0 + (int)firstLineMaxHeight;
                case TextAlignment.Center:
                    return (int)(Bounds.Height - totalYHeight - firstLineMaxHeight - lastLineMinHeight) / 2;
                case TextAlignment.Bottom:
                    return Bounds.Height - (int)(totalYHeight + lastLineMinHeight);
                default: return 0;
            }
        }
        #endregion

        #region Rendering
        public unsafe void Render()
        {
            TextShader.Use();
            TextShader.SetMatrix4x4("projection", Camera2D.main.GetUnscaledProjectionMatrix());
            TextShader.SetColor("mainColor", TextColor);
            TextShader.SetColor("outlineColor", OutlineColor);
            glActiveTexture(GL_TEXTURE0);
            glBindVertexArray(VAO);

            float x = GetXOffset(0) + Position.X;
            float _x = x;
            float _y = Position.Y - GetYOffset();
            uint previousIndex = FT_Get_Char_Index(FontGlyphStore.Face, ' ');
            uint glyphIndex = 0;
            int newlineCount = 0;
            for (int i = 0; i < formattedText.Length; i++)
            {
                Character ch = FontGlyphStore.Characters[formattedText[i]];
                glyphIndex = FT_Get_Char_Index(FontGlyphStore.Face, formattedText[i]);

                // If word is a newline character, handle it separately
                if (formattedText[i] == '\n')
                {
                    newlineCount++;
                    _x = GetXOffset(newlineCount);
                    _y -= FontGlyphStore.Arguments.FontSize * LineHeightMultiplier;
                    previousIndex = FT_Get_Char_Index(FontGlyphStore.Face, ' ');
                    continue;
                }

                // Kerning
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

                float xpos = _x + ch.Bearing.X * Scale;
                float ypos = _y - (ch.Size.Y - ch.Bearing.Y) * Scale; // Causes text to be slightly vertically offset by 1 pixel

                float w = ch.Size.X * Scale;
                float h = ch.Size.Y * Scale;

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

                _x += ch.Advance * Scale;

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