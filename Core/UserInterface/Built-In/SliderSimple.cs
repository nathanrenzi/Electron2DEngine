using Electron2D.Core.UI;
using System.Drawing;
using System.Numerics;

namespace Electron2D.Core.UserInterface
{
    public class SliderSimple : UiComponent
    {
        /// <summary>
        /// Listens to UI events. Replace with a non-private listener class in the future.
        /// </summary>
        private class Listener : UiListener
        {
            public bool ButtonHeld;

            public void OnUiAction(object _sender, UiEvent _event)
            {
                if(_event == UiEvent.LeftClickDown)
                {
                    ButtonHeld = true;
                }
                else if(_event == UiEvent.LeftClickUp)
                {
                    ButtonHeld = false;
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
                this.value = MathF.Max(MathF.Min(value, MaxValue), MinValue);
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
                this.value = MathF.Min(MathF.Max((value * (MaxValue - MinValue)) + MinValue, 0), 1);
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

        public bool Interactable;

        private Panel backgroundPanel;
        private Panel completedPanel;
        private Panel handlePanel;
        private Listener listener = new Listener();

        public SliderSimple(Color _backgroundColor, Color _completedColor, Color _handleColor, float _value, float _minValue, float _maxValue,
            int _sizeX, int _sizeY, int _fgSizeY, int _handleSize, int _uiRenderLayer = 0, bool _useScreenPosition = true)
            : base(_uiRenderLayer, _sizeX, _sizeY, true, _useScreenPosition, false)
        {
            backgroundPanel = new Panel(_backgroundColor, _uiRenderLayer, _sizeX, _sizeY, _useScreenPosition);
            completedPanel = new Panel(_completedColor, _uiRenderLayer + 1, _sizeX, _fgSizeY, _useScreenPosition);
            completedPanel.Anchor = new Vector2(-1, 0);
            completedPanel.Transform.Position = new Vector2(Transform.Position.X + LeftXBound, 0);

            handlePanel = new Panel(_handleColor, _uiRenderLayer + 2, _handleSize, _handleSize, _useScreenPosition);
            handlePanel.AddUiListener(listener);

            minValue = _minValue;
            maxValue = _maxValue;
            Value = _value;

            UpdateDisplay();
            Game.OnUpdateEvent += HandlePress;
        }

        private void UpdateDisplay()
        {
            completedPanel.SizeX = SizeX * Value01;
            handlePanel.Transform.Position = new Vector2(Transform.Position.X + LeftXBound + (SizeX * Value01), 0);
        }

        private void HandlePress()
        {
            if(listener.ButtonHeld)
            {
                Vector2 position = Input.GetMouseScreenPositionRaw(true);
                Value01 = (position.X - (Transform.Position.X + LeftXBound)) / SizeX;
            }
        }
    }
}
