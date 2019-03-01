using System.Diagnostics;

namespace DpvClassLibrary.Logging
{
	public interface ILogger
	{
		void LogEvent(string eventSource, string title, string description, EventLogEntryType entryType);
	}
}
