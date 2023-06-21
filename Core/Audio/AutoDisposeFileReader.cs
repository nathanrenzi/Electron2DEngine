using NAudio.Wave;

namespace Electron2D.Core.Audio
{
    public class AutoDisposeFileReader : ISampleProvider
    {
        private readonly AudioFileReader reader;
        private bool isDisposed;
        public AutoDisposeFileReader(AudioFileReader _reader)
        {
            this.reader = _reader;
            this.WaveFormat = _reader.WaveFormat;
        }

        public int Read(float[] _buffer, int _offset, int _count)
        {
            if (isDisposed)
                return 0;
            int read = reader.Read(_buffer, _offset, _count);
            if (read == 0)
            {
                reader.Dispose();
                isDisposed = true;
            }
            return read;
        }

        public WaveFormat WaveFormat { get; private set; }
    }
}
