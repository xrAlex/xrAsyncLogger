using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private const string DebugString = "DEBUG";
        private const string InfoString = "INFO";
        private const string WarnString = "WARN";
        private const string ErrorString = "ERROR";
        private const string FatalString = "FATAL";
        private readonly object _locker = new ();

        private readonly AutoResetEvent _logResetEvent;
        private readonly ConcurrentQueue<LogArgs> _logsQueue;
        private bool _isDisposing;
        private readonly LoggerConfiguration _configuration;


        private void DeleteOldLogFiles()
        {
            var filesPath = Directory.GetFiles(_configuration.LogFilesDirectory);

            var logFiles =
                (from path in filesPath
                 where
                    !path.Contains("[ FATAL ]")
                    && path.Contains(_configuration.LogFileName)
                    && path.Contains(_configuration.LogFileFormat)
                 select new FileInfo(path)).ToList();

            if (logFiles.Count > _configuration.MaxLogFilesCount)
            {
                logFiles.Sort((x, y) => DateTime.Compare(x.CreationTime, y.CreationTime));
                logFiles.First().Delete();
            }
        }

        public Logger(LoggerConfiguration configuration)
        {
            _logsQueue = new ConcurrentQueue<LogArgs>();
            _logResetEvent = new AutoResetEvent(false);
            _configuration = configuration;

            Info("Starting logger...");

            DeleteOldLogFiles();

            var loggerThread = new Thread(ProcessLogs)
            {
                IsBackground = configuration.BackgroundLoggerThread!,
                Priority = configuration.LoggerThreadPriority,
                Name = "Simple Loggger"
            };

            loggerThread.Start();
        }

        /// <summary>
        /// Gets logs args from ConcurrentQueue and send on writing to file
        /// </summary>
        private void ProcessLogs()
        {
            while (!_isDisposing)
            {
                _logResetEvent.WaitOne();

                while (_logsQueue.TryDequeue(out var log))
                {
                    TryWriteLogToFile(log);
                    CheckFileSize();
                    DeleteOldLogFiles();
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

                using var fileStream = new StreamWriter(_configuration.LogFilePath, true, Encoding.UTF8);
                fileStream.Write(logMessage);

                if (_configuration.DuplicateInConsole)
                {
                    Console.WriteLine(logMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Logger FATAL: cannot write log to file!\n" + ex);
            }
        }

        /// <summary>
        /// Tries write fatal log to file
        /// </summary>
        private void TryWriteFatalLog(LogArgs log)
        {
            lock (_locker)
            {
                try
                {
                    var logMessage = log.ToString();

                    using var fileStream = new StreamWriter(
                        Path.Combine(
                            _configuration.LogFilesDirectory,
                            $"{_configuration.FatalLogFileName}" +
                            $" {DateTime.Now:yy.MM.dd HH.mm.ss ff}" +
                            $"{_configuration.LogFileFormat}"), true, Encoding.UTF8);

                    fileStream.Write(logMessage);

                    if (_configuration.DuplicateInConsole)
                    {
                        Console.WriteLine(logMessage);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Logger FATAL: cannot write log to file!\n" + ex);
                }
            }
        }

        /// <summary>
        /// Tries write log to log file
        /// </summary>
        private void CheckFileSize()
        {
            if (_configuration.MaxFileSizeInBytes > 0 && File.Exists(_configuration.LogFilePath))
            {
                var fileInfo = new FileInfo(_configuration.LogFilePath);
                if (fileInfo.Length > _configuration.MaxFileSizeInBytes)
                {
                    fileInfo.CopyTo($"{_configuration.LogFileName} " + $"{DateTime.Now:yy.MM.dd HH.mm.ss ff}" + $"{_configuration.LogFileFormat}");
                    fileInfo.Delete();
                }
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
    }
}