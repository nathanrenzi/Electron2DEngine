using GLFW;

namespace Electron2D
{
    public static class Debug
    {
        public static bool DebuggingEnabled = true;
        public static bool EnableLogMessages = true;
        public static bool EnableErrorMessages = true;
        public static bool EnableWarningMessages = true;
        public static bool EnableCollapsing = true;

        private static string _lastMessage = "";
        private static int _messageCount = 1;
        private static bool _hasLoggedMessage = false;
        private static StreamWriter _logFile = null;

        /// <summary>
        /// Used to log formatted and togglable messages to the console and the latest log file.
        /// </summary>
        /// <param name="message"></param>
        public static void Log(object message, ConsoleColor messageColor = ConsoleColor.Gray)
        {
            if (!DebuggingEnabled || !EnableLogMessages) return;

            Console.ForegroundColor = messageColor;
            Write($"{message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Used to log formatted and togglable error messages to the console and the latest log file.
        /// </summary>
        /// <param name="message"></param>
        public static void LogError(object message)
        {
            if (!DebuggingEnabled || !EnableErrorMessages) return;

            Console.ForegroundColor = ConsoleColor.Red;
            Write($"ERROR: {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Used to log formatted and togglable warning messages to the console and the latest log file.
        /// </summary>
        /// <param name="message"></param>
        public static void LogWarning(object message)
        {
            if (!DebuggingEnabled || !EnableWarningMessages) return;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Write($"WARNING: {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void Write(string message)
        {
            if (_lastMessage.Equals(message) && EnableCollapsing)
            {
                _messageCount++;

                string m = $"\r[{GetTimeString()}] {message} (x{_messageCount})";
                Console.Write(m);
                _logFile?.Write(m);
            }
            else
            {
                _messageCount = 1;

                if (_hasLoggedMessage)
                {
                    Console.WriteLine();
                    _logFile?.WriteLine();
                }

                string m = $"[{GetTimeString()}] {message}";
                Console.Write(m);
                _logFile?.Write(m);
            }

            _lastMessage = message;
            _hasLoggedMessage = true;
        }

        private static string GetTimeString()
        {
            return TimeSpan.FromSeconds(Glfw.Time).ToString();
        }

        public static void OpenLogFile()
        {
            if(!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
            }
            _logFile = new StreamWriter(File.Create("Logs/latest.txt")); 
        }

        public static void CloseLogFile()
        {
            _logFile.Close();
            _logFile = null;
        }
    }
}
