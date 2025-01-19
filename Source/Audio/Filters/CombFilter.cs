namespace Electron2D.Audio.Effects
{
    public class CombFilter
    {
        private float[] _delayBuffer;
        private int _delayBufferPosition = 0;
        private bool _disabled = false;
        private float _decayFactor;

        public CombFilter(int bufferLength)
        {
            if(bufferLength <= 0)
            {
                _disabled = true;
                return;
            }
            _delayBuffer = new float[bufferLength];
            _decayFactor = CalculateDecayFactor(bufferLength);
        }

        public float Read(float input, float dampening)
        {
            if (_disabled) return 0;

            float delayedSample = _delayBuffer[_delayBufferPosition];
            delayedSample *= MathF.Max(dampening, 0);

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
