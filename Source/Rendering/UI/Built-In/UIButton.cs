using System.Drawing;
using System.Numerics;

namespace Electron2D.UI
{
    public sealed class UIButton : UIElement
    {
        public UIElement? Icon { get; private set; }
        public UIText? TextElement { get; private set; }
        public UIElement Background { get; private set; }
        public Color NormalBackgroundColor
        {
            get => _normalBackgroundColor;
            set { _normalBackgroundColor = value; UpdateColors(); }
        }
        public Color HoverBackgroundColor
        {
            get => _hoverBackgroundColor;
            set { _hoverBackgroundColor = value; UpdateColors(); }
        }
        public Color PressedBackgroundColor
        {
            get => _pressedBackgroundColor;
            set { _pressedBackgroundColor = value; UpdateColors(); }
        }
        public Color DisabledBackgroundColor
        {
            get => _disabledBackgroundColor;
            set { _disabledBackgroundColor = value; UpdateColors(); }
        }
        public Color NormalForegroundColor
        {
            get => _normalForegroundColor;
            set { _normalForegroundColor = value; UpdateColors(); }
        }
        public Color HoverForegroundColor
        {
            get => _hoverForegroundColor;
            set { _hoverForegroundColor = value; UpdateColors(); }
        }
        public Color PressedForegroundColor
        {
            get => _pressedForegroundColor;
            set { _pressedForegroundColor = value; UpdateColors(); }
        }
        public Color DisabledForegroundColor
        {
            get => _disabledForegroundColor;
            set { _disabledForegroundColor = value; UpdateColors(); }
        }
        private Color _normalBackgroundColor;
        private Color _hoverBackgroundColor;
        private Color _pressedBackgroundColor;
        private Color _disabledBackgroundColor;
        private Color _normalForegroundColor;
        private Color _hoverForegroundColor;
        private Color _pressedForegroundColor;
        private Color _disabledForegroundColor;
        private bool _isHovered = false;
        private bool _isPressed = false;

        public UIButton(UITextStyle textStyle, string text, UIPanelDef? backgroundDef = null,
            UIRenderArgs? arguments = null) : base(arguments, false, true)
        {
            SetupBackground(backgroundDef, arguments);
            SetupForeground(textStyle.Color);
            TextElement = new UIText(textStyle, text, arguments);
            TextElement.Interactable = false;
            AddChild(TextElement);
            SetupEvents();
            CanAddChildren = false;
            Size = new Vector2(60, 20);
        }

        public UIButton(UIPanelDef iconDef, Vector2 iconSize,
            UIPanelDef? backgroundDef = null, UIRenderArgs? arguments = null)
            : base(arguments, false, true)
        {
            SetupBackground(backgroundDef, arguments);
            SetupForeground(iconDef.Color ?? Color.White);
            Icon = iconDef.Create(arguments);
            Icon.MinSize = iconSize;
            Icon.MaxSize = iconSize;
            Icon.Interactable = false;
            Icon.Pivot = new Vector2(0.5f, 0.5f);
            Icon.Anchor = new Vector2(0.5f, 0.5f);
            AddChild(Icon);
            SetupEvents();
            CanAddChildren = false;
            Size = new Vector2(60, 20);
        }

        private void SetupBackground(UIPanelDef? backgroundDef, UIRenderArgs? arguments)
        {
            Background = backgroundDef != null
                ? backgroundDef.Create(arguments)
                : new UIContainer();
            Background.Interactable = false;
            AddChild(Background);

            Color c = Background.Renderer?.Material?.MainColor ?? Color.White;
            _normalBackgroundColor = c;
            _hoverBackgroundColor = c;
            _pressedBackgroundColor = c;
            _disabledBackgroundColor = c;
        }

        private void SetupForeground(Color normalColor)
        {
            _normalForegroundColor = normalColor;
            _hoverForegroundColor = normalColor;
            _pressedForegroundColor = normalColor;
            _disabledForegroundColor = normalColor;
        }

        public void UpdateColors()
        {
            Color bg, fg;

            if (!Interactable)
            {
                bg = _disabledBackgroundColor;
                fg = _disabledForegroundColor;
            }
            else if (_isPressed)
            {
                bg = _pressedBackgroundColor;
                fg = _pressedForegroundColor;
            }
            else if (_isHovered)
            {
                bg = _hoverBackgroundColor;
                fg = _hoverForegroundColor;
            }
            else
            {
                bg = _normalBackgroundColor;
                fg = _normalForegroundColor;
            }

            Background.SetColor(bg);
            TextElement?.SetColor(fg);
            Icon?.SetColor(fg);
        }

        private void SetupEvents()
        {
            AddEventListener(UIEventType.MouseDown, evt =>
            {
                _isPressed = true;
                UpdateColors();
            });
            AddEventListener(UIEventType.MouseUp, evt =>
            {
                _isPressed = false;
                UpdateColors();
            });
            AddEventListener(UIEventType.MouseEnter, evt =>
            {
                _isHovered = true;
                if (!_isPressed) UpdateColors();
            });
            AddEventListener(UIEventType.MouseLeave, evt =>
            {
                _isHovered = false;
                if (!_isPressed) UpdateColors();
            });
            AddEventListener(UIEventType.LoseInteractability, evt =>
            {
                UpdateColors();
            });
        }

        public override void UpdateMesh() { }

        // TODO: Add combined icon + text button constructor
    }
}
