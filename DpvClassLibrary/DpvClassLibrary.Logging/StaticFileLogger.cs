using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace DpvClassLibrary.Logging
{
	public class StaticFileLogger : ILogger
	{
		private Random _rnd = new Random();

		public static StaticFileLogger Current
		{
			get;
			private set;
		} = new StaticFileLogger();


		public string LogFileDirectory
		{
			get;
			private set;
		}

		private string GetLogFilePath(string logfilePrefix = null)
		{
			if (!string.IsNullOrEmpty(logfilePrefix))
			{
				logfilePrefix += "_";
			}
			return Path.Combine(LogFileDirectory, string.Format("{0}Logfile_{1}.csv", logfilePrefix, DateTime.Now.ToString("yyyy-MM-dd")));
		}

		private string GetAlternativeLogFilePath(string logfilePrefix = null)
		{
			if (!string.IsNullOrEmpty(logfilePrefix))
			{
				logfilePrefix += "_";
			}
			return Path.Combine(LogFileDirectory, string.Format("{0}Logfile_{1}.csv", logfilePrefix, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_" + _rnd.Next(100000))));
		}

		protected StaticFileLogger()
		{
			LogFileDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}

		public void LogEvent(string eventSource, string title, string description, EventLogEntryType entryType)
		{
			LogEvent(eventSource, title, description, entryType, null);
			Console.WriteLine($"eventSource='{eventSource}', title='{title}', description='{description}', entryType='{entryType}'");
		}

		public void LogEvent(string eventSource, string title, string description, EventLogEntryType entryType, string logfilePrefix = null)
		{
			lock (new object())
			{
				string logFilePath = GetLogFilePath();
				try
				{
					if (!File.Exists(logFilePath))
					{
						File.AppendAllText(logFilePath, "Timestamp\tEventsource\tTitle\tDescription\tEntryType\n");
					}
					string text = string.Format("\"{0}\"\t\"{1}\"\t\"{2}\"\t\"{3}\"\t\"{4}\"\n", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), eventSource, title, description, entryType);
					Console.WriteLine(text);
					File.AppendAllText(logFilePath, text);
				}
				catch (Exception ex)
				{
					string alternativeLogFilePath = GetAlternativeLogFilePath();
					File.AppendAllText(alternativeLogFilePath, string.Format("\"{0}\"\t\"{1}\"\t\"{2}\"\t\"{3}\"\t\"{4}\"\n", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), eventSource, title, description, entryType));
					File.AppendAllText(alternativeLogFilePath, string.Format("\"{0}\"\t\"Logging\"\t\"unable to log\"\t\"{1}\"\t\"{2}\"\n", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), ex.Message, EventLogEntryType.Error));
				}
			}
		}
	}
}
