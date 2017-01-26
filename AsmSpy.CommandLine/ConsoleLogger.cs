using System;
using AsmSpy.Core;

namespace AsmSpy.CommandLine
{
    public class ConsoleLogger : ILogger
    {
        private readonly bool _showMessages;

        public ConsoleLogger(bool showMessages)
        {
            _showMessages = showMessages;
        }

        public virtual void WriteLine(string message, ConsoleColor color)
        {
            var restoreColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = restoreColor;
        }

        public virtual void LogError(string message)
        {
            WriteLine(message, ConsoleColor.Red);
        }

        public virtual void LogMessage(string message)
        {
            if (_showMessages)
            {
                Console.WriteLine(message);
            }
        }

        public virtual void LogWarning(string message)
        {
            WriteLine(message, ConsoleColor.Yellow);
        }
    }
}