using Electron2D.Rendering.Shaders;
using Electron2D.Rendering.Text;
using System.Drawing;
using System.Numerics;

namespace Electron2D.UserInterface
{
    public class TextLabel : UIComponent
    {
        public string Text
        {
            get => Renderer.Text;
            set => Renderer.Text = value;
        }
        public Color TextColor
        {
            get => Renderer.TextColor;
            set => Renderer.TextColor = value;
        }
        public Color OutlineColor
        {
            get => Renderer.OutlineColor;
            set => Renderer.OutlineColor = value;
        }
        public Vector2 Bounds
        {
            get => new Vector2(SizeX, SizeY);
            set { SizeX = value.X; SizeY = value.Y; }
        }
        public TextAlignment HorizontalAlignment
        {
            get => Renderer.HorizontalAlignment;
            set => Renderer.HorizontalAlignment = value;
        }
        public TextAlignment VerticalAlignment
        {
            get => Renderer.VerticalAlignment;
            set => Renderer.VerticalAlignment = value;
        }
        public TextAlignmentMode AlignmentMode
        {
            get => Renderer.AlignmentMode;
            set => Renderer.AlignmentMode = value;
        }
        public TextOverflowMode OverflowMode
        {
            get => Renderer.OverflowMode;
            set => Renderer.OverflowMode = value;
        }

        public new TextRenderer Renderer { get; private set; }
        private SharedResource<FontGlyphStore> _font;

        public TextLabel(TextLabelDef def, bool useScreenPosition = true, int uiRenderLayer = 0, bool ignorePostProcessing = true)
            : base(ignorePostProcessing, uiRenderLayer, useScreenPosition: useScreenPosition, useMeshRenderer: false)
        {
            SizeX = def.SizeX;
            SizeY = def.SizeY;
            _font = Resources.GetFont(def.TextFontArguments.FontFile, def.TextFontArguments.FontSize,
                def.TextFontArguments.FontScale, def.TextFontArguments.OutlineWidth);
            Renderer = new TextRenderer(Transform, _font, def.TextMaterial.Value.Shader, def.Text, new Vector2(SizeX, SizeY), def.TextColor, Color.Black,
                def.TextHorizontalAlignment, def.TextVerticalAlignment, def.TextAlignmentMode, def.TextOverflowMode, useScreenPosition);
        }

        protected override void OnUIEvent(UIEvent _event)
        {
            switch (_event)
            {
                case UIEvent.Resize:
                    if (Renderer != null)
                    {
                        Renderer.Bounds = new Rectangle(0, 0, (int)SizeX, (int)SizeY);
                        Renderer.UpdateMesh();
                    }
                    break;
                case UIEvent.Anchor:
                    if (Renderer != null)
                    {
                        Renderer.Anchor = Anchor;
                        Renderer.UpdateMesh();
                    }
                    break;
                case UIEvent.Position:
                    if (Renderer != null)
                    {
                        Renderer.UpdateMesh();
                    }
                    break;
            }
        }

        public override void SetColor(Color color)
        {
            if (Renderer != null)
                TextColor = color;
        }

        public override void Render()
        {
            if (Constraints.IsDirty)
            {
                ApplyConstraints();
                Constraints.IsDirty = false;
            }
            if (Visible)
                Renderer.Render();
        }

        protected override void OnDispose()
        {
            Renderer.Dispose();
            _font.Release();
        }
    }
}