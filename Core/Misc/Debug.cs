using Electron2D.Core;
using GLFW;

namespace Electron2D
{
    public static class Debug
    {
        public static bool DebuggingEnabled = true;
        public static bool EnableLogMessages = true;
        public static bool EnableErrorMessages = true;
        public static bool EnableCollapsing = true;

        private static string lastMessage = "";
        private static int messageCount = 1;
        private static bool hasLoggedMessage = false;

        /// <summary>
        /// Used to log formatted and togglable messages to the console. Ex: "Physics System Enabled!"
        /// </summary>
        /// <param name="_message"></param>
        public static void Log(string _message)
        {
            if (!DebuggingEnabled || !EnableLogMessages) return;

            Console.ForegroundColor = ConsoleColor.Gray;
            Write(_message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Used to log formatted and togglable error messages to the console that won't throw an exception.
        /// </summary>
        /// <param name="_message"></param>
        public static void LogError(string _message)
        {
            if (!DebuggingEnabled || !EnableErrorMessages) return;

            Console.ForegroundColor = ConsoleColor.Red;
            Write($"ERROR: {_message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void Write(string _message)
        {
            if (lastMessage.Equals(_message))
            {
                messageCount++;
                Console.Write($"\r[{GetTimeString()}] {_message} (x{messageCount})");
            }
            else
            {
                messageCount = 1;
                if (hasLoggedMessage) Console.WriteLine();
                Console.Write($"[{GetTimeString()}] {_message}");
            }

            lastMessage = _message;
            hasLoggedMessage = true;
        }

        private static string GetTimeString()
        {
            return TimeSpan.FromSeconds(Glfw.Time).ToString();
        }
    }
}
