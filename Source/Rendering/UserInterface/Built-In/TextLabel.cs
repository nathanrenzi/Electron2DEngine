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

        public TextLabel(TextLabelDef def, bool useScreenPosition = true, int uiRenderLayer = 0, bool ignorePostProcessing = false)
            : base(ignorePostProcessing, uiRenderLayer, useScreenPosition: useScreenPosition, useMeshRenderer: false)
        {
            SizeX = def.SizeX;
            SizeY = def.SizeY;
            fgh = def.TextFont;

            Renderer = new TextRenderer(Transform, fgh, def.TextMaterial.Shader, def.Text, new Vector2(SizeX, SizeY), def.TextColor, Color.Black,
                def.TextHorizontalAlignment, def.TextVerticalAlignment, def.TextAlignmentMode, def.TextOverflowMode);
        }

        protected override void OnUIEvent(UIEvent _event)
        {
            switch(_event)
            {
                case UIEvent.Resize:
                    if (Renderer != null)
                    {
                        Renderer.Bounds = new Rectangle(0, 0, (int)SizeX, (int)SizeY);
                    }
                    break;
                case UIEvent.Anchor:
                    if(Renderer != null)
                    {
                        Renderer.Anchor = Anchor;
                    }
                    break;
                case UIEvent.Position:
                    if(Renderer != null)
                    {
                        Renderer.UpdateMesh();
                    }
                    break;
            }
        }

        public override void Render()
        {
            if (!Visible) return;
            Renderer.Render();
        }

        protected override void OnDispose() { }
    }
}
