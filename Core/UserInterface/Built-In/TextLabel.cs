using Electron2D.Core.Management;
using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.Rendering.Text;
using Electron2D.Core.UI;
using System.Drawing;
using System.Numerics;

namespace Electron2D.Core.UserInterface
{
    public class TextLabel : UiComponent
    {
        public string Text
        {
            get
            {
                return textRenderer.Text;
            }
            set
            {
                textRenderer.Text = value;
            }
        }
        public Color TextColor
        {
            get
            {
                return textRenderer.TextColor;
            }
            set
            {
                textRenderer.TextColor = value;
            }
        }
        public Color OutlineColor
        {
            get
            {
                return textRenderer.OutlineColor;
            }
            set
            {
                textRenderer.OutlineColor = value;
            }
        }
        public Vector2 Bounds
        {
            get
            {
                return new Vector2(SizeX, SizeY);
            }
            set
            {
                SizeX = value.X;
                SizeY = value.Y;
            }
        }
        public TextAlignment HorizontalAlignment
        {
            get { return textRenderer.HorizontalAlignment; }
            set { textRenderer.HorizontalAlignment = value; }
        }
        public TextAlignment VerticalAlignment
        {
            get { return textRenderer.VerticalAlignment; }
            set { textRenderer.VerticalAlignment = value; }
        }
        public TextAlignmentMode AlignmentMode
        {
            get { return textRenderer.AlignmentMode; }
            set { textRenderer.AlignmentMode = value; }
        }
        public TextOverflowMode OverflowMode
        {
            get { return textRenderer.OverflowMode; }
            set { textRenderer.OverflowMode = value; }
        }

        private TextRenderer textRenderer;
        private FontGlyphStore fgh;

        public TextLabel(string _text, string _fontFile, int _fontSize, Color _textColor, Color _outlineColor,
            Vector2 _bounds, TextAlignment _horizontalAlignment = TextAlignment.Left, TextAlignment _verticalAlignment = TextAlignment.Top,
            TextAlignmentMode _alignmentMode = TextAlignmentMode.Baseline, TextOverflowMode _overflowMode = TextOverflowMode.Disabled,
            int _outlineSize = 0, int _uiRenderLayer = 0, Shader _customShader = null,
            bool _useScreenPosition = true) : base(_uiRenderLayer, _useScreenPosition: _useScreenPosition, _useMeshRenderer: false)
        {
            SizeX = _bounds.X;
            SizeY = _bounds.Y;
            fgh = ResourceManager.Instance.LoadFont(_fontFile, _fontSize, _outlineSize);

            Shader shader = null;
            if (_customShader != null)
            {
                if(!_customShader.Compiled) _customShader.Compile();
            }
            else
            {
                shader = GlobalShaders.DefaultText;
            }
            textRenderer = new TextRenderer(Transform, fgh, shader, _text, _bounds, _textColor, _outlineColor,
                _horizontalAlignment, _verticalAlignment, _alignmentMode, _overflowMode);
            AddComponent(textRenderer);
        }

        protected override void OnUiEvent(UiEvent _event)
        {
            switch(_event)
            {
                case UiEvent.Resize:
                    if (textRenderer != null)
                    {
                        textRenderer.Bounds = new Rectangle(0, 0, (int)SizeX, (int)SizeY);
                    }
                    break;
                case UiEvent.Anchor:
                    if(textRenderer != null)
                    {
                        textRenderer.Anchor = Anchor;
                    }
                    break;
            }
        }

        public override void Render()
        {
            textRenderer.Render();
        }
    }
}
