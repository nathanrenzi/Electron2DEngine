using static Electron2D.OpenGL.GL;

namespace Electron2D.Core
{
    public static class Utilities
    {
        public static int GetSizeOfVertexAttribPointerType(int _attribType)
        {
            switch(_attribType)
            {
                case GL_UNSIGNED_BYTE:
                    return 1;
                case GL_UNSIGNED_INT:
                    return 4;
                case GL_FLOAT:
                    return 4;
                default:
                    return 0;
            }
        }
    }
}
