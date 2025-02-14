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
            public bool UsePositionInInit;
            public bool UseRotationInInit;
            public bool UseScaleInInit;
            public bool Interpolate;
            public float SendInterval;
        }

        private static int _registerID;
        public static NetworkTransform FactoryMethod(string json)
        {
            NetworkTransform networkTransform = new NetworkTransform(new Transform(), -1, false, false);
            networkTransform.SetJson(json);
            return networkTransform;
        }

        public Transform Transform { get; }
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
        public bool Interpolate { get; set; }
        public MessageSendMode PositionSendMode { get; set; }
        public MessageSendMode RotationSendMode { get; set; }
        public MessageSendMode ScaleSendMode { get; set; }

        private uint _scaleUpdateVersion = 0;
        private uint _positionUpdateVersion = 0;
        private uint _rotationUpdateVersion = 0;
        private float _sendInterval;
        private float _currentIntervalTime = 0;
        private Queue<Vector2> _toPositionQueue = new Queue<Vector2>();
        private Vector2 _toPosition;
        private float _toRotation;
        private Vector2 _toScale;
        private Vector2 _fromPosition;
        private float _fromRotation;
        private Vector2 _fromScale;
        private float _positionTime;
        private float _rotationTimeReceived;
        private float _scaleTimeReceived;
        private Vector2 _lastSentScale;
        private Vector2 _lastSentPosition;
        private float _lastRotation;
        private bool _sendPosition;
        private bool _sendRotation;
        private bool _sendScale;
        private bool _checkIfDirtyBeforeSending;

        /// <summary>
        /// Creates a <see cref="NetworkTransform"/> object.
        /// </summary>
        /// <param name="transform">The transform to update through the network.</param>
        /// <param name="sendInterval">The send interval, in seconds. If set to zero, updates will be sent every frame.
        /// If set to -1, no automatic updates will be sent and updates will have to be sent manually using <see cref="SendUpdate(bool)"/></param>
        public NetworkTransform(Transform transform, float sendInterval, bool interpolate, bool checkIfDirtyBeforeSending = false,
            MessageSendMode positionSendMode = MessageSendMode.Unreliable,
            MessageSendMode rotationSendMode = MessageSendMode.Unreliable,
            MessageSendMode scaleSendMode = MessageSendMode.Unreliable,
            bool sendPosition = true, bool sendRotation = true, bool sendScale = true)
        {
            Transform = transform;
            if(sendInterval == -1)
            {
                _sendInterval = -1;
                return;
            }
            Interpolate = interpolate;
            _checkIfDirtyBeforeSending = checkIfDirtyBeforeSending;
            _sendInterval = MathF.Max(sendInterval, 0.05f);
            PositionSendMode = positionSendMode;
            RotationSendMode = rotationSendMode;
            ScaleSendMode = scaleSendMode;
            _sendPosition = sendPosition;
            _sendRotation = sendRotation;
            _sendScale = sendScale;
        }

        public override void Update()
        {
            if(IsOwner && IsNetworkInitialized && _sendInterval != -1)
            {
                if (_currentIntervalTime >= _sendInterval)
                {
                    SendUpdate(_checkIfDirtyBeforeSending);
                    _currentIntervalTime -= _sendInterval;
                }
                _currentIntervalTime += Time.DeltaTime;
            }
            else if(!IsOwner && IsNetworkInitialized && Interpolate)
            {
                InterpolatePosition();
                InterpolateRotation();
                InterpolateScale();
            }
        }

        private void InterpolatePosition()
        {
            while (_positionTime >= _sendInterval && _toPositionQueue.Count > 0)
            {
                _fromPosition = _toPosition;
                _toPosition = _toPositionQueue.Dequeue();
                _positionTime -= _sendInterval;
            }

            float pt = _positionTime / _sendInterval;
            Transform.Position = Vector2.Lerp(_fromPosition, _toPosition, MathF.Min(pt, 1));
            _positionTime += Time.DeltaTime + Time.DeltaTime * _toPositionQueue.Count * _toPositionQueue.Count;
        }

        private void InterpolateRotation()
        {
            float rt = (Time.GameTime - _rotationTimeReceived) / _sendInterval;
            if (rt <= 1)
            {
                Transform.Rotation = (_fromRotation * (1f - rt)) + (_toRotation * rt);
            }
            else
            {
                Transform.Rotation = _toRotation;
            }
        }

        private void InterpolateScale()
        {
            float st = (Time.GameTime - _scaleTimeReceived) / _sendInterval;
            if (st <= 1)
            {
                Transform.Scale = Vector2.Lerp(_fromScale, _toScale, st);
            }
            else
            {
                Transform.Scale = _toScale;
            }
        }

        public override void FixedUpdate() { }

        public static void SetRegisterID(int registerID) => _registerID = registerID;
        public override int GetRegisterID() => _registerID;

        /// <summary>
        /// Manually sends a Transform update.
        /// </summary>
        public void SendUpdate(bool checkIfDirty = true)
        {
            if (checkIfDirty)
            {
                if (_sendPosition && Position != _lastSentPosition)
                {
                    SendPositionUpdate();
                }
                if (_sendRotation && Rotation != _lastRotation)
                {
                    SendRotationUpdate();
                }
                if (_sendScale && Scale != _lastSentScale)
                {
                    SendScaleUpdate();
                }
            }
            else
            {
                if(_sendPosition)
                {
                    SendPositionUpdate();
                }
                if(_sendRotation)
                {
                    SendRotationUpdate();
                }
                if(_sendScale)
                {
                    SendScaleUpdate();
                }
            }
        }

        /// <summary>
        /// Manually sends a position update.
        /// </summary>
        public void SendPositionUpdate()
        {
            _lastSentPosition = Position;
            string json = JsonConvert.SerializeObject(Position);
            Send(PositionSendMode, json, 1);
        }

        /// <summary>
        /// Manually sends a rotation update.
        /// </summary>
        public void SendRotationUpdate()
        {
            _lastRotation = Rotation;
            string json = JsonConvert.SerializeObject(Rotation);
            Send(RotationSendMode, json, 2);
        }

        /// <summary>
        /// Manually sends a scale update;
        /// </summary>
        public void SendScaleUpdate()
        {
            _lastSentScale = Scale;
            string json = JsonConvert.SerializeObject(Scale);
            Send(ScaleSendMode, json, 3);
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
            _toPositionQueue.Clear();
        }

        public override void OnDisposed()
        {
            _toPositionQueue.Clear();
            _toPositionQueue = null;
        }

        public override void OnNetworkInitialized() { }

        public override void ReceiveData(ushort type, string json)
        {
            if(type == 1)
            {
                Vector2 position = JsonConvert.DeserializeObject<Vector2>(json);
                if (Interpolate)
                {
                    _toPositionQueue.Enqueue(position);
                }
                else 
                {
                    Transform.Position = position;
                }
            }
            else if(type == 2)
            {
                _toRotation = JsonConvert.DeserializeObject<float>(json);
                _fromRotation = Transform.Rotation;
                _rotationTimeReceived = Time.GameTime;
                if (!Interpolate) Transform.Rotation = _toRotation;
            }
            else if(type == 3)
            {
                _toScale = JsonConvert.DeserializeObject<Vector2>(json);
                _fromScale = Transform.Scale;
                _scaleTimeReceived = Time.GameTime;
                if (!Interpolate) Transform.Scale = _toScale;
            }
        }

        public override string ToJson()
        {
            NetworkTransformInitializationJson initJson = new NetworkTransformInitializationJson();
            initJson.Position = _lastSentPosition;
            initJson.Rotation = _lastRotation;
            initJson.Scale = _lastSentScale;
            initJson.UsePositionInInit = _sendPosition;
            initJson.UseRotationInInit = _sendRotation;
            initJson.UseScaleInInit = _sendScale;
            initJson.Interpolate = Interpolate;
            initJson.SendInterval = _sendInterval;
            return JsonConvert.SerializeObject(initJson);
        }

        protected override void SetJson(string json)
        {
            NetworkTransformInitializationJson initJson = JsonConvert.DeserializeObject<NetworkTransformInitializationJson>(json);
            if(initJson.UsePositionInInit) Transform.Position = initJson.Position;
            if(initJson.UseRotationInInit) Transform.Rotation = initJson.Rotation;
            if(initJson.UseScaleInInit) Transform.Scale = initJson.Scale;
            Interpolate = initJson.Interpolate;
            _sendInterval = initJson.SendInterval;
            _toPosition = _fromPosition = Transform.Position;
            _toRotation = _fromRotation = Transform.Rotation;
            _toScale = _fromScale = Transform.Scale;
            _rotationTimeReceived = Time.GameTime;
            _scaleTimeReceived = Time.GameTime;
            _positionTime = 0;
            _toPositionQueue.Clear();
        }
    }
}
