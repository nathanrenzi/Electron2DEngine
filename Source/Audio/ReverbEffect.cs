using Electron2D.Audio.Filters;
using NAudio.Dsp;
using NAudio.Wave;

namespace Electron2D.Audio
{
    public class ReverbEffect : IAudioEffect, ISampleProvider
    {
        public ReverbFilterSettings Settings { get; private set; }
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
        private BiQuadFilter _lowPassFilter;
        private BiQuadFilter _highPassFilter;
        private Random _random = new Random(13371337);
        private const float _offsetMin = 0.55f;
        private const float _offsetMax = 1.45f;
        private bool _initialized = false;

        public ReverbEffect(ReverbFilterSettings settings, float dryWetMix, int quality = 40)
        {
            Quality = quality;
            DryWetMix = dryWetMix;
            _combFilters = new CombFilter[quality];
            Settings = new ReverbFilterSettings(settings);
        }

        public void Initialize(ISampleProvider sampleProvider)
        {
            Source = sampleProvider;
            _initialized = true;
            SetFilterSettings(Settings);
            SetupCombFilters();
        }

        public void SetDecayTime(float decayTimeInSeconds)
        {
            Settings.DecayTimeInSeconds = decayTimeInSeconds;
        }

        public void SetDampening(float dampening)
        {
            Settings.Dampening = MathEx.Clamp01(dampening);
        }

        public void SetFilterSettings(ReverbFilterSettings settings)
        {
            Settings = settings;
            if (_initialized)
            {
                settings.Dampening = MathEx.Clamp01(settings.Dampening);
                _allPassFilter1 = BiQuadFilter.AllPassFilter(WaveFormat.SampleRate,
                    settings.AllPassFrequency1, settings.AllPassResonance1);
                _allPassFilter2 = BiQuadFilter.AllPassFilter(WaveFormat.SampleRate,
                    settings.AllPassFrequency2, settings.AllPassResonance2);
                _allPassFilter3 = BiQuadFilter.AllPassFilter(WaveFormat.SampleRate,
                    settings.AllPassFrequency3, settings.AllPassResonance3);
                _lowPassFilter = BiQuadFilter.LowPassFilter(WaveFormat.SampleRate,
                    settings.LowPassCutoff, settings.LowPassResonance);
                _highPassFilter = BiQuadFilter.HighPassFilter(WaveFormat.SampleRate,
                    settings.HighPassCutoff, settings.HighPassResonance);
            }
        }

        private void SetupCombFilters()
        {
            for (int i = 0; i < _combFilters.Length; i++)
            {
                float offset = 1 / _combFilters.Length * GetRandomOffset();
                float decayTimeInSeconds = Settings.DecayTimeInSeconds - (offset * i * i);
                _combFilters[i] = new CombFilter((int)(decayTimeInSeconds * Source.WaveFormat.SampleRate), Settings.Dampening);
            }
        }

        private float GetRandomOffset()
        {
            return MathEx.RandomFloatInRange(_random, _offsetMin, _offsetMax) / Quality;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = Source.Read(buffer, offset, count);

            int channels = WaveFormat.Channels;
            int totalFrames = samplesRead / channels;

            for (int i = 0; i < totalFrames; i++)
            {
                for (int ch = 0; ch < channels; ch++)
                {
                    int index = offset + i * channels + ch;
                    float inputSample = buffer[index];

                    float reverbSample = inputSample;
                    for (int x = 0; x < _combFilters.Length; x++)
                    {
                        float newSample = _combFilters[x].Transform(reverbSample);
                        newSample = _lowPassFilter.Transform(newSample);
                        newSample = _highPassFilter.Transform(newSample);
                        reverbSample += newSample / _combFilters.Length;
                    }
                    reverbSample = _allPassFilter1.Transform(reverbSample);
                    reverbSample = _allPassFilter2.Transform(reverbSample);
                    reverbSample = _allPassFilter3.Transform(reverbSample);
                    buffer[index] += (1 - DryWetMix) * inputSample
                        + (DryWetMix * reverbSample);
                }
            }

            return samplesRead;
        }
    }
}
