using Electron2D.Misc.Input;
using Electron2D.Rendering;
using Electron2D.Rendering.Shaders;
using GLFW;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace Electron2D.UI
{
    public sealed class UITextInput : UIElement, IKeyListener
    {
        private const float REPEAT_DELAY = 0.4f;
        private const float REPEAT_RATE = 0.035f;

        public event Action<string> OnTextUpdate;
        public event Action OnTextUpdateFailed;
        public event Action<string> OnTextSubmit;

        public UIElement Background { get; }
        public UIText TextElement { get; }
        public UIElement CaretPanel { get; }
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if(_text != value)
                {
                    if (MaxCharacterCount > -1 && value.Length > MaxCharacterCount)
                        _text = value.Substring(0, MaxCharacterCount);
                    else
                        _text = value;
                    _builder.Clear();
                    _builder.Append(_text);
                    _caretIndex = Math.Min(_caretIndex, _text.Length);
                    UpdateTextElement();
                }
            }
        }
        private string _text = "";
        public string PromptText
        {
            get
            {
                return _promptText;
            }
            set
            {
                _promptText = value;
                UpdateTextElement();
            }
        }
        private string _promptText;
        public Color TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                UpdateTextElement();
            }
        }
        private Color _textColor;
        public Color PromptTextColor
        {
            get => _promptTextColor;
            set
            {
                _promptTextColor = value;
                UpdateTextElement();
                if (string.IsNullOrEmpty(_text))
                    UpdateCaret();
            }
        }
        private Color _promptTextColor;
        public int MaxCharacterCount
        {
            get => _maxCharacterCount;
            set
            {
                if(value != _maxCharacterCount)
                {
                    _maxCharacterCount = value;
                    if (_maxCharacterCount > -1 && _text.Length > _maxCharacterCount)
                    {
                        _text = _text.Substring(0, _maxCharacterCount);
                        _builder.Clear();
                        _builder.Append(_text);
                        _caretIndex = Math.Min(_caretIndex, _text.Length);
                        UpdateTextElement();
                        UpdateCaret();
                    }
                }
            }
        }
        private int _maxCharacterCount;
        public int MaxLineCount { get; set; }

        private KeyCode _holdingKey = KeyCode.Unknown;
        private float _startHoldingTime = float.MaxValue;
        private float _currentHoldingTime = 0;
        private bool _isLeftControlPressed = false;
        private bool _isRightControlPressed = false;
        private bool _isControlPressed => _isLeftControlPressed || _isRightControlPressed;
        private bool _isEditing = false;
        private int _caretIndex = 0;
        private List<(string, int)> _words = new();
        private StringBuilder _builder = new();

        public UITextInput(UITextInputStyle style, string text, string promptText = "", int maxCharacterCount = -1,
            int maxLineCount = -1, UIRenderArgs? arguments = null) : base(arguments, false)
        {
            _promptText = promptText;
            MaxCharacterCount = maxCharacterCount;
            MaxLineCount = maxLineCount;

            Background = style.BackgroundDef.Create(arguments);
            Background.Padding = style.TextAreaPadding;
            Background.Interactable = false;
            AddChild(Background);

            TextElement = new UIText(style.TextStyle, text, arguments: arguments)
            {
                Interactable = false
            };
            Background.AddChild(TextElement);
            TextColor = style.TextColor;
            PromptTextColor = style.PromptTextColor;
            SetHoverCursorType(CursorType.Beam);

            Vector2 caretSize = UICanvas.Instance.VirtualToScreen(new Vector2(style.CaretWidth,
                style.TextStyle.FontArguments.FontSize));
            caretSize = new Vector2(MathF.Round(caretSize.X), MathF.Round(caretSize.Y));
            caretSize = UICanvas.Instance.ScreenToVirtual(caretSize);
            if(style.CaretDef != null)
            {
                CaretPanel = style.CaretDef.Create(arguments);
            }
            else
            {
                SharedResource<Shader> shader = SharedResource<Shader>.Create(new Shader(Shader.ParseShader(
                    Resources.GetEngineResourcePath("Shaders/CaretBlink.glsl")), globalUniformTags: ["time"]));
                SharedResource<Material> mat = SharedResource<Material>.Create(Material.Create(shader));
                CaretPanel = new UIPanel(mat, arguments);
                shader.Release();
                mat.Release();
            }
            CaretPanel.IgnoreLayout = true;
            CaretPanel.ExplicitSize = caretSize;
            CaretPanel.Interactable = false;
            CaretPanel.Pivot = new Vector2(0, (float)TextElement.FontGlyphStore.Value.Ascent / style.TextStyle.FontArguments.FontSize);
            CaretPanel.Visible = Focused;
            TextElement.AddChild(CaretPanel);
            TextElement.OnLayoutComplete += UpdateCaret;

            UpdateText(text);

            AddEventListener(UIEventType.MouseDown, (evt) =>
            {
                _caretIndex = Text.Length > 0 ? TextElement.GetCharacterIndexAt(evt.MousePosition) : 0;
                UpdateCaret();
            });
            AddEventListener(UIEventType.GainFocus, (evt) =>
            {
                CaretPanel.Visible = true;
                _isEditing = true;
                Input.LockKeyInput(this, 1);
            });
            AddEventListener(UIEventType.LoseFocus, (evt) =>
            {
                CaretPanel.Visible = false;
                _isEditing = false;
                Input.UnlockKeyInput(this, 1);
            });

            Input.AddListener(this);

            Engine.Game.LateUpdateEvent += () =>
            {
                if (_holdingKey != KeyCode.Unknown)
                {
                    if (Time.GameTime - _startHoldingTime >= REPEAT_DELAY)
                    {
                        _currentHoldingTime += Time.DeltaTime;
                        if (_currentHoldingTime >= REPEAT_RATE)
                        {
                            _currentHoldingTime -= REPEAT_RATE;
                            DoSpecialKey(_holdingKey);
                        }
                    }
                }
            };
        }

        private void UpdateCaret()
        {
            if(CaretPanel != null)
            {
                CaretPanel.Position = TextElement.GetCharacterPositionAt(_caretIndex);
                CaretPanel.Renderer.Material.Value.Shader.Value.SetFloat("startTime", Time.GameTime);
            }
        }

        private void UpdateTextElement()
        {
            if(string.IsNullOrEmpty(Text))
            {
                TextElement.Text = PromptText;
                TextElement.SetColor(PromptTextColor);
            }
            else
            {
                TextElement.Text = Text;
                TextElement.SetColor(TextColor);
            }
        }

        private void DoSpecialKey(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.Left:
                    if (_isControlPressed)
                    {
                        _caretIndex = GetPreviousWordBoundary(_caretIndex);
                    }
                    else
                    {
                        _caretIndex = Math.Max(_caretIndex - 1, 0);
                    }
                    UpdateCaret();
                    break;

                case KeyCode.Right:
                    if (_isControlPressed)
                    {
                        _caretIndex = GetNextWordBoundary(_caretIndex);
                    }
                    else
                    {
                        _caretIndex = Math.Min(_caretIndex + 1, Text.Length);
                    }
                    UpdateCaret();
                    break;

                case KeyCode.Backspace:
                    if (Text.Length == 0) return;

                    if (_isControlPressed)
                    {
                        if (_caretIndex == 0) return;
                        int prevWordStart = GetPreviousWordBoundary(_caretIndex);
                        _builder.Remove(prevWordStart, _caretIndex - prevWordStart);
                        _caretIndex = prevWordStart;
                    }
                    else
                    {
                        if (_caretIndex == 0) return;
                        _builder.Remove(_caretIndex - 1, 1);
                        _caretIndex--;
                    }
                    UpdateText(_builder.ToString());
                    break;

                case KeyCode.Delete:
                    if (_caretIndex >= Text.Length) return;

                    if (_isControlPressed)
                    {
                        int nextWordEnd = GetNextWordBoundary(_caretIndex);
                        _builder.Remove(_caretIndex, nextWordEnd - _caretIndex);
                    }
                    else
                    {
                        _builder.Remove(_caretIndex, 1);
                    }
                    UpdateText(_builder.ToString());
                    break;
            }
        }

        public void OnKeyEvent(KeyEvent keyEvent)
        {
            if (!_isEditing) return;

            if (keyEvent.Type == KeyEventType.Character && keyEvent.Character.HasValue)
            {
                if (MaxCharacterCount > -1 && Text.Length >= MaxCharacterCount)
                {
                    OnTextUpdateFailed?.Invoke();
                    return;
                }
                _builder.Insert(_caretIndex, keyEvent.Character.Value);
                string text = _builder.ToString();
                _caretIndex = (int)MathF.Min(_caretIndex + 1, text.Length);
                UpdateText(text);
            }
            else if(keyEvent.IsPressed)
            {
                switch (keyEvent.KeyCode)
                {
                    case KeyCode.Enter:
                        OnTextSubmit?.Invoke(_builder.ToString());
                        Unfocus();
                        break;

                    case KeyCode.Left:
                    case KeyCode.Right:
                    case KeyCode.Backspace:
                    case KeyCode.Delete:
                        DoSpecialKey(keyEvent.KeyCode);
                        _holdingKey = keyEvent.KeyCode;
                        _currentHoldingTime = REPEAT_RATE;
                        _startHoldingTime = Time.GameTime;
                        break;

                    case KeyCode.A:
                        if (_isControlPressed)
                        {
                            // Select all
                        }
                        break;

                    case KeyCode.V:
                        if (_isControlPressed)
                        {
                            string clipboardString = Glfw.GetClipboardString(Display.Window);
                            if (MaxCharacterCount > -1 && Text.Length + clipboardString.Length > MaxCharacterCount)
                            {
                                int difference = MaxCharacterCount - Text.Length;
                                clipboardString = clipboardString.Substring(0, difference);
                                OnTextUpdateFailed?.Invoke();
                            }
                            _builder.Insert(_caretIndex, clipboardString);
                            _caretIndex = _caretIndex + clipboardString.Length;
                            UpdateText(_builder.ToString());
                        }
                        break;

                    case KeyCode.Home:
                        _caretIndex = 0;
                        UpdateCaret();
                        break;

                    case KeyCode.End:
                        _caretIndex = Text.Length;
                        UpdateCaret();
                        break;

                    case KeyCode.LeftControl:
                        _isLeftControlPressed = true;
                        break;

                    case KeyCode.RightControl:
                        _isRightControlPressed = true;
                        break;
                }
            }
            else if (!keyEvent.IsPressed)
            {
                switch(keyEvent.KeyCode)
                {
                    case KeyCode.Left:
                    case KeyCode.Right:
                    case KeyCode.Backspace:
                    case KeyCode.Delete:
                        if(_holdingKey == keyEvent.KeyCode)
                        {
                            _holdingKey = KeyCode.Unknown;
                        }
                        break;

                    case KeyCode.LeftControl:
                        _isLeftControlPressed = false;
                        break;

                    case KeyCode.RightControl:
                        _isRightControlPressed = false;
                        break;
                }
            }
        }

        private int GetPreviousWordBoundary(int fromIndex)
        {
            if (fromIndex <= 0 || _words.Count == 0) return 0;

            for (int i = _words.Count - 1; i >= 0; i--)
            {
                if (_words[i].Item2 < fromIndex)
                {
                    return _words[i].Item2;
                }
            }

            return 0;
        }

        private int GetNextWordBoundary(int fromIndex)
        {
            if (fromIndex >= Text.Length || _words.Count == 0) return Text.Length;

            for (int i = 0; i < _words.Count; i++)
            {
                if (_words[i].Item2 > fromIndex)
                {
                    return _words[i].Item2;
                }
            }

            return Text.Length;
        }

        private void UpdateText(string text)
        {
            _words.Clear();
            Text = text;
            OnTextUpdate?.Invoke(text);
            int start = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n' || text[i] == ' ')
                {
                    if (i > start)
                    {
                        _words.Add((text.Substring(start, i - start), i));
                    }
                    start = i + 1;
                }
            }

            if (start < text.Length)
            {
                _words.Add((text.Substring(start, text.Length - start), text.Length));
            }
        }

        public override void UpdateMesh() { }
    }
}
