using System.Numerics;

namespace Electron2D.UserInterface
{
    /// <summary>
    /// A simple color-only slider component. Will be updated with a textured one.
    /// </summary>
    public class Slider : UIComponent, IGameClass
    {
        /// <summary>
        /// Listens to UI events. Replace with a non-private listener class for all UI in the future.
        /// </summary>
        private class SliderListener : UIListener
        {
            public bool ClickHeld;

            public void OnUiAction(object _sender, UIEvent _event)
            {
                if(_event == UIEvent.LeftClickDown)
                {
                    ClickHeld = true;
                }
                else if(_event == UIEvent.LeftClickUp)
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
                float length = MaxValue - MinValue;
                float distance = value - MinValue;
                return distance / length;
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

        public int SliderSizeY;
        public int BackgroundSizeY;
        public int HandleSizeXY;
        public int HandlePadding;
        public bool ForceWholeNumbers = false;
        public bool AllowNonHandleValueUpdates = true;
        public bool SliderInteractable;

        private UIComponent _backgroundPanel;
        private UIComponent _sliderPanel;
        private UIComponent _handlePanel;
        private SliderListener _handleListener = new SliderListener();
        private SliderListener _backgroundListener = new SliderListener();

        private bool _initialized = false;

        public Slider(SliderDef def, bool useScreenPosition = true, int uiRenderLayer = 0,
            bool ignorePostProcessing = false)
            : base(ignorePostProcessing, uiRenderLayer, def.SizeX, (int)MathF.Max(def.SliderSizeY, def.BackgroundSizeY),
                0, true, useScreenPosition, false, false)
        {
            SliderSizeY = def.SliderSizeY;
            BackgroundSizeY = def.BackgroundSizeY;
            HandleSizeXY = def.HandleSizeXY;
            HandlePadding = def.HandlePadding;
            ForceWholeNumbers = def.ForceWholeNumbers;
            SliderInteractable = def.Interactable;
            AllowNonHandleValueUpdates = def.AllowNonHandleValueUpdates;
            MinValue = def.MinValue;
            MaxValue = def.MaxValue;
            Value = def.InitialValue;

            if(def.BackgroundPanelDef != null)
            {
                _backgroundPanel = new SlicedPanel(def.BackgroundPanelDef, def.BackgroundMaterial, def.SizeX, BackgroundSizeY,
                    uiRenderLayer, useScreenPosition, ignorePostProcessing);
                _backgroundPanel.AddUIListener(_backgroundListener);
            }
            else
            {
                _backgroundPanel = new Panel(def.BackgroundMaterial, uiRenderLayer, def.SizeX,
                    def.BackgroundSizeY, useScreenPosition, ignorePostProcessing);
                _backgroundPanel.AddUIListener(_backgroundListener);
            }

            if (def.SliderPanelDef != null)
            {
                _sliderPanel = new SlicedPanel(def.SliderPanelDef, def.SliderMaterial, def.SizeX - HandlePadding, SliderSizeY,
                    uiRenderLayer + 1, useScreenPosition, ignorePostProcessing);
                _sliderPanel.Anchor = new Vector2(-1, 0);
            }
            else
            {
                _sliderPanel = new Panel(def.SliderMaterial, uiRenderLayer + 1, def.SizeX - HandlePadding,
                    def.SliderSizeY, useScreenPosition, ignorePostProcessing);
                _sliderPanel.Anchor = new Vector2(-1, 0);
            }

            if (def.HandlePanelDef != null)
            {
                _handlePanel = new SlicedPanel(def.HandlePanelDef, def.HandleMaterial, HandleSizeXY, HandleSizeXY,
                    uiRenderLayer + 2, useScreenPosition, ignorePostProcessing);
                _handlePanel.AddUIListener(_handleListener);
            }
            else
            {
                _handlePanel = new Panel(def.HandleMaterial, uiRenderLayer + 2, def.HandleSizeXY,
                    def.HandleSizeXY, useScreenPosition, ignorePostProcessing);
                _handlePanel.AddUIListener(_handleListener);
            }

            _initialized = true;

            UpdateDisplay();
            Engine.Game.RegisterGameClass(this);
        }

        public void FixedUpdate() { }
        protected override void OnDispose()
        {
            _sliderPanel.Dispose();
            _backgroundPanel.Dispose();
            _handlePanel.Dispose();
        }

        public override void UpdateMesh()
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (!_initialized) return;

            // Background panel
            _backgroundPanel.Transform.Position = new Vector2(
                Transform.Position.X,
                Transform.Position.Y + (Anchor.Y * BackgroundSizeY / 2f)
            );
            _backgroundPanel.SizeX = SizeX;
            _backgroundPanel.SizeY = BackgroundSizeY;

            // Slider fill
            float endPosition = ((SizeX - HandlePadding * 2) * Value01);
            _sliderPanel.Transform.Position = new Vector2(
                Transform.Position.X + LeftXBound + HandlePadding / 2f,
                Transform.Position.Y + (Anchor.Y * BackgroundSizeY / 2f)
            );
            _sliderPanel.SizeX = endPosition + HandlePadding;
            _sliderPanel.SizeY = SliderSizeY;

            // Handle position
            _handlePanel.Transform.Position = new Vector2(
                Transform.Position.X + LeftXBound + HandlePadding + endPosition,
                Transform.Position.Y + (Anchor.Y * BackgroundSizeY / 2f)
            );
        }

        public void Update()
        {
            if (!SliderInteractable) return;

            if(_backgroundListener.ClickHeld && AllowNonHandleValueUpdates)
            {
                Vector2 position = Input.GetMouseScreenPosition();
                Value01 = (position.X - (Transform.Position.X + LeftXBound + HandlePadding)) / (SizeX - HandlePadding * 2);
            }
            else if(_handleListener.ClickHeld)
            {
                Vector2 position = Input.GetMouseScreenPosition();
                Value01 = (position.X - (Transform.Position.X + LeftXBound + HandlePadding)) / (SizeX - HandlePadding * 2);
            }
        }

        protected override void OnUIEvent(UIEvent uiEvent)
        {
            switch(uiEvent)
            {
                case UIEvent.Position:
                case UIEvent.Anchor:
                case UIEvent.Resize:
                    UpdateDisplay();
                    break;
                case UIEvent.Visibility:
                    _sliderPanel.Visible = Visible;
                    _backgroundPanel.Visible = Visible;
                    _handlePanel.Visible = Visible;
                    break;
            }
        }
    }
}
