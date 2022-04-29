using System;

namespace xrAsyncLogger
{
    public readonly struct LogArgs
    {
        /// <summary>
        /// Message to write in log
        /// </summary>
        private readonly string _message;

        /// <summary>
        /// Log mark
        /// </summary>
        private readonly string _label;

        /// <summary>
        /// Log time
        /// </summary>
        private readonly DateTime _time;

        /// <summary>
        /// Log exception
        /// </summary>
        private readonly Exception _ex;

        public override string ToString()
        {
            var logMessage = $"{_label} " + $"[{_time:dd.MM.yy HH:mm:ss fff}] {_message} \r\n";
            if (_ex != null)
            {
                logMessage += $"[{_ex.TargetSite?.DeclaringType}.{_ex.TargetSite?.Name}()] " +
                              $"{_ex.Message}]\n" +
                              "[StackTrace]\n" +
                              $"{_ex.StackTrace}\r\n";
            }

            return logMessage;
        }

        /// <summary>
        /// New log object
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="label">Mark</param>
        public LogArgs(string message, string label, Exception ex = null)
        {
            _label = label;
            _message = message;
            _ex = ex;
            _time = DateTime.Now;
        }
    }
}
