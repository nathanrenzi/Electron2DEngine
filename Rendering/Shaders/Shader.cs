using static Electron2D.OpenGL.GL;
using GLFW;
using System.Diagnostics;
using System.Numerics;

namespace Electron2D.Rendering.Shaders
{
    public class Shader
    {
        private string vertexCode;
        private string fragmentCode;
        private bool loaded = false;

        public uint programID;

        public Shader(string _vertexCode, string _fragmentCode)
        {
            vertexCode = _vertexCode;
            fragmentCode = _fragmentCode;
        }

        public void Load()
        {
            if (loaded) return;

            uint vs, fs;

            vs = glCreateShader(GL_VERTEX_SHADER);
            glShaderSource(vs, vertexCode);
            glCompileShader(vs);

            // Writing any errors to the console
            int[] status = glGetShaderiv(vs, GL_COMPILE_STATUS, 1);
            if (status[0] == 0)
            {
                // Failed to compile
                string error = glGetShaderInfoLog(vs);
                Debug.WriteLine("ERROR COMPILING VERTEX SHADER: " + error);
            }
            // ------------------

            fs = glCreateShader(GL_FRAGMENT_SHADER);
            glShaderSource(fs, fragmentCode);
            glCompileShader(fs);

            // Writing any errors to the console
            status = glGetShaderiv(fs, GL_COMPILE_STATUS, 1);
            if (status[0] == 0)
            {
                // Failed to compile
                string error = glGetShaderInfoLog(fs);
                Debug.WriteLine("ERROR COMPILING VERTEX SHADER: " + error);
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

            loaded = true;
        }

        public void Use()
        {
            glUseProgram(programID);
        }

        public void SetMatrix4x4(string _uniformName, Matrix4x4 mat)
        {
            // Finding where location is in memory
            int location = glGetUniformLocation(programID, _uniformName);
            glUniformMatrix4fv(location, 1, false, GetMatrix4x4Values(mat));
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
