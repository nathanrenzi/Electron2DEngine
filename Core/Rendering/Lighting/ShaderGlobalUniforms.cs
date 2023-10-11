﻿using Electron2D.Core.Rendering.Shaders;

namespace Electron2D.Core.Rendering
{
    /// <summary>
    /// An interface that allows classes to register into <see cref="ShaderGlobalUniforms"/> and pass data to all shaders.
    /// </summary>
    public interface IGlobalUniform
    {
        public void ApplyUniform(Shader _shader);
    }

    /// <summary>
    /// Global uniform manager that can apply data to all shaders if they have the appropriate tags.
    /// </summary>
    public static class ShaderGlobalUniforms
    {
        private static Dictionary<string, IGlobalUniform> globalUniforms = new Dictionary<string, IGlobalUniform>();
        private static List<Shader> shaders = new List<Shader>();

        public static void Initialize()
        {
            Game.OnUpdateEvent += UpdateShaders;
        }

        public static void RegisterShader(Shader _shader)
        {
            shaders.Add(_shader);

            _shader.Use();
        }

        public static void UnregisterShader(Shader _shader)
        {
            shaders.Remove(_shader);
        }

        public static void RegisterGlobalUniform(string _uniformKey, IGlobalUniform _uniformReference)
        {
            globalUniforms.Add(_uniformKey, _uniformReference);
        }

        public static void UnregisterGlobalUniform(string _uniformKey)
        {
            globalUniforms.Remove(_uniformKey);
        }

        /// <summary>
        /// Updates all shaders with global uniform data.
        /// </summary>
        public static void UpdateShaders()
        {
            for (int i = 0; i < shaders.Count; i++)
            {
                UpdateShader(shaders[i]);
            }
        }

        /// <summary>
        /// Updates an individual shader with global uniform data.
        /// </summary>
        public static void UpdateShader(Shader _shader)
        {
            _shader.Use();
            for (int x = 0; x < _shader.GlobalUniformTags.Length; x++)
            {
                SetUniform(_shader.GlobalUniformTags[x], _shader);
            }
        }

        /// <summary>
        /// Sets the uniform data from an IGlobalUniform to a shader.
        /// </summary>
        private static void SetUniform(string _uniformKey, Shader _shader)
        {
            if(globalUniforms.TryGetValue(_uniformKey, out IGlobalUniform uniform))
            {
                uniform.ApplyUniform(_shader);
            }
            else
            {
                Console.WriteLine($"Global uniform does not exist: {_uniformKey}");
            }
        }
    }
}
