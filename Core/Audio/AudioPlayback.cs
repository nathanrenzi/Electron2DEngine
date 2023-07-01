﻿using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Electron2D.Core.Audio
{
    public class AudioPlayback : IDisposable
    {
        // MUST BE PLAYED IN THE MAIN THREAD FOR IT TO BE HEARD
        // OTHERWISE NEW INSTANCE IS NEEDED (will break if new instance is created so don't do this)
        public static AudioPlayback Instance;
        public float masterVolume { get { return volumeMixer.Volume; } }
        
        private readonly IWavePlayer outputDevice;

        private readonly MixingSampleProvider mixer;
        private readonly VolumeSampleProvider volumeMixer;

        public static void Initialize(float _masterVolume)
        {
            if (Instance == null) Instance = new AudioPlayback(_masterVolume);
        }

        public AudioPlayback(float _masterVolume, int _sampleRate = 44100, int _channelCount = 2)
        {
            //string[] driverNames = AsioOut.GetDriverNames();
            // must select correct output device in driver list
            //outputDevice = new AsioOut(driverNames.Length - 1);
            outputDevice = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 50);
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(_sampleRate, _channelCount));
            mixer.ReadFully = true;

            volumeMixer = new VolumeSampleProvider(mixer);
            volumeMixer.Volume = _masterVolume;

            outputDevice.Init(volumeMixer);
            outputDevice.Play();
        }

        public void SetMasterVolume(float _masterVolume)
        {
            volumeMixer.Volume = _masterVolume;
        }

        private ISampleProvider ConvertToCorrectChannelCount(ISampleProvider _input)
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

        public void PlaySound(CachedSound _sound, float _volume = 1f, float _pitch = 1f, float _panning = 0f)
        {
            ISampleProvider finalMixer;

            VolumeSampleProvider clipVolume = new VolumeSampleProvider(new CachedSoundSampleProvider(_sound));
            clipVolume.Volume = _volume;

            SmbPitchShiftingSampleProvider pitch = new SmbPitchShiftingSampleProvider(clipVolume);
            pitch.PitchFactor = _pitch;
            finalMixer = pitch;

            // If panning is enabled, the sound must be converted to mono
            if(_panning != 0)
            {
                StereoToMonoSampleProvider mono = new StereoToMonoSampleProvider(pitch);
                PanningSampleProvider panning = new PanningSampleProvider(mono);
                panning.Pan = _panning;
                finalMixer = panning;
            }

            AddMixerInput(finalMixer);
        }

        private void AddMixerInput(ISampleProvider input)
        {
            mixer.AddMixerInput(ConvertToCorrectChannelCount(input));
        }

        public void Dispose()
        {
            outputDevice.Dispose();
        }
    }
}