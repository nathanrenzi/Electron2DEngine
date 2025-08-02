using static Electron2D.OpenGL.GL;
using System.Numerics;
using System.Drawing;

namespace Electron2D.Rendering.Shaders
{
    public class Shader
    {
        public uint ProgramID { get; private set; }
        public bool Compiled { get; private set; }
        public string[] GlobalUniformTags { get; private set; }
        public string FilePath
        {
            get
            {
                return shaderProgramSource.FilePath;
            }
        }

        private readonly IDictionary<string, int> localUniforms = new Dictionary<string, int>();

        private ShaderProgramSource shaderProgramSource { get; }

        public Shader(ShaderProgramSource _shaderProgramSource, bool _compile = false, string[] _globalUniformTags = null)
        {
            shaderProgramSource = _shaderProgramSource;

            if(_globalUniformTags != null)
            {
                GlobalUniformTags = _globalUniformTags;
            }

            if(_compile)
            {
                Compile();
            }
        }

        ~Shader()
        {
            ShaderGlobalUniforms.UnregisterShader(this);
        }

        public static ShaderProgramSource ParseShader(string _filePath)
        {
            string[] shaderSource = new string[2];
            ShaderType shaderType = ShaderType.NONE;
            if(!File.Exists(_filePath))
            {
                Debug.LogError("Shader does not exist: " + _filePath);
                return null;
            }
            var allLines = File.ReadAllLines(_filePath);
            for (int i = 0; i < allLines.Length; i++)
            {
                string current = allLines[i];
                if(current.ToLower().Contains("#shader"))
                {
                    if(current.ToLower().Contains("vertex"))
                    {
                        shaderType = ShaderType.VERTEX;
                    }
                    else if(current.ToLower().Contains("fragment"))
                    {
                        shaderType = ShaderType.FRAGMENT;
                    }
                    else
                    {
                        Debug.LogError("No shader type has been supplied for: " + _filePath);
                    }
                }
                else
                {
                    shaderSource[(int)shaderType] += current + Environment.NewLine;
                }
            }
            return new ShaderProgramSource(_filePath, shaderSource[(int)ShaderType.VERTEX], shaderSource[(int)ShaderType.FRAGMENT]);
        }

        public unsafe bool Compile()
        {
            if(Compiled)
            {
                Debug.LogError("Trying to compile shader when it has already been compiled.");
                return false;
            }

            if (shaderProgramSource == null)
            {
                Debug.LogError("Could not load shader. Source is null.");
                return false;
            }

            uint vs, fs;

            vs = glCreateShader(GL_VERTEX_SHADER);
            glShaderSource(vs, shaderProgramSource.VertexShaderSource);
            glCompileShader(vs);

            // Writing any errors to the console
            int[] status = glGetShaderiv(vs, GL_COMPILE_STATUS, 1);
            if (status[0] == 0)
            {
                // Failed to compile
                string error = glGetShaderInfoLog(vs);
                Debug.LogError("Vertex Shader: " + error);
                return false;
            }
            // ------------------

            fs = glCreateShader(GL_FRAGMENT_SHADER);
            glShaderSource(fs, shaderProgramSource.FragmentShaderSource);
            glCompileShader(fs);

            // Writing any errors to the console
            status = glGetShaderiv(fs, GL_COMPILE_STATUS, 1);
            if (status[0] == 0)
            {
                // Failed to compile
                string error = glGetShaderInfoLog(fs);
                Debug.LogError("Fragment Shader: " + error);
                return false;
            }
            // ------------------

            ProgramID = glCreateProgram();
            glAttachShader(ProgramID, vs);
            glAttachShader(ProgramID, fs);
            glLinkProgram(ProgramID);

            // Delete Shaders
            glDetachShader(ProgramID, vs);
            glDetachShader(ProgramID, fs);
            glDeleteShader(vs);
            glDeleteShader(fs);

            // Pre-grabbing uniforms - Keep this here in case it is worth it for performance
            // Up the buffer size (its one of the GL parameters) if used because uniforms are being cut short

            //int[] count = glGetProgramiv(ProgramID, GL_ACTIVE_UNIFORMS, 1);
            //for (int i = 0; i < count[0]; i++)
            //{
            //    string key;
            //    glGetActiveUniform(ProgramID, (uint)i, 20, out _, out _, out _, out key);
            //    int location = glGetUniformLocation(ProgramID, key);
            //    uniforms.Add(key, location);
            //}

            Compiled = true;

            // Registering the shader to receive uniform data now that it is compiled
            ShaderGlobalUniforms.RegisterShader(this);

            return true;
        }

        public int GetUniformLocation(string _uniformName) => localUniforms[_uniformName];

        public void Use()
        {
            if(Compiled)
            {
                glUseProgram(ProgramID);
            }
            else
            {
                Debug.LogError("Shader has not been compiled yet, cannot use.");
            }
        }

        public void SetMatrix4x4(string _uniformName, Matrix4x4 _mat)
        {
            // Finding where location is in memory
            int location;
            if (!localUniforms.TryGetValue(_uniformName, out location))
            {
                // If the uniform isnt saved to memory, find and save it
                location = glGetUniformLocation(ProgramID, _uniformName);
                localUniforms.Add(_uniformName, location);
            }
            glUniformMatrix4fv(location, 1, false, GetMatrix4x4Values(_mat));
        }

        public void SetInt(string _uniformName, int _value)
        {
            int location;
            if (!localUniforms.TryGetValue(_uniformName, out location))
            {
                // If the uniform isnt saved to memory, find and save it
                location = glGetUniformLocation(ProgramID, _uniformName);
                localUniforms.Add(_uniformName, location);
            }
            glUniform1i(location, _value);
        }

        public void SetFloat(string _uniformName, float _value)
        {
            int location;
            if (!localUniforms.TryGetValue(_uniformName, out location))
            {
                // If the uniform isnt saved to memory, find and save it
                location = glGetUniformLocation(ProgramID, _uniformName);
                localUniforms.Add(_uniformName, location);
            }
            glUniform1f(location, _value);
        }

        public void SetVector2(string _uniformName, Vector2 _value)
        {
            int location;
            if (!localUniforms.TryGetValue(_uniformName, out location))
            {
                // If the uniform isnt saved to memory, find and save it
                location = glGetUniformLocation(ProgramID, _uniformName);
                localUniforms.Add(_uniformName, location);
            }
            glUniform2f(location, _value.X, _value.Y);
        }

        public void SetVector3(string _uniformName, Vector3 _value)
        {
            int location;
            if (!localUniforms.TryGetValue(_uniformName, out location))
            {
                // If the uniform isnt saved to memory, find and save it
                location = glGetUniformLocation(ProgramID, _uniformName);
                localUniforms.Add(_uniformName, location);
            }
            glUniform3f(location, _value.X, _value.Y, _value.Z);
        }

        public void SetVector4(string _uniformName, Vector4 _value)
        {
            int location;
            if (!localUniforms.TryGetValue(_uniformName, out location))
            {
                // If the uniform isnt saved to memory, find and save it
                location = glGetUniformLocation(ProgramID, _uniformName);
                localUniforms.Add(_uniformName, location);
            }
            glUniform4f(location, _value.X, _value.Y, _value.Z, _value.W);
        }

        /// <summary>
        /// Sets a color uniform in the shader.
        /// </summary>
        /// <param name="_uniformName"></param>
        /// <param name="_value"></param>
        public void SetColor(string _uniformName, Color _value)
        {
            int location;
            if (!localUniforms.TryGetValue(_uniformName, out location))
            {
                // If the uniform isnt saved to memory, find and save it
                location = glGetUniformLocation(ProgramID, _uniformName);
                localUniforms.Add(_uniformName, location);
            }
            float[] colArray = { MathF.Pow(_value.R / 255f, 2.2f), MathF.Pow(_value.G / 255f, 2.2f), MathF.Pow(_value.B / 255f, 2.2f), _value.A / 255f };
            glUniform4fv(location, 1, colArray);
        }

        private float[] GetMatrix4x4Values(Matrix4x4 m)
        {
            return new float[]
            {
                m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44
            };
        }
    }
}
