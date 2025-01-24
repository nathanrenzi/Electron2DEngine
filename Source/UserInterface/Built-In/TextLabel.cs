using Electron2D.Rendering.Shaders;
using Electron2D.Rendering.Text;
using System.Drawing;
using System.Numerics;

namespace Electron2D.UserInterface
{
    public class TextLabel : UiComponent
    {
        public string Text
        {
            get
            {
                return Renderer.Text;
            }
            set
            {
                Renderer.Text = value;
            }
        }
        public Color TextColor
        {
            get
            {
                return Renderer.TextColor;
            }
            set
            {
                Renderer.TextColor = value;
            }
        }
        public Color OutlineColor
        {
            get
            {
                return Renderer.OutlineColor;
            }
            set
            {
                Renderer.OutlineColor = value;
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
            get { return Renderer.HorizontalAlignment; }
            set { Renderer.HorizontalAlignment = value; }
        }
        public TextAlignment VerticalAlignment
        {
            get { return Renderer.VerticalAlignment; }
            set { Renderer.VerticalAlignment = value; }
        }
        public TextAlignmentMode AlignmentMode
        {
            get { return Renderer.AlignmentMode; }
            set { Renderer.AlignmentMode = value; }
        }
        public TextOverflowMode OverflowMode
        {
            get { return Renderer.OverflowMode; }
            set { Renderer.OverflowMode = value; }
        }

        public new TextRenderer Renderer { get; private set; }
        private FontGlyphStore fgh;

        public TextLabel(string _text, string _fontFile, int _fontSize, Color _textColor, Color _outlineColor,
            Vector2 _bounds, TextAlignment _horizontalAlignment = TextAlignment.Left, TextAlignment _verticalAlignment = TextAlignment.Top,
            TextAlignmentMode _alignmentMode = TextAlignmentMode.Baseline, TextOverflowMode _overflowMode = TextOverflowMode.Disabled,
            int _outlineSize = 0, int _uiRenderLayer = 0, Shader _customShader = null,
            bool _useScreenPosition = true, bool _ignorePostProcessing = false) : base(_ignorePostProcessing, _uiRenderLayer, useScreenPosition: _useScreenPosition, useMeshRenderer: false)
        {
            SizeX = _bounds.X;
            SizeY = _bounds.Y;
            fgh = ResourceManager.Instance.LoadFont(_fontFile, _fontSize, _outlineSize);

            Shader shader;
            if (_customShader != null)
            {
                if(!_customShader.Compiled) _customShader.Compile();
                shader = _customShader;
            }
            else
            {
                shader = GlobalShaders.DefaultText;
            }
            Renderer = new TextRenderer(Transform, fgh, shader, _text, _bounds, _textColor, _outlineColor,
                _horizontalAlignment, _verticalAlignment, _alignmentMode, _overflowMode);
        }

        protected override void OnUiEvent(UiEvent _event)
        {
            switch(_event)
            {
                case UiEvent.Resize:
                    if (Renderer != null)
                    {
                        Renderer.Bounds = new Rectangle(0, 0, (int)SizeX, (int)SizeY);
                    }
                    break;
                case UiEvent.Anchor:
                    if(Renderer != null)
                    {
                        Renderer.Anchor = Anchor;
                    }
                    break;
            }
        }

        public override void Render()
        {
            if (!Visible) return;
            Renderer.Render();
        }
    }
}
