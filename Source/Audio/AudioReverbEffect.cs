using NAudio.Wave;

namespace Electron2D.Audio
{
    public class AudioReverbEffect : IAudioEffect, ISampleProvider
    {
        public float DecayTimeInSeconds { get; set; }
        public float RoomSize { get; set; }
        public float Dampening { get; set; }
        public float EffectStrength { get; set; }
        public ISampleProvider Source { get; private set; }

        public WaveFormat WaveFormat => Source.WaveFormat;

        private float _decayFactor;
        private int _delayBufferLength;
        private float[] _delayBuffer;
        private int _delayBufferPosition;
        public AudioReverbEffect(float decayTimeInSeconds, float roomSize, float dampening, float effectStrength)
        {
            DecayTimeInSeconds = decayTimeInSeconds;
            RoomSize = roomSize;
            Dampening = dampening;
            EffectStrength = effectStrength;
        }

        public void Initialize(ISampleProvider sampleProvider)
        {
            Source = sampleProvider;
            _delayBufferLength = (int)(DecayTimeInSeconds * sampleProvider.WaveFormat.SampleRate);
            _delayBuffer = new float[_delayBufferLength];
            _delayBufferPosition = 0;
            _decayFactor = CalculateDecayFactor(DecayTimeInSeconds);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = Source.Read(buffer, offset, count);

            for (int i = 0; i < samplesRead; i++)
            {
                float inputSample = buffer[offset + i];

                float delayedSample = _delayBuffer[_delayBufferPosition];
                float reverbSample = RoomSize * _decayFactor * delayedSample;
                reverbSample *= (1.0f - Dampening);

                buffer[offset + i] += EffectStrength * reverbSample;
                _delayBuffer[_delayBufferPosition] = inputSample + EffectStrength * reverbSample;

                _delayBufferPosition++;
                if (_delayBufferPosition >= _delayBufferLength)
                {
                    _delayBufferPosition = 0;
                }
            }

            return samplesRead;
        }

        private float CalculateDecayFactor(float decayTimeInSeconds)
        {
            return (float)Math.Pow(10, (-3.0 / (decayTimeInSeconds * Source.WaveFormat.SampleRate)));
        }
    }
}
