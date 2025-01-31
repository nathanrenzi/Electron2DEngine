using Electron2D.Rendering;
using Newtonsoft.Json;
using System.Drawing;

namespace Electron2D.Networking.Examples
{
    public class ExampleNetworkGameClass : NetworkGameClass
    {
        [Serializable]
        private class ExampleJsonDataInitialization
        {
            public float Value;
            public float X;
            public float Y;

            public ExampleJsonDataInitialization(float value, float x, float y)
            {
                Value = value;
                X = x;
                Y = y;
            }
        }

        [Serializable]
        private class ExampleJsonDataUpdate1
        {
            public float X;
            public float Y;

            public ExampleJsonDataUpdate1(float x, float y)
            {
                X = x;
                Y = y;
            }
        }

        [Serializable]
        private class ExampleJsonDataUpdate2
        {
            public float Value;

            public ExampleJsonDataUpdate2(float value)
            {
                Value = value;
            }
        }

        public float Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                Send(Riptide.MessageSendMode.Reliable, JsonConvert.SerializeObject(new ExampleJsonDataUpdate2(_value)), 1);
            }
        }
        private float _value;
        private Sprite _sprite;
        private static int _registerID;

        public static ExampleNetworkGameClass FactoryMethod(string json)
        {
            ExampleNetworkGameClass exampleNetworkGameClass = new ExampleNetworkGameClass();
            exampleNetworkGameClass.SetJson(json);
            return exampleNetworkGameClass;
        }

        public ExampleNetworkGameClass()
        {
            _sprite = new Sprite(Material.CreateCircle(Color.Red));
        }

        public override void Update()
        {
            if (IsOwner && _sprite != null)
            {
                _sprite.Transform.Position = new System.Numerics.Vector2(MathF.Sin(Time.GameTime) * 500, MathF.Cos(Time.GameTime) * 500);
            }
        }
        public override void FixedUpdate()
        {
            if (IsOwner && _sprite != null)
            {
                Send(Riptide.MessageSendMode.Unreliable, JsonConvert.SerializeObject(
                    new ExampleJsonDataUpdate1(_sprite.Transform.Position.X, _sprite.Transform.Position.Y)), 0);
            }
        }

        public static void SetRegisterID(int registerID) => _registerID = registerID;
        public override int GetRegisterID() => _registerID;

        public override string ToJson()
        {
            float x = _sprite != null ? _sprite.Transform.Position.X : 0;
            float y = _sprite != null ? _sprite.Transform.Position.Y : 0;
            return JsonConvert.SerializeObject(new ExampleJsonDataInitialization(_value, x, y));
        }
        protected override void SetJson(string json)
        {
            ExampleJsonDataInitialization update = JsonConvert.DeserializeObject<ExampleJsonDataInitialization>(json);
            if(update != null)
            {
                _value = update.Value;
                if(_sprite != null)
                {
                    _sprite.Transform.Position = new System.Numerics.Vector2(update.X, update.Y);
                }
            }
        }
        public override void ReceiveData(ushort type, string json)
        {
            if(type == 0)
            {
                // Position update
                ExampleJsonDataUpdate1 update = JsonConvert.DeserializeObject<ExampleJsonDataUpdate1>(json);
                if(update != null)
                {
                    if (_sprite != null)
                    {
                        _sprite.Transform.Position = new System.Numerics.Vector2(update.X, update.Y);
                    }
                }
            }
            else if(type == 1)
            {
                // Value update
                ExampleJsonDataUpdate2 update = JsonConvert.DeserializeObject<ExampleJsonDataUpdate2>(json);
                if (update != null)
                {
                    _value = update.Value;
                    Debug.Log($"Example network game class updated value to {_value}");
                }
            }
        }
        public override bool CheckUpdateVersion(ushort type, uint version)
        {
            // type is not used as only one update type is used
            return version > UpdateVersion;
        }
        public override void OnNetworkInitialized()
        {
            Debug.Log("Example network game class initialized!");
        }
        public override void OnDespawned()
        {
            Debug.Log("Example network game class despawned!");
        }
        public override void OnDisposed()
        {
            Debug.Log("Example network game class disposed!");
            _sprite.Dispose();
        }
    }
}
