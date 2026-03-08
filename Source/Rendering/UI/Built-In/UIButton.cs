using System.Drawing;
using System.Numerics;

namespace Electron2D.UI
{
    public sealed class UIButton : UIElement
    {
        public UIElement? Icon { get; private set; }
        public UIText? TextElement { get; private set; }
        public UIElement Background { get; private set; }
        public Color NormalBackgroundColor { get; set; }
        public Color HoverBackgroundColor { get; set; }
        public Color PressedBackgroundColor { get; set; }
        public Color DisabledBackgroundColor { get; set; }
        public Color NormalForegroundColor { get; set; }
        public Color HoverForegroundColor { get; set; }
        public Color PressedForegroundColor { get; set; }
        public Color DisabledForegroundColor { get; set; }
        private bool _isHovered = false;
        private bool _isPressed = false;

        public UIButton(UITextStyle textStyle, string text, UIPanelDef? backgroundDef = null,
            UIRenderArgs? arguments = null) : base(arguments, false, true)
        {
            SetupBackground(backgroundDef, arguments);
            SetupForeground(textStyle.Color);
            TextElement = new UIText(textStyle, text, arguments);
            TextElement.Interactable = false;
            Background!.AddChild(TextElement);
            SetupEvents();
            CanAddChildren = false;
            Size = new Vector2(60, 20);
        }

        public UIButton(UIPanelDef iconDef, Vector2 iconSize,
            UIPanelDef? backgroundDef = null, UIRenderArgs? arguments = null)
            : base(arguments, true, false)
        {
            SetupBackground(backgroundDef, arguments);
            SetupForeground(iconDef.Color ?? Color.White);
            Icon = iconDef.Create(new UIRenderArgs(arguments ?? default));
            Icon.Size = iconSize;
            Icon.Interactable = false;
            Background!.AddChild(Icon);
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

            NormalBackgroundColor = Background.Renderer?.Material?.MainColor ?? Color.White;
            HoverBackgroundColor = NormalBackgroundColor;
            PressedBackgroundColor = NormalBackgroundColor;
            DisabledBackgroundColor = NormalBackgroundColor;
        }

        private void SetupForeground(Color normalColor)
        {
            NormalForegroundColor = normalColor;
            HoverForegroundColor = normalColor;
            PressedForegroundColor = normalColor;
            DisabledForegroundColor = normalColor;
        }

        private void SetupEvents()
        {
            AddEventListener(UIEventType.MouseDown, evt =>
            {
                if (Interactable)
                {
                    Background.SetColor(PressedBackgroundColor);
                    TextElement?.SetColor(PressedForegroundColor);
                    Icon?.SetColor(PressedForegroundColor);
                }
                _isPressed = true;
            });
            AddEventListener(UIEventType.MouseUp, evt =>
            {
                Background.SetColor(Interactable ? _isHovered ? HoverBackgroundColor : NormalBackgroundColor : DisabledBackgroundColor);

                Color foregroundColor = Interactable ? _isHovered ? HoverForegroundColor : NormalForegroundColor : DisabledForegroundColor;
                TextElement?.SetColor(foregroundColor);
                Icon?.SetColor(foregroundColor);

                _isPressed = false;
            });
            AddEventListener(UIEventType.MouseEnter, evt =>
            {
                if (!_isPressed)
                {
                    Background.SetColor(HoverBackgroundColor);
                    TextElement?.SetColor(HoverForegroundColor);
                    Icon?.SetColor(HoverForegroundColor);
                }
                _isHovered = true;
            });
            AddEventListener(UIEventType.MouseLeave, evt =>
            {
                if (!_isPressed)
                {
                    Background.SetColor(NormalBackgroundColor);
                    TextElement?.SetColor(NormalForegroundColor);
                    Icon?.SetColor(NormalForegroundColor);
                }
                _isHovered = false;
            });
            AddEventListener(UIEventType.LoseInteractability, evt =>
            {
                Background.SetColor(DisabledBackgroundColor);
                TextElement?.SetColor(DisabledForegroundColor);
                Icon?.SetColor(DisabledForegroundColor);
            });
        }

        // TODO: Add combined icon + text button constructor

        public override void UpdateMesh()
        {
            if (Icon != null)
            {
                Rect rect = GetVirtualBounds();
                Icon.Pivot = new Vector2(0.5f, 0.5f);
                Icon.Position = new Vector2(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
            }
        }

        public override void Render()
        {
            base.Render();
            Icon?.Render();
        }

        protected override void OnDispose()
        {
            Icon?.Dispose();
        }
    }
}
