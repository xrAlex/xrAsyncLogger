using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace xrAsyncLogger
{
    /// <summary>
    /// Simple thread save async logger
    /// </summary>
    public partial class Logger : IDisposable
    {
        private static bool _initialized;
        private const string DebugString = "DEBUG";
        private const string InfoString = "INFO";
        private const string WarnString = "WARN";
        private const string ErrorString = "ERROR";
        private const string FatalString = "FATAL";
        private readonly AutoResetEvent _logResetEvent;

        private readonly Encoding _encoding = Encoding.UTF8;
        private readonly ConcurrentQueue<LogArgs> _logsQueue;

        private bool _isDisposing;

        private readonly string _logFilePath;
        private readonly string _logFileName;
        private readonly string _logFileFormat;
        private readonly string _logFileDirectory;

        private readonly long _maxFileSizeInBytes;
        private readonly bool _enableDebugLogs;
        private readonly bool _duplicateInConsole;

        /// <summary>
        /// Initialize logger
        /// </summary>
        /// <param name="logFileDirectory"> Directory for log files</param>
        /// <param name="logFileName"> Name of log file</param>
        /// <param name="deleteOldLogsOnStart"> If true deletes old log files when logger initializing</param>
        /// <param name="maxLogFileSize"> Maximum size of log file in MB (0 - unlimited)</param>
        /// <param name="duplicateInConsole"> Duplicates logs in console</param>
        /// <param name="loggerThreadPriority"> Priority of logger Thread</param>
        /// <param name="enableDebugLogs"> Enable debug logs writing</param>
        /// <param name="backgroundLogThread"> Sets log thread IsBackground property</param>
        /// <param name="logFileFormat"> Log file format</param>
        public Logger(
            string logFileDirectory = null,
            string logFileName = null,
            string logFileFormat = null,
            bool deleteOldLogsOnStart = false,
            double maxLogFileSize = 10,
            bool duplicateInConsole = false,
            ThreadPriority loggerThreadPriority = ThreadPriority.BelowNormal,
            bool backgroundLogThread = true,
            bool enableDebugLogs = true)
        {
            if (_initialized) return;

            _logsQueue = new ConcurrentQueue<LogArgs>();
            _logResetEvent = new AutoResetEvent(false);

            Info("Starting logger...");
            _logFileDirectory = logFileDirectory ?? ".\\";
            _logFileName = logFileName ?? "Program";
            _logFileFormat = logFileFormat ?? ".log";

            _enableDebugLogs = enableDebugLogs;
            _duplicateInConsole = duplicateInConsole;
            _logFilePath = Path.Combine(_logFileDirectory, _logFileName + _logFileFormat);

            _maxFileSizeInBytes = (long)(maxLogFileSize * 1048576);

            if (deleteOldLogsOnStart) DeleteOldLogFiles(_logFileDirectory);

            var loggerThread = new Thread(ProcessWriteLog)
            {
                IsBackground = backgroundLogThread,
                Priority = loggerThreadPriority,
                Name = "LogThread"
            };
            loggerThread.Start();

            _initialized = true;
        }

        private void DeleteOldLogFiles(string directory)
        {
            foreach (var file in Directory.GetFiles(directory))
            {
                if (file.Contains("_xrLog_")) File.Delete(file);
            }
        }

        /// <summary>
        /// Dispose and release all resources
        /// </summary>
        public void Dispose()
        {
            Info("Stopping logger...");
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            _isDisposing = true;
            _logResetEvent.Set();
        }
        /// <summary>
        /// Gets logs args from ConcurrentQueue and send on writing to file
        /// </summary>
        private void ProcessWriteLog()
        {
            while (!_isDisposing)
            {
                _logResetEvent.WaitOne();

                while (_logsQueue.TryDequeue(out var log))
                {
                    TryWriteLogToFile(log);
                }
            }
        }

        /// <summary>
        /// Tries write log to log file
        /// </summary>
        private void TryWriteLogToFile(LogArgs log)
        {
            try
            {
                var logMessage = log.ToString();
                if (_duplicateInConsole) Console.WriteLine(logMessage);

                CheckFileSize();

                using var fileStream = new StreamWriter(_logFilePath, true, _encoding);
                fileStream.Write(logMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Logger FATAL: cannot write log to file!\n" + ex);
            }
        }

        /// <summary>
        /// Tries write log to log file
        /// </summary>
        private void CheckFileSize()
        {
            if (_maxFileSizeInBytes > 0 && File.Exists(_logFilePath))
            {
                var fileInfo = new FileInfo(_logFilePath);
                if (fileInfo.Length > _maxFileSizeInBytes)
                {
                    fileInfo.CopyTo($"{_logFileName}_xrLog_" + $"{DateTime.Now:yy.MM.dd_HH.mm.ss_ff}" + $"{_logFileFormat}");
                    fileInfo.Delete();
                }
            }
        }
    }
}