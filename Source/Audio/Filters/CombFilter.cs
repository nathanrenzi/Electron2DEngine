namespace Electron2D.Audio.Filters
{
    public class CombFilter
    {
        public float Dampening { get; set; }

        private float[] _delayBuffer;
        private int _delayBufferPosition = 0;
        private bool _disabled = false;
        private float _decayFactor;

        public CombFilter(int bufferLength, float dampening)
        {
            if(bufferLength <= 0)
            {
                _disabled = true;
                return;
            }
            Dampening = dampening;
            _delayBuffer = new float[bufferLength];
            _decayFactor = CalculateDecayFactor(bufferLength);
        }

        public float Transform(float input)
        {
            if (_disabled) return 0;

            float delayedSample = _delayBuffer[_delayBufferPosition];
            delayedSample *= MathF.Max(Dampening, 0);

            _delayBuffer[_delayBufferPosition] = input + delayedSample * _decayFactor;

            _delayBufferPosition++;
            if (_delayBufferPosition >= _delayBuffer.Length)
            {
                _delayBufferPosition = 0;
            }
            return delayedSample;
        }

        private float CalculateDecayFactor(int samples)
        {
            return (float)Math.Pow(10f, (-3.0f / samples));
        }
    }
}
