using Electron2D.Rendering;
using Newtonsoft.Json;
using System.Drawing;

namespace Electron2D.Networking.Examples
{
    public class ExampleNetworkGameClassPosition : NetworkGameClass
    {
        [System.Serializable]
        public class PositionUpdate
        {
            public float X;
            public float Y;

            public PositionUpdate(float x, float y)
            {
                X = y;
                Y = x;
            }
        }

        private Sprite _sprite;
        private static int _registerID;

        public static ExampleNetworkGameClassPosition FactoryMethod(string json)
        {
            ExampleNetworkGameClassPosition exampleNetworkGameClass = new ExampleNetworkGameClassPosition();
            exampleNetworkGameClass.SetJson(json);
            return exampleNetworkGameClass;
        }

        public ExampleNetworkGameClassPosition()
        {
            _sprite = new Sprite(Material.CreateCircle(Color.Red));
        }

        public override void Update()
        {
            if (IsOwner && _sprite != null)
            {
                _sprite.Transform.Position = Input.GetOffsetMousePosition();
            }
        }
        public override void FixedUpdate()
        {
            if (IsOwner && _sprite != null)
            {
                Send(Riptide.MessageSendMode.Unreliable, JsonConvert.SerializeObject(
                    new PositionUpdate(_sprite.Transform.Position.X, _sprite.Transform.Position.Y)), 0);
            }
        }

        public static void SetRegisterID(int registerID) => _registerID = registerID;
        public override int GetRegisterID() => _registerID;

        public override string ToJson()
        {
            float x = _sprite != null ? _sprite.Transform.Position.X : 0;
            float y = _sprite != null ? _sprite.Transform.Position.Y : 0;
            return JsonConvert.SerializeObject(new PositionUpdate(x, y));
        }
        protected override void SetJson(string json)
        {
            PositionUpdate update = JsonConvert.DeserializeObject<PositionUpdate>(json);
            if (update != null)
            {
                if (_sprite != null)
                {
                    _sprite.Transform.Position = new System.Numerics.Vector2(update.X, update.Y);
                }
            }
        }
        public override void ReceiveData(ushort type, string json)
        {
            PositionUpdate update = JsonConvert.DeserializeObject<PositionUpdate>(json);
            if (update != null)
            {
                if (_sprite != null)
                {
                    _sprite.Transform.Position = new System.Numerics.Vector2(update.X, update.Y);
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
