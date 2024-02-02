﻿using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Electron2D.Core.Audio
{
    /// <summary>
    /// A cached sound clip loaded from a file.
    /// </summary>
    public class AudioClip
    {
        public string FileName { get; }
        public float[] AudioData { get; private set; }
        public WaveFormat WaveFormat { get; private set; }
        
        public AudioClip(string _fileName)
        {
            FileName = _fileName;

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

        public AudioStream GetNewStream(AudioInstance _instance, bool _is3D, IPanStrategy _panStrategy = null)
        {
            return new AudioStream(_instance, new AudioFileReader(FileName), _is3D, _panStrategy);
        }
    }
}
