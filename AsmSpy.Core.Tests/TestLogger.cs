using System;
using Xunit.Abstractions;

namespace AsmSpy.Core.Tests
{
    public class TestLogger : ILogger
    {
        private readonly ITestOutputHelper output;

        public TestLogger(ITestOutputHelper output)
        {
            this.output = output ?? throw new ArgumentNullException(nameof(output));
        }

        public void LogError(string message)
        {
            output.WriteLine($"[ERROR] {message}");
        }

        public void LogMessage(string message)
        {
            output.WriteLine($"[LOG] {message}");
        }

        public void LogWarning(string message)
        {
            output.WriteLine($"[WARNING] {message}");
        }
    }
}
