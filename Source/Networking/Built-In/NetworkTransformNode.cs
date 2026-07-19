using Newtonsoft.Json;
using Riptide;
using System.Numerics;

namespace Atlas2D.Networking
{
    /// <summary>
    /// Replicates position, rotation, and scale over the network.
    /// </summary>
    public class NetworkTransformNode : NetworkNode, INetworkFactory
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

        public static NetworkNode FactoryMethod(string json)
        {
            NetworkTransformNode node = new NetworkTransformNode();
            node.SetJson(json);
            return node;
        }

        public Vector2 Scale
        {
            get => LocalScale;
            set { if (IsOwner) LocalScale = value; }
        }
        public Vector2 Position
        {
            get => LocalPosition;
            set { if (IsOwner) LocalPosition = value; }
        }
        public float Rotation
        {
            get => LocalRotation;
            set { if (IsOwner) LocalRotation = value; }
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

        public NetworkTransformNode()
        {
            PositionNetworkSettings = new NetworkValueSettings() { SendAutomatically = true, MessageSendMode = MessageSendMode.Unreliable };
            RotationNetworkSettings = new NetworkValueSettings() { SendAutomatically = true, MessageSendMode = MessageSendMode.Unreliable };
            ScaleNetworkSettings = new NetworkValueSettings() { SendAutomatically = true, MessageSendMode = MessageSendMode.Unreliable };
        }

        public NetworkTransformNode(NetworkValueSettings positionNetworkSettings,
            NetworkValueSettings rotationNetworkSettings, NetworkValueSettings scaleNetworkSettings)
        {
            PositionNetworkSettings = positionNetworkSettings;
            RotationNetworkSettings = rotationNetworkSettings;
            ScaleNetworkSettings = scaleNetworkSettings;
        }

        protected override void OnUpdate()
        {
            if (IsOwner && IsNetworkInitialized)
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
            else if (!IsOwner && IsNetworkInitialized)
            {
                if (PositionNetworkSettings.Interpolate)
                    InterpolatePosition();
                if (RotationNetworkSettings.Interpolate)
                    InterpolateRotation();
                if (ScaleNetworkSettings.Interpolate)
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
            LocalPosition = Vector2.Lerp(_interpolateFromPosition, _interpolateToPosition, MathF.Min(pt, 1));
            _interpolatePositionTime += Time.DeltaTime * (1 + _interpolateToPositionQueue.Count * 0.005f);
        }

        private void InterpolateRotation()
        {
            float rt = (Time.GameTime - _rotationTimeReceived) / RotationNetworkSettings.SendInterval;
            LocalRotation = rt <= 1
                ? (_interpolateFromRotation * (1f - rt)) + (_interpolateToRotation * rt)
                : _interpolateToRotation;
        }

        private void InterpolateScale()
        {
            float st = (Time.GameTime - _scaleTimeReceived) / ScaleNetworkSettings.SendInterval;
            LocalScale = st <= 1
                ? Vector2.Lerp(_interpolateFromScale, _interpolateToScale, st)
                : _interpolateToScale;
        }

        /// <summary>
        /// Manually sends a position update.
        /// </summary>
        public void SendPositionUpdate(MessageSendMode messageSendMode)
        {
            _lastSentPosition = Position;
            Send(messageSendMode, JsonConvert.SerializeObject(Position), 1);
        }

        /// <summary>
        /// Manually sends a rotation update.
        /// </summary>
        public void SendRotationUpdate(MessageSendMode messageSendMode)
        {
            _lastRotation = Rotation;
            Send(messageSendMode, JsonConvert.SerializeObject(Rotation), 2);
        }

        /// <summary>
        /// Manually sends a scale update.
        /// </summary>
        public void SendScaleUpdate(MessageSendMode messageSendMode)
        {
            _lastSentScale = Scale;
            Send(messageSendMode, JsonConvert.SerializeObject(Scale), 3);
        }

        protected internal override bool CheckAndHandleUpdateVersion(ushort type, uint version)
        {
            if (type == 1)
            {
                if (version > _positionUpdateVersion) { _positionUpdateVersion = version; return true; }
            }
            else if (type == 2)
            {
                if (version > _rotationUpdateVersion) { _rotationUpdateVersion = version; return true; }
            }
            else if (type == 3)
            {
                if (version > _scaleUpdateVersion) { _scaleUpdateVersion = version; return true; }
            }
            return false;
        }

        protected override void OnDespawned()
        {
            _interpolateToPositionQueue.Clear();
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            _interpolateToPositionQueue.Clear();
            _interpolateToPositionQueue = null;
        }

        protected override void OnNetworkInitialized() { }

        protected internal override void ReceiveData(ushort type, string json)
        {
            if (type == 1)
            {
                Vector2 position = JsonConvert.DeserializeObject<Vector2>(json);
                if (PositionNetworkSettings.Interpolate)
                    _interpolateToPositionQueue.Enqueue(position);
                else
                    LocalPosition = position;
            }
            else if (type == 2)
            {
                _interpolateToRotation = JsonConvert.DeserializeObject<float>(json);
                _interpolateFromRotation = LocalRotation;
                _rotationTimeReceived = Time.GameTime;
                if (!RotationNetworkSettings.Interpolate) LocalRotation = _interpolateToRotation;
            }
            else if (type == 3)
            {
                _interpolateToScale = JsonConvert.DeserializeObject<Vector2>(json);
                _interpolateFromScale = LocalScale;
                _scaleTimeReceived = Time.GameTime;
                if (!ScaleNetworkSettings.Interpolate) LocalScale = _interpolateToScale;
            }
        }

        protected internal override string ToJson()
        {
            return JsonConvert.SerializeObject(new NetworkTransformInitializationJson
            {
                Position = _lastSentPosition,
                Rotation = _lastRotation,
                Scale = _lastSentScale,
                PositionNetworkSettings = PositionNetworkSettings,
                RotationNetworkSettings = RotationNetworkSettings,
                ScaleNetworkSettings = ScaleNetworkSettings
            });
        }

        protected internal override void SetJson(string json)
        {
            NetworkTransformInitializationJson initJson = JsonConvert.DeserializeObject<NetworkTransformInitializationJson>(json);
            PositionNetworkSettings = initJson.PositionNetworkSettings;
            RotationNetworkSettings = initJson.RotationNetworkSettings;
            ScaleNetworkSettings = initJson.ScaleNetworkSettings;
            _interpolateToPosition = _interpolateFromPosition = LocalPosition = initJson.Position;
            _interpolateToRotation = _interpolateFromRotation = LocalRotation = initJson.Rotation;
            _interpolateToScale = _interpolateFromScale = LocalScale = initJson.Scale;
            _rotationTimeReceived = Time.GameTime;
            _scaleTimeReceived = Time.GameTime;
            _interpolatePositionTime = 0;
            _interpolateToPositionQueue.Clear();
        }
    }
}
