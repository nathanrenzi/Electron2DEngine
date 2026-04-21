using NAudio.Wave;
using System.Numerics;

namespace Electron2D.Audio
{
    public class AudioSpatializer : IGameClass
    {
        public List<AudioInstance> AudioInstances { get; private set; } = new();
        public float MinRange { get; set; } = 100f;
        public float MaxRange { get; set; } = Display.REFERENCE_WINDOW_WIDTH * 0.5f;
        public float PanningSpatializationMultiplier { get; set; } = 1.0f;
        public float VolumeSpatializationMultiplier { get; set; } = 1.0f;
        public bool Is3D { get; }
        public Curve FalloffCurve { get; set; }
        public float DirectionBasedPanning { get; private set; }
        public float DistanceBasedVolumeMultiplier01 { get; private set; }

        private Transform _transform;

        public AudioSpatializer(Transform transform, bool is3D, AudioInstance[] audioInstances)
        {
            _transform = transform;
            Is3D = is3D;
            foreach (var instance in audioInstances)
                AddAudioInstance(instance);
            Engine.Game.RegisterGameClass(this);
        }

        public AudioSpatializer(Transform transform, bool is3D)
        {
            _transform = transform;
            Is3D = is3D;
            Engine.Game.RegisterGameClass(this);
        }

        public void AddAudioInstance(AudioInstance instance)
        {
            if (AudioInstances.Contains(instance)) return;

            AudioInstances.Add(instance);

            if (Is3D)
            {
                float fadeTime = instance.Stream.GetFadeTime();
                long position = instance.Stream.Position;
                bool wasPlaying = instance.PlaybackState == PlaybackState.Playing;

                instance.Stream.SetFadeTime(0.0001f);
                if (wasPlaying) instance.Stop();

                var newStream = new AudioStream(instance, instance.Stream.FileName, true);
                instance.Stream.Dispose();
                instance.Stream = newStream;
                instance.Stream.Position = position;
                instance.Stream.SetFadeTime(0.0001f);

                if (wasPlaying) instance.Play();
                instance.Stream.SetFadeTime(fadeTime);
            }

            instance.SetSpatializer(this);
        }

        public void RemoveAudioInstance(AudioInstance instance)
        {
            if (!AudioInstances.Contains(instance)) return;
            AudioInstances.Remove(instance);
            instance.SetSpatializer(null);
        }

        public void Update()
        {
            CalculateDistanceMultiplier();
            CalculatePanning();
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
            Engine.Game.UnregisterGameClass(this);
            GC.SuppressFinalize(this);
        }
    }
}