using Electron2D.Rendering;
using Newtonsoft.Json;
using Riptide;

namespace Electron2D.Networking.Examples
{
    /// <summary>
    /// An example network game class that bypasses the built-in client messaging pipeline.
    /// </summary>
    public class ExampleNetworkGameClass2 : NetworkGameClass
    {
        public static int _registerID;
        public static ExampleNetworkGameClass2 FactoryMethod(string json)
        {
            ExampleNetworkGameClass2 exampleNetworkGameClass = new ExampleNetworkGameClass2();
            exampleNetworkGameClass.SetJson(json);
            return exampleNetworkGameClass;
        }

        public float Value;
        private Random random = new Random();

        public override void Update()
        {
            if(IsNetworkInitialized)
            {
                Value = (float)random.NextDouble();
                Message message = Message.Create(MessageSendMode.Unreliable, 3);
                message.AddFloat(Value);
                _client.Send(message);
            }
        }
        public override void FixedUpdate()
        {
        }

        public static void SetRegisterID(int registerID) => _registerID = registerID;
        public override int GetRegisterID() => _registerID;

        public override string ToJson()
        {
            return JsonConvert.SerializeObject(Value);
        }
        protected override void SetJson(string json)
        {
            Value = JsonConvert.DeserializeObject<float>(json);
        }
        public override void ReceiveData(ushort type, string json)
        {
            // Do nothing, messages are handled separately
        }
        public override bool CheckUpdateVersion(ushort type, uint version)
        {
            return version > UpdateVersion;
        }
        public override void OnNetworkInitialized()
        {
            if (IsOwner) return;
            _client.MessageReceived += (sender, e) =>
            {
                if (e.MessageId != 3) return;
                Value = e.Message.GetFloat();
                Debug.Log($"Value set to: {Value}!");
            };
        }
        public override void OnDespawned()
        {

        }
        public override void OnDisposed()
        {

        }
    }
}
