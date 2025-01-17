using Electron2D.ECS;
using System.Numerics;

namespace Electron2D.Audio
{
    public class AudioSpatialListener : Component
    {
        public static AudioSpatialListener Instance { get; private set; }

        private Transform transform;

        public AudioSpatialListener()
        {
            AudioSpatializerSystem.Register(this);
        }

        public Vector2 GetPosition()
        {
            return transform.Position;
        }

        public override void OnAdded()
        {
            transform = GetComponent<Transform>();
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("There can only be one AudioSpatialListener in the scene!");
            }
        }

        protected override void OnDispose()
        {
            AudioSpatializerSystem.Unregister(this);
        }
    }
}
