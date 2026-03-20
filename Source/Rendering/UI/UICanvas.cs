using Electron2D.Rendering;
using System.Numerics;

namespace Electron2D.UI
{
    public class UICanvas
    {
        public static UICanvas Instance { get; private set; }
        public event Action<float> OnUIScaleChanged;
        public Matrix4x4 UIModelMatrix { get; private set; }
        public Matrix4x4 UIModelMatrixInverse { get; private set; }
        public Vector2 VirtualResolution { get; private set; }
        public float Scale => UIModelMatrix.M11;

        private List<UIElement> _rootElements = new List<UIElement>();
        private List<UIElement> _allElements = new List<UIElement>();
        private UIElement _focusedElement = null;
        private UIElement _hoveredElement = null;
        private UIElement _draggedElement = null;

        private UIScalingMode _scalingMode;
        private bool _maintainAspect;

        private Vector2 _lastMousePosition;

        public UICanvas()
        {
            if (Instance != null)
            {
                Debug.LogError("Only one UICanvas can exist at a time.");
                return;
            }
            Instance = this;

            _scalingMode = ProjectSettings.UISettings.ScalingMode;
            VirtualResolution = ProjectSettings.UISettings.VirtualResolution;
            _maintainAspect = ProjectSettings.UISettings.MaintainAspect;

            UpdateScaling();

            Engine.Game.LateUpdateEvent += Update;
            Display.OnWindowResize += OnWindowResized;
        }

        public void RegisterUIElement(UIElement element)
        {
            if (!_allElements.Contains(element))
            {
                _allElements.Add(element);

                if (element.Parent == null)
                    _rootElements.Add(element);
            }
        }

        public void UnregisterUIElement(UIElement element)
        {
            _allElements.Remove(element);
            _rootElements.Remove(element);
        }

        public void OnElementParented(UIElement element)
        {
            _rootElements.Remove(element);
        }

        public void OnElementUnparented(UIElement element)
        {
            if (!_rootElements.Contains(element))
                _rootElements.Add(element);
        }

        private void OnWindowResized()
        {
            UpdateScaling();

            foreach (var root in _rootElements)
            {
                root.InvalidateMeasure();
            }
        }

        private void UpdateScaling()
        {
            if (_scalingMode == UIScalingMode.RealResolution)
            {
                UIModelMatrix = Matrix4x4.Identity;
                UIModelMatrixInverse = Matrix4x4.Identity;
            }
            else
            {
                VirtualResolution = ProjectSettings.UISettings.VirtualResolution;
                float scaleX = Display.WindowSize.X / VirtualResolution.X;
                float scaleY = Display.WindowSize.Y / VirtualResolution.Y;

                if (_maintainAspect)
                {
                    float uniformScale = MathF.Min(scaleX, scaleY);
                    scaleX = scaleY = uniformScale;
                }

                float offsetX = (Display.WindowSize.X - VirtualResolution.X * scaleX) / 2f;
                float offsetY = (Display.WindowSize.Y - VirtualResolution.Y * scaleY) / 2f;

                UIModelMatrix = Matrix4x4.CreateScale(scaleX, scaleY, 1f) *
                    Matrix4x4.CreateTranslation(offsetX, offsetY, 0f);

                Matrix4x4.Invert(UIModelMatrix, out var inverse);
                UIModelMatrixInverse = inverse;
            }

            OnUIScaleChanged?.Invoke(Scale);
        }

        public Vector2 VirtualToScreen(Vector2 position)
        {
            Vector4 r = Vector4.Transform(new Vector4(position, 0, 1), UIModelMatrix);
            return new Vector2(r.X, r.Y);
        }

        public Vector2 ScreenToVirtual(Vector2 position)
        {
            Vector4 r = Vector4.Transform(new Vector4(position, 0, 1), UIModelMatrixInverse);
            return new Vector2(r.X, r.Y);
        }

        public Vector2 ScreenToWorld(Vector2 position)
        {
            Vector2 centered = new Vector2(
                position.X - Display.WindowSize.X * 0.5f,
                (Display.WindowSize.Y * 0.5f) - position.Y
            );
            centered /= Camera2D.Main.Zoom;

            return centered + Camera2D.Main.Transform.Position;
        }

        public void UpdateLayout()
        {
            Vector2 canvasSize = ScreenToVirtual(Display.WindowSize);
            foreach (var root in _rootElements)
            {
                if (!root.IsMeasureValid)
                {
                    root.Measure(canvasSize);
                }

                if (!root.IsArrangeValid)
                {
                    root.Arrange(new Rect(0, 0, root.DesiredSize.X, root.DesiredSize.Y));
                }
            }
        }

        private void Update()
        {
            UpdateLayout();
            ProcessInput();
        }

        private void ProcessInput()
        {
            Vector2 mousePos = Input.GetMouseScreenPosition();
            Vector2 mousePosVirtual = ScreenToVirtual(mousePos);
            Vector2 mouseDelta = mousePosVirtual - _lastMousePosition;
            _lastMousePosition = mousePosVirtual;

            UIElement hitElement = HitTestTopMost(mousePosVirtual);

            if (hitElement != _hoveredElement)
            {
                if (_hoveredElement != null)
                {
                    var leaveEvt = new UIEvent
                    {
                        Type = UIEventType.MouseLeave,
                        Target = _hoveredElement,
                        Phase = EventPhase.Target,
                        MousePosition = mousePosVirtual
                    };
                    _hoveredElement.RaiseEvent(leaveEvt);
                }

                if (hitElement != null)
                {
                    var enterEvt = new UIEvent
                    {
                        Type = UIEventType.MouseEnter,
                        Target = hitElement,
                        Phase = EventPhase.Target,
                        MousePosition = mousePosVirtual
                    };
                    hitElement.RaiseEvent(enterEvt);
                }

                _hoveredElement = hitElement;
            }

            if (_draggedElement != null)
            {
                var dragEvt = new UIEvent
                {
                    Type = UIEventType.Drag,
                    Target = _draggedElement,
                    Phase = EventPhase.Target,
                    MousePosition = mousePosVirtual,
                    MouseDelta = mouseDelta
                };
                _draggedElement.RaiseEvent(dragEvt);

                if (Input.GetMouseButtonUp(MouseButton.Left))
                {
                    var upEvt = new UIEvent
                    {
                        Type = UIEventType.MouseUp,
                        Target = _draggedElement,
                        Phase = EventPhase.Bubble,
                        MousePosition = mousePosVirtual,
                        MouseButton = MouseButton.Left
                    };
                    _draggedElement.RaiseEvent(upEvt);

                    var dragEndEvt = new UIEvent
                    {
                        Type = UIEventType.DragEnd,
                        Target = _draggedElement,
                        Phase = EventPhase.Target,
                        MousePosition = mousePosVirtual
                    };
                    _draggedElement.RaiseEvent(dragEndEvt);

                    if (hitElement == _draggedElement)
                    {
                        var clickEvt = new UIEvent
                        {
                            Type = UIEventType.Click,
                            Target = _draggedElement,
                            Phase = EventPhase.Bubble,
                            MousePosition = mousePosVirtual,
                            MouseButton = MouseButton.Left
                        };
                        _draggedElement.RaiseEvent(clickEvt);
                    }

                    _draggedElement = null;
                }
            }
            else
            {
                if (hitElement != null)
                {
                    if (Input.GetMouseButtonDown(MouseButton.Left))
                    {
                        var downEvt = new UIEvent
                        {
                            Type = UIEventType.MouseDown,
                            Target = hitElement,
                            Phase = EventPhase.Bubble,
                            MousePosition = mousePosVirtual,
                            MouseButton = MouseButton.Left
                        };
                        hitElement.RaiseEvent(downEvt);

                        var dragStartEvt = new UIEvent
                        {
                            Type = UIEventType.DragStart,
                            Target = hitElement,
                            Phase = EventPhase.Target,
                            MousePosition = mousePosVirtual
                        };
                        hitElement.RaiseEvent(dragStartEvt);

                        _draggedElement = hitElement;

                        if (_focusedElement != hitElement)
                        {
                            _focusedElement?.RaiseEvent(new UIEvent
                            {
                                Type = UIEventType.LoseFocus,
                                Target = _focusedElement,
                                Phase = EventPhase.Target
                            });
                            _focusedElement = hitElement;
                            hitElement.RaiseEvent(new UIEvent
                            {
                                Type = UIEventType.GainFocus,
                                Target = hitElement,
                                Phase = EventPhase.Target
                            });
                        }
                    }

                    if (Input.GetMouseButtonDown(MouseButton.Right))
                    {
                        var downEvt = new UIEvent
                        {
                            Type = UIEventType.MouseDown,
                            Target = hitElement,
                            Phase = EventPhase.Bubble,
                            MousePosition = mousePosVirtual,
                            MouseButton = MouseButton.Right
                        };
                        hitElement.RaiseEvent(downEvt);
                    }

                    if (Input.GetMouseButtonUp(MouseButton.Right))
                    {
                        var upEvt = new UIEvent
                        {
                            Type = UIEventType.MouseUp,
                            Target = hitElement,
                            Phase = EventPhase.Bubble,
                            MousePosition = mousePosVirtual,
                            MouseButton = MouseButton.Right
                        };
                        hitElement.RaiseEvent(upEvt);

                        var clickEvt = new UIEvent
                        {
                            Type = UIEventType.Click,
                            Target = hitElement,
                            Phase = EventPhase.Bubble,
                            MousePosition = mousePosVirtual,
                            MouseButton = MouseButton.Right
                        };
                        hitElement.RaiseEvent(clickEvt);
                    }

                    if (Input.GetMouseButtonDown(MouseButton.Middle))
                    {
                        var downEvt = new UIEvent
                        {
                            Type = UIEventType.MouseDown,
                            Target = hitElement,
                            Phase = EventPhase.Bubble,
                            MousePosition = mousePosVirtual,
                            MouseButton = MouseButton.Middle
                        };
                        hitElement.RaiseEvent(downEvt);
                    }

                    if (Input.GetMouseButtonUp(MouseButton.Middle))
                    {
                        var upEvt = new UIEvent
                        {
                            Type = UIEventType.MouseUp,
                            Target = hitElement,
                            Phase = EventPhase.Bubble,
                            MousePosition = mousePosVirtual,
                            MouseButton = MouseButton.Middle
                        };
                        hitElement.RaiseEvent(upEvt);

                        var clickEvt = new UIEvent
                        {
                            Type = UIEventType.Click,
                            Target = hitElement,
                            Phase = EventPhase.Bubble,
                            MousePosition = mousePosVirtual,
                            MouseButton = MouseButton.Middle
                        };
                        hitElement.RaiseEvent(clickEvt);
                    }
                }
                else
                {
                    if (Input.GetMouseButtonDown(MouseButton.Left)
                        || Input.GetMouseButtonDown(MouseButton.Right)
                        || Input.GetMouseButtonDown(MouseButton.Middle))
                    {
                        if (_focusedElement != null)
                        {
                            _focusedElement.RaiseEvent(new UIEvent
                            {
                                Type = UIEventType.LoseFocus,
                                Target = _focusedElement,
                                Phase = EventPhase.Target
                            });
                            _focusedElement = null;
                        }
                    }
                }
            }
        }

        private UIElement? HitTestTopMost(Vector2 mousePos)
        {
            // Test all root elements and their children (in reverse order, last = top)
            for (int i = _rootElements.Count - 1; i >= 0; i--)
            {
                UIElement element = _rootElements[i];
                Vector2 pos;
                if(element.UseWorldPosition)
                {
                    pos = ScreenToWorld(VirtualToScreen(mousePos));
                    pos.Y = -pos.Y;
                }
                else
                {
                    pos = mousePos;
                }
                var hit = HitTestRecursive(element, pos);
                if (hit != null) return hit;
            }
            return null;
        }

        private UIElement? HitTestRecursive(UIElement element, Vector2 position)
        {
            if (!element.Visible || !element.Enabled) return null;

            bool hitSelf = element.HitTest(position);

            if (!element.Mask || hitSelf)
            {
                for (int i = element.Children.Count - 1; i >= 0; i--)
                {
                    var hit = HitTestRecursive(element.Children[i], position);
                    if (hit != null) return hit;
                }
            }

            if (element.Interactable && hitSelf)
                return element;

            return null;
        }

        public void Focus(UIElement element)
        {
            if (_focusedElement == element) return;

            _focusedElement?.RaiseEvent(new UIEvent
            {
                Type = UIEventType.LoseFocus,
                Target = _focusedElement,
                Phase = EventPhase.Target
            });

            _focusedElement = element;
            element.Focused = true;
        }

        public void Unfocus(UIElement element)
        {
            if (element != _focusedElement) return;

            element.Focused = false;
            _focusedElement = null;
        }
    }
}