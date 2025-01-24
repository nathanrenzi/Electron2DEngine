using NAudio.Wave.SampleProviders;
using NAudio.Wave;

namespace Electron2D.Audio
{
    //https://github.com/naudio/NAudio?tab=readme-ov-file

    public static class AudioSystem
    {
        public static float MasterVolume
        {
            get
            {
                return _masterVolumeSampleProvider.Volume;
            }
            set
            {
                _masterVolumeSampleProvider.Volume = value;
            }
        }

        private static IWavePlayer _outputDevice;
        private static MixingSampleProvider _mixer;
        private static VolumeSampleProvider _masterVolumeSampleProvider;

        public static void Initialize(float masterVolume, int sampleRate = 44100, int channelCount = 2)
        {
            _outputDevice = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 0);
            _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
            _mixer.ReadFully = true;
            _masterVolumeSampleProvider = new VolumeSampleProvider(_mixer);
            MasterVolume = MathEx.Clamp(masterVolume, 0, 2);
            _outputDevice.Init(_masterVolumeSampleProvider);
            _outputDevice.Play();
        }

        public static AudioInstance CreateInstance(string fileName, float volume = 1, float pitch = 1, bool isLoop = false)
        {
            AudioClip clip = ResourceManager.Instance.LoadAudioClip(fileName);
            return new AudioInstance(clip, volume, pitch, isLoop);
        }

        public static AudioInstance CreateInstance(AudioClip clip, float volume = 1, float pitch = 1, bool isLoop = false)
        {
            return new AudioInstance(clip, volume, pitch, isLoop);
        }

        public static void PlayAudioInstance(AudioInstance audioInstance)
        {
            if (_mixer.MixerInputs.Contains(audioInstance.Stream.SampleProvider)) return;
            _mixer.AddMixerInput(ConvertToRightChannelCount(audioInstance.Stream.SampleProvider));
        }

        private static ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
        {
            if (input.WaveFormat.Channels == _mixer.WaveFormat.Channels)
            {
                return input;
            }
            if (input.WaveFormat.Channels == 1 && _mixer.WaveFormat.Channels == 2)
            {
                return new MonoToStereoSampleProvider(input);
            }
            throw new NotImplementedException("Not yet implemented this channel count conversion");
        }

        public static void Dispose()
        {
            _mixer.RemoveAllMixerInputs();
            _outputDevice.Stop();
            _outputDevice.Dispose();
        }
    }
}
