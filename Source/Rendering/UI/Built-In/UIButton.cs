using System.Drawing;
using System.Numerics;

namespace Electron2D.UI
{
    public enum UIButtonLayout
    {
        /// <summary>Icon is to the left of the text.</summary>
        IconLeft,
        /// <summary>Icon is to the right of the text.</summary>
        IconRight,
        /// <summary>Icon is above the text.</summary>
        IconTop,
        /// <summary>Icon is below the text.</summary>
        IconBottom,
    }

    /// <summary>
    /// A clickable button element that supports text, an icon, or both simultaneously.
    /// </summary>
    public sealed class UIButton : UIElement
    {
        /// <summary>
        /// The icon element, or <see langword="null"/> if this button has no icon.
        /// </summary>
        public UIElement? Icon { get; private set; }

        /// <summary>
        /// The text element, or <see langword="null"/> if this button has no text.
        /// </summary>
        public UIText? TextElement { get; private set; }

        /// <summary>
        /// The background element of the button.
        /// </summary>
        public UIElement Background { get; private set; }

        /// <summary>
        /// The content container holding the icon and/or text. Use this to adjust
        /// alignment, padding, or spacing via its <see cref="UIElement.Layout"/> after construction.
        /// </summary>
        public UIElement Content { get; private set; }

        /// <summary>
        /// The spacing in pixels between the icon and text. Only meaningful on icon+text buttons.
        /// Maps directly to <see cref="LinearUILayout.Spacing"/> on <see cref="Content"/>.
        /// </summary>
        public float IconSpacing
        {
            get => (Content.Layout as LinearUILayout)?.Spacing ?? 0f;
            set
            {
                if (Content.Layout is LinearUILayout layout)
                    layout.Spacing = value;
            }
        }

        /// <summary>
        /// The visual style controlling per-state colors for each layer.
        /// Assigning a new style immediately refreshes the button's colors.
        /// </summary>
        public UIButtonStyle Style
        {
            get => _style;
            set { _style = value; UpdateColors(); }
        }
        private UIButtonStyle _style;

        /// <summary>
        /// Fired when the button is clicked.
        /// </summary>
        public event Action? OnClick;

        private bool _isHovered = false;
        private bool _isPressed = false;

        /// <summary>
        /// Creates a text-only button.
        /// </summary>
        /// <param name="textStyle">The style used for the button text.</param>
        /// <param name="text">The text to display on the button.</param>
        /// <param name="style">The visual style for the button layers. Defaults to white on white if null.</param>
        /// <param name="backgroundDef">The panel definition for the background. Defaults to an empty container.</param>
        public UIButton(UITextStyle textStyle, string text, UIButtonStyle? style = null,
            UIPanelDef? backgroundDef = null, UIRenderArgs? arguments = null) : base(arguments, false)
        {
            Background = SetupBackground(backgroundDef, arguments);
            Content = CreateContent(UIButtonLayout.IconLeft);
            TextElement = new UIText(textStyle, text, arguments) { Interactable = false };
            Content.AddChild(TextElement);
            AddChild(Content);

            _style = style ?? UIButtonStyle.Flat(
                Background.Renderer?.Material.Value.MainColor ?? Color.White,
                textStyle.Color);

            SetupEvents();
            UpdateColors();
        }

        /// <summary>
        /// Creates an icon-only button.
        /// </summary>
        /// <param name="iconDef">The panel definition for the icon.</param>
        /// <param name="iconSize">The fixed size of the icon in pixels.</param>
        /// <param name="style">The visual style for the button layers. Defaults to white on white if null.</param>
        /// <param name="backgroundDef">The panel definition for the background. Defaults to an empty container.</param>
        public UIButton(UIPanelDef iconDef, Vector2 iconSize, UIButtonStyle? style = null,
            UIPanelDef? backgroundDef = null, UIRenderArgs? arguments = null) : base(arguments, false)
        {
            Background = SetupBackground(backgroundDef, arguments);
            Content = CreateContent(UIButtonLayout.IconLeft);
            Icon = SetupIcon(iconDef, iconSize, arguments);
            Content.AddChild(Icon);
            AddChild(Content);

            _style = style ?? UIButtonStyle.Flat(
                Background.Renderer?.Material.Value.MainColor ?? Color.White,
                iconDef.Color ?? Color.White);

            SetupEvents();
            UpdateColors();
        }

        /// <summary>
        /// Creates a button with both an icon and text.
        /// </summary>
        /// <param name="iconDef">The panel definition for the icon.</param>
        /// <param name="iconSize">The fixed size of the icon in pixels.</param>
        /// <param name="textStyle">The style used for the button text.</param>
        /// <param name="text">The text to display on the button.</param>
        /// <param name="layout">Controls how the icon and text are arranged relative to each other.</param>
        /// <param name="iconSpacing">The spacing in pixels between the icon and text.</param>
        /// <param name="style">The visual style for the button layers. Defaults to white on white if null.</param>
        /// <param name="backgroundDef">The panel definition for the background. Defaults to an empty container.</param>
        public UIButton(UIPanelDef iconDef, Vector2 iconSize, UITextStyle textStyle, string text,
            UIButtonLayout layout = UIButtonLayout.IconLeft, float iconSpacing = 4f,
            UIButtonStyle? style = null, UIPanelDef? backgroundDef = null,
            UIRenderArgs? arguments = null) : base(arguments, false)
        {
            Background = SetupBackground(backgroundDef, arguments);
            Content = CreateContent(layout);
            IconSpacing = iconSpacing;

            Icon = SetupIcon(iconDef, iconSize, arguments);
            TextElement = new UIText(textStyle, text, arguments) { Interactable = false };

            bool iconFirst = layout is UIButtonLayout.IconLeft or UIButtonLayout.IconTop;
            if (iconFirst)
            {
                Content.AddChild(Icon);
                Content.AddChild(TextElement);
            }
            else
            {
                Content.AddChild(TextElement);
                Content.AddChild(Icon);
            }

            AddChild(Content);

            _style = style ?? UIButtonStyle.Flat(
                Background.Renderer?.Material.Value.MainColor ?? Color.White,
                textStyle.Color);

            SetupEvents();
            UpdateColors();
        }

        private UIElement SetupBackground(UIPanelDef? backgroundDef, UIRenderArgs? arguments)
        {
            UIElement bg = backgroundDef != null
                ? backgroundDef.Create(arguments)
                : new UIContainer();
            bg.Interactable = false;
            AddChild(bg);
            return bg;
        }

        private UIElement SetupIcon(UIPanelDef iconDef, Vector2 iconSize, UIRenderArgs? arguments)
        {
            UIElement icon = iconDef.Create(arguments);
            icon.MinSize = iconSize;
            icon.MaxSize = iconSize;
            icon.Interactable = false;
            return icon;
        }

        private static UIElement CreateContent(UIButtonLayout layout)
        {
            UIElement content = new UIContainer();
            content.Layout = layout is UIButtonLayout.IconTop or UIButtonLayout.IconBottom
                ? new VerticalUILayout { MainAxisAlignment = UILayoutAlignment.Center, CrossAxisAlignment = UILayoutAlignment.Center }
                : new HorizontalUILayout { MainAxisAlignment = UILayoutAlignment.Center, CrossAxisAlignment = UILayoutAlignment.Center };
            content.Interactable = false;
            return content;
        }

        private void UpdateColors()
        {
            bool disabled = !Interactable;

            if (_style.Background != null)
                Background.SetColor(_style.Background.Resolve(disabled, _isPressed, _isHovered));

            if (_style.Text != null)
                TextElement?.SetColor(_style.Text.Resolve(disabled, _isPressed, _isHovered));

            if (_style.Icon != null)
                Icon?.SetColor(_style.Icon.Resolve(disabled, _isPressed, _isHovered));
        }

        private void SetupEvents()
        {
            AddEventListener(UIEventType.MouseDown, _ =>
            {
                _isPressed = true;
                UpdateColors();
            });
            AddEventListener(UIEventType.MouseUp, _ =>
            {
                _isPressed = false;
                UpdateColors();
            });
            AddEventListener(UIEventType.Click, _ =>
            {
                OnClick?.Invoke();
            });
            AddEventListener(UIEventType.MouseEnter, _ =>
            {
                _isHovered = true;
                UpdateColors();
            });
            AddEventListener(UIEventType.MouseLeave, _ =>
            {
                _isHovered = false;
                _isPressed = false;
                UpdateColors();
            });
            AddEventListener(UIEventType.LoseInteractability, _ =>
            {
                _isHovered = false;
                _isPressed = false;
                UpdateColors();
            });
        }

        public override void UpdateMesh() { }
    }
}