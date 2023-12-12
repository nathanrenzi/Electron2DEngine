using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.Rendering;
using System.Drawing;
using System.Numerics;

namespace Electron2D.Core
{
    /// <summary>
    /// A global manager for lights.
    /// </summary>
    public class LightManager : IGlobalUniform
    {
        /// <summary>
        /// The ambient color of the scene.
        /// </summary>
        public static Color AmbientColor;

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

        public Action<Light.LightType> OnLightsUpdated;

        public void ApplyUniform(Shader _shader)
        {
            _shader.SetColor("ambientColor", AmbientColor);
            for (int i = 0; i < PointLightsInScene.Count; i++)
            {
                Light l = PointLightsInScene[i];
                if (i < MAX_POINT_LIGHTS)
                {
                    _shader.SetVector2($"pointLights[{i}].position", l.Transform.Position);
                    _shader.SetFloat($"pointLights[{i}].height", l.Height);
                    _shader.SetFloat($"pointLights[{i}].radius", l.Radius);
                    _shader.SetFloat($"pointLights[{i}].intensity", l.Intensity);
                    _shader.SetVector3($"pointLights[{i}].color",
                        new Vector3(l.Color.R / 255f, l.Color.G / 255f, l.Color.B / 255f));
                }
                else
                {
                    Debug.LogError($"Hit the maximum {l.Type} light limit! Number of lights: {PointLightsInScene.Count}");
                }
            }

            // Add spotlights

            // Add directional lights (might remove directionals?)
        }
    }
}
