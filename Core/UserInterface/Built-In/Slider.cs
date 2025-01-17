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
    public class Slider : UiComponent
    {
        /// <summary>
        /// Listens to UI events. Replace with a non-private listener class for all UI in the future.
        /// </summary>
        private class SliderListener : UiListener
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

        private UiComponent backgroundPanel;
        private UiComponent sliderPanel;
        private UiComponent handlePanel;
        private SliderListener handleListener = new SliderListener();
        private SliderListener backgroundListener = new SliderListener();

        private bool initialized = false;

        public Slider(SliderDef _def, bool _useScreenPosition = true, int _uiRenderLayer = 0,
            bool _ignorePostProcessing = false)
            : base(_ignorePostProcessing, _uiRenderLayer, _def.SizeX, (int)MathF.Max(_def.SliderSizeY, _def.BackgroundSizeY),
                0, true, _useScreenPosition, false, false)
        {
            SliderSizeY = _def.SliderSizeY;
            BackgroundSizeY = _def.BackgroundSizeY;
            HandleSizeXY = _def.HandleSizeXY;
            HandlePadding = _def.HandlePadding;
            ForceWholeNumbers = _def.ForceWholeNumbers;
            SliderInteractable = _def.Interactable;
            AllowNonHandleValueUpdates = _def.AllowNonHandleValueUpdates;
            MinValue = _def.MinValue;
            MaxValue = _def.MaxValue;
            Value = _def.InitialValue;

            if(_def.BackgroundPanelDef != null)
            {
                backgroundPanel = new SlicedPanel(_def.BackgroundMaterial, _def.SizeX, BackgroundSizeY,
                    _def.BackgroundPanelDef, _uiRenderLayer, _ignorePostProcessing);
                backgroundPanel.AddUiListener(backgroundListener);
            }
            else
            {
                backgroundPanel = new Panel(_def.BackgroundMaterial, _uiRenderLayer, _def.SizeX,
                    _def.BackgroundSizeY, _useScreenPosition, _ignorePostProcessing);
                backgroundPanel.AddUiListener(backgroundListener);
            }

            if (_def.SliderPanelDef != null)
            {
                sliderPanel = new SlicedPanel(_def.SliderMaterial, _def.SizeX - HandlePadding, SliderSizeY,
                    _def.SliderPanelDef, _uiRenderLayer + 1, _ignorePostProcessing);
                sliderPanel.Anchor = new Vector2(-1, 0);
            }
            else
            {
                sliderPanel = new Panel(_def.SliderMaterial, _uiRenderLayer + 1, _def.SizeX - HandlePadding,
                    _def.SliderSizeY, _useScreenPosition, _ignorePostProcessing);
                sliderPanel.Anchor = new Vector2(-1, 0);
            }

            if (_def.HandlePanelDef != null)
            {
                handlePanel = new SlicedPanel(_def.HandleMaterial, HandleSizeXY, HandleSizeXY,
                    _def.HandlePanelDef, _uiRenderLayer + 2, _ignorePostProcessing);
                handlePanel.AddUiListener(handleListener);
            }
            else
            {
                handlePanel = new Panel(_def.HandleMaterial, _uiRenderLayer + 2, _def.HandleSizeXY,
                    _def.HandleSizeXY, _useScreenPosition, _ignorePostProcessing);
                handlePanel.AddUiListener(handleListener);
            }

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

            backgroundPanel.Transform.Position = new Vector2(Transform.Position.X, Transform.Position.Y + (-Anchor.Y * BackgroundSizeY / 2f));
            backgroundPanel.SizeX = SizeX;
            backgroundPanel.SizeY = BackgroundSizeY;

            float endPosition = ((SizeX-HandlePadding*2) * Value01);
            sliderPanel.Transform.Position = new Vector2(Transform.Position.X + LeftXBound + HandlePadding / 2f, Transform.Position.Y + (-Anchor.Y * BackgroundSizeY / 2f));
            sliderPanel.SizeX = endPosition + HandlePadding;
            sliderPanel.SizeY = SliderSizeY;

            handlePanel.Transform.Position = new Vector2(Transform.Position.X + LeftXBound + HandlePadding + endPosition, Transform.Position.Y + (-Anchor.Y * BackgroundSizeY / 2f));
        }

        private void OnUpdate_Interact()
        {
            if (!SliderInteractable) return;

            if(backgroundListener.ClickHeld && AllowNonHandleValueUpdates)
            {
                Vector2 position = Input.GetOffsetMousePosition();
                Value01 = (position.X - (Transform.Position.X + LeftXBound + HandlePadding)) / (SizeX - HandlePadding * 2);
            }
            else if(handleListener.ClickHeld)
            {
                Vector2 position = Input.GetOffsetMousePosition();
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
