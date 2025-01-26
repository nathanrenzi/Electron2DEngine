using Newtonsoft.Json;

namespace Electron2D.Networking.Examples
{
    public class ExampleNetworkGameClass : NetworkGameClass
    {
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
                SendData(Riptide.MessageSendMode.Reliable, GetJson());
            }
            get
            {
                return _value;
            }
        }
        private float _value;
        private static int _registerID;

        public static ExampleNetworkGameClass FactoryMethod(string json)
        {
            ExampleNetworkGameClass exampleNetworkGameClass = new ExampleNetworkGameClass();
            exampleNetworkGameClass.FromJson(json);
            return exampleNetworkGameClass;
        }

        private string GetJson()
        {
            return JsonConvert.SerializeObject(new ExampleJsonDataUpdate(_value));
        }

        // Not used, inherited from IGameClass
        public override void FixedUpdate() { }

        public override string ToJson()
        {
            // Not used in this example
            return JsonConvert.SerializeObject(new ExampleJsonDataUpdate(_value));
        }

        protected override void FromJson(string json)
        {
            _value = JsonConvert.DeserializeObject<ExampleJsonDataUpdate>(json).NewValue;
            Debug.Log($"Example network game class value initialized to {_value}");
        }

        public override void ReceiveData(ushort type, string json)
        {
            // type is not used as only one update type is used
            _value = JsonConvert.DeserializeObject<ExampleJsonDataUpdate>(json).NewValue;
            Debug.Log($"Example network game class value set to {_value}");
        }

        // Not used, inherited from IGameClass
        public override void Update() { }

        public static void SetRegisterID(int registerID) => _registerID = registerID;
        public override int GetRegisterID() => _registerID;

        public override void OnNetworkInitialized()
        {
            Debug.Log("Example network game class initialized!");
        }

        public override bool CheckUpdateVersion(ushort type, uint version)
        {
            // type is not used as only one update type is used
            if(version > _updateVersion)
            {
                _updateVersion = version;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
