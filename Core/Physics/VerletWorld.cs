using Electron2D.Core.Misc;
using System.Numerics;

namespace Electron2D.Core.Physics
{
    public static class VerletWorld
    {
        private static List<VerletBody> verletBodies = new List<VerletBody>();

        private const float GRAVITY_FORCE = -9.81f;
        private const float PIXELS_PER_METER = 80f;

        public static void Step()
        {
            for (int i = 0; i < verletBodies.Count; i++)
            {
                StepBody(verletBodies[i]);
            }
        }

        private static void StepBody(VerletBody _body)
        {
            // Add dynamic acceleration - https://en.wikipedia.org/wiki/Verlet_integration
            Vector2 gravity = new Vector2(0, GRAVITY_FORCE * PIXELS_PER_METER);

            Vector2 newPosition = _body.attachedTransform.position + (_body.velocity*Time.deltaTime) + (gravity*(Time.deltaTime*Time.deltaTime*0.5f));
            Vector2 newVelocity = _body.velocity + (gravity + gravity) * (Time.deltaTime * 0.5f);

            _body.attachedTransform.position = newPosition;
            _body.velocity = newVelocity;
        }

        public static void RegisterBody(VerletBody _body)
        {
            if(!verletBodies.Contains(_body))
            {
                verletBodies.Add(_body);
            }

            Console.WriteLine($"Verlet Body Count: {verletBodies.Count} | Render Time: {PerformanceTimings.renderMilliseconds} ms | Physics Time: {PerformanceTimings.physicsMilliseconds} ms");
        }

        public static void UnregisterBody(VerletBody _body)
        {
            verletBodies.Remove(_body);
        }
    }
}
