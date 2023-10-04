using Electron2D.Core.ECS;
using Electron2D.Core.Rendering.Shaders;
using System.Drawing;
using System.Numerics;

namespace Electron2D.Core
{
    public class LightSystem : BaseSystem<Light> { }
    public class Light : Component
    {
        public const int MAX_POINT_LIGHTS = 256;
        public static List<Light> pointLightsInScene = new List<Light>();

        public const int MAX_SPOTLIGHTS = 256;
        public static List<Light> spotlightsInScene = new List<Light>();

        public const int MAX_DIRECTIONAL_LIGHTS = 1;
        public static List<Light> directionalLightsInScene = new List<Light>();

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
                    pointLightsInScene.Add(this);
                    break;
                case LightType.Spot:
                    spotlightsInScene.Add(this);
                    break;
                case LightType.Directional:
                    directionalLightsInScene.Add(this);
                    break;
            }
            OnLightsUpdated?.Invoke(Type);

            LightSystem.Register(this);
        }

        protected override void OnDispose()
        {
            switch (Type)
            {
                case LightType.Point:
                    pointLightsInScene.Remove(this);
                    break;
                case LightType.Spot:
                    spotlightsInScene.Remove(this);
                    break;
                case LightType.Directional:
                    directionalLightsInScene.Remove(this);
                    break;
            }
            OnLightsUpdated?.Invoke(Type);

            LightSystem.Unregister(this);
        }

        public void ApplyValues(Shader _shader, int _index)
        {
            int max = Type == LightType.Point ? MAX_POINT_LIGHTS : Type == LightType.Spot ? MAX_SPOTLIGHTS : MAX_DIRECTIONAL_LIGHTS;
            if(_index < max)
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
        }
    }
}
