using Electron2D.Rendering;
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
        private bool _isHovered = false;
        private bool _isPressed = false;

        public UIButton(UITextStyle textStyle, string text, UIPanelDef? backgroundDef = null,
            UIRenderArgs? arguments = null) : base(arguments, false, true)
        {
            SetupBackground(backgroundDef, arguments);
            TextElement = new UIText(textStyle, text, arguments);
            TextElement.Interactable = false;
            Background!.AddChild(TextElement);
            SetupEvents();
            CanAddChildren = false;
        }

        public UIButton(UIPanelDef iconDef, Vector2 iconSize,
            UIPanelDef? backgroundDef = null, UIRenderArgs? arguments = null)
            : base(arguments, true, false)
        {
            SetupBackground(backgroundDef, arguments);
            Icon = iconDef.Create(new UIRenderArgs(arguments ?? default));
            Icon.Size = iconSize;
            Icon.Interactable = false;
            Background!.AddChild(Icon);
            SetupEvents();
            CanAddChildren = false;
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

        private void SetupEvents()
        {
            AddEventListener(UIEventType.MouseDown, evt =>
            {
                if (Interactable) Background.SetColor(PressedBackgroundColor);
                _isPressed = true;
            });
            AddEventListener(UIEventType.MouseUp, evt =>
            {
                Background.SetColor(Interactable ? _isHovered ? HoverBackgroundColor : NormalBackgroundColor : DisabledBackgroundColor);
                _isPressed = false;
            });
            AddEventListener(UIEventType.MouseEnter, evt =>
            {
                if (!_isPressed) Background.SetColor(HoverBackgroundColor);
                _isHovered = true;
            });
            AddEventListener(UIEventType.MouseLeave, evt =>
            {
                if (!_isPressed) Background.SetColor(NormalBackgroundColor);
                _isHovered = false;
            });
            AddEventListener(UIEventType.LoseInteractability, evt =>
            {
                Background.SetColor(DisabledBackgroundColor);
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
