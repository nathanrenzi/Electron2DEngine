using Electron2D.Rendering.Shaders;
using Electron2D.UserInterface;
using FreeTypeSharp.Native;
using System;
using System.Drawing;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using static FreeTypeSharp.Native.FT;

namespace Electron2D.Rendering.Text
{
    public class TextRenderer : MeshRenderer
    {
        public class Iterator : IDisposable
        {
            public int Index { get; private set; }
            internal int _extraIncrement = 0;
            private TextRenderer _renderer;

            internal Iterator(TextRenderer renderer)
            {
                _renderer = renderer;
                _renderer._iterators.Add(this);
            }
            
            ~Iterator()
            {
                Dispose();
            }

            public void SetIndex(int index)
            {
                Index = index;
            }

            public void Increment()
            {
                Index += 1 + _extraIncrement;
            }

            public void Decrement()
            {
                Index--;
            }

            public void Validate()
            {
                if (Index >= _renderer._text.Length) Index = _renderer._text.Length;
                if (Index < 0) Index = 0;
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
                _renderer._iterators.Remove(this);
            }
        }

        public FontGlyphStore FontGlyphStore { get; set; }
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                UpdateTextFormatting(value);
            }
        }
        private string _text;
        public float LineHeightMultiplier { get; set; } = 1.35f;
        public TextAlignment HorizontalAlignment { get; set; }
        public TextAlignment VerticalAlignment { get; set; }
        public TextAlignmentMode AlignmentMode { get; set; }
        public TextOverflowMode OverflowMode { get; set; }
        public float MeshScale { get; set; } = 1;
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
        public Color OutlineColor { get; set; }
        public Transform Transform { get; set; }
        public Vector2 Anchor
        {
            get => _anchor;
            set => _anchor = value;
        }
        private Vector2 _anchor;
        private Vector2 _position => new Vector2(
            (Transform.Position.X - Bounds.Width / 2f) + (Bounds.Width / 2f * -_anchor.X),
            (Transform.Position.Y - Bounds.Height / 2f) + (Bounds.Height / 2f * -_anchor.Y)
        );
        public Rectangle Bounds
        {
            get
            {
                return _bounds;
            }
            set
            {
                _bounds = value;
                UpdateTextFormatting(Text);
            }
        }
        private Rectangle _bounds;
        private Shader _shader;
        private List<Iterator> _iterators = new List<Iterator>();
        private List<int> _lineOffsets = new List<int>(); // Stores the pixel distance between the end of the line and the right bound
        private string _formattedText;
        private float _totalYHeight;
        private float _firstLineMaxHeight = 0;
        private float _lastLineMinHeight = 0;

        public unsafe TextRenderer(Transform transform, FontGlyphStore fontGlyphStore, Shader shader, string text,
            Vector2 bounds, Color textColor, Color outlineColor,
            TextAlignment horizontalAlignment = TextAlignment.Left,
            TextAlignment verticalAlignment = TextAlignment.Top,
            TextAlignmentMode alignmentMode = TextAlignmentMode.Baseline,
            TextOverflowMode overflowMode = TextOverflowMode.Word)
            : base(transform, Material.Create(shader, new Texture2D(fontGlyphStore.TextureHandle, fontGlyphStore.TextureAtlasWidth, fontGlyphStore.Arguments.FontSize)))
        {
            _shader = shader;
            FontGlyphStore = fontGlyphStore;
            Transform = transform;
            TextColor = textColor;
            OutlineColor = outlineColor;
            Bounds = new Rectangle(0, 0, (int)bounds.X, (int)bounds.Y);
            HorizontalAlignment = horizontalAlignment;
            VerticalAlignment = verticalAlignment;
            AlignmentMode = alignmentMode;
            OverflowMode = overflowMode;

            UseUnscaledProjectionMatrix = true;

            // This must be the last thing initialized, as it will reformat the text
            Text = text;
            Load();
        }

        public Iterator GetIterator()
        {
            return new Iterator(this);
        }

        public int GetCaretIndexFromWorldPosition(Vector2 worldPosition)
        {
            if (_formattedText.Length == 0) return 0;
            Vector2 localpos = worldPosition - _position;
            int offset = -1;
            int xpos = GetXOffset(0);
            int ypos = GetYOffset();
            int newlineCount = 0;
            if(localpos.X <= xpos)
            {
                return 0;
            }

            if (AlignmentMode == TextAlignmentMode.Baseline)
            {
                if (VerticalAlignment == TextAlignment.Top)
                {
                    ypos += FontGlyphStore.Arguments.FontSize;
                }
                else if (VerticalAlignment == TextAlignment.Center)
                {
                    ypos += FontGlyphStore.Ascent / 2;
                }
            }

            for (int i = 1; i < _formattedText.Length; i++)
            {
                Character ch = FontGlyphStore.Characters[_formattedText[i + offset]];

                if (_formattedText[i + offset] == '\n')
                {
                    newlineCount++;
                    xpos = GetXOffset(newlineCount);
                    ypos += (int)(FontGlyphStore.Arguments.FontSize * LineHeightMultiplier);
                }
                else
                {
                    xpos += (int)(ch.Advance * Transform.Scale.X);
                }

                int lineTop = ypos - (AlignmentMode == TextAlignmentMode.Baseline ? 0 : 0);
                int lineBottom = ypos + (int)(FontGlyphStore.Arguments.FontSize * LineHeightMultiplier);
                bool onThisLine = localpos.Y >= lineTop && localpos.Y < lineBottom;
                if (onThisLine)
                {
                    if (localpos.X <= xpos)
                    {
                        if (localpos.X < xpos - ((ch.Advance * Transform.Scale.X) / 2f))
                        {
                            return (int)MathF.Max(i - 1, 0);
                        }
                        else
                        {
                            return i;
                        }
                    }
                }
            }

            return _formattedText.Length;
        }

        public Vector2 GetCaretWorldPostion(int index)
        {
            if(_formattedText.Length == 0)
            {
                int ypos = GetYOffset();
                if (VerticalAlignment == TextAlignment.Top)
                {
                    ypos += FontGlyphStore.Arguments.FontSize;
                }
                else if (VerticalAlignment == TextAlignment.Center)
                {
                    ypos += FontGlyphStore.Ascent / 2;
                }
                return new Vector2(GetXOffset(0), ypos) + _position;
            }
            if(index <= _formattedText.Length && index >= 0)
            {
                int offset = -1;
                int xpos = GetXOffset(0);
                int ypos = GetYOffset();
                int newlineCount = 0;

                if (AlignmentMode == TextAlignmentMode.Baseline)
                {
                    if (VerticalAlignment == TextAlignment.Top)
                    {
                        ypos += FontGlyphStore.Arguments.FontSize;
                    }
                    else if (VerticalAlignment == TextAlignment.Center)
                    {
                        ypos += FontGlyphStore.Ascent / 2;
                    }
                }

                for (int i = 1; i <= index; i++)
                {
                    Character ch = FontGlyphStore.Characters[_formattedText[i + offset]];

                    if (_formattedText[i + offset] == '\n')
                    {
                        newlineCount++;
                        xpos = GetXOffset(newlineCount);
                        ypos += (int)(FontGlyphStore.Arguments.FontSize * LineHeightMultiplier);
                    }
                    else
                    {
                        xpos += (int)(ch.Advance * Transform.Scale.X);
                    }
                }

                return new Vector2(xpos, ypos) + _position;
            }
            else
            {
                return Vector2.Zero;
            }
        }

        #region Text Formatting
        private unsafe void UpdateTextFormatting(string _inputText)
        {
            string unformattedText = _inputText == null ? "" : _inputText;
            _lineOffsets.Clear();

            // Split input text into substrings
            bool skipNewlines = false;
            string[] words = null;
            if (OverflowMode == TextOverflowMode.Word)
            {
                words = Regex.Split(unformattedText, @"(\s)");
            }
            else if (OverflowMode == TextOverflowMode.Character)
            {
                words = unformattedText.ToCharArray().Select(c => c.ToString()).ToArray();
            }
            else if (OverflowMode == TextOverflowMode.Disabled)
            {
                words = Regex.Split(unformattedText, @"(\s)");
                float stringSize = 0;
                uint g = 0;
                uint p = 0;

                // Measuring the input string
                for (int i = 0; i < unformattedText.Length; i++)
                {
                    // Skipping newline characters, rich text is not supported
                    if (unformattedText[i] == '\n') continue;

                    Character ch = FontGlyphStore.Characters[unformattedText[i]];
                    g = FT_Get_Char_Index(FontGlyphStore.Face, unformattedText[i]);

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
                            Debug.LogError($"FREETYPE: Unable to get kerning for font {FontGlyphStore.Arguments.FontFile}");
                        }
                    }

                    stringSize += ch.Advance * Transform.Scale.X;

                    p = g;
                }

                _formattedText = unformattedText;
                _lineOffsets.Add(Bounds.Width - (int)stringSize);
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
                    if (words[w] == "\n")
                    {
                        _x = Bounds.X;
                        _y -= FontGlyphStore.Arguments.FontSize * LineHeightMultiplier;
                        newlineCount++;
                        _lineOffsets.Add(Bounds.Width + Bounds.X - (int)_x);

                        // Reset minimum height since a new line was created
                        minHeight = 0;
                        break;
                    }

                    // Kerning
                    if (FontGlyphStore.UseKerning)
                    {
                        if (FT_Get_Kerning(FontGlyphStore.Face, previousIndex, glyphIndex, (uint)FT_Kerning_Mode.FT_KERNING_DEFAULT, out FT_Vector delta) == FT_Error.FT_Err_Ok)
                        {
                            // 26.6 fixed-point to float pixels
                            float kx = delta.x / 64f;
                            _x += kx * Transform.Scale.X;
                        }
                        else
                        {
                            Debug.LogError($"FREETYPE: Unable to get kerning for font {FontGlyphStore.Arguments.FontFile}");
                        }
                    }

                    // If the character overlaps the bounds, move to new line

                    if (!outsideBoundsFlag)
                    {
                        float nextX = _x + (ch.Advance * Transform.Scale.X);
                        if (nextX - Bounds.X > Bounds.Width)
                            outsideBoundsFlag = true;
                    }

                    // Advancing x position
                    _x += ch.Advance * Transform.Scale.X;
                    wordLength += ch.Advance * Transform.Scale.X;

                    previousIndex = glyphIndex;
                }

                // Handle outside bounds flag
                if (outsideBoundsFlag && !skipNewlines)
                {
                    newlineCount++;
                    _lineOffsets.Add(Bounds.Width - (int)(_x - wordLength));
                    _x = wordLength;
                    _y -= FontGlyphStore.Arguments.FontSize * LineHeightMultiplier;
                    minHeight = 0;
                    builder.Append('\n');
                }

                // If this word is the last word, add an offset as it otherwise would not be added
                if (w == words.Length - 1)
                {
                    // If this is the last word
                    _lineOffsets.Add(Bounds.Width - (int)_x);
                }
                wordLength = 0;

                builder.Append(words[w]);
            }

            _firstLineMaxHeight = AlignmentMode == TextAlignmentMode.Geometry ? maxHeight : 0;
            _lastLineMinHeight = AlignmentMode == TextAlignmentMode.Geometry ? minHeight : 0;
            _totalYHeight = Math.Abs(_y) + _firstLineMaxHeight + _lastLineMinHeight;
            _formattedText = builder.ToString();

            UpdateMesh();
        }

        private int GetXOffset(int _iteration)
        {
            if (_lineOffsets.Count > _iteration)
            {
                switch (HorizontalAlignment)
                {
                    case TextAlignment.Left:
                        return 0;
                    case TextAlignment.Center:
                        return _lineOffsets[_iteration] / 2;
                    case TextAlignment.Right:
                        return _lineOffsets[_iteration];
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
                    return 0 + (int)_firstLineMaxHeight;
                case TextAlignment.Center:
                    return (int)(Bounds.Height - _totalYHeight) / 2 + (int)_firstLineMaxHeight;
                case TextAlignment.Bottom:
                    return (int)(Bounds.Height - _lastLineMinHeight);
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

        public unsafe void UpdateMesh()
        {
            List<float[]> tempVertexArrays = new List<float[]>();
            List<uint> tempIndices = new List<uint>();

            float x = GetXOffset(0) + _position.X;
            float _x = x;
            float _y = _position.Y + GetYOffset();
            uint previousIndex = FT_Get_Char_Index(FontGlyphStore.Face, ' ');
            uint glyphIndex = 0;
            int newlineCount = 0;

            if (AlignmentMode == TextAlignmentMode.Baseline)
            {
                if(VerticalAlignment == TextAlignment.Top)
                {
                    _y += FontGlyphStore.Arguments.FontSize;
                }
                else if(VerticalAlignment == TextAlignment.Center)
                {
                    _y += FontGlyphStore.Ascent / 2f;
                }
            }

            if (_formattedText.Length > 0)
            {
                for (int i = 0; i < _formattedText.Length; i++)
                {
                    Character ch = FontGlyphStore.Characters[_formattedText[i]];
                    glyphIndex = FT_Get_Char_Index(FontGlyphStore.Face, _formattedText[i]);

                    // If word is a newline character, handle it separately
                    if (_formattedText[i] == '\n')
                    {
                        newlineCount++;
                        _x = GetXOffset(newlineCount) + _position.X;
                        // If the newline is the first character, it is meant to offset the first line, so use one as the multiplier
                        _y += FontGlyphStore.Arguments.FontSize * LineHeightMultiplier;
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
                            Debug.LogError($"FREETYPE: Unable to get kerning for font {FontGlyphStore.Arguments.FontFile}");
                        }
                    }

                    int xpos = (int)(_x + ch.Bearing.X * MeshScale);
                    int ypos = (int)(_y - ch.Bearing.Y * MeshScale);

                    float w = ch.Size.X * MeshScale;
                    float h = ch.Size.Y * MeshScale;

                    float L = ch.UVX.X;
                    float R = ch.UVX.Y;
                    float T = ch.UVY.X;
                    float B = ch.UVY.Y;

                    uint count = (uint)tempVertexArrays.Count;
                    tempVertexArrays.Add(new float[] { xpos, ypos, L, T }); // Top Left
                    tempVertexArrays.Add(new float[] { xpos + w, ypos, R, T }); // Top Right
                    tempVertexArrays.Add(new float[] { xpos + w, ypos + h, R, B }); // Bottom Right
                    tempVertexArrays.Add(new float[] { xpos, ypos + h, L, B }); // Bottom Left
                    tempIndices.Add(count + 0);
                    tempIndices.Add(count + 1);
                    tempIndices.Add(count + 2);
                    tempIndices.Add(count + 0);
                    tempIndices.Add(count + 2);
                    tempIndices.Add(count + 3);

                    _x += ch.Advance * MeshScale;

                    previousIndex = glyphIndex;
                }
            }
            else
            {
                // Send fake data for a point offscreen instead of sending no vertex data (causes crash)
                tempVertexArrays.Add(new float[] { -10000, -10000, 0, 0 });
                tempVertexArrays.Add(new float[] { -10000, -10000, 0, 0 });
                tempVertexArrays.Add(new float[] { -10000, -10000, 0, 0 });
                tempIndices.Add(0);
                tempIndices.Add(1);
                tempIndices.Add(2);
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
            Matrix4x4 model = Matrix4x4.CreateScale(Transform.Scale.X, Transform.Scale.Y, 1f);
            Material.Shader.SetMatrix4x4("model", model);
            Material.Shader.SetColor("outlineColor", OutlineColor);
        }
    }
}