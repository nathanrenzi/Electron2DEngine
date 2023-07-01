﻿using Electron2D.Core.GameObjects;
using Electron2D.Core;
using Electron2D.Core.Rendering;
using System.Numerics;

namespace Electron2D.Build.Resources.Objects
{
    public class Boid : GameObject
    {
        public Vector2 velocity;
        public bool isPredator;

        public Boid(float _x, float _y, Vector2 _velocity)
        {
            transform.position.X = _x;
            transform.position.Y = _y;
            velocity = _velocity;
        }

        public override void Start()
        {
            renderer.SetSprite(0, 0, 0);
        }

        public void SetPredator()
        {
            if (isPredator) return;
            isPredator = true;

            transform.scale = Vector2.One * 0.8f;
            renderer.SetSprite(0, 1, 0);
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
                velocity.X = velocity.X / speed * maxSpeed;
                velocity.Y = velocity.Y / speed * maxSpeed;
            }
            else if (speed < minSpeed)
            {
                velocity.X = velocity.X / speed * minSpeed;
                velocity.Y = velocity.Y / speed * minSpeed;
            }

            if (double.IsNaN(velocity.X))
                velocity.X = 0;
            if (double.IsNaN(velocity.Y))
                velocity.Y = 0;

            Vector2 dir = transform.position - (transform.position + velocity);
            dir = NormalizeVector2(dir);
            transform.rotation = (float)Math.Atan2(dir.X, -dir.Y) * (180 / (float)Math.PI); // Converting to degrees
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