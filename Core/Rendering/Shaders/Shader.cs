using static Electron2D.OpenGL.GL;
using GLFW;
using System.Diagnostics;
using System.Numerics;
using System.Drawing;

namespace Electron2D.Core.Rendering.Shaders
{
    public class Shader
    {
        public uint programID { get; private set; }
        public bool compiled { get; private set; }
        private readonly IDictionary<string, int> uniforms = new Dictionary<string, int>();

        private ShaderProgramSource shaderProgramSource { get; }

        public Shader(ShaderProgramSource _shaderProgramSource, bool _compile = false)
        {
            shaderProgramSource = _shaderProgramSource;
            if(_compile)
            {
                CompileShader();
            }
        }

        public static ShaderProgramSource ParseShader(string _filePath)
        {
            string[] shaderSource = new string[2];
            eShaderType shaderType = eShaderType.NONE;
            if(!File.Exists(_filePath))
            {
                Console.WriteLine("Shader does not exist: " + _filePath);
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
                        shaderType = eShaderType.VERTEX;
                    }
                    else if(current.ToLower().Contains("fragment"))
                    {
                        shaderType = eShaderType.FRAGMENT;
                    }
                    else
                    {
                        Console.WriteLine("Error. No shader type has been supplied for: " + _filePath);
                    }
                }
                else
                {
                    shaderSource[(int)shaderType] += current + Environment.NewLine;
                }
            }
            return new ShaderProgramSource(shaderSource[(int)eShaderType.VERTEX], shaderSource[(int)eShaderType.FRAGMENT]);
        }

        public unsafe bool CompileShader()
        {
            if(compiled)
            {
                Console.WriteLine("Trying to compile shader when it has already been compiled.");
                return false;
            }

            if (shaderProgramSource == null)
            {
                Console.WriteLine("Could not load shader. Source is null.");
                return false;
            }

            uint vs, fs;

            vs = glCreateShader(GL_VERTEX_SHADER);
            glShaderSource(vs, shaderProgramSource.vertexShaderSource);
            glCompileShader(vs);

            // Writing any errors to the console
            int[] status = glGetShaderiv(vs, GL_COMPILE_STATUS, 1);
            if (status[0] == 0)
            {
                // Failed to compile
                string error = glGetShaderInfoLog(vs);
                Debug.WriteLine("ERROR COMPILING VERTEX SHADER: " + error);
                return false;
            }
            // ------------------

            fs = glCreateShader(GL_FRAGMENT_SHADER);
            glShaderSource(fs, shaderProgramSource.fragmentShaderSource);
            glCompileShader(fs);

            // Writing any errors to the console
            status = glGetShaderiv(fs, GL_COMPILE_STATUS, 1);
            if (status[0] == 0)
            {
                // Failed to compile
                string error = glGetShaderInfoLog(fs);
                Debug.WriteLine("ERROR COMPILING VERTEX SHADER: " + error);
                return false;
            }
            // ------------------

            programID = glCreateProgram();
            glAttachShader(programID, vs);
            glAttachShader(programID, fs);
            glLinkProgram(programID);

            // Delete Shaders
            glDetachShader(programID, vs);
            glDetachShader(programID, fs);
            glDeleteShader(vs);
            glDeleteShader(fs);

            int[] count = glGetProgramiv(programID, GL_ACTIVE_UNIFORMS, 1);
            for (int i = 0; i < count[0]; i++)
            {
                string key;
                glGetActiveUniform(programID, (uint)i, 20, out _, out _, out _, out key);
                int location = glGetUniformLocation(programID, key);
                uniforms.Add(key, location);
            }

            compiled = true;
            return true;
        }

        public int GetUniformLocation(string _uniformName) => uniforms[_uniformName];

        public void Use()
        {
            if(compiled)
            {
                glUseProgram(programID);
            }
            else
            {
                Console.WriteLine("Shader has not been compiled yet, cannot use.");
            }
        }

        public void SetMatrix4x4(string _uniformName, Matrix4x4 _mat)
        {
            // Finding where location is in memory
            int location;
            if (!uniforms.TryGetValue(_uniformName, out location))
            {
                // If the uniform isnt saved to memory, find and save it
                location = glGetUniformLocation(programID, _uniformName);
                uniforms.Add(_uniformName, location);
            }
            glUniformMatrix4fv(location, 1, false, GetMatrix4x4Values(_mat));
        }

        public void SetFloat(string _uniformName, float _value)
        {
            int location;
            if (!uniforms.TryGetValue(_uniformName, out location))
            {
                // If the uniform isnt saved to memory, find and save it
                location = glGetUniformLocation(programID, _uniformName);
                uniforms.Add(_uniformName, location);
            }
            glUniform1f(location, _value);
        }

        /// <summary>
        /// Sets a color uniform in the shader.
        /// </summary>
        /// <param name="_uniformName"></param>
        /// <param name="_value"></param>
        public void SetColor(string _uniformName, Color _value)
        {
            int location;
            if (!uniforms.TryGetValue(_uniformName, out location))
            {
                // If the uniform isnt saved to memory, find and save it
                location = glGetUniformLocation(programID, _uniformName);
                uniforms.Add(_uniformName, location);
            }
            float[] colArray = { _value.R / 255f, _value.G / 255f, _value.B / 255f, _value.A / 255f };
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
