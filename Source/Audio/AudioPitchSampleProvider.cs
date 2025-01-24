using NAudio.Wave;

namespace Electron2D.Audio
{
    public class AudioPitchSampleProvider : ISampleProvider
    {
        public float Pitch { get; set; }
        public ISampleProvider Source { get; private set; }
        public WaveFormat WaveFormat => Source.WaveFormat;

        public AudioPitchSampleProvider(ISampleProvider source)
        {
            Source = source;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = Source.Read(buffer, offset, count);

            //for (int i = 0; i < samplesRead; i++)
            //{
            //    float c = (offset + i) * Pitch;
            //    int newOffset = (int)c;
            //    float t = ((offset + i) * Pitch) - newOffset;
            //    float inputSample = buffer[newOffset];
            //    float x0 = 0;
            //    float x2 = 0;
            //    float x3 = 0;
            //    if (newOffset - 1 >= 0)
            //    {
            //        x0 = buffer[newOffset - 1];
            //    }
            //    if (newOffset + 1 < buffer.Length)
            //    {
            //        x2 = buffer[newOffset + 1];
            //    }
            //    if (newOffset + 2 < buffer.Length)
            //    {
            //        x3 = buffer[newOffset + 2];
            //    }
            //    InterpolateHermite4pt3oX(x0, inputSample, x2, x3, t);
            //}

            return samplesRead;
        }

        public static float InterpolateHermite(float x0, float x1, float x2, float x3, float t)
        {
            float c0 = x1;
            float c1 = .5F * (x2 - x0);
            float c2 = x0 - (2.5F * x1) + (2 * x2) - (.5F * x3);
            float c3 = (.5F * (x3 - x0)) + (1.5F * (x1 - x2));
            return (((((c3 * t) + c2) * t) + c1) * t) + c0;
        }
    }
}
