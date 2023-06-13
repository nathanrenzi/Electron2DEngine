using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenGLTest.GameObjects
{
    public class Transform
    {
        public Vector2 position;
        public Vector2 scale = Vector2.One;
        public double rotation;

        public Matrix4x4 GetPositionMatrix()
        {
            return Matrix4x4.CreateTranslation(position.X, position.Y, 0);
        }

        public Matrix4x4 GetScaleMatrix()
        {
            return Matrix4x4.CreateScale(scale.X, scale.Y, 1);
        }

        public Matrix4x4 GetRotationMatrix()
        {
            return Matrix4x4.CreateRotationZ((float)rotation);
        }
    }
}
