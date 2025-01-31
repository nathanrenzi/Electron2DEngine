using Electron2D.Rendering;
using Newtonsoft.Json;
using System.Drawing;

namespace Electron2D.Networking.Examples
{
    public class ExampleNetworkGameClass : NetworkGameClass
    {
        [Serializable]
        private class ExampleJsonDataUpdate
        {
            public float NewValue;

            public ExampleJsonDataUpdate(float newValue)
            {
                NewValue = newValue;
            }
        }

        public float Value
        {
            set
            {
                _value = value;
                Send(Riptide.MessageSendMode.Reliable, ToJson());
            }
            get
            {
                return _value;
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

        public ExampleNetworkGameClass(string networkID = "") : base(networkID)
        {
            _sprite = new Sprite(Material.CreateCircle(Color.Red));
        }

        public override void FixedUpdate() { }
        public override void Update() { }

        public static void SetRegisterID(int registerID) => _registerID = registerID;
        public override int GetRegisterID() => _registerID;

        public override string ToJson()
        {
            return JsonConvert.SerializeObject(new ExampleJsonDataUpdate(_value));
        }
        protected override void SetJson(string json)
        {
            ExampleJsonDataUpdate update = JsonConvert.DeserializeObject<ExampleJsonDataUpdate>(json);
            if(update != null)
            {
                _value = update.NewValue;
            }
            Debug.Log($"Example network game class value set to {_value}");
        }
        public override void ReceiveData(ushort type, string json)
        {
            // type is not used as only one update type is used
            SetJson(json); // In this example, receive data does the same thing as FromJson
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
            _sprite.Dispose();
        }
    }
}
