using Electron2D.Core.ECS;
using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.UserInterface;
using FreeTypeSharp.Native;
using System.Drawing;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using static Electron2D.OpenGL.GL;
using static FreeTypeSharp.Native.FT;

namespace Electron2D.Core.Rendering.Text
{
    public class TextRendererSystem : BaseSystem<TextRenderer> { }
    /// <summary>
    /// Formats and renders text. Use <see cref="TextLabel"/>, which implements this.
    /// </summary>
    public class TextRenderer : Component
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
        public float LineHeightMultiplier = 1.35f;
        public TextAlignment HorizontalAlignment;
        public TextAlignment VerticalAlignment;
        public TextAlignmentMode AlignmentMode;
        public TextOverflowMode OverflowMode;
        public Color TextColor;
        public Color OutlineColor;
        private Transform transform;
        private Vector2 position
        {
            get
            {
                return new Vector2(transform.Position.X, transform.Position.Y);
            }
        }
        public Rectangle Bounds
        {
            get
            {
                return bounds;
            }
            set
            {
                bounds = value;
                UpdateTextFormatting(Text);
            }
        }
        private Rectangle bounds;
        public bool ShowBoundsDebug = false;
        private Sprite s1, s2, s3, s4; // testing sprites, remove these

        private List<int> lineOffsets = new List<int>(); // Stores the pixel distance between the end of the line and the right bound
        private string formattedText;
        private float totalYHeight;
        private float firstLineMaxHeight = 0;
        private float lastLineMinHeight = 0;
        private uint VAO, VBO;

        public unsafe TextRenderer(Transform _transform, FontGlyphStore _fontGlyphStore, Shader _shader, string _text,
            Vector2 _bounds, Color _textColor, Color _outlineColor,
            TextAlignment _horizontalAlignment = TextAlignment.Left,
            TextAlignment _verticalAlignment = TextAlignment.Top,
            TextAlignmentMode _alignmentMode = TextAlignmentMode.Baseline,
            TextOverflowMode _overflowMode = TextOverflowMode.Word)
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

            transform = _transform;
            TextColor = _textColor;
            OutlineColor = _outlineColor;
            Bounds = new Rectangle(0, 0, (int)_bounds.X, (int)_bounds.Y);
            HorizontalAlignment = _horizontalAlignment;
            VerticalAlignment = _verticalAlignment;
            AlignmentMode = _alignmentMode;
            OverflowMode = _overflowMode;

            // This must be the last thing initialized, as it will reformat the text
            Text = _text;

            Shader shader = GlobalShaders.DefaultTexture;
            s1 = new Sprite(Material.Create(shader, Color.Black));
            s1.Renderer.UseUnscaledProjectionMatrix = true;
            s2 = new Sprite(Material.Create(shader, Color.Black));
            s2.Renderer.UseUnscaledProjectionMatrix = true;
            s3 = new Sprite(Material.Create(shader, Color.Black));
            s3.Renderer.UseUnscaledProjectionMatrix = true;
            s4 = new Sprite(Material.Create(shader, Color.Black));
            s4.Renderer.UseUnscaledProjectionMatrix = true;
            s1.Transform.Scale = Vector2.Zero;
            s2.Transform.Scale = Vector2.Zero;
            s3.Transform.Scale = Vector2.Zero;
            s4.Transform.Scale = Vector2.Zero;

            TextRendererSystem.Register(this);
        }

        #region Text Formatting
        private unsafe void UpdateTextFormatting(string _inputText)
        {
            if (_inputText == null || _inputText == "") return;
            lineOffsets.Clear();

            // Split input text into substrings
            bool skipNewlines = false;
            string[] words = null;
            if (OverflowMode == TextOverflowMode.Word)
            {
                words = Regex.Split(_inputText, @"(\s)");
            }
            else if (OverflowMode == TextOverflowMode.Character)
            {
                words = _inputText.ToCharArray().Select(c => c.ToString()).ToArray();
            }
            else if (OverflowMode == TextOverflowMode.Disabled)
            {
                words = Regex.Split(_inputText, @"(\s)");
                float stringSize = 0;
                uint g = 0;
                uint p = 0;

                // Measuring the input string
                for (int i = 0; i < _inputText.Length; i++)
                {
                    // Skipping newline characters, rich text is not supported
                    if (_inputText[i] == '\n') continue;

                    Character ch = FontGlyphStore.Characters[_inputText[i]];
                    g = FT_Get_Char_Index(FontGlyphStore.Face, _inputText[i]);

                    // Kerning
                    if (FontGlyphStore.UseKerning)
                    {
                        if (FT_Get_Kerning(FontGlyphStore.Face, p, g, (uint)FT_Kerning_Mode.FT_KERNING_DEFAULT, out FT_Vector delta) == FT_Error.FT_Err_Ok)
                        {
                            long* temp = (long*)delta.x;
                            long res = *temp;
                            stringSize += res;
                        }
                        else
                        {
                            Debug.LogError($"FREETYPE: Unable to get kerning for font {FontGlyphStore.Arguments.FontName}");
                        }
                    }

                    stringSize += ch.Advance * transform.Scale.X;

                    p = g;
                }

                formattedText = _inputText;
                lineOffsets.Add(Bounds.Width - (int)stringSize);
                skipNewlines = true;
            }

            // Loop through all words and check if they overlap with boundary
            StringBuilder builder = new StringBuilder();
            float _x = Bounds.X;
            float _y = Bounds.Y;
            uint previousIndex = 0;
            uint glyphIndex = 0;
            float maxHeight = 0;
            float minHeight = 0;
            int newlineCount = 0;
            float wordLength = 0;

            if (VerticalAlignment == TextAlignment.Top && AlignmentMode == TextAlignmentMode.Baseline)
            {
                builder.Append('\n');
            }

            for (int w = 0; w < words.Length; w++)
            {
                bool outsideBoundsFlag = false;
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
                        continue;

                        _x = Bounds.X;
                        _y -= FontGlyphStore.Arguments.FontSize * LineHeightMultiplier;
                        newlineCount++;
                        lineOffsets.Add(Bounds.Width + Bounds.X - (int)_x);

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
                    // Ignoring the Y height since that currently does not matter and was causing issues
                    if (!outsideBoundsFlag && !Bounds.Contains(new Rectangle((int)_x, 0,
                        (int)((ch.Size.X + ch.Bearing.X) * transform.Scale.X), (int)(ch.Size.Y * transform.Scale.X))))
                    {
                        outsideBoundsFlag = true;
                    }

                    // Advancing x position
                    _x += ch.Advance * transform.Scale.X;
                    wordLength += ch.Advance * transform.Scale.X;

                    previousIndex = glyphIndex;
                }

                // Handle outside bounds flag
                if (outsideBoundsFlag && !skipNewlines)
                {
                    newlineCount++;
                    lineOffsets.Add(Bounds.Width - (int)(_x - wordLength));

                    _x = wordLength;
                    _y -= FontGlyphStore.Arguments.FontSize * LineHeightMultiplier;
                    minHeight = 0;
                    builder.Append('\n');
                }

                // If this word is the last word, add an offset as it otherwise would not be added
                if (w == words.Length - 1)
                {
                    // If this is the last word
                    lineOffsets.Add(Bounds.Width - (int)_x);
                }
                wordLength = 0;

                builder.Append(words[w]);
            }

            firstLineMaxHeight = AlignmentMode == TextAlignmentMode.Geometry ? maxHeight : 0;
            lastLineMinHeight = AlignmentMode == TextAlignmentMode.Geometry ? minHeight : 0;
            totalYHeight = Math.Abs(_y) + firstLineMaxHeight + lastLineMinHeight;
            formattedText = builder.ToString();
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
                    return (int)(Bounds.Height - totalYHeight) / 2 + (int)firstLineMaxHeight;
                case TextAlignment.Bottom:
                    return (int)(Bounds.Height - lastLineMinHeight);
                default: return 0;
            }
        }
        #endregion

        #region Rendering
        public unsafe void Render()
        {
            TextShader.Use();
            TextShader.SetMatrix4x4("projection", Camera2D.main.GetUnscaledProjectionMatrix());
            TextShader.SetMatrix4x4("model", Matrix4x4.CreateScale(Game.WINDOW_SCALE, Game.WINDOW_SCALE, 1));
            TextShader.SetColor("mainColor", TextColor);
            TextShader.SetColor("outlineColor", OutlineColor);
            glActiveTexture(GL_TEXTURE0);
            glBindVertexArray(VAO);

            float x = GetXOffset(0) + position.X;
            float _x = x;
            float _y = position.Y - GetYOffset();
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
                    _x = GetXOffset(newlineCount) + position.X;
                    // If the newline is the first character, it is meant to offset the first line, so use one as the multiplier
                    _y -= FontGlyphStore.Arguments.FontSize * (i == 0 ? 1 : LineHeightMultiplier);
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

                float xpos = _x + ch.Bearing.X * transform.Scale.X;
                float ypos = _y - (ch.Size.Y - ch.Bearing.Y) * transform.Scale.X; // Causes text to be slightly vertically offset by 1 pixel

                float w = ch.Size.X * transform.Scale.X;
                float h = ch.Size.Y * transform.Scale.X;

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

                _x += ch.Advance * transform.Scale.X;

                previousIndex = glyphIndex;
            }

            glBindVertexArray(0);
            glBindTexture(GL_TEXTURE_2D, 0);

            // Bounds visualization
            if (ShowBoundsDebug)
            {
                s1.Transform.Position = new Vector2(position.X + Bounds.Size.Width / 2, position.Y);
                s1.Transform.Scale = new Vector2(Bounds.Size.Width, 1);
                s2.Transform.Position = new Vector2(position.X + Bounds.Size.Width, position.Y - Bounds.Size.Height / 2);
                s2.Transform.Scale = new Vector2(1, Bounds.Size.Height);
                s3.Transform.Position = new Vector2(position.X + Bounds.Size.Width / 2, position.Y - Bounds.Size.Height);
                s3.Transform.Scale = new Vector2(Bounds.Size.Width, 1);
                s4.Transform.Position = new Vector2(position.X, position.Y - Bounds.Size.Height / 2);
                s4.Transform.Scale = new Vector2(1, Bounds.Size.Height);
            }
        }
        #endregion
    }
}