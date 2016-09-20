using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsmSpy
{
    public class ConsoleLogger : ILogger
    {
        bool _ShowMessages;

        public ConsoleLogger(bool showMessages)
        {
            _ShowMessages = showMessages; 
        }

        void WriteLine(string message, ConsoleColor color)
        {
            var restoreColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = restoreColor;
        }

        public void LogError(string message)
        {
            WriteLine(message, ConsoleColor.Red);
        }

        public void LogMessage(string message)
        {
            if (_ShowMessages)
            {
                Console.WriteLine(message);
            }
        }

        public void LogWarning(string message)
        {
            WriteLine(message, ConsoleColor.Yellow);
        }
    }
}
