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

        public override void FromJson(string json)
        {
            // Json is used instead of state because json contains all data needed to initialize object,
            // state would contain extra data that is applied once object is initialized.
            // This doesn't matter in this example, but in more complicated objects having it separated might be useful.
            _value = JsonConvert.DeserializeObject<ExampleJsonDataUpdate>(json).NewValue;
        }

        public override void ReceiveData(ushort type, string json)
        {
            // type is not used here, only one update type is used
            _value = JsonConvert.DeserializeObject<ExampleJsonDataUpdate>(json).NewValue;
        }

        // Not used, inherited from IGameClass
        public override void Update() { }

        public static void SetRegisterID(int registerID) => _registerID = registerID;
        public override int GetRegisterID() => _registerID;

        // Not used, inherited from NetworkGameClass
        public override void OnNetworkInitialized() { }
    }
}
