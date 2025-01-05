using Electron2D.Core.ECS;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;
using System.Drawing;
using System.Numerics;
using static Electron2D.Core.Light;

namespace Electron2D.Core
{
    /// <summary>
    /// A light object that can light the scene.
    /// </summary>
    public class Light : Entity
    {
        public enum LightType { Point, Spot, Directional }
        public LightType Type { get; private set; }

        public Color Color
        {
            get => color;
            set
            {
                color = value;
                IsDirty = true;
            }
        }
        private Color color;

        public float Radius
        {
            get => radius;
            set
            {
                radius = value;
                IsDirty = true;
            }
        }
        public float radius;

        public float Height
        {
            get => height;
            set
            {
                height = value;
                IsDirty = true;
            }
        }
        public float height;

        public float Intensity
        {
            get => intensity;
            set
            {
                intensity = value;
                IsDirty = true;
            }
        }
        public float intensity;

        public Transform Transform;

        public bool IsDirty { get; set; }

        public Light(Color _color, float _radius, float _height, LightType _type = LightType.Point, float _intensity = 1)
        {
            Type = _type;
            Color = _color;
            Radius = _radius;
            Height = _height;
            Intensity = _intensity;

            Transform = new Transform();
            AddComponent(Transform);
            Transform.OnPositionChanged += () => IsDirty = true;

            LightManager.Instance.RegisterLight(this, _type);
        }

        protected override void OnDispose()
        {
            LightManager.Instance.UnregisterLight(this, Type);
        }
    }
}
