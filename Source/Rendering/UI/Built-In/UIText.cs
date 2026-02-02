using Electron2D.Rendering.Shaders;
using Electron2D.Rendering.Text;

namespace Electron2D.UI
{
    public class UIText : UIElement
    {
        public string Text
        {
            get
            {
                return _renderer.Text;
            }
            set
            {
                _renderer.Text = value;
                InvalidateMeasure();
            }
        }
        public TextAlignment HorizontalAlignment
        {
            get { return _renderer.HorizontalAlignment; }
            set { _renderer.HorizontalAlignment = value; }
        }
        public TextAlignment VerticalAlignment
        {
            get { return _renderer.VerticalAlignment; }
            set { _renderer.VerticalAlignment = value; }
        }
        public TextAlignmentMode AlignmentMode
        {
            get { return _renderer.AlignmentMode; }
            set { _renderer.AlignmentMode = value; }
        }
        public TextOverflowMode OverflowMode
        {
            get { return _renderer.OverflowMode; }
            set { _renderer.OverflowMode = value; }
        }

        private TextRenderer _renderer;
        private FontGlyphStore fgh;

        public UIText(string text, FontArguments fontArguments, int sizeX = 0, int sizeY = 0, int uiRenderLayer = 0,
            bool useScreenPosition = true, bool ignorePostProcessing = true)
            : base(sizeX, sizeY, uiRenderLayer, useScreenPosition, ignorePostProcessing, false)
        {
            fgh = ResourceManager.Instance.LoadFont(fontArguments.FontFile, fontArguments.FontSize, fontArguments.FontScale, 0);
            _renderer = new TextRenderer(Position, fgh, GlobalShaders.DefaultText, text, Size,
                TextAlignment.Left, TextAlignment.Top, TextAlignmentMode.Baseline, TextOverflowMode.Word, useScreenPosition, false);
            Renderer = _renderer;
        }

        public override void UpdateMesh()
        {
            _renderer.Position = Position;
            _renderer.Pivot = Pivot;
            _renderer.Bounds = new Rect(0, 0, Size.X, Size.Y);
            _renderer.UpdateMesh();
        }

        protected override void OnDispose()
        {
            fgh = null;
        }
    }
}
