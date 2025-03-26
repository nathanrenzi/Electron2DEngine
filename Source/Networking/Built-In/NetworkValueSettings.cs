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
        public bool CheckIfDirtyBeforeSending;

        public NetworkValueSettings(bool sendAutomatically, float sendInterval, MessageSendMode messageSendMode,
            bool interpolate, bool checkIfDirtyBeforeSending)
        {
            SendAutomatically = sendAutomatically;
            SendInterval = sendInterval;
            MessageSendMode = messageSendMode;
            Interpolate = interpolate;
            CheckIfDirtyBeforeSending = checkIfDirtyBeforeSending;
        }
    }
}
