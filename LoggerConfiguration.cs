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
    public sealed class LoggerConfiguration
    {
        internal string LogFilePath { get; private set; }
        internal string FatalLogFileName { get; private set; }
        internal string LogFileName { get; private set; } = "Program";
        internal string LogFileFormat { get; private set; } = ".log";
        internal string LogFilesDirectory { get; private set; } = ".\\";
        internal int MaxLogFilesCount { get; private set; } = 10;
        internal ThreadPriority LoggerThreadPriority { get; private set; } = ThreadPriority.Lowest;
        internal long MaxFileSizeInBytes { get; private set; } = 1048576;
        internal bool DebugLogs { get; private set; } = true;
        internal bool DuplicateInConsole { get; private set; }
        internal bool BackgroundLoggerThread { get; private set; } = true;

        /// <summary>
        /// Disable logging debug logs in log file
        /// </summary>
        public LoggerConfiguration WriteDebugLogs(bool writeDebug)
        {
            DebugLogs = writeDebug;
            return this;
        }


        /// <summary>
        /// Directory for log files
        /// </summary>
        public LoggerConfiguration SetLogFilesDirectory(string logsPath)
        {
            LogFilesDirectory = logsPath;
            return this;
        }

        /// <summary>
        /// Name of log file
        /// </summary>
        public LoggerConfiguration SetLogFileName(string logFileName)
        {
            LogFileName = logFileName;
            return this;
        }

        /// <summary>
        /// Format of logs files (like .log)
        /// </summary>
        public LoggerConfiguration SetLogFileFormat(string logFileFormat)
        {
            LogFileFormat = logFileFormat;
            return this;
        }

        /// <summary>
        /// Maximum size of log file in MB (0 - unlimited)
        /// </summary>
        public LoggerConfiguration SetMaxLogFileSize(double size)
        {
            MaxFileSizeInBytes = (long)(size * 1048576);
            return this;
        }

        /// <summary>
        /// Sets priority of logger thread
        /// </summary>
        public LoggerConfiguration SetLoggerThreadPriority(ThreadPriority priority)
        {
            LoggerThreadPriority = priority;
            return this;
        }

        /// <summary>
        /// Forces logger thread to be background
        /// </summary>
        public LoggerConfiguration SetLoggerThreadBackground()
        {
            BackgroundLoggerThread = true;
            return this;
        }

        /// <summary>
        /// Max log files count
        /// </summary>
        public LoggerConfiguration SetLogFilesCleanup(int maxLogFiles)
        {
            MaxLogFilesCount = maxLogFiles;
            return this;
        }

        /// <summary>
        /// Duplicate log files in console window
        /// </summary>
        public LoggerConfiguration DuplicateLogsInConsole()
        {
            DuplicateInConsole = true;
            return this;
        }

        /// <summary>
        /// Build and start logger
        /// </summary>
        public Logger BuildLogger()
        {
            LogFilePath = Path.Combine(LogFilesDirectory, LogFileName + LogFileFormat);
            FatalLogFileName = $"[ FATAL ] {LogFileName}";

            return new Logger(this);
        }
    }
}
