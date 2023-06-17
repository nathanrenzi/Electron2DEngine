using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Electron2D.Core.GameObjects
{
    public class Transform
    {
        private const float REFERENCE_WIDTH = 1920f;

        public Vector2 position;
        public Vector2 scale = Vector2.One;
        public double rotation;

        public Matrix4x4 GetPositionMatrix()
        {
            // Calculations below ensure that the position will scale with the screen width
            return Matrix4x4.CreateTranslation(position.X * (Program.game.currentWindowWidth / REFERENCE_WIDTH) + Program.game.currentWindowWidth / 2f,
                position.Y * (Program.game.currentWindowWidth / REFERENCE_WIDTH) + Program.game.currentWindowHeight / 2f, 0);
        }

        public Matrix4x4 GetScaleMatrix()
        {
            // Calculations below ensure that the scale will remain constant between screen sizes
            return Matrix4x4.CreateScale(Program.game.currentWindowWidth * (scale.X / 100f), Program.game.currentWindowWidth * (scale.Y / 100f), 1);
        }

        public Matrix4x4 GetRotationMatrix()
        {
            return Matrix4x4.CreateRotationZ((float)rotation);
        }
    }
}
