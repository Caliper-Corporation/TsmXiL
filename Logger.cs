using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TsmXiL
{
    public class Logger
    {
        private readonly ReaderWriterLock _locker = new ReaderWriterLock();

        public Logger(string logFile, string dataFile = null)
        {
            LogFile = !string.IsNullOrEmpty(logFile) ? logFile : "log.txt";
            DataFile = !string.IsNullOrEmpty(dataFile) ? dataFile : "data.csv";
        }

        public string LogFile { get; private set; }
        private string DataFile { get; }

        public void Info(string msg, string type = Constants.INFO)
        {
            WriteToLogFile(msg, type);
        }

        private void WriteToLogFile(string msg, string type)
        {
            var now = DateTime.Now.ToString("HH:mm:ss");
            var logType = type == Constants.INFO ? string.Empty : $"[{type}]";
            msg = $"[{now}]{logType} {msg}";
            if (Debugger.IsAttached)
            {
                Console.WriteLine(msg);
            }

            File.AppendAllLines(LogFile, new[] { msg });
        }

        public void Error(string msg)
        {
            WriteToLogFile(msg, Constants.ERROR);
        }

        public void Data(string data)
        {
            if (string.IsNullOrEmpty(DataFile))
            {
                throw new Exception("Data file is required and has not been specified.");
            }

            try
            {
                File.AppendAllLines(DataFile, new[] { data });
            }
            catch (Exception exception)
            {
                Error(exception.Message);
                throw;
            }
        }

        internal void Close()
        {
            LogFile = null;
        }
    }

    internal static class Constants
    {
        public const string INFO = "INFO";
        public const string ERROR = "ERROR";
    }
}