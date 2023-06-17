using System.Numerics;
using Electron2D.Core.GameObjects;
using Electron2D.Core;

namespace Electron2D.Build.Resources.Objects
{
    public class BoidField : GameObject
    {
        public readonly List<Boid> boids = new List<Boid>();

        private BoidForcefieldDisplay forcefield;
        private readonly Random rand = new Random();
        private int predatorCount;

        public BoidField(int _predatorCount = 3, int _boidCount = 100, float _baseSpeed = 5) : base(false)
        {
            forcefield = new BoidForcefieldDisplay();
            predatorCount = _predatorCount;

            for (int i = 0; i < _boidCount; i++)
            {
                int xPos = rand.Next(-(Program.game.currentWindowWidth / 4), Program.game.currentWindowWidth / 4 + 1);
                int yPos = rand.Next(-(Program.game.currentWindowHeight / 4), Program.game.currentWindowHeight / 4 + 1);
                float xVel = rand.Next(-100, 101) / 100f;
                float yVel = rand.Next(-100, 101) / 100f;
                Vector2 velocity = NormalizeVector2(new Vector2(xVel, yVel)) * _baseSpeed;

                Boid newBoid = new Boid(xPos, yPos, velocity);
                newBoid.transform.scale = Vector2.One * 0.4f;
                boids.Add(newBoid);
            }
        }

        public override void Update()
        {
            Advance();
            forcefield.transform.position = Input.GetMouseWorldPosition();
        }

        public void Advance(bool bounceOffWalls = true, bool wrapAroundEdges = false)
        {
            // update void speed and direction (velocity) based on rules
            foreach (var boid in boids)
            {
                (double flockXvel, double flockYvel) = Flock(boid, 50, .0003);
                (double alignXvel, double alignYvel) = Align(boid, 50, .01);
                (double avoidXvel, double avoidYvel) = Avoid(boid, 20, .001);
                (double predXvel, double predYval) = Predator(boid, 150, .00005);
                boid.velocity.X += (float)(flockXvel + avoidXvel + alignXvel + predXvel);
                boid.velocity.Y += (float)(flockYvel + avoidYvel + alignYvel + predYval);
            }

            // move all boids forward in time
            foreach (var boid in boids)
            {
                boid.MoveForward();
                if (bounceOffWalls)
                    BounceOffWalls(boid);
                if (wrapAroundEdges)
                    WrapAround(boid);
            }
        }

        private (double xVel, double yVel) Flock(Boid boid, double distance, double power)
        {
            var neighbors = boids.Where(x => x.GetDistance(boid) < distance);
            double meanX = neighbors.Sum(x => x.transform.position.X) / neighbors.Count();
            double meanY = neighbors.Sum(x => x.transform.position.Y) / neighbors.Count();
            double deltaCenterX = meanX - boid.transform.position.X;
            double deltaCenterY = meanY - boid.transform.position.Y;
            return (deltaCenterX * power, deltaCenterY * power);
        }

        private (double xVel, double yVel) Align(Boid boid, double distance, double power)
        {
            var neighbors = boids.Where(x => x.GetDistance(boid) < distance);
            double meanXvel = neighbors.Sum(x => x.velocity.X) / neighbors.Count();
            double meanYvel = neighbors.Sum(x => x.velocity.Y) / neighbors.Count();
            double dXvel = meanXvel - boid.velocity.X;
            double dYvel = meanYvel - boid.velocity.Y;
            return (dXvel * power, dYvel * power);
        }

        private (double xVel, double yVel) Avoid(Boid boid, double distance, double power)
        {
            var neighbors = boids.Where(x => x.GetDistance(boid) < distance);
            (double sumClosenessX, double sumClosenessY) = (0, 0);
            foreach (var neighbor in neighbors)
            {
                double closeness = distance - boid.GetDistance(neighbor);
                sumClosenessX += (boid.transform.position.X - neighbor.transform.position.X) * closeness;
                sumClosenessY += (boid.transform.position.Y - neighbor.transform.position.Y) * closeness;
            }
            return (sumClosenessX * power, sumClosenessY * power);
        }

        private (double xVel, double yVel) Predator(Boid boid, double distance, double power)
        {
            (double sumClosenessX, double sumClosenessY) = (0, 0);
            for (int i = 0; i < predatorCount; i++)
            {
                Boid predator = boids[i];
                predator.SetPredator();
                double _distanceAway = boid.GetDistance(predator);
                if (_distanceAway < distance)
                {
                    double closeness = distance - _distanceAway;
                    sumClosenessX += (boid.transform.position.X - predator.transform.position.X) * closeness;
                    sumClosenessY += (boid.transform.position.Y - predator.transform.position.Y) * closeness;
                }
            }
            // Adding forcefield to predator list
            double distanceAway = Vector2.Distance(boid.transform.position, forcefield.transform.position);
            if (distanceAway < distance)
            {
                double closeness = distance - distanceAway;
                sumClosenessX += (boid.transform.position.X - forcefield.transform.position.X) * closeness;
                sumClosenessY += (boid.transform.position.Y - forcefield.transform.position.Y) * closeness;
            }

            return (sumClosenessX * power, sumClosenessY * power);
        }

        private void BounceOffWalls(Boid boid)
        {
            int width = Program.game.currentWindowWidth;
            int height = Program.game.currentWindowHeight;

            float pad = 50f;
            float turn = 0.35f;
            if (boid.transform.position.X + width / 2f < pad)
                boid.velocity.X += turn;

            if (boid.transform.position.X + width / 2f > width - pad)
                boid.velocity.X -= turn;

            if (boid.transform.position.Y + height / 2f < pad)
                boid.velocity.Y += turn;

            if (boid.transform.position.Y + height / 2f > height - pad)
                boid.velocity.Y -= turn;
        }

        private void WrapAround(Boid boid)
        {
            int width = Program.game.currentWindowWidth;
            int height = Program.game.currentWindowHeight;

            if (boid.transform.position.X < -(width / 2f))
                boid.transform.position.X += width;

            if (boid.transform.position.X > width / 2f)
                boid.transform.position.X -= width;

            if (boid.transform.position.Y < -(height / 2f))
                boid.transform.position.Y += height;

            if (boid.transform.position.Y > height / 2f)
                boid.transform.position.Y -= height;
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
