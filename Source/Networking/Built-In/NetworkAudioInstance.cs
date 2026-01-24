using Electron2D.Audio;
using Newtonsoft.Json;

namespace Electron2D.Networking
{
    public class NetworkAudioInstance : NetworkGameClass
    {
        [Serializable]
        private class NetworkAudioInstanceInitializationJson
        {
            public string AudioClipFilePath = "";
            public float Volume;
            public float Pitch;
            public bool IsLoop;
            public PlaybackState PlaybackState;
            public long Position;
            public long TimeSent;
            public string SpatializerTransformNetworkID = "";
            public bool SpatializerIs3D;
            public float StartStopVolumeFadeTime;
            public List<string> Effects;
        }

        private static int _registerID;
        public static NetworkAudioInstance FactoryMethod(string json)
        {
            NetworkAudioInstance networkAudioInstance = new NetworkAudioInstance();
            networkAudioInstance.SetJson(json);
            return networkAudioInstance;
        }

        public AudioClip AudioClip => _audioInstance.AudioClip;
        public AudioStream Stream => _audioInstance.Stream;
        public PlaybackState PlaybackState => _audioInstance.PlaybackState;
        public bool IsLoop { get; private set; }
        public float Volume
        {
            get
            {
                return _audioInstance.Volume;
            }
            set
            {
                if(IsOwner)
                {
                    _audioInstance.Volume = value;
                    Send(Riptide.MessageSendMode.Reliable, JsonConvert.SerializeObject(value), 4);
                }
            }
        }
        public float Panning
        {
            get
            {
                return _audioInstance.Panning;
            }
            set
            {
                if (IsOwner)
                {
                    _audioInstance.Panning = value;
                    Send(Riptide.MessageSendMode.Reliable, JsonConvert.SerializeObject(value), 5);
                }
            }
        }
        public float Pitch
        {
            get
            {
                return _audioInstance.Pitch;
            }
            set
            {
                if (IsOwner)
                {
                    _audioInstance.Pitch = value;
                    Send(Riptide.MessageSendMode.Reliable, JsonConvert.SerializeObject(value), 6);
                }
            }
        }

        private string _audioSpatializerTransformNetworkID;
        private AudioSpatializer _audioSpatializer;
        private AudioInstance _audioInstance;
        private float _startStopVolumeFadeTime;
        private PlaybackState _cachedPlaybackState;

        public NetworkAudioInstance(AudioClip clip, float volume, float pitch, bool isLoop, float startStopVolumeFadeTime = 0.001f)
        {
            IsLoop = isLoop;
            _startStopVolumeFadeTime = startStopVolumeFadeTime;
            _audioInstance = new AudioInstance(clip, volume, pitch, isLoop, startStopVolumeFadeTime);
        }

        private NetworkAudioInstance() { }

        protected internal override bool CheckAndHandleUpdateVersion(ushort type, uint version)
        {
            if (version > UpdateVersion)
            {
                SetUpdateVersion(version);
                return true;
            }
            return false;
        }

        public override void FixedUpdate() { }
        public override void Update() { }

        public static void SetRegisterID(int registerID) => _registerID = registerID;
        protected internal override int GetRegisterID() => _registerID;

        protected internal override void OnDespawned()
        {
            _audioInstance.Stop();
            _audioInstance.Dispose();
        }

        protected internal override void OnDisposed()
        {
            _audioInstance.Stop();
            _audioInstance.Dispose();
        }

        protected internal override void OnNetworkInitialized() { }

        public void Play()
        {
            if(IsOwner)
            {
                _audioInstance.Play();
                Send(Riptide.MessageSendMode.Reliable, JsonConvert.SerializeObject(_audioInstance.Stream.Position), 0);
            }
        }

        public void Play(long position)
        {
            if(IsOwner)
            {
                _audioInstance.Play(position);
                Send(Riptide.MessageSendMode.Reliable, JsonConvert.SerializeObject(position), 0);
            }
        }

        public void Stop()
        {
            if (IsOwner)
            {
                _audioInstance.Stop();
                Send(Riptide.MessageSendMode.Reliable, "", 3);
            }
        }

        public void Pause()
        {
            if (IsOwner)
            {
                _audioInstance.Pause();
                Send(Riptide.MessageSendMode.Reliable, "", 1);
            }
        }

        public void Unpause()
        {
            if (IsOwner)
            {
                _audioInstance.Unpause();
                Send(Riptide.MessageSendMode.Reliable, JsonConvert.SerializeObject(_audioInstance.Stream.Position), 2);
            }
        }

        public void Spatialize(NetworkTransform transform, bool is3D)
        {
            if(IsOwner)
            {
                if(SetSpatializer(transform, is3D))
                {
                    Send(Riptide.MessageSendMode.Reliable, JsonConvert.SerializeObject((transform.NetworkID, is3D)), 7);
                }
            }
        }

        private bool SetSpatializer(NetworkTransform transform, bool is3D)
        {
            if (_audioSpatializer == null)
            {
                if (!transform.IsNetworkInitialized || transform.NetworkID == string.Empty)
                {
                    Debug.LogError("NetworkTransform must be initialized over the network to be used to spatialize an audio source!");
                    return false;
                }
                _audioSpatializerTransformNetworkID = transform.NetworkID;
                _audioSpatializer = new AudioSpatializer(transform.Transform, is3D);
                _audioSpatializer.AddAudioInstance(_audioInstance);
                return true;
            }
            return false;
        }

        public void AddEffect(IAudioEffect effect)
        {
            if(IsNetworkInitialized)
            {
                Debug.LogError("Cannot add audio effects once an audio source has been spawned over the network.");
                return;
            }
            _audioInstance.AddEffect(effect);
        }

        protected internal override void ReceiveData(ushort type, string json)
        {
            switch (type)
            {
                case 0:
                    _cachedPlaybackState = PlaybackState.Playing;
                    _audioInstance.Stream.Position = JsonConvert.DeserializeObject<long>(json);
                    _audioInstance.Play();
                    break;
                case 1:
                    _cachedPlaybackState = PlaybackState.Paused;
                    _audioInstance.Pause();
                    break;
                case 2:
                    _cachedPlaybackState = PlaybackState.Playing;
                    _audioInstance.Stream.Position = JsonConvert.DeserializeObject<long>(json);
                    _audioInstance.Unpause();
                    break;
                case 3:
                    _cachedPlaybackState = PlaybackState.Stopped;
                    _audioInstance.Stop();
                    break;
                case 4:
                    _audioInstance.Volume = JsonConvert.DeserializeObject<float>(json);
                    break;
                case 5:
                    _audioInstance.Panning = JsonConvert.DeserializeObject<float>(json);
                    break;
                case 6:
                    _audioInstance.Pitch = JsonConvert.DeserializeObject<float>(json);
                    break;
                case 7:
                    (string, bool) data = JsonConvert.DeserializeObject<(string, bool)>(json);
                    if(data.Item1 != null && data.Item1 != string.Empty 
                        && Network.Instance.Client.TryGetNetworkGameClass(data.Item1, out var networkTransform))
                    {
                        SetSpatializer((NetworkTransform)networkTransform, data.Item2);
                    }
                    else
                    {
                        Debug.LogError($"Cannot create audio spatializer for NetworkAudioInstance with ID: [{NetworkID}].");
                    }
                    break;
            }
        }

        protected internal override string ToJson()
        {
            NetworkAudioInstanceInitializationJson json = new NetworkAudioInstanceInitializationJson()
            {
                AudioClipFilePath = _audioInstance.AudioClip.FileName,
                Volume = _audioInstance.Volume,
                Pitch = _audioInstance.Pitch,
                IsLoop = _audioInstance.IsLoop,
                PlaybackState = _audioInstance.PlaybackState,
                Position = _audioInstance.Stream.Position,
                SpatializerTransformNetworkID = _audioInstance != null ? _audioSpatializerTransformNetworkID : "",
                SpatializerIs3D = _audioSpatializer != null ? _audioSpatializer.Is3D : false,
                StartStopVolumeFadeTime = _startStopVolumeFadeTime,
                TimeSent = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Effects = new List<string>()
            };
            //for(int i  = 0; i < _audioInstance.Effects.Count; i++)
            //{
            //    json.Effects.Add(_audioInstance.Effects[i].ToJson());
            //}
            return JsonConvert.SerializeObject(json);
        }

        protected override void SetJson(string json)
        {
            NetworkAudioInstanceInitializationJson data = JsonConvert.DeserializeObject<NetworkAudioInstanceInitializationJson>(json);
            _audioInstance = new AudioInstance(ResourceManager.Instance.LoadAudioClip(data.AudioClipFilePath), data.Volume, data.Pitch, data.IsLoop, data.StartStopVolumeFadeTime);
            IsLoop = data.IsLoop;
            _cachedPlaybackState = data.PlaybackState;
            if (!string.IsNullOrEmpty(data.SpatializerTransformNetworkID))
            {
                AddDependency<NetworkTransform>(data.SpatializerTransformNetworkID, (networkGameClass) =>
                {
                    SetSpatializer(networkGameClass, data.SpatializerIs3D);
                    switch (_cachedPlaybackState)
                    {
                        case PlaybackState.Playing:
                            _audioInstance.Play(data.Position);
                            break;
                        case PlaybackState.Stopped:
                        case PlaybackState.Paused:
                            _audioInstance.Stream.Position = data.Position;
                            _audioInstance.Pause();
                            break;
                    }
                });
            }
            //for(int i = 0; i < data.Effects.Count; i++)
            //{
            //    (string, List<object>) effect = JsonConvert.DeserializeObject<(string, List<object>)>(data.Effects[i]);
            //}
        }
    }
}
