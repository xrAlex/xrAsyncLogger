using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xrAsyncLogger
{
    public partial class Logger
    {
        /// <summary>
        /// Debug is used for internal system events that are not necessarily observable from the outside, but useful when determining how something happened.
        /// </summary>
        /// <param name="msg"> Message</param>
        public void Debug(string msg)
        {
            if (_configuration.DebugLogs)
            {
                Write(msg, DebugString);
            }
        }
        /// <summary>
        /// Information events describe things happening in the system that correspond to its responsibilities and functions. Generally these are the observable actions the system can perform.
        /// </summary>
        /// <param name="msg"> Message</param>
        public void Info(string msg)
            => Write(msg, InfoString);

        /// <summary>
        /// When service is degraded, endangered, or may be behaving outside of its expected parameters, Warning level events are used.
        /// </summary>
        /// <param name="msg"> Message</param>
        /// <param name="ex"> Exception</param>
        public void Warn(string msg, Exception ex = null)
            => Write(msg, WarnString, ex);

        /// <summary>
        /// When functionality is unavailable or expectations broken, an Error event is used.
        /// </summary>
        /// <param name="msg"> Message</param>
        /// <param name="ex"> Exception</param>
        public void Error(string msg, Exception ex = null)
            => Write(msg, ErrorString, ex);

        /// <summary>
        /// The most critical level, Fatal events demand immediate attention.
        /// </summary>
        /// <remarks>Immediately writes data to a separate file</remarks>
        /// <param name="msg"> Message</param>
        /// <param name="ex"> Exception</param>
        public void Fatal(string msg, Exception ex = null)
            => TryWriteFatalLog(new LogArgs(msg, FatalString, ex));

        /// <summary>
        /// Push to log
        /// </summary>
        /// <param name="msg"> Message</param>
        /// <param name="label"> Label</param>
        private void Write(string label, string msg, Exception ex = null)
        {
            var evArgs = new LogArgs(label, msg, ex);
            _logsQueue.Enqueue(evArgs);
            _logResetEvent.Set();
        }
    }
}
