using Electron2D.Core.ECS;
using Electron2D.Core.Rendering.Shaders;
using System.Drawing;
using System.Numerics;

namespace Electron2D.Core
{
    public class LightSystem : BaseSystem<Light> { }
    public class Light : Component
    {
        public const int MAX_POINT_LIGHTS = 16;
        public static List<Light> PointLightsInScene = new List<Light>();

        public const int MAX_SPOTLIGHTS = 16;
        public static List<Light> SpotLightsInScene = new List<Light>();

        public const int MAX_DIRECTIONAL_LIGHTS = 1;
        public static List<Light> DirectionalLightsInScene = new List<Light>();

        public static Action<LightType> OnLightsUpdated;

        public enum LightType { Point, Spot, Directional }
        public LightType Type;
        public Color Color;
        public float Radius;
        public float Intensity;

        private Transform transform;

        public Light(Transform _transform, Color _color, float _radius, LightType _type = LightType.Point, float _intensity = 1)
        {
            Type = _type;
            Color = _color;
            Radius = _radius;
            Intensity = _intensity;
            transform = _transform;

            switch (Type)
            {
                case LightType.Point:
                    PointLightsInScene.Add(this);
                    break;
                case LightType.Spot:
                    SpotLightsInScene.Add(this);
                    break;
                case LightType.Directional:
                    DirectionalLightsInScene.Add(this);
                    break;
            }
            LightSystem.Register(this);

            OnLightsUpdated?.Invoke(Type);
        }

        protected override void OnDispose()
        {
            switch (Type)
            {
                case LightType.Point:
                    PointLightsInScene.Remove(this);
                    break;
                case LightType.Spot:
                    SpotLightsInScene.Remove(this);
                    break;
                case LightType.Directional:
                    DirectionalLightsInScene.Remove(this);
                    break;
            }
            OnLightsUpdated?.Invoke(Type);

            LightSystem.Unregister(this);
        }

        public void ApplyValues(Shader _shader, int _index)
        {
            switch(Type)
            {
                case LightType.Point:
                    if (_index < MAX_POINT_LIGHTS)
                    {
                        _shader.SetVector2($"pointLights[{_index}].position", transform.Position);
                        _shader.SetFloat($"pointLights[{_index}].radius", Radius);
                        _shader.SetFloat($"pointLights[{_index}].intensity", Intensity);
                        _shader.SetVector3($"pointLights[{_index}].color",
                            new Vector3(Color.R / 255f, Color.G / 255f, Color.B / 255f));
                    }
                    else
                    {
                        Console.WriteLine($"Hit the maximum {Type} light limit! Number of lights: {_index + 1}");
                    }
                    break;
            }
        }
    }
}
