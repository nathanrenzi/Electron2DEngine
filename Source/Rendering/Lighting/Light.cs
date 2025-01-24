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

        public float Radius
        {
            get => _radius;
            set
            {
                _radius = value;
                IsDirty = true;
            }
        }
        public float _radius;

        public float Height
        {
            get => _height;
            set
            {
                _height = value;
                IsDirty = true;
            }
        }
        public float _height;

        public float Intensity
        {
            get => _intensity;
            set
            {
                _intensity = value;
                IsDirty = true;
            }
        }
        public float _intensity;

        public Transform Transform { get; private set; }

        public bool IsDirty { get; set; }

        public Light(Color color, float radius, float height, LightType type = LightType.Point, float intensity = 1)
        {
            Type = type;
            Color = color;
            Radius = radius;
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
