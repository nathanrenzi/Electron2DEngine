using Electron2D.Rendering;
using System.Numerics;

namespace Electron2D.UI
{
    public sealed class UISlider : UIElement
    {
        public event Action<float> OnValueChanged;
        public event Action<float> OnValueChanged01;

        public UIElement Background { get; private set; }
        public UIElement Foreground { get; private set; }
        public UIElement Handle { get; private set; }
        public float MinValue
        {
            get => _minValue;
            set
            {
                if(value != _minValue)
                {
                    _minValue = value;
                    UpdateValue(true);
                    UpdateMesh();
                }
            }
        }
        private float _minValue;
        public float MaxValue
        {
            get => _maxValue;
            set
            {
                if (value != _maxValue)
                {
                    _maxValue = value;
                    UpdateValue(true);
                    UpdateMesh();
                }
            }
        }
        private float _maxValue;
        public float Value
        {
            get => _value;
            set
            {
                if (value != _value)
                {
                    _value = value;
                    UpdateValue(true);
                    UpdateMesh();
                }
            }
        }
        private float _value;
        public float Value01 => MathEx.Clamp01((Value - MinValue) / (MaxValue - MinValue));
        private int _handleEndPadding;

        public UISlider(UISliderStyle style, float value = 0, float minValue = 0, float maxValue = 1,
            UIRenderArgs? arguments = null) : base(arguments, false)
        {
            _value = value;
            _minValue = minValue;
            _maxValue = maxValue;

            Background = style.BackgroundDef.Create(arguments);
            Background.Margin = style.BackgroundMargin;
            Background.Interactable = false;
            AddChild(Background);

            _handleEndPadding = style.HandleEndPadding;
            Foreground = style.ForegroundDef.Create(arguments);
            Foreground.Margin = new Border(style.ForegroundMargin.Left, style.ForegroundMargin.Top,
                style.ForegroundMargin.Right, style.ForegroundMargin.Bottom);
            Foreground.Interactable = false;
            AddChild(Foreground);

            Handle = style.HandleDef.Create(arguments);
            Handle.IgnoreLayout = true;
            Handle.ExplicitSize = style.HandleSize;
            AddEventListener(UIEventType.GainVisibility, (evt) => Handle.Visible = true);
            AddEventListener(UIEventType.LoseVisibility, (evt) => Handle.Visible = false);
            AddEventListener(UIEventType.GainInteractability, (evt) => Handle.Interactable = true);
            AddEventListener(UIEventType.LoseInteractability, (evt) => Handle.Interactable = false);
            AddEventListener(UIEventType.Drag, (evt) => OnDrag(evt.MousePosition));
            Handle.AddEventListener(UIEventType.Drag, (evt) => OnDrag(evt.MousePosition));
            AddChild(Handle);

            UpdateMesh();
        }

        private void OnDrag(Vector2 mouseVirtualPosition)
        {
            Rect rect = GetVirtualBounds();
            float value01 = MathEx.Clamp01((mouseVirtualPosition.X - (rect.X + _handleEndPadding)) / (rect.Width - _handleEndPadding * 2));
            Value = value01 * (MaxValue - MinValue) + MinValue;
        }

        private void UpdateValue(bool invokeEvents)
        {
            Foreground.ExplicitSize = new Vector2(MathEx.Clamp(_handleEndPadding + (Size.X - _handleEndPadding * 2) * Value01,
                0, Size.X), Size.Y - Foreground.Margin.Top - Foreground.Margin.Bottom);
            if (invokeEvents)
            {
                OnValueChanged?.Invoke(Value);
                OnValueChanged01?.Invoke(Value01);
            }
        }

        public override void UpdateMesh()
        {
            Rect rect = GetVirtualBounds();
            Handle.Pivot = new Vector2(0.5f, 0.5f);
            Handle.Position = new Vector2((int)(_handleEndPadding + (rect.Width - _handleEndPadding * 2) * Value01), (int)(rect.Height / 2f));
            UpdateValue(false);
        }
    }
}
