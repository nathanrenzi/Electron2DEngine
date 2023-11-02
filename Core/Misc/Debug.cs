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
        private static StreamWriter logFile = null;

        /// <summary>
        /// Used to log formatted and togglable messages to the console and the latest log file.
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
        /// Used to log formatted and togglable error messages to the console and the latest log file.
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

                string m = $"\r[{GetTimeString()}] {_message} (x{messageCount})";
                Console.Write(m);
                logFile?.Write(m);
            }
            else
            {
                messageCount = 1;

                if (hasLoggedMessage)
                {
                    Console.WriteLine();
                    logFile?.WriteLine();
                }

                string m = $"[{GetTimeString()}] {_message}";
                Console.Write(m);
                logFile?.Write(m);
            }

            lastMessage = _message;
            hasLoggedMessage = true;
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
            logFile = new StreamWriter(File.Create("Logs/latest.txt")); 
        }

        public static void CloseLogFile()
        {
            logFile.Close();
            logFile = null;
        }
    }
}
