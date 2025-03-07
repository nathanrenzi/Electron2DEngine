using System.Drawing;
using System.Numerics;

namespace Electron2D
{
    public class Particle
    {
        public Vector2 Origin { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Rotation { get; set; }
        public float AngularVelocity { get; set; }
        public Color Color { get; set; }
        public float Size { get; set; }
        public float Lifetime { get; set; }
        public float InitialLifetime { get; set; }
        public bool IsDead { get; set; }

        public void Initialize(Vector2 _origin, Vector2 _position, Vector2 _velocity, float _rotation, float _angularVelocity, Color _color, float _size, float _lifetime)
        {
            Origin = _origin;
            Position = _position;
            Velocity = _velocity;
            Rotation = _rotation;
            AngularVelocity = _angularVelocity;
            Color = _color;
            Size = _size;
            InitialLifetime = _lifetime;
            Lifetime = _lifetime;
            IsDead = false;
        }
    }
}