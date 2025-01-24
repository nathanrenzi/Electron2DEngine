using NAudio.Dsp;
using NAudio.Wave;

namespace Electron2D.Audio
{
    public class HighPassFilterEffect : IAudioEffect
    {
        public float CutoffFrequency
        {
            get
            {
                return _cutoffFrequency;
            }
            set
            {
                _cutoffFrequency = value;
                _filter.SetLowPassFilter(WaveFormat.SampleRate, CutoffFrequency, Resonance);
            }
        }
        private float _cutoffFrequency;
        public float Resonance
        {
            get
            {
                return _resonance;
            }
            set
            {
                _resonance = value;
                _filter.SetLowPassFilter(WaveFormat.SampleRate, CutoffFrequency, Resonance);
            }
        }
        private float _resonance;
        public ISampleProvider Source { get; private set; }
        public WaveFormat WaveFormat => Source.WaveFormat;
        private BiQuadFilter _filter;

        public HighPassFilterEffect(float cutoff, float resonance)
        {
            _cutoffFrequency = cutoff;
            _resonance = resonance;
        }

        public void Initialize(ISampleProvider sampleProvider)
        {
            Source = sampleProvider;
            _filter = BiQuadFilter.HighPassFilter(WaveFormat.SampleRate, CutoffFrequency, Resonance);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = Source.Read(buffer, offset, count);

            for (int i = 0; i < samplesRead; i++)
            {
                float inputSample = buffer[offset + i];
                buffer[offset + i] = _filter.Transform(inputSample);
            }

            return samplesRead;
        }
    }
}
