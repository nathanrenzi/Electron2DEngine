using System.Drawing;
using System.Numerics;

namespace Electron2D.UI
{
    /// <summary>
    /// A clickable button element that supports text or an icon, with per-state background and foreground colors.
    /// </summary>
    public sealed class UIButton : UIElement
    {
        /// <summary>
        /// The icon element, or <see langword="null"/> if this button uses text.
        /// </summary>
        public UIElement? Icon { get; private set; }

        /// <summary>
        /// The text element, or <see langword="null"/> if this button uses an icon.
        /// </summary>
        public UIText? TextElement { get; private set; }

        /// <summary>
        /// The background element of the button.
        /// </summary>
        public UIElement Background { get; private set; }

        /// <summary>
        /// The background color when the button is in its normal state.
        /// </summary>
        public Color NormalBackgroundColor
        {
            get => _normalBackgroundColor;
            set { _normalBackgroundColor = value; UpdateColors(); }
        }

        /// <summary>
        /// The background color when the button is hovered.
        /// </summary>
        public Color HoverBackgroundColor
        {
            get => _hoverBackgroundColor;
            set { _hoverBackgroundColor = value; UpdateColors(); }
        }

        /// <summary>
        /// The background color when the button is pressed.
        /// </summary>
        public Color PressedBackgroundColor
        {
            get => _pressedBackgroundColor;
            set { _pressedBackgroundColor = value; UpdateColors(); }
        }

        /// <summary>
        /// The background color when the button is disabled.
        /// </summary>
        public Color DisabledBackgroundColor
        {
            get => _disabledBackgroundColor;
            set { _disabledBackgroundColor = value; UpdateColors(); }
        }

        /// <summary>
        /// The foreground color when the button is in its normal state.
        /// </summary>
        public Color NormalForegroundColor
        {
            get => _normalForegroundColor;
            set { _normalForegroundColor = value; UpdateColors(); }
        }

        /// <summary>
        /// The foreground color when the button is hovered.
        /// </summary>
        public Color HoverForegroundColor
        {
            get => _hoverForegroundColor;
            set { _hoverForegroundColor = value; UpdateColors(); }
        }

        /// <summary>
        /// The foreground color when the button is pressed.
        /// </summary>
        public Color PressedForegroundColor
        {
            get => _pressedForegroundColor;
            set { _pressedForegroundColor = value; UpdateColors(); }
        }

        /// <summary>
        /// The foreground color when the button is disabled.
        /// </summary>
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

        /// <summary>
        /// Creates a text button.
        /// </summary>
        /// <param name="textStyle">The style used for the button text.</param>
        /// <param name="text">The text to display on the button.</param>
        /// <param name="backgroundDef">The panel definition for the background. Defaults to an empty container.</param>
        public UIButton(UITextStyle textStyle, string text, UIPanelDef? backgroundDef = null,
            UIRenderArgs? arguments = null) : base(arguments, false)
        {
            SetupBackground(backgroundDef, arguments);
            SetupForeground(textStyle.Color);
            TextElement = new UIText(textStyle, text, arguments)
            {
                Interactable = false
            };
            AddChild(TextElement);
            SetupEvents();
        }

        /// <summary>
        /// Creates an icon button.
        /// </summary>
        /// <param name="iconDef">The panel definition for the icon.</param>
        /// <param name="iconSize">The fixed size of the icon in pixels.</param>
        /// <param name="backgroundDef">The panel definition for the background. Defaults to an empty container.</param>
        public UIButton(UIPanelDef iconDef, Vector2 iconSize,
            UIPanelDef? backgroundDef = null, UIRenderArgs? arguments = null)
            : base(arguments, false)
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
        }

        private void SetupBackground(UIPanelDef? backgroundDef, UIRenderArgs? arguments)
        {
            Background = backgroundDef != null
                ? backgroundDef.Create(arguments)
                : new UIContainer();
            Background.Interactable = false;
            AddChild(Background);

            Color c = Background.Renderer?.Material.Value.MainColor ?? Color.White;
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

        /// <summary>
        /// Updates the background and foreground colors based on the current button state.
        /// </summary>
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
