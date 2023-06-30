using Electron2D.Core.GameObjects;
using System.Numerics;

namespace Electron2D.Core.Physics
{
    public class VerletBody
    {
        public Transform attachedTransform;
        public Vector2 velocity;

        public VerletBody(Transform _attachedTransform)
        {
            attachedTransform = _attachedTransform;
            VerletWorld.RegisterBody(this);
        }

        ~VerletBody()
        { 
            VerletWorld.UnregisterBody(this);
        }
    }
}
