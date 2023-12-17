using Electron2D.Core.Management;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.UI;
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
                Display();
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
                Display();
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
                Display();
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
                Display();
            }
        }
        private float maxValue;

        public bool ForceWholeNumbers = false;
        public bool AllowNonHandleValueUpdates = true;
        public bool Interactable;

        private Panel backgroundPanel;
        private Panel completedPanel;
        private Panel handlePanel;
        private Listener handleListener = new Listener();
        private Listener backgroundListener = new Listener();

        public SliderSimple(Color _backgroundColor, Color _completedColor, Color _handleColor, float _value, float _minValue, float _maxValue,
            int _sizeX, int _sizeY, int _fgSizeY, int _handleSize, int _uiRenderLayer = 0, bool _useScreenPosition = true, bool _interactable = true,
            bool _allowNonHandleValueUpdates = true, bool _forceWholeNumbers = false)
            : base(_uiRenderLayer, _sizeX, _sizeY, true, _useScreenPosition, false)
        {
            backgroundPanel = new Panel(_backgroundColor, _uiRenderLayer, _sizeX, _sizeY, _useScreenPosition);
            backgroundPanel.AddUiListener(backgroundListener);

            completedPanel = new Panel(_completedColor, _uiRenderLayer + 1, _sizeX, _fgSizeY, _useScreenPosition);
            completedPanel.Anchor = new Vector2(-1, 0);
            completedPanel.Transform.Position = new Vector2(Transform.Position.X + LeftXBound, 0);

            Texture2D t = ResourceManager.Instance.LoadTexture("Build/Resources/Textures/white_circle.png");
            handlePanel = new Panel(Material.Create(GlobalShaders.DefaultTexture, _handleColor, t, _useLinearFiltering: true), _uiRenderLayer + 2, _handleSize, _handleSize, _useScreenPosition);
            handlePanel.AddUiListener(handleListener);

            ForceWholeNumbers = _forceWholeNumbers;
            Interactable = _interactable;
            AllowNonHandleValueUpdates = _allowNonHandleValueUpdates;
            minValue = _minValue;
            maxValue = _maxValue;
            Value = _value;

            Display();
            Game.OnUpdateEvent += Update_HandlePress;
        }

        private void Display()
        {
            float endPosition = SizeX * Value01;
            completedPanel.SizeX = endPosition;
            handlePanel.Transform.Position = new Vector2(Transform.Position.X + LeftXBound + endPosition, 0);
        }

        private void Update_HandlePress()
        {
            if (!Interactable) return;

            if(backgroundListener.ClickHeld && AllowNonHandleValueUpdates)
            {
                Vector2 position = Input.GetMouseScreenPositionRaw(true);
                Value01 = (position.X - (Transform.Position.X + LeftXBound)) / SizeX;
            }
            else if(handleListener.ClickHeld)
            {
                Vector2 position = Input.GetMouseScreenPositionRaw(true);
                Value01 = (position.X - (Transform.Position.X + LeftXBound)) / SizeX;
            }
        }
    }
}
