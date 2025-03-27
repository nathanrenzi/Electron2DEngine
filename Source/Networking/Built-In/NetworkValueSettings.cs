using Riptide;

namespace Electron2D.Networking
{
    [Serializable]
    public struct NetworkValueSettings
    {
        public bool SendAutomatically;
        public float SendInterval;
        public MessageSendMode MessageSendMode;
        public bool Interpolate;

        public NetworkValueSettings(bool sendAutomatically, float sendInterval, MessageSendMode messageSendMode,
            bool interpolate)
        {
            SendAutomatically = sendAutomatically;
            SendInterval = sendInterval;
            MessageSendMode = messageSendMode;
            Interpolate = interpolate;
        }
    }
}
