using Electron2D.Audio.Effects;
using NAudio.Wave;

namespace Electron2D.Audio
{
    public class BasicAudioReverbEffect : IAudioEffect, ISampleProvider
    {
        public float DecayTimeInSeconds { get; private set; }
        public float Dampening
        {
            get
            {
                return _damping;
            }
            set
            {
                _damping = MathEx.Clamp01(value);
            }
        }
        private float _damping;
        public float DryWetMix
        {
            get
            {
                return _dryWetMix;
            }
            set
            {
                _dryWetMix = MathEx.Clamp01(value);
            }
        }
        private float _dryWetMix;
        public int Quality { get; private set; }
        public ISampleProvider Source { get; private set; }
        public WaveFormat WaveFormat => Source.WaveFormat;

        private CombFilter[] _combFilters;
        private Random _random = new Random(13371337);
        private const float _offsetMin = 0.95f;
        private const float _offsetMax = 1.05f;

        public BasicAudioReverbEffect(float decayTimeInSeconds, float dampening, float dryWetMix, int quality = 40)
        {
            Quality = quality;
            DecayTimeInSeconds = decayTimeInSeconds;
            Dampening = dampening;
            DryWetMix = dryWetMix;
            _combFilters = new CombFilter[quality];
        }

        public void Initialize(ISampleProvider sampleProvider)
        {
            Source = sampleProvider;
            SetupCombFilters();
        }

        private void SetupCombFilters()
        {
            for (int i = 0; i < _combFilters.Length; i++)
            {
                float offset = 1 / Quality * GetRandomOffset();
                float decayTimeInSeconds = DecayTimeInSeconds - (offset * i * i);
                _combFilters[i] = new CombFilter((int)(decayTimeInSeconds * Source.WaveFormat.SampleRate));
            }
        }

        private float GetRandomOffset()
        {
            return MathEx.RandomFloatInRange(_random, _offsetMin, _offsetMax) / Quality;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = Source.Read(buffer, offset, count);

            for (int i = 0; i < samplesRead; i++)
            {
                float inputSample = buffer[offset + i];

                float reverbSample = inputSample;
                for(int x = 0; x < _combFilters.Length; x++)
                {
                    reverbSample += _combFilters[x].Read(reverbSample, Dampening) / Quality;
                }
                //float finalSample = _allPassFilter1.Read(reverbSample);
                //finalSample = _allPassFilter2.Read(finalSample);
                buffer[offset + i] += (1 - DryWetMix) * inputSample
                    + (DryWetMix * reverbSample);
            }

            return samplesRead;
        }
    }
}
