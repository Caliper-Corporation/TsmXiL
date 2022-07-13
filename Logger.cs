using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace TsmXiL
{
    internal class Logger
    {
        private readonly ReaderWriterLock _locker = new ReaderWriterLock();

        public Logger(string logFilePath = null, Stopwatch stopwatch = null)
        {
            LogFilePath = !string.IsNullOrEmpty(logFilePath) ? logFilePath : "log.txt";
            Watch = stopwatch;
        }

        public string LogFilePath { get; private set; }
        private Stopwatch Watch { get; }

        public void Log(string msg, string type = Constants.INFO)
        {
            var now = DateTime.Now.ToString("G");
            var time = Watch == null || !Watch.IsRunning ? string.Empty : $"({Watch.ElapsedMilliseconds} ms)";
            msg = $"[{now}][{type}] {msg} {time}";
            if (Debugger.IsAttached) Console.WriteLine(msg);

            try
            {
                _locker.AcquireWriterLock(5000);
                File.AppendAllLines(LogFilePath, new[] { msg });
            }
            finally
            {
                _locker.ReleaseWriterLock();
            }
        }

        public void Error(string msg)
        {
            Log(msg, Constants.ERROR);
        }

        internal void Close()
        {
            LogFilePath = null;
        }
    }

    internal static class Constants
    {
        public const string INFO = "INFO";
        public const string ERROR = "ERROR";
    }
}