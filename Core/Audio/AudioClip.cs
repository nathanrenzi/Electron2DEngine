using NAudio.Wave;

namespace Electron2D.Core.Audio
{
    /// <summary>
    /// A cached sound clip loaded from a file.
    /// </summary>
    public class AudioClip
    {
        public float[] AudioData { get; private set; }
        public WaveFormat WaveFormat { get; private set; }
        
        public AudioClip(string _fileName)
        {
            using (var audioFileReader = new AudioFileReader(_fileName))
            {
                WaveFormat = audioFileReader.WaveFormat;
                var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
                var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
                int samplesRead;
                while((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    wholeFile.AddRange(readBuffer.Take(samplesRead));
                }
                AudioData = wholeFile.ToArray();
            }
        }
    }
}
