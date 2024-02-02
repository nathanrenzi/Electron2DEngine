using NAudio.Wave.SampleProviders;
using NAudio.Wave;

namespace Electron2D.Core.Audio
{
    //https://github.com/naudio/NAudio?tab=readme-ov-file

    public static class AudioSystem
    {
        public static float MasterVolume
        {
            get
            {
                return masterVolumeSampleProvider.Volume;
            }
            set
            {
                masterVolumeSampleProvider.Volume = value;
            }
        }

        private static Dictionary<string, AudioClip> cachedClips = new Dictionary<string, AudioClip>();
        private static IWavePlayer outputDevice;
        private static MixingSampleProvider mixer;
        private static VolumeSampleProvider masterVolumeSampleProvider;

        public static void Initialize(int _sampleRate = 44100, int _channelCount = 2)
        {
            outputDevice = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 50);
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(_sampleRate, _channelCount));
            mixer.ReadFully = true;
            masterVolumeSampleProvider = new VolumeSampleProvider(mixer);
            outputDevice.Init(masterVolumeSampleProvider);
            outputDevice.Play();
        }

        public static AudioInstance CreateInstance(string _fileName, float _volume = 1, float _pitch = 1, bool _isLoop = false)
        {
            AudioClip clip;
            if(cachedClips.ContainsKey(_fileName))
            {
                clip = cachedClips[_fileName];
            }
            else
            {
                clip = new AudioClip(_fileName);
                cachedClips.Add(_fileName, clip);
            }

            return new AudioInstance(clip, _volume, _pitch, _isLoop);
        }

        public static AudioInstance CreateInstance(AudioClip _clip, float _volume = 1, float _pitch = 1, bool _isLoop = false)
        {
            return new AudioInstance(_clip, _volume, _pitch, _isLoop);
        }

        public static AudioClip LoadClip(string _fileName)
        {
            if (cachedClips.ContainsKey(_fileName))
            {
                return cachedClips[_fileName];
            }
            else
            {
                AudioClip clip = new AudioClip(_fileName);
                cachedClips.Add(_fileName, clip);
                return clip;
            }
        }

        public static void PlayAudioInstance(AudioInstance _audioInstance)
        {
            mixer.AddMixerInput(ConvertToRightChannelCount(_audioInstance.Stream.SampleProvider));
        }

        private static ISampleProvider ConvertToRightChannelCount(ISampleProvider _input)
        {
            if (_input.WaveFormat.Channels == mixer.WaveFormat.Channels)
            {
                return _input;
            }
            if (_input.WaveFormat.Channels == 1 && mixer.WaveFormat.Channels == 2)
            {
                return new MonoToStereoSampleProvider(_input);
            }
            throw new NotImplementedException("Not yet implemented this channel count conversion");
        }

        public static void Dispose()
        {
            mixer.RemoveAllMixerInputs();
            cachedClips.Clear();
        }
    }
}
