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
            public bool UsePosition;
            public bool UseRotation;
            public bool UseScale;
        }

        private static int _registerID;
        public static NetworkTransform FactoryMethod(string json)
        {
            NetworkTransform networkTransform = new NetworkTransform(new Transform(), -1);
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

        private uint _scaleUpdateVersion = 0;
        private uint _positionUpdateVersion = 0;
        private uint _rotationUpdateVersion = 0;

        private float _sendInterval;
        private float _currentIntervalTime = 0;

        private Vector2 _lastScale;
        private Vector2 _lastPosition;
        private float _lastRotation;
        private MessageSendMode _positionSendMode;
        private MessageSendMode _rotationSendMode;
        private MessageSendMode _scaleSendMode;
        private bool _sendPosition;
        private bool _sendRotation;
        private bool _sendScale;

        /// <summary>
        /// Creates a <see cref="NetworkTransform"/> object.
        /// </summary>
        /// <param name="transform">The transform to update through the network.</param>
        /// <param name="sendInterval">The send interval, in seconds. If set to zero, updates will be sent every frame.
        /// If set to -1, no automatic updates will be sent and updates will have to be sent manually using <see cref="SendUpdate(bool)"/></param>
        public NetworkTransform(Transform transform, float sendInterval,
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
            _sendInterval = MathF.Max(sendInterval, 0.05f);
            _positionSendMode = positionSendMode;
            _rotationSendMode = rotationSendMode;
            _scaleSendMode = scaleSendMode;
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
                    SendUpdate(true);
                }
                _currentIntervalTime += Time.DeltaTime;
            }
        }
        public override void FixedUpdate() { }

        public static void SetRegisterID(int registerID) => _registerID = registerID;
        public override int GetRegisterID() => _registerID;

        /// <summary>
        /// Sends a Transform update. Should only be used if send interval is set to -1, as that disables automatic Transform updates.
        /// </summary>
        public void SendUpdate(bool checkIfDirty = true)
        {
            if (checkIfDirty)
            {
                if (_sendPosition && Position != _lastPosition)
                {
                    _lastPosition = Position;
                    string json = JsonConvert.SerializeObject(Position);
                    Send(_positionSendMode, json, 1);
                }
                if (_sendRotation && Rotation != _lastRotation)
                {
                    _lastRotation = Rotation;
                    string json = JsonConvert.SerializeObject(Rotation);
                    Send(_rotationSendMode, json, 2);
                }
                if (_sendScale && Scale != _lastScale)
                {
                    _lastScale = Scale;
                    string json = JsonConvert.SerializeObject(Scale);
                    Send(_scaleSendMode, json, 3);
                }
            }
            else
            {
                if(_sendPosition)
                {
                    string jsonPosition = JsonConvert.SerializeObject(Position);
                    Send(_positionSendMode, jsonPosition, 1);
                }
                if(_sendRotation)
                {
                    string jsonRotation = JsonConvert.SerializeObject(Rotation);
                    Send(_rotationSendMode, jsonRotation, 2);
                }
                if(_sendScale)
                {
                    string jsonScale = JsonConvert.SerializeObject(Scale);
                    Send(_scaleSendMode, jsonScale, 3);
                }
            }
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

        public override void OnDespawned() { }

        public override void OnDisposed() { }

        public override void OnNetworkInitialized() { }

        public override void ReceiveData(ushort type, string json)
        {
            if(type == 1)
            {
                Transform.Position = JsonConvert.DeserializeObject<Vector2>(json);
            }
            else if(type == 2)
            {
                Transform.Rotation = JsonConvert.DeserializeObject<float>(json);
            }
            else if(type == 3)
            {
                Transform.Scale = JsonConvert.DeserializeObject<Vector2>(json);
            }
        }

        public override string ToJson()
        {
            NetworkTransformInitializationJson initJson = new NetworkTransformInitializationJson();
            initJson.Position = _lastPosition;
            initJson.Rotation = _lastRotation;
            initJson.Scale = _lastScale;
            initJson.UsePosition = _sendPosition;
            initJson.UseRotation = _sendRotation;
            initJson.UseScale = _sendScale;
            return JsonConvert.SerializeObject(initJson);
        }

        protected override void SetJson(string json)
        {
            Debug.Log(json);
            NetworkTransformInitializationJson initJson = JsonConvert.DeserializeObject<NetworkTransformInitializationJson>(json);
            if(initJson.UsePosition) Transform.Position = initJson.Position;
            if(initJson.UseRotation) Transform.Rotation = initJson.Rotation;
            if(initJson.UseScale) Transform.Scale = initJson.Scale;
        }
    }
}
