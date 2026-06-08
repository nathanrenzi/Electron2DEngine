using Atlas2D.Rendering.Shaders;
using Atlas2D.Rendering;
using System.Numerics;
using static Atlas2D.LightNode;

namespace Atlas2D
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

        public bool IsDirty { get; set; }

        // MAKE SURE TO CHANGE THESE VALUES IN SHADERS TOO
        public const int MAX_POINT_LIGHTS = 16;
        public List<LightNode> PointLightsInScene = new List<LightNode>();

        // MAKE SURE TO CHANGE THESE VALUES IN SHADERS TOO
        public const int MAX_SPOTLIGHTS = 16;
        public List<LightNode> SpotLightsInScene = new List<LightNode>();

        // MAKE SURE TO CHANGE THESE VALUES IN SHADERS TOO
        public const int MAX_DIRECTIONAL_LIGHTS = 1;
        public List<LightNode> DirectionalLightsInScene = new List<LightNode>();

        public void RegisterLight(LightNode _light, LightType _type)
        {
            switch (_type)
            {
                case LightType.Point:
                    PointLightsInScene.Add(_light);
                    break;
                case LightType.Spot:
                    SpotLightsInScene.Add(_light);
                    break;
                case LightType.Directional:
                    DirectionalLightsInScene.Add(_light);
                    break;
            }
        }

        public void UnregisterLight(LightNode _light, LightType _type)
        {
            switch (_type)
            {
                case LightType.Point:
                    PointLightsInScene.Remove(_light);
                    break;
                case LightType.Spot:
                    SpotLightsInScene.Remove(_light);
                    break;
                case LightType.Directional:
                    DirectionalLightsInScene.Remove(_light);
                    break;
            }
        }

        public void CheckDirty()
        {
            for (int i = 0; i < PointLightsInScene.Count; i++)
            {
                if (PointLightsInScene[i].IsDirty)
                {
                    IsDirty = true;
                    PointLightsInScene[i].IsDirty = false;
                }
            }
        }

        public void ApplyUniform(Shader _shader)
        {
            _shader.SetColor("ambientColor", AmbientColor);
            for (int i = 0; i < PointLightsInScene.Count; i++)
            {
                LightNode l = PointLightsInScene[i];
                if (i < MAX_POINT_LIGHTS)
                {
                    _shader.SetFloat($"pointLights[{i}].initialized", 1);
                    _shader.SetVector2($"pointLights[{i}].position", l.WorldPosition);
                    _shader.SetFloat($"pointLights[{i}].height", l.Height);
                    _shader.SetFloat($"pointLights[{i}].quadratic", l.QuadraticFalloff);
                    _shader.SetFloat($"pointLights[{i}].constant", l.Constant);
                    _shader.SetFloat($"pointLights[{i}].intensity", l.Intensity);
                    _shader.SetVector3($"pointLights[{i}].color",
                        new Vector3(l.Color.R, l.Color.G, l.Color.B));
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
