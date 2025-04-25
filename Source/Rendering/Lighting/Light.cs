using System.Drawing;

namespace Electron2D
{
    /// <summary>
    /// A light object that can light the scene.
    /// </summary>
    public class Light
    {
        public enum LightType { Point, Spot, Directional }
        public LightType Type { get; private set; }

        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                IsDirty = true;
            }
        }
        private Color _color;

        public float Constant
        {
            get => _constant;
            set
            {
                _constant = value;
                IsDirty = true;
            }
        }
        private float _constant;

        public float QuadraticFalloff
        {
            get => _quadraticFalloff;
            set
            {
                _quadraticFalloff = value;
                IsDirty = true;
            }
        }
        private float _quadraticFalloff;

        public float Height
        {
            get => _height;
            set
            {
                _height = value;
                IsDirty = true;
            }
        }
        private float _height;

        public float Intensity
        {
            get => _intensity;
            set
            {
                _intensity = value;
                IsDirty = true;
            }
        }
        private float _intensity;

        public Transform Transform { get; private set; }

        public bool IsDirty { get; set; }

        public Light(Color color, float height, float quadratic, float constant = 1, float intensity = 1, LightType type = LightType.Point)
        {
            Type = type;
            Color = color;
            QuadraticFalloff = quadratic;
            Constant = constant;
            Height = height;
            Intensity = intensity;

            Transform = new Transform();
            Transform.OnPositionChanged += () => IsDirty = true;

            LightManager.Instance.RegisterLight(this, type);
        }

        ~Light()
        {
            LightManager.Instance.UnregisterLight(this, Type);
        }
    }
}
