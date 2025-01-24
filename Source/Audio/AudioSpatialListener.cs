using System.Numerics;

namespace Electron2D.Audio
{
    public class AudioSpatialListener
    {
        public static AudioSpatialListener Instance { get; private set; }

        private Transform _transform;

        public Vector2 GetPosition()
        {
            return _transform.Position;
        }

        public AudioSpatialListener(Transform transform)
        {
            _transform = transform;
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("There can only be one AudioSpatialListener in the scene!");
            }
        }
    }
}
