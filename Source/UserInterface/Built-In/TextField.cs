using Electron2D.Misc.Input;
using Electron2D.Rendering;
using Electron2D.Rendering.Shaders;
using Electron2D.Rendering.Text;
using GLFW;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace Electron2D.UserInterface
{
    public class TextField : UIComponent, IKeyListener
    {
        public Color TextColor
        {
            get
            {
                return _textColor;
            }
            set
            {
                _textColor = value;
                if(_builder?.Length > 0)
                {
                    if(_textLabel != null) _textLabel.TextColor = value;
                }
            }
        }
        private Color _textColor;

        public Color PromptTextColor
        {
            get
            {
                return _promptTextColor;
            }
            set
            {
                _promptTextColor = value;
                if (_builder?.Length == 0)
                {
                    if (_textLabel != null) _textLabel.TextColor = value;
                }
            }
        }
        private Color _promptTextColor;

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                if (_initialized)
                {
                    _textLabel.Text = value.Length > 0 ? value : _promptText;
                    UpdateCaretDisplay();
                    if (!WaitForEnterToUpdate) OnTextEntered?.Invoke(value);
                }
            }
        }
        private string _text;

        public string PromptText
        {
            get
            {
                return _promptText;
            }
            set
            {
                _promptText = value;
            }
        }
        private string _promptText;

        public Vector4 TextAreaPadding
        {
            get
            {
                return _textAreaPadding;
            }
            set
            {
                _textAreaPadding = value;
                UpdateDisplay();
            }
        }
        private Vector4 _textAreaPadding;

        public uint MaxCharacterCount
        {
            get
            {
                return _maxCharacterCount;
            }
            set
            {
                _maxCharacterCount = value;
                if(Text.Length > _maxCharacterCount)
                {
                    Text = Text.Substring(0, (int)_maxCharacterCount);
                }
            }
        }
        private uint _maxCharacterCount;

        public int CaretWidth
        {
            get
            {
                return _caretIndex;
            }
            set
            {
                _caretIndex = value;
                if(_caretPanel != null) _caretPanel.SizeX = value;
            }
        }
        private int _caretWidth;

        public new TextRenderer Renderer => _textLabel.Renderer;

        public bool WaitForEnterToUpdate { get; set; }

        public event Action<string> OnTextEntered;

        private Panel _caretPanel;
        private UIComponent _backgroundPanel;
        private TextLabel _textLabel;
        private bool _initialized = false;
        private int _caretIndex;
        private StringBuilder _builder;
        private Material _caretMaterial;
        private bool _flagUpdateCaret = false;
        private char _holdingChar;
        private float _holdingCharTime = 0;
        private float _holdingRepeatTime = 0;
        private bool _holdingLeftControl = false;
        private const float HOLDING_ACTION_TIME = 0.5f;
        private const float HOLDING_REPEAT_INTERVAL = 1/30f;

        public TextField(TextFieldDef def, bool useScreenPosition = true, int uiRenderLayer = 0,
            bool ignorePostProcessing = false)
            : base(ignorePostProcessing, uiRenderLayer, def.SizeX, def.SizeY,
                0, true, useScreenPosition, false, true)
        {
            Text = def.Text;
            PromptText = def.PromptText;
            WaitForEnterToUpdate = def.WaitForEnterToUpdate;
            TextAreaPadding = def.TextAreaPadding;
            _caretWidth = def.CaretWidth;
            _maxCharacterCount = def.MaxCharacterCount;
            if(def.BackgroundPanelDef != null)
            {
                _backgroundPanel = new SlicedPanel(def.BackgroundPanelMaterial, def.SizeX, def.SizeY, def.BackgroundPanelDef,
                    uiRenderLayer - 1, useScreenPosition, ignorePostProcessing);
            }
            else
            {
                _backgroundPanel = new Panel(def.BackgroundPanelMaterial, uiRenderLayer - 1, def.SizeX, def.SizeY,
                    useScreenPosition, ignorePostProcessing);
            }
            _backgroundPanel.Interactable = false;

            _textLabel = new TextLabel(def.Text, def.TextFont.Arguments.FontName, def.TextFont.Arguments.FontSize, Color.White, Color.White,
                new Vector2(def.SizeX - (def.TextAreaPadding.X + def.TextAreaPadding.Y),
                def.SizeY - (def.TextAreaPadding.Z + def.TextAreaPadding.W)), def.TextHorizontalAlignment, def.TextVerticalAlignment, def.TextAlignmentMode, def.TextOverflowMode, 0, uiRenderLayer,
                def.TextMaterial.Shader, useScreenPosition, ignorePostProcessing);
            _textLabel.Interactable = false;
            _builder = new StringBuilder(Text);
            _caretMaterial = Material.Create(new Shader(Shader.ParseShader("Resources/Built-In/Shaders/UserInterface/CaretBlink.glsl"), true, new string[] { "time" }));
            _caretPanel = new Panel(_caretMaterial, uiRenderLayer + 2, _caretWidth, def.TextFont.Arguments.FontSize, useScreenPosition, ignorePostProcessing);
            _caretPanel.Visible = false;
            _caretPanel.Interactable = false;
            _textColor = def.TextColor;
            _promptTextColor = def.PromptTextColor;
            UpdateCaretDisplay();
            _initialized = true;
            UpdateDisplay();
            Game.LateUpdateEvent += LateUpdate;
        }

        public void LateUpdate()
        {
            if (_holdingChar != (char)0)
            {
                if (_holdingCharTime >= HOLDING_ACTION_TIME)
                {
                    if (_holdingRepeatTime >= HOLDING_REPEAT_INTERVAL)
                    {
                        _holdingRepeatTime -= HOLDING_REPEAT_INTERVAL;
                        KeyPressed(_holdingChar);
                    }
                    _holdingRepeatTime += Time.DeltaTime;
                }
                _holdingCharTime += Time.DeltaTime;
            }

            // Workaround since updates to caret shader in input callback were not working
            if (_flagUpdateCaret)
            {
                UpdateCaretDisplay();
                _flagUpdateCaret = false;
            }
        }

        public override void UpdateMesh()
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (!_initialized) return;
            _backgroundPanel.SizeX = SizeX;
            _backgroundPanel.SizeY = SizeY;
            _backgroundPanel.Transform.Position = Transform.Position;
            _textLabel.SizeX = SizeX - (TextAreaPadding.X + TextAreaPadding.Y);
            _textLabel.SizeY = SizeY - (TextAreaPadding.Z + TextAreaPadding.W);
            _textLabel.Transform.Position = Transform.Position + (new Vector2(TextAreaPadding.X, TextAreaPadding.W) * 0.5f) - (new Vector2(TextAreaPadding.Y, TextAreaPadding.Z) * 0.5f);
            UpdateCaretDisplay();
        }

        private void UpdateCaretDisplay()
        {
            if(!_initialized) return;
            _caretPanel.Transform.Position = _textLabel.Renderer.GetCaretWorldPostion(_caretIndex) + new Vector2(_caretPanel.SizeX / 2f, _caretPanel.SizeY / 3f);
            _caretMaterial.Shader.SetFloat("startTime", Time.GameTime);
        }

        private void OnClick()
        {
            _caretIndex = _builder.Length == 0 ? 0 : _textLabel.Renderer.GetCaretIndexFromWorldPosition(Input.GetOffsetMousePosition());
            UpdateCaretDisplay();
        }

        private void OnFocus()
        {
            if (!_initialized) return;
            UpdateCaretDisplay();
            _caretPanel.Visible = true;
            Input.LockKeyInput(this);
            Input.AddListener(this);
        }

        private void OnLoseFocus()
        {
            if (!_initialized) return;
            _caretPanel.Visible = false;
            Input.UnlockKeyInput(this);
            Input.RemoveListener(this);
        }

        protected override void OnUiEvent(UiEvent uiEvent)
        {
            switch (uiEvent)
            {
                case UiEvent.ClickDown:
                    OnClick();
                    break;
                case UiEvent.LoseFocus:
                    OnLoseFocus();
                    break;
                case UiEvent.Focus:
                    OnFocus();
                    break;
                case UiEvent.Position:
                case UiEvent.Anchor:
                case UiEvent.Resize:
                    UpdateDisplay();
                    break;
                case UiEvent.Visibility:
                    _backgroundPanel.Visible = Visible;
                    _textLabel.Visible = Visible;
                    break;
            }
        }

        public void KeyPressed(char code)
        {
            _flagUpdateCaret = true;
            if (code == (char)Keys.LeftControl)
            {
                _holdingLeftControl = true;
            }
            else if (!char.IsAscii(code) && _holdingChar != code)
            {
                _holdingChar = code;
                _holdingCharTime = 0;
                _holdingRepeatTime = 0;
            }

            if (code == (char)259)
            {
                if (_caretIndex == 0) return;
                if (_builder.Length == 0) return;
                int toIndex = _holdingLeftControl ? _builder.ToString().LastIndexOf(" ", _caretIndex - 1) : _caretIndex - 1;
                toIndex = toIndex == -1 ? 0 : toIndex;
                do
                {
                    _builder.Remove(_caretIndex - 1, 1);
                    _caretIndex--;
                } while (_caretIndex - 1 >= toIndex && _caretIndex - 1 >= 0);
                if (_caretIndex < 0) _caretIndex = 0;
                if (!WaitForEnterToUpdate) OnTextEntered?.Invoke(_builder.ToString());
            }
            else if(code == 262)
            {
                int toIndex = _holdingLeftControl ? _builder.ToString().IndexOf(" ", _caretIndex) : _caretIndex;
                toIndex = toIndex == -1 ? _builder.Length : toIndex;
                do
                {
                    _caretIndex++;
                } while (_caretIndex < toIndex);
                if(_caretIndex > _builder.Length) _caretIndex = _builder.Length;
            }
            else if(code == 263)
            {
                int toIndex = _holdingLeftControl ? _builder.ToString().LastIndexOf(" ", _caretIndex) : _caretIndex;
                toIndex = toIndex == -1 ? 0 : toIndex;
                do
                {
                    _caretIndex--;
                } while (_caretIndex > toIndex);
                if (_caretIndex < 0) _caretIndex = 0;
            }
            else
            {
                if(_builder.Length >= _maxCharacterCount) return;
                if (char.IsAscii(code))
                {
                    if (_builder.Length == 0) _caretIndex = 0;
                    _builder.Insert(_caretIndex, code);
                    _caretIndex++;
                }
            }

            if(_builder.Length == 0)
            {
                Text = _promptText;
                _textLabel.TextColor = _promptTextColor;
                _caretIndex = Renderer.HorizontalAlignment == TextAlignment.Right ? _promptText.Length : 0;
            }
            else
            {
                Text = _builder.ToString();
                _textLabel.TextColor = _textColor;
            }
        }

        public void KeyNonASCIIReleased(char code)
        {
            if(code == (char)Keys.LeftControl)
            {
                _holdingLeftControl = false;
            }
            else if(_holdingChar == code)
            {
                _holdingChar = (char)0;
                _holdingCharTime = 0;
                _holdingRepeatTime = 0;
            }
        }
    }
}
