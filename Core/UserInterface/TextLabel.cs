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
        private TextRenderer textRenderer;
        private FontGlyphStore fgh;

        public TextLabel(string _text, string _fontFile, int _fontSize, Color _textColor, Color _outlineColor,
            Vector2 _bounds, TextAlignment _horizontalAlignment = TextAlignment.Left, TextAlignment _verticalAlignment = TextAlignment.Top,
            TextAlignmentMode _alignmentMode = TextAlignmentMode.Baseline, TextOverflowMode _overflowMode = TextOverflowMode.Disabled,
            int _outlineSize = 0, int _renderLayer = 0, Shader _customShader = null,
            bool _useScreenPosition = true) : base(_renderLayer, _useScreenPosition: _useScreenPosition, _useMeshRenderer: false)
        {
            SizeX = _bounds.X / 2;
            SizeY = _bounds.Y / 2;
            fgh = ResourceManager.Instance.LoadFont(_fontFile, _fontSize, _outlineSize);

            Shader shader = null;
            if (_customShader != null)
            {
                if(!_customShader.Compiled) _customShader.Compile();
            }
            else
            {
                shader = new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultText.glsl"), true);
            }
            textRenderer = new TextRenderer(Transform, fgh, shader, _text, _bounds, _textColor, _outlineColor,
                _horizontalAlignment, _verticalAlignment, _alignmentMode, _overflowMode);
            AddComponent(textRenderer);
        }

        protected override void OnUiEvent(UiEvent _event)
        {
            switch(_event)
            {
                case UiEvent.ChangeSize:
                    if(textRenderer != null) textRenderer.Bounds = new Rectangle(0, 0, (int)SizeX / 2, (int)SizeY / 2);
                    break;
            }
        }

        public override void Render()
        {
            textRenderer.Render();
        }
    }
}
