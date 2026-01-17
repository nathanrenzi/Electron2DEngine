using Newtonsoft.Json;
using Riptide;
using System.Numerics;

namespace Electron2D.Networking
{
    /// <summary>
    /// Replicates a Transform object over the network. Uses <see cref="NetworkGameClass"/>.
    /// </summary>
    public class NetworkTransform : NetworkGameClass
    {
        [Serializable]
        private class NetworkTransformInitializationJson
        {
            public Vector2 Position;
            public float Rotation;
            public Vector2 Scale;
            public NetworkValueSettings PositionNetworkSettings;
            public NetworkValueSettings RotationNetworkSettings;
            public NetworkValueSettings ScaleNetworkSettings;
        }

        private static int _registerID;
        public static NetworkTransform FactoryMethod(string json)
        {
            NetworkTransform networkTransform = new NetworkTransform(new Transform());
            networkTransform.SetJson(json);
            return networkTransform;
        }

        public Transform Transform { get; private set; }
        public Vector2 Scale
        { 
            get
            {
                return Transform.Scale;
            }
            set
            {
                if(IsOwner)
                {
                    Transform.Scale = value;
                }
            }
        }
        public Vector2 Position
        {
            get
            {
                return Transform.Position;
            }
            set
            {
                if(IsOwner)
                {
                    Transform.Position = value;
                }
            }
        }
        public float Rotation
        {
            get
            {
                return Transform.Rotation;
            }
            set
            {
                if(IsOwner)
                {
                    Transform.Rotation = value;
                }
            }
        }
        public NetworkValueSettings PositionNetworkSettings { get; set; }
        public NetworkValueSettings RotationNetworkSettings { get; set; }
        public NetworkValueSettings ScaleNetworkSettings { get; set; }
        private uint _scaleUpdateVersion = 0;
        private uint _positionUpdateVersion = 0;
        private uint _rotationUpdateVersion = 0;
        private Queue<Vector2> _interpolateToPositionQueue = new Queue<Vector2>();
        private Vector2 _interpolateToPosition;
        private float _interpolateToRotation;
        private Vector2 _interpolateToScale;
        private Vector2 _interpolateFromPosition;
        private float _interpolateFromRotation;
        private Vector2 _interpolateFromScale;
        private float _interpolatePositionTime;
        private float _rotationTimeReceived;
        private float _scaleTimeReceived;
        private float _currentPositionSendIntervalTime;
        private float _currentRotationSendIntervalTime;
        private float _currentScaleSendIntervalTime;
        private Vector2 _lastSentScale;
        private Vector2 _lastSentPosition;
        private float _lastRotation;

        /// <summary>
        /// Creates a <see cref="NetworkTransform"/> object.
        /// </summary>
        /// <param name="transform">The transform to update through the network.</param>
        /// <param name="positionNetworkSettings">The network settings for position.</param>
        /// <param name="rotationNetworkSettings">The network settings for rotation.</param>
        /// <param name="scaleNetworkSettings">The network settings for scale.</param>
        public NetworkTransform(Transform transform, NetworkValueSettings positionNetworkSettings,
            NetworkValueSettings rotationNetworkSettings, NetworkValueSettings scaleNetworkSettings)
        {
            Transform = transform;
            PositionNetworkSettings = positionNetworkSettings;
            RotationNetworkSettings = rotationNetworkSettings;
            ScaleNetworkSettings = scaleNetworkSettings;
        }

        /// <summary>
        /// Creates a <see cref="NetworkTransform"/> object with automatic position, scale, and rotation sending disabled by default.
        /// </summary>
        /// <param name="transform">The transform to update through the network.</param>
        public NetworkTransform(Transform transform)
        {
            Transform = transform;
            PositionNetworkSettings = new NetworkValueSettings() { SendAutomatically = true, MessageSendMode = MessageSendMode.Unreliable };
            RotationNetworkSettings = new NetworkValueSettings() { SendAutomatically = true, MessageSendMode = MessageSendMode.Unreliable };
            ScaleNetworkSettings = new NetworkValueSettings() { SendAutomatically = true, MessageSendMode = MessageSendMode.Unreliable };
        }

        public override void Update()
        {
            if(IsOwner && IsNetworkInitialized)
            {
                if (_currentPositionSendIntervalTime >= PositionNetworkSettings.SendInterval)
                {
                    if (PositionNetworkSettings.SendAutomatically)
                        SendPositionUpdate(PositionNetworkSettings.MessageSendMode);
                    _currentPositionSendIntervalTime -= PositionNetworkSettings.SendInterval;
                }
                if (_currentRotationSendIntervalTime >= RotationNetworkSettings.SendInterval)
                {
                    if (RotationNetworkSettings.SendAutomatically)
                        SendRotationUpdate(RotationNetworkSettings.MessageSendMode);
                    _currentRotationSendIntervalTime -= RotationNetworkSettings.SendInterval;
                }
                if (_currentScaleSendIntervalTime >= ScaleNetworkSettings.SendInterval)
                {
                    if (ScaleNetworkSettings.SendAutomatically)
                        SendScaleUpdate(ScaleNetworkSettings.MessageSendMode);
                    _currentScaleSendIntervalTime -= ScaleNetworkSettings.SendInterval;
                }

                _currentPositionSendIntervalTime += Time.DeltaTime;
                _currentRotationSendIntervalTime += Time.DeltaTime;
                _currentScaleSendIntervalTime += Time.DeltaTime;
            }
            else if(!IsOwner && IsNetworkInitialized)
            {
                if(PositionNetworkSettings.Interpolate)
                    InterpolatePosition();
                if(RotationNetworkSettings.Interpolate)
                    InterpolateRotation();
                if(ScaleNetworkSettings.Interpolate)
                    InterpolateScale();
            }
        }

        private void InterpolatePosition()
        {
            while (_interpolatePositionTime >= PositionNetworkSettings.SendInterval && _interpolateToPositionQueue.Count > 0)
            {
                _interpolateFromPosition = _interpolateToPosition;
                _interpolateToPosition = _interpolateToPositionQueue.Dequeue();
                _interpolatePositionTime -= PositionNetworkSettings.SendInterval;
            }

            float pt = _interpolatePositionTime / PositionNetworkSettings.SendInterval;
            Transform.Position = Vector2.Lerp(_interpolateFromPosition, _interpolateToPosition, MathF.Min(pt, 1));
            _interpolatePositionTime += Time.DeltaTime * (1 + _interpolateToPositionQueue.Count * 0.005f);
        }

        private void InterpolateRotation()
        {
            float rt = (Time.GameTime - _rotationTimeReceived) / RotationNetworkSettings.SendInterval;
            if (rt <= 1)
            {
                Transform.Rotation = (_interpolateFromRotation * (1f - rt)) + (_interpolateToRotation * rt);
            }
            else
            {
                Transform.Rotation = _interpolateToRotation;
            }
        }

        private void InterpolateScale()
        {
            float st = (Time.GameTime - _scaleTimeReceived) / ScaleNetworkSettings.SendInterval;
            if (st <= 1)
            {
                Transform.Scale = Vector2.Lerp(_interpolateFromScale, _interpolateToScale, st);
            }
            else
            {
                Transform.Scale = _interpolateToScale;
            }
        }

        public override void FixedUpdate() { }

        public static void SetRegisterID(int registerID) => _registerID = registerID;
        public override int GetRegisterID() => _registerID;

        /// <summary>
        /// Manually sends a position update.
        /// </summary>
        public void SendPositionUpdate(MessageSendMode messageSendMode)
        {
            _lastSentPosition = Position;
            string json = JsonConvert.SerializeObject(Position);
            Send(messageSendMode, json, 1);
        }

        /// <summary>
        /// Manually sends a rotation update.
        /// </summary>
        public void SendRotationUpdate(MessageSendMode messageSendMode)
        {
            _lastRotation = Rotation;
            string json = JsonConvert.SerializeObject(Rotation);
            Send(messageSendMode, json, 2);
        }

        /// <summary>
        /// Manually sends a scale update;
        /// </summary>
        public void SendScaleUpdate(MessageSendMode messageSendMode)
        {
            _lastSentScale = Scale;
            string json = JsonConvert.SerializeObject(Scale);
            Send(messageSendMode, json, 3);
        }

        public override bool CheckAndHandleUpdateVersion(ushort type, uint version)
        {
            if(type == 1)
            {
                if(version > _positionUpdateVersion)
                {
                    _positionUpdateVersion = version;
                    return true;
                }
            }
            else if(type == 2)
            {
                if (version > _rotationUpdateVersion)
                {
                    _rotationUpdateVersion = version;
                    return true;
                }
            }
            else if (type == 3)
            {
                if (version > _scaleUpdateVersion)
                {
                    _scaleUpdateVersion = version;
                    return true;
                }
            }
            return false;
        }

        public override void OnDespawned()
        {
            _interpolateToPositionQueue.Clear();
        }

        public override void OnDisposed()
        {
            Transform = null;
            _interpolateToPositionQueue.Clear();
            _interpolateToPositionQueue = null;
        }

        public override void OnNetworkInitialized() { }

        public override void ReceiveData(ushort type, string json)
        {
            if(type == 1)
            {
                Vector2 position = JsonConvert.DeserializeObject<Vector2>(json);
                if (PositionNetworkSettings.Interpolate)
                {
                    _interpolateToPositionQueue.Enqueue(position);
                }
                else 
                {
                    Transform.Position = position;
                }
            }
            else if(type == 2)
            {
                _interpolateToRotation = JsonConvert.DeserializeObject<float>(json);
                _interpolateFromRotation = Transform.Rotation;
                _rotationTimeReceived = Time.GameTime;
                if (!RotationNetworkSettings.Interpolate) Transform.Rotation = _interpolateToRotation;
            }
            else if(type == 3)
            {
                _interpolateToScale = JsonConvert.DeserializeObject<Vector2>(json);
                _interpolateFromScale = Transform.Scale;
                _scaleTimeReceived = Time.GameTime;
                if (!ScaleNetworkSettings.Interpolate) Transform.Scale = _interpolateToScale;
            }
        }

        public override string ToJson()
        {
            NetworkTransformInitializationJson initJson = new NetworkTransformInitializationJson();
            initJson.Position = _lastSentPosition;
            initJson.Rotation = _lastRotation;
            initJson.Scale = _lastSentScale;
            initJson.PositionNetworkSettings = PositionNetworkSettings;
            initJson.RotationNetworkSettings = RotationNetworkSettings;
            initJson.ScaleNetworkSettings = ScaleNetworkSettings;
            return JsonConvert.SerializeObject(initJson);
        }

        protected override void SetJson(string json)
        {
            NetworkTransformInitializationJson initJson = JsonConvert.DeserializeObject<NetworkTransformInitializationJson>(json);
            PositionNetworkSettings = initJson.PositionNetworkSettings;
            RotationNetworkSettings = initJson.RotationNetworkSettings;
            ScaleNetworkSettings = initJson.ScaleNetworkSettings;
            _interpolateToPosition = _interpolateFromPosition = Transform.Position = initJson.Position;
            _interpolateToRotation = _interpolateFromRotation = Transform.Rotation = initJson.Rotation;
            _interpolateToScale = _interpolateFromScale = Transform.Scale = initJson.Scale;
            _rotationTimeReceived = Time.GameTime;
            _scaleTimeReceived = Time.GameTime;
            _interpolatePositionTime = 0;
            _interpolateToPositionQueue.Clear();
        }
    }
}
