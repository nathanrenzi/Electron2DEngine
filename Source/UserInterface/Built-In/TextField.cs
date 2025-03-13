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
        /// <summary>
        /// The color of the text.
        /// </summary>
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

        /// <summary>
        /// The color of the prompt text.
        /// </summary>
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

        /// <summary>
        /// The text in the text field.
        /// </summary>
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
                }
            }
        }
        private string _text;

        /// <summary>
        /// The text displayed when the text field is empty.
        /// </summary>
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

        /// <summary>
        /// The padding of the text area in relation to the background.
        /// </summary>
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

        /// <summary>
        /// The maximum amount of characters in the text field.
        /// </summary>
        public int MaxCharacterCount
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
        private int _maxCharacterCount;

        /// <summary>
        /// The maximum amount of lines in the text field.
        /// </summary>
        public int MaxLineCount
        {
            get
            {
                return _maxLineCount;
            }
            set
            {
                _maxLineCount = value;
            }
        }
        private int _maxLineCount;

        /// <summary>
        /// The width of the caret in pixels.
        /// </summary>
        public int CaretWidth
        {
            get
            {
                return _caretWidth;
            }
            set
            {
                _caretWidth = value;
                if(_caretPanel != null) _caretPanel.SizeX = value;
            }
        }
        private int _caretWidth;

        /// <summary>
        /// The text renderer being used.
        /// </summary>
        public TextRenderer TextRenderer => _textLabel.Renderer;

        /// <summary>
        /// The renderer being used for the background.
        /// </summary>
        public new MeshRenderer Renderer => _backgroundPanel.Renderer;

        /// <summary>
        /// Should the <see cref="OnTextEntered"/> event be called when the enter key is pressed, or when the text is updated?
        /// </summary>
        public bool WaitForEnterKey { get; set; }

        /// <summary>
        /// Called when the user enters text (either when text updates or enter key is pressed, see <see cref="WaitForEnterKey"/>).
        /// </summary>
        public event Action<string> OnTextEntered;

        /// <summary>
        /// Called when the text is updated.
        /// </summary>
        public event Action OnTextUpdated;

        private float _currentLineCount
        {
            get
            {
                int count = 0;
                if (_text.Length > 0) count++;
                for(int i = 0; i < _text.Length; i++)
                {
                    if (_text[i] == '\n') count++;
                }
                return count;
            }
        }
        private Panel _caretPanel;
        private UIComponent _backgroundPanel;
        private TextLabel _textLabel;
        private bool _initialized = false;
        private TextRenderer.Iterator _iterator;
        private StringBuilder _builder;
        private Material _caretMaterial;
        private bool _flagUpdateCaret = false;
        private char _holdingChar;
        private float _holdingCharTime = 0;
        private float _holdingRepeatTime = 0;
        private bool _holdingLeftControl = false;
        private const float HOLDING_ACTION_TIME = 0.5f;
        private const float HOLDING_REPEAT_INTERVAL = 1/30f;

        /// <summary>
        /// Creates a new text field.
        /// </summary>
        /// <param name="def">The definition of the text field.</param>
        /// <param name="useScreenPosition">Whether the position of this object represents the screen position or world position.</param>
        /// <param name="uiRenderLayer">The UI render layer of this object. Added onto <see cref="RenderLayer.Interface"/> so that UI components are rendered on top.</param>
        /// <param name="ignorePostProcessing">Should this object ignore post processing effects?</param>
        public TextField(TextFieldDef def, bool useScreenPosition = true, int uiRenderLayer = 0,
            bool ignorePostProcessing = false)
            : base(ignorePostProcessing, uiRenderLayer, def.SizeX, def.SizeY,
                0, true, useScreenPosition, false, true)
        {
            Text = def.Text;
            PromptText = def.PromptText;
            WaitForEnterKey = def.WaitForEnterKey;
            TextAreaPadding = def.TextAreaPadding;
            _caretWidth = def.CaretWidth;
            _maxCharacterCount = def.MaxCharacterCount;
            _maxLineCount = def.MaxLineCount;
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
            _caretMaterial = def.CaretMaterial == null ? 
                Material.Create(new Shader(Shader.ParseShader("Resources/Built-In/Shaders/UserInterface/CaretBlink.glsl"), true, new string[] { "time" }))
                : def.CaretMaterial;
            _caretPanel = new Panel(_caretMaterial, uiRenderLayer + 2, _caretWidth, def.TextFont.Arguments.FontSize, useScreenPosition, ignorePostProcessing);
            _caretPanel.Visible = false;
            _caretPanel.Interactable = false;
            _textColor = def.TextColor;
            _promptTextColor = def.PromptTextColor;
            _iterator = _textLabel.Renderer.GetIterator();
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
            _caretPanel.Transform.Position = _textLabel.Renderer.GetCaretWorldPostion(_iterator.Index) + new Vector2(_caretPanel.SizeX / 2f, _caretPanel.SizeY / 3f);
            _caretMaterial.Shader.SetFloat("startTime", Time.GameTime);
        }

        private void OnClick()
        {
            _iterator.SetIndex(_builder.Length == 0 ? 0 : _textLabel.Renderer.GetCaretIndexFromWorldPosition(Input.GetMouseScreenPosition()));
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
            _holdingChar = (char)0;
            _holdingCharTime = 0;
            _holdingRepeatTime = 0;
            _holdingLeftControl = false;
        }

        protected override void OnUIEvent(UIEvent uiEvent)
        {
            switch (uiEvent)
            {
                case UIEvent.ClickDown:
                    OnClick();
                    break;
                case UIEvent.LoseFocus:
                    OnLoseFocus();
                    break;
                case UIEvent.Focus:
                    OnFocus();
                    break;
                case UIEvent.Position:
                case UIEvent.Anchor:
                case UIEvent.Resize:
                    UpdateDisplay();
                    break;
                case UIEvent.Visibility:
                    _backgroundPanel.Visible = Visible;
                    _textLabel.Visible = Visible;
                    break;
            }
        }

        public void KeyPressed(char code)
        {
            bool textUpdated = false;
            if (code == (char)Keys.Enter)
            {
                if(_holdingChar == (char)Keys.LeftShift || _holdingChar == (char)Keys.RightShift)
                {
                    if (_builder.Length >= _maxCharacterCount) return;
                    if (_builder.Length == 0) _iterator.SetIndex(0);
                    if (_currentLineCount >= _maxLineCount) return;
                    _builder.Insert(_iterator.Index, '\n');
                    _iterator.Increment();
                    textUpdated = true;
                    _flagUpdateCaret = true;
                }
                else
                {
                    OnTextEntered?.Invoke(Text);
                    Unfocus();
                    return;
                }
            }
            else
            {
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
                    if (_iterator.Index == 0) return;
                    if (_builder.Length == 0) return;
                    do
                    {
                        _builder.Remove(_iterator.Index - 1, 1);
                        _iterator.Decrement();
                    } while (_holdingLeftControl && _iterator.Index - 1 >= 0 && _builder[_iterator.Index - 1] != ' ');
                    textUpdated = true;
                    _flagUpdateCaret = true;
                }
                else if (code == 262)
                {
                    // Right
                    do
                    {
                        _iterator.Increment();
                    } while (_holdingLeftControl && _iterator.Index < _builder.Length && (_iterator.Index == 0 || (_iterator.Index != 0 && _builder[_iterator.Index - 1] != ' ')));
                    _flagUpdateCaret = true;
                }
                else if (code == 263)
                {
                    // Left
                    do
                    {
                        _iterator.Decrement();
                    } while (_holdingLeftControl && _iterator.Index > 0 && (_iterator.Index == _builder.Length || (_iterator.Index < _builder.Length && _builder[_iterator.Index - 1] != ' ')));
                    _flagUpdateCaret = true;
                }
                else
                {
                    if (_builder.Length >= _maxCharacterCount) return;
                    if (char.IsAscii(code) && !char.IsControl(code))
                    {
                        if (_builder.Length == 0) _iterator.SetIndex(0);
                        _builder.Insert(_iterator.Index, code);
                        _iterator.Increment();
                        textUpdated = true;
                        _flagUpdateCaret = true;
                    }
                }
            }

            if(_builder.Length == 0)
            {
                _text = "";
                _textLabel.Text = _promptText;
                _textLabel.TextColor = _promptTextColor;
                _iterator.SetIndex(TextRenderer.HorizontalAlignment == TextAlignment.Right ? _promptText.Length : 0);
            }
            else
            {
                Text = _builder.ToString();
                _textLabel.TextColor = _textColor;
            }

            if(textUpdated)
            {
                OnTextUpdated?.Invoke();
                if (!WaitForEnterKey) OnTextEntered?.Invoke(_text);
            }
            _iterator.Validate();
        }

        public void KeyNonAlphaReleased(char code)
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
