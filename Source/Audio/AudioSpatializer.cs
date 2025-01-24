using System.Numerics;

namespace Electron2D.Audio
{
    public class AudioSpatializer : IGameClass
    {
        public List<AudioInstance> AudioInstances { get; private set; } = new List<AudioInstance>();
        public float MinRange { get; set; } = 100f;
        public float MaxRange { get; set; } = Display.REFERENCE_WINDOW_WIDTH * 0.5f;
        public float PanningSpatializationMultiplier { get; set; } = 1.0f;
        public float VolumeSpatializationMultiplier { get; set; } = 1.0f;
        public bool Is3D { get; }
        // add bezier curve for falloff

        public float DirectionBasedPanning { get; private set; }
        public float DistanceBasedVolumeMultiplier01 { get; private set; }

        private Transform _transform;

        public AudioSpatializer(Transform transform, bool is3D, AudioInstance[] audioInstances)
        {
            _transform = transform;
            Is3D = is3D;

            for (int i = 0; i < audioInstances.Length; i++)
            {
                AddAudioInstance(audioInstances[i]);
            }

            Program.Game.RegisterGameClass(this);
        }

        public AudioSpatializer(Transform transform, bool _is3D)
        {
            _transform = transform;
            Is3D = _is3D;

            Program.Game.RegisterGameClass(this);
        }

        ~AudioSpatializer()
        {
            Dispose();
        }

        public void AddAudioInstance(AudioInstance _audioInstance)
        {
            if(!AudioInstances.Contains(_audioInstance))
            {
                AudioInstances.Add(_audioInstance);
                if(Is3D)
                {
                    _audioInstance.Stream = _audioInstance.AudioClip.GetNewStream(_audioInstance, true);
                }
                _audioInstance.SetSpatializerReference(this);
            }
        }

        public void RemoveAudioInstance(AudioInstance _audioInstance)
        {
            if (AudioInstances.Contains(_audioInstance))
            {
                AudioInstances.Remove(_audioInstance);
                _audioInstance.SetSpatializerReference(null);
            }
        }

        public void Update()
        {
            CalculateDistanceMultiplier();
            CalculatePanning();

            for (int i = 0; i < AudioInstances.Count; i++)
            {
                AudioInstance instance = AudioInstances[i];
                instance.VolumeMultiplier = DistanceBasedVolumeMultiplier01;
                instance.PanningAdditive = DirectionBasedPanning;
            }
        }

        private void CalculateDistanceMultiplier()
        {
            float rawDistance = Vector2.Distance(AudioSpatialListener.Instance.GetPosition(), _transform.Position);
            DistanceBasedVolumeMultiplier01 = 1 - (MathEx.Clamp01((rawDistance / (MaxRange - MinRange)) + (MinRange / MaxRange)) * MathEx.Clamp(VolumeSpatializationMultiplier, 0, 1));
        }

        private void CalculatePanning()
        {
            DirectionBasedPanning = ((_transform.Position.X - AudioSpatialListener.Instance.GetPosition().X) / (Display.REFERENCE_WINDOW_WIDTH * 0.5f)) * MathEx.Clamp(PanningSpatializationMultiplier, 0, 10);
        }

        public void FixedUpdate() { }

        public void Dispose()
        {
            Program.Game.UnregisterGameClass(this);
            GC.SuppressFinalize(this);
        }
    }
}
