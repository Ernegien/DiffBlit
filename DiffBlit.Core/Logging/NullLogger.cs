using System;

namespace DiffBlit.Core.Logging
{
    /// <summary>
    /// A logger that does nothing.
    /// </summary>
    public class NullLogger : LoggerBase
    {
        public override bool IsEnabled { get; set; } = false;
        public override LogLevel Level { get; set; } = LogLevel.Fatal;
        public override string Path { get; set; }

        public override void Log(LogLevel level, Exception exception, string message, params object[] parameters)
        {
            // do nothing
        }

        public override void Flush()
        {
            // do nothing
        }

        public override void Dispose()
        {
            // do nothing
        }
    }
}
