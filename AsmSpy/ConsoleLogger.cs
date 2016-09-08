using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsmSpy
{
    public class ConsoleLogger : ILogger
    {
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
            Console.WriteLine(message);
        }

        public void LogWarning(string message)
        {
            WriteLine(message, ConsoleColor.Yellow);
        }
    }
}
