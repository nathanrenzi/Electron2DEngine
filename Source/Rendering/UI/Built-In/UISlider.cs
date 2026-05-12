using System.Numerics;

namespace Electron2D.UI
{
    /// <summary>
    /// A UI element that allows the user to select a value within a defined range by dragging a handle.
    /// </summary>
    public sealed class UISlider : UIElement
    {
        /// <summary>
        /// Fired when the slider value changes.
        /// </summary>
        public event Action<float> OnValueChanged;

        /// <summary>
        /// Fired when the slider value changes, normalized between 0 and 1.
        /// </summary>
        public event Action<float> OnValueChanged01;

        /// <summary>
        /// The background track element.
        /// </summary>
        public UIElement Background { get; private set; }

        /// <summary>
        /// The foreground fill element that represents the current value.
        /// </summary>
        public UIElement Foreground { get; private set; }

        /// <summary>
        /// The draggable handle element.
        /// </summary>
        public UIElement Handle { get; private set; }

        /// <summary>
        /// The minimum value of the slider.
        /// </summary>
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

        /// <summary>
        /// The maximum value of the slider.
        /// </summary>
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

        /// <summary>
        /// The current value of the slider, clamped between <see cref="MinValue"/> and <see cref="MaxValue"/>.
        /// </summary>
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

        /// <summary>
        /// The current value of the slider normalized between 0 and 1.
        /// </summary>
        public float Value01 => MathEx.Clamp01((Value - MinValue) / (MaxValue - MinValue));
        private int _handleEndPadding;

        /// <summary>
        /// Creates a new <see cref="UISlider"/>.
        /// </summary>
        /// <param name="style">The visual style of the slider.</param>
        /// <param name="value">The initial value. Defaults to 0.</param>
        /// <param name="minValue">The minimum value. Defaults to 0.</param>
        /// <param name="maxValue">The maximum value. Defaults to 1.</param>
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
