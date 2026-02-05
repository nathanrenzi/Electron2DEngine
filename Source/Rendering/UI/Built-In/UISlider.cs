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
                    UpdateValue();
                    UpdateMesh();
                }
            }
        }
        private float _minValue = 0;
        public float MaxValue
        {
            get => _maxValue;
            set
            {
                if (value != _maxValue)
                {
                    _maxValue = value;
                    UpdateValue();
                    UpdateMesh();
                }
            }
        }
        private float _maxValue = 1;
        public float Value
        {
            get => _value;
            set
            {
                if (value != _value)
                {
                    _value = value;
                    UpdateValue();
                    UpdateMesh();
                }
            }
        }
        private float _value = 0;
        public float Value01 => MathEx.Clamp01((Value - MinValue) / (MaxValue - MinValue));
        private int _handleEndPadding;
        private Border _foregroundMargin;

        public UISlider(UISliderStyle style, int sizeX = 0, int sizeY = 0,
            int renderLayer = 0, bool useScreenPosition = true, bool ignorePostProcessing = true)
            : base(sizeX, sizeY, renderLayer, useScreenPosition, ignorePostProcessing, false, true)
        {
            Background = style.BackgroundDef.Create(sizeX, sizeY, renderLayer, useScreenPosition, ignorePostProcessing);
            Background.Margin = style.BackgroundMargin;
            AddChild(Background);

            _handleEndPadding = style.HandleEndPadding;
            Foreground = style.ForegroundDef.Create(sizeX, sizeY, renderLayer, useScreenPosition, ignorePostProcessing);
            _foregroundMargin = style.ForegroundMargin;
            Foreground.Margin = _foregroundMargin;
            AddChild(Foreground);

            Handle = style.HandleDef.Create((int)style.HandleSize.X, (int)style.HandleSize.Y, renderLayer, useScreenPosition, ignorePostProcessing);
            AddEventListener(UIEventType.GainVisibility, (evt) => Handle.Visible = true);
            AddEventListener(UIEventType.LoseVisibility, (evt) => Handle.Visible = false);
            AddEventListener(UIEventType.Drag, (evt) => OnDrag(evt.MousePosition));
            Handle.AddEventListener(UIEventType.Drag, (evt) => OnDrag(evt.MousePosition));
            RenderLayerManager.RemoveRenderable(Handle);

            UpdateMesh();

            CanAddChildren = false;
        }

        private void OnDrag(Vector2 mouseVirtualPosition)
        {
            Rect rect = GetVirtualRect();
            float value01 = MathEx.Clamp01((mouseVirtualPosition.X - (rect.X + _handleEndPadding)) / (rect.Width - _handleEndPadding * 2));
            Value = value01 * (MaxValue - MinValue) + MinValue;
        }

        private void UpdateValue()
        {
            OnValueChanged?.Invoke(Value);
            OnValueChanged01?.Invoke(Value01);
            Foreground.Margin = new Border(0, _foregroundMargin.Top, MathEx.Clamp(Size.X * (1 - Value01), 0, Size.X), _foregroundMargin.Bottom);
        }

        public override void UpdateMesh()
        {
            Rect rect = GetVirtualRect();
            Handle.Pivot = new Vector2(0.5f, 0.5f);
            Handle.Position = new Vector2((int)(rect.X + _handleEndPadding + (rect.Width - _handleEndPadding * 2) * Value01), (int)(rect.Y + rect.Height / 2f));
        }

        public override void Render()
        {
            base.Render();
            Handle.Render();
        }

        protected override void OnDispose()
        {
            Handle.Dispose();
            Handle = null;
        }
    }
}
