using System;
using System.Diagnostics;

namespace Microsoft.Alm.Cli
{
    public class Win32Logger : ILogger
    {
        public void LogEvent(Program program, string message, string eventTypeName)
        {
            if (program is null)
                throw new ArgumentNullException(nameof(program));
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            if (eventTypeName is null)
                throw new ArgumentNullException(nameof(eventTypeName));

            /*** try-squelch due to UAC issues which require a proper installer to work around ***/

            program.Trace.WriteLine(message);

            try
            {
                var eventType = (EventLogEntryType)Enum.Parse(typeof(EventLogEntryType), eventTypeName);
                EventLog.WriteEntry(Program.EventSource, message, eventType);
            }
            catch { /* squelch */ }
        }
    }
}