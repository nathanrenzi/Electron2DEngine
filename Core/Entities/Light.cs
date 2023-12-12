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
        public LightType Type;
        public Color Color;
        public float Radius;
        public float Height;
        public float Intensity;
        public Transform Transform;

        public Light(Color _color, float _radius, float _height, LightType _type = LightType.Point, float _intensity = 1)
        {
            Type = _type;
            Color = _color;
            Radius = _radius;
            Height = _height;
            Intensity = _intensity;
            Transform = new Transform();
            AddComponent(Transform);

            switch (Type)
            {
                case LightType.Point:
                    LightManager.Instance.PointLightsInScene.Add(this);
                    break;
                case LightType.Spot:
                    LightManager.Instance.SpotLightsInScene.Add(this);
                    break;
                case LightType.Directional:
                    LightManager.Instance.DirectionalLightsInScene.Add(this);
                    break;
            }

            LightManager.Instance.OnLightsUpdated?.Invoke(Type);
        }

        protected override void OnDispose()
        {
            switch (Type)
            {
                case LightType.Point:
                    LightManager.Instance.PointLightsInScene.Remove(this);
                    break;
                case LightType.Spot:
                    LightManager.Instance.SpotLightsInScene.Remove(this);
                    break;
                case LightType.Directional:
                    LightManager.Instance.DirectionalLightsInScene.Remove(this);
                    break;
            }

            LightManager.Instance.OnLightsUpdated?.Invoke(Type);
        }
    }
}
