using System.Numerics;

namespace Electron2D.UI
{
    public sealed class UISliderStyle
    {
        public UIPanelDef BackgroundDef { get; }
        public UIPanelDef ForegroundDef { get; }
        public UIPanelDef HandleDef { get; }

        public Border BackgroundMargin { get; }
        public Border ForegroundMargin { get; }

        public Vector2 HandleSize { get; }
        public int HandleEndPadding { get; }

        public UISliderStyle(UIPanelDef backgroundDef, UIPanelDef foregroundDef,
            UIPanelDef handleDef, Vector2 handleSize, Border? backgroundMargin = null,
            Border? foregroundMargin = null, int endPadding = 0)
        {
            BackgroundDef = backgroundDef ?? throw new ArgumentNullException(nameof(backgroundDef));
            ForegroundDef = foregroundDef ?? throw new ArgumentNullException(nameof(foregroundDef));
            HandleDef = handleDef ?? throw new ArgumentNullException(nameof(handleDef));
            HandleSize = handleSize;
            BackgroundMargin = backgroundMargin ?? new Border(0);
            ForegroundMargin = foregroundMargin ?? new Border(0);
            HandleEndPadding = endPadding;
        }
    }
}
