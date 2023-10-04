﻿using Electron2D.Core.Rendering.Shaders;

namespace Electron2D.Core.Rendering.Lighting
{
    // Replace this with a tagging system for shaders so multiple different data types can be applied at once
    public static class ShaderLightDataUpdater
    {
        private static List<Shader> litShaders = new List<Shader>();

        public static void Initialize()
        {
            Light.OnLightsUpdated += UpdateShaders;
        }

        public static void RegisterShader(Shader _shader)
        {
            litShaders.Add(_shader);
            UpdateShader(_shader, Light.LightType.Point);
            UpdateShader(_shader, Light.LightType.Spot);
            UpdateShader(_shader, Light.LightType.Directional);
        }

        public static void UnregisterShader(Shader _shader)
        {
            litShaders.Remove(_shader);
        }

        public static void UpdateShaders(Light.LightType _type)
        {
            foreach (Shader shader in litShaders)
            {
                UpdateShader(shader, _type);
            }
        }

        private static void UpdateShader(Shader _shader, Light.LightType _type)
        {
            for (int i = 0; i < Light.pointLightsInScene.Count; i++)
            {
                Light.pointLightsInScene[i].ApplyValues(_shader, i);
            }
        }
    }
}
