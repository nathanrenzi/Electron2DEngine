using Riptide;

namespace Electron2D.Networking.Examples
{
    /// <summary>
    /// An instance of this must exist on the server for <see cref="ExampleNetworkGameClass2"/> to function properly.
    /// It passes all messages with id 3 to all other clients.
    /// </summary>
    public class Example2ServerMessageListener
    {
        public Example2ServerMessageListener()
        {
            NetworkManager.Instance.Server.MessageReceived += (sender, e) =>
            {
                if (e.MessageId != 3) return;
                Message message = Message.Create(MessageSendMode.Unreliable, 3);
                message.AddMessage(e.Message);
                NetworkManager.Instance.Server.SendToAll(message, e.FromConnection.Id);
            };
        }
    }
}
