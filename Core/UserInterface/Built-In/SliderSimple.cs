using Electron2D.Core.Management;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.UserInterface;
using System.Drawing;
using System.Numerics;

namespace Electron2D.Core.UserInterface
{
    /// <summary>
    /// A simple color-only slider component. Will be updated with a textured one.
    /// </summary>
    public class SliderSimple : UiComponent
    {
        /// <summary>
        /// Listens to UI events. Replace with a non-private listener class for all UI in the future.
        /// </summary>
        private class Listener : UiListener
        {
            public bool ClickHeld;

            public void OnUiAction(object _sender, UiEvent _event)
            {
                if(_event == UiEvent.LeftClickDown)
                {
                    ClickHeld = true;
                }
                else if(_event == UiEvent.LeftClickUp)
                {
                    ClickHeld = false;
                }
            }
        }

        public float Value
        {
            get
            {
                return value;
            }
            set
            {
                float f = MathF.Max(MathF.Min(value, MaxValue), MinValue);
                this.value = ForceWholeNumbers ? MathF.Round(f) : f;
                UpdateDisplay();
            }
        }
        private float value;
        public float Value01
        {
            get
            {
                return MathF.Min(MathF.Max(value / (MaxValue - MinValue), 0), 1);
            }
            set
            {
                float f = MathF.Min(MathF.Max((value * (MaxValue - MinValue)) + MinValue, MinValue), MaxValue);
                this.value = ForceWholeNumbers ? MathF.Round(f) : f;
                UpdateDisplay();
            }
        }
        public float MinValue
        {
            get
            {
                return minValue;
            }
            set
            {
                minValue = value;
                UpdateDisplay();
            }
        }
        private float minValue;
        public float MaxValue
        {
            get
            {
                return maxValue;
            }
            set
            {
                maxValue = value;
                UpdateDisplay();
            }
        }
        private float maxValue;

        public float SliderHeight;
        public float BackgroundHeight;
        public float HandleSize;
        public float HandlePadding;
        public bool ForceWholeNumbers = false;
        public bool AllowNonHandleValueUpdates = true;
        public bool Interactable;

        private Panel backgroundPanel;
        private Panel sliderPanel;
        private Panel handlePanel;
        private Listener handleListener = new Listener();
        private Listener backgroundListener = new Listener();

        private bool initialized = false;

        public SliderSimple(Color _backgroundColor, Color _sliderColor, Color _handleColor, float _value, float _minValue, float _maxValue,
            int _sizeX, int _sliderHeight, int _backgroundHeight, int _handleSize, int _handlePadding = 0, int _uiRenderLayer = 0, bool _useScreenPosition = true, bool _interactable = true,
            bool _allowNonHandleValueUpdates = true, bool _forceWholeNumbers = false)
            : base(_uiRenderLayer, _sizeX, (int)MathF.Max(_sliderHeight, _backgroundHeight), true, _useScreenPosition, false, false)
        {
            SliderHeight = _sliderHeight;
            BackgroundHeight = _backgroundHeight;
            HandleSize = _handleSize;
            HandlePadding = _handlePadding;
            ForceWholeNumbers = _forceWholeNumbers;
            Interactable = _interactable;
            AllowNonHandleValueUpdates = _allowNonHandleValueUpdates;
            MinValue = _minValue;
            MaxValue = _maxValue;
            Value = _value;

            backgroundPanel = new Panel(_backgroundColor, _uiRenderLayer, _sizeX, _backgroundHeight, _useScreenPosition);
            backgroundPanel.AddUiListener(backgroundListener);

            sliderPanel = new Panel(_sliderColor, _uiRenderLayer + 1, _sizeX, _sliderHeight, _useScreenPosition);
            sliderPanel.Anchor = new Vector2(-1, 0);

            Texture2D t = ResourceManager.Instance.LoadTexture("Build/Resources/Textures/white_circle.png");
            handlePanel = new Panel(_handleColor, _uiRenderLayer + 2, _handleSize, _handleSize, _useScreenPosition);
            handlePanel.AddUiListener(handleListener);

            initialized = true;

            UpdateDisplay();
            Game.OnUpdateEvent += OnUpdate_Interact;
        }

        public override void UpdateMesh()
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (!initialized) return;

            backgroundPanel.Transform.Position = new Vector2(Transform.Position.X, Transform.Position.Y + (-Anchor.Y * BackgroundHeight/2f));
            backgroundPanel.SizeX = SizeX;
            backgroundPanel.SizeY = BackgroundHeight;

            float endPosition = ((SizeX-HandlePadding*2) * Value01);
            sliderPanel.Transform.Position = new Vector2(Transform.Position.X + LeftXBound, Transform.Position.Y + (-Anchor.Y * BackgroundHeight / 2f));
            sliderPanel.SizeX = endPosition + HandlePadding;
            sliderPanel.SizeY = SliderHeight;
            handlePanel.Transform.Position = new Vector2(Transform.Position.X + LeftXBound + HandlePadding + endPosition, Transform.Position.Y + (-Anchor.Y * BackgroundHeight / 2f));
        }

        private void OnUpdate_Interact()
        {
            if (!Interactable) return;

            if(backgroundListener.ClickHeld && AllowNonHandleValueUpdates)
            {
                Vector2 position = Input.GetMouseScreenPositionRaw(true);
                Value01 = (position.X - (Transform.Position.X + LeftXBound + HandlePadding)) / (SizeX - HandlePadding * 2);
            }
            else if(handleListener.ClickHeld)
            {
                Vector2 position = Input.GetMouseScreenPositionRaw(true);
                Value01 = (position.X - (Transform.Position.X + LeftXBound + HandlePadding)) / (SizeX - HandlePadding * 2);
            }
        }

        protected override void OnUiEvent(UiEvent _event)
        {
            switch(_event)
            {
                case UiEvent.Position:
                case UiEvent.Anchor:
                case UiEvent.Resize:
                    UpdateDisplay();
                    break;
                case UiEvent.Visibility:
                    sliderPanel.Visible = Visible;
                    backgroundPanel.Visible = Visible;
                    handlePanel.Visible = Visible;
                    break;
            }
        }
    }
}
