using Electron2D.Audio.Filters;
using NAudio.Dsp;
using NAudio.Wave;

namespace Electron2D.Audio
{
    public class ReverbEffect : IAudioEffect, ISampleProvider
    {
        public ReverbAllPassSettings AllPassSettings { get; private set; }
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
        private BiQuadFilter _allPassFilter1;
        private BiQuadFilter _allPassFilter2;
        private BiQuadFilter _allPassFilter3;
        private Random _random = new Random(13371337);
        private const float _offsetMin = 0.95f;
        private const float _offsetMax = 1.05f;
        private bool _initialized = false;

        public ReverbEffect(float decayTimeInSeconds, float dampening, float dryWetMix, int quality = 40)
        {
            Quality = quality;
            DecayTimeInSeconds = decayTimeInSeconds;
            Dampening = dampening;
            DryWetMix = dryWetMix;
            _combFilters = new CombFilter[quality];
            AllPassSettings = new ReverbAllPassSettings();
        }

        public void Initialize(ISampleProvider sampleProvider)
        {
            Source = sampleProvider;
            _initialized = true;
            SetAllPassSettings(AllPassSettings);
            SetupCombFilters();
        }

        public void SetAllPassSettings(ReverbAllPassSettings settings)
        {
            AllPassSettings = settings;
            if (_initialized )
            {
                _allPassFilter1 = BiQuadFilter.AllPassFilter(WaveFormat.SampleRate,
                    AllPassSettings.Frequency1, AllPassSettings.Resonance1);
                _allPassFilter2 = BiQuadFilter.AllPassFilter(WaveFormat.SampleRate,
                    AllPassSettings.Frequency2, AllPassSettings.Resonance2);
                _allPassFilter3 = BiQuadFilter.AllPassFilter(WaveFormat.SampleRate,
                    AllPassSettings.Frequency3, AllPassSettings.Resonance3);
            }
        }

        private void SetupCombFilters()
        {
            for (int i = 0; i < _combFilters.Length; i++)
            {
                float offset = 1 / Quality * GetRandomOffset();
                float decayTimeInSeconds = DecayTimeInSeconds - (offset * i * i);
                _combFilters[i] = new CombFilter((int)(decayTimeInSeconds * Source.WaveFormat.SampleRate), Dampening);
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
                    reverbSample += _combFilters[x].Transform(reverbSample) / Quality;
                }
                reverbSample = _allPassFilter1.Transform(reverbSample);
                reverbSample = _allPassFilter2.Transform(reverbSample);
                reverbSample = _allPassFilter3.Transform(reverbSample);
                buffer[offset + i] += (1 - DryWetMix) * inputSample
                    + (DryWetMix * reverbSample); ;
            }

            return samplesRead;
        }

        public struct ReverbAllPassSettings
        {
            public float Frequency1;
            public float Resonance1;
            public float Frequency2;
            public float Resonance2;
            public float Frequency3;
            public float Resonance3;

            public static ReverbAllPassSettings NormalRoom = new ReverbAllPassSettings(80, 0.7f, 200, 1.0f, 400, 1.2f);
            public static ReverbAllPassSettings LargeRoom = new ReverbAllPassSettings(100f, 0.6f, 300f, 0.8f, 800f, 1.0f);
            public static ReverbAllPassSettings SmallRoom = new ReverbAllPassSettings(120f, 0.5f, 250f, 0.7f, 500f, 1.0f);
            public static ReverbAllPassSettings BrightStudio = new ReverbAllPassSettings(150f, 0.5f, 400f, 0.9f, 1200f, 1.2f);
            public static ReverbAllPassSettings Cave = new ReverbAllPassSettings(80f, 0.7f, 200f, 0.9f, 600f, 1.1f);
            public static ReverbAllPassSettings WarmConcertHall = new ReverbAllPassSettings(90f, 0.6f, 350f, 0.8f, 900f, 1.0f);

            public ReverbAllPassSettings(float frequency1, float resonance1, float frequency2, float resonance2,
                float frequency3, float resonance3)
            {
                Frequency1 = frequency1;
                Resonance1 = resonance1;
                Frequency2 = frequency2;
                Resonance2 = resonance2;
                Frequency3 = frequency3;
                Resonance3 = resonance3;
            }
        }
    }
}
