using Electron2D.Framework;
using Electron2D.GameObjects;
using Electron2D.Rendering.Images;
using System.Numerics;

namespace OpenGLTest.Game.Objects.Boids
{
    public class Boid : GameObject
    {
        public Vector2 velocity;
        public bool isPredator;

        public Boid(bool _isPredator, float _x, float _y, Vector2 _velocity)
        {
            isPredator = _isPredator;
            transform.position.X = _x;
            transform.position.Y = _y;
            velocity = _velocity;
        }

        public override void Start()
        {
            string image = isPredator ? "boid2.png" : "boid1.png";
            renderer.SetImage(new ImageTexture(image));
        }

        public float GetDistance(Boid _boid)
        {
            return Vector2.Distance(transform.position, _boid.transform.position);
        }

        public void MoveForward(float minSpeed = 1, float maxSpeed = 5)
        {
            transform.position.X += velocity.X * Time.deltaTime * 100;
            transform.position.Y += velocity.Y * Time.deltaTime * 100;

            // The magnitude of the velocity vector
            float speed = (float)Math.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y);
            if (speed > maxSpeed)
            {
                velocity.X = (velocity.X / speed) * maxSpeed;
                velocity.Y = (velocity.Y / speed) * maxSpeed;
            }
            else if (speed < minSpeed)
            {
                velocity.X = (velocity.X / speed) * minSpeed;
                velocity.Y = (velocity.Y / speed) * minSpeed;
            }

            if (double.IsNaN(velocity.X))
                velocity.X = 0;
            if (double.IsNaN(velocity.Y))
                velocity.Y = 0;

            Vector2 dir = (transform.position + velocity) - transform.position;
            dir = NormalizeVector2(dir);
            transform.rotation = (float)Math.Atan2(dir.X, -dir.Y);
        }

        private Vector2 NormalizeVector2(Vector2 _vector)
        {
            float max = 0;
            if (Math.Abs(_vector.X) > max)
            {
                max = Math.Abs(_vector.X);
            }
            else if (Math.Abs(_vector.Y) > max)
            {
                max = Math.Abs(_vector.Y);
            }
            else
            {
                return _vector;
            }

            return _vector / max;
        }
    }
}
