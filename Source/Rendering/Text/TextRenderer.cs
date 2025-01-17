using Electron2D.Rendering.Shaders;
using FreeTypeSharp.Native;
using System.Drawing;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using static FreeTypeSharp.Native.FT;

namespace Electron2D.Rendering.Text
{
    public class TextRenderer : MeshRenderer
    {
        public FontGlyphStore FontGlyphStore;
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
        public Color TextColor
        {
            get
            {
                return Material.MainColor;
            }
            set
            {
                Material.MainColor = value;
            }
        }
        public Color OutlineColor;
        private Transform transform;
        public Vector2 Anchor
        {
            get => anchor;
            set => anchor = value;
        }
        private Vector2 anchor;
        private Vector2 position
        {
            get
            {
                return new Vector2((transform.Position.X - Bounds.Width/2f) + (Bounds.Width/2f * -anchor.X),
                                (transform.Position.Y + Bounds.Height/2f) + (Bounds.Height / 2f * -anchor.Y));
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

        private List<int> lineOffsets = new List<int>(); // Stores the pixel distance between the end of the line and the right bound
        private string formattedText;
        private float totalYHeight;
        private float firstLineMaxHeight = 0;
        private float lastLineMinHeight = 0;

        public unsafe TextRenderer(Transform _transform, FontGlyphStore _fontGlyphStore, Shader _shader, string _text,
            Vector2 _bounds, Color _textColor, Color _outlineColor,
            TextAlignment _horizontalAlignment = TextAlignment.Left,
            TextAlignment _verticalAlignment = TextAlignment.Top,
            TextAlignmentMode _alignmentMode = TextAlignmentMode.Baseline,
            TextOverflowMode _overflowMode = TextOverflowMode.Word)
            : base(_transform, Material.Create(_shader, new Texture2D(_fontGlyphStore.TextureHandle, _fontGlyphStore.TextureAtlasWidth, _fontGlyphStore.Arguments.FontSize)))
        {
            FontGlyphStore = _fontGlyphStore;
            transform = _transform;
            TextColor = _textColor;
            OutlineColor = _outlineColor;
            Bounds = new Rectangle(0, 0, (int)_bounds.X, (int)_bounds.Y);
            HorizontalAlignment = _horizontalAlignment;
            VerticalAlignment = _verticalAlignment;
            AlignmentMode = _alignmentMode;
            OverflowMode = _overflowMode;

            UseUnscaledProjectionMatrix = true;

            // This must be the last thing initialized, as it will reformat the text
            Text = _text;
            Load();
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

            UpdateMesh();
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

        protected override void CreateBufferLayout()
        {
            // Telling the vertex array how the vertices are structured
            Layout = new BufferLayout();
            Layout.Add<float>(2); // Position
            Layout.Add<float>(2); // UV
        }

        private unsafe void UpdateMesh()
        {
            List<float[]> tempVertexArrays = new List<float[]>();
            List<uint> tempIndices = new List<uint>();

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

                int xpos = (int)(_x + ch.Bearing.X * transform.Scale.X);
                int ypos = (int)(_y - (ch.Size.Y - ch.Bearing.Y) * transform.Scale.X); // Causes text to be slightly vertically offset by 1 pixel

                float w = ch.Size.X * transform.Scale.X;
                float h = ch.Size.Y * transform.Scale.X;

                float L = ch.UVX.X;
                float R = ch.UVX.Y;
                float T = ch.UVY.X;
                float B = ch.UVY.Y;

                uint count = (uint)tempVertexArrays.Count;
                tempVertexArrays.Add(new float[] { xpos, ypos + h, L, T });     // Top Left
                tempVertexArrays.Add(new float[] { xpos + w, ypos + h, R, T }); // Top Right
                tempVertexArrays.Add(new float[] { xpos + w, ypos, R, B });     // Bottom Right
                tempVertexArrays.Add(new float[] { xpos, ypos, L, B });         // Bottom Left
                tempIndices.Add(count + 0);
                tempIndices.Add(count + 1);
                tempIndices.Add(count + 2);
                tempIndices.Add(count + 0);
                tempIndices.Add(count + 2);
                tempIndices.Add(count + 3);

                _x += ch.Advance * transform.Scale.X;

                previousIndex = glyphIndex;
            }

            List<float> tempVertices = new List<float>();

            for (int i = 0; i < tempVertexArrays.Count; i++)
            {
                for (int z = 0; z < tempVertexArrays[i].Length; z++)
                {
                    tempVertices.Add(tempVertexArrays[i][z]);
                }
            }

            SetVertexArrays(tempVertices.ToArray(), tempIndices.ToArray(), false, IsLoaded);
        }

        protected override void BeforeRender()
        {
            Material.Shader.SetMatrix4x4("model", Matrix4x4.CreateScale(Display.WindowScale, Display.WindowScale, 1));
            Material.Shader.SetColor("outlineColor", OutlineColor);
        }
    }
}