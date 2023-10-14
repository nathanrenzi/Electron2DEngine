using Electron2D.Core.ECS;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;
using System.Drawing;
using System.Numerics;
using static Electron2D.Core.Light;

namespace Electron2D.Core
{
    /// <summary>
    /// A backend component system for <see cref="Light"/> to improve performance. Ignore this.
    /// </summary>
    public class LightSystem : BaseSystem<Light> { }

    /// <summary>
    /// A global manager for lights.
    /// </summary>
    public class LightManager : IGlobalUniform
    {
        private static LightManager instance = null;
        private static readonly object loc = new();
        public static LightManager Instance
        {
            get
            {
                lock (loc)
                {
                    if (instance is null)
                    {
                        instance = new LightManager();
                    }
                    return instance;
                }
            }
        }

        public const int MAX_POINT_LIGHTS = 64;
        public List<Light> PointLightsInScene = new List<Light>();

        public const int MAX_SPOTLIGHTS = 16;
        public List<Light> SpotLightsInScene = new List<Light>();

        public const int MAX_DIRECTIONAL_LIGHTS = 1;
        public List<Light> DirectionalLightsInScene = new List<Light>();

        public Action<LightType> OnLightsUpdated;

        public void ApplyUniform(Shader _shader)
        {
            for (int i = 0; i < PointLightsInScene.Count; i++)
            {
                Light l = PointLightsInScene[i];
                if (i < MAX_POINT_LIGHTS)
                {
                    _shader.SetVector2($"pointLights[{i}].position", l.GetComponent<Transform>().Position);
                    _shader.SetFloat($"pointLights[{i}].height", l.Height);
                    _shader.SetFloat($"pointLights[{i}].radius", l.Radius);
                    _shader.SetFloat($"pointLights[{i}].intensity", l.Intensity);
                    _shader.SetVector3($"pointLights[{i}].color",
                        new Vector3(l.Color.R / 255f, l.Color.G / 255f, l.Color.B / 255f));
                }
                else
                {
                    Console.WriteLine($"Hit the maximum {l.Type} light limit! Number of lights: {PointLightsInScene.Count}");
                }
            }

            // Add spotlights

            // Add directional lights (might remove directionals?)
        }
    }

    /// <summary>
    /// A light object that can light the scene.
    /// </summary>
    public class Light : Component
    {
        public enum LightType { Point, Spot, Directional }
        public LightType Type;
        public Color Color;
        public float Radius;
        public float Height;
        public float Intensity;

        public Light(Color _color, float _radius, float _height, LightType _type = LightType.Point, float _intensity = 1)
        {
            Type = _type;
            Color = _color;
            Radius = _radius;
            Height = _height;
            Intensity = _intensity;

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
            LightSystem.Register(this);

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
            LightSystem.Unregister(this);
        }
    }
}
