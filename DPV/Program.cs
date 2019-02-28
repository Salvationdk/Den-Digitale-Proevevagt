using DpvClassLibrary.Logging;
using DpvClassLibrary.Tools;
using RestSharp.Contrib;
using System;
using System.Collections.Specialized;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;

namespace DPV
{
	internal static class Program
	{
		private static Mutex mutex = new Mutex(initiallyOwned: true, "{8F6F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F}");

		[STAThread]
		private static void Main()
		{
			ScreenCaptureTool.SetDpiAwareness();
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
			AppDomain.CurrentDomain.UnhandledException += NotifyOfUnhandledException;
			Application.ThreadException += NotifyOfUnhandledException;
			if (mutex.WaitOne(TimeSpan.Zero, exitContext: true))
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(defaultValue: false);
				NameValueCollection queryStringParameters = GetQueryStringParameters();
				string studentId = queryStringParameters["elevid"];
				Application.Run(new MainForm(studentId));
				mutex.ReleaseMutex();
			}
		}

		public static void NotifyOfUnhandledException(object sender, EventArgs e)
		{
			Exception ex = null;
			if (e is ThreadExceptionEventArgs)
			{
				ex = ((ThreadExceptionEventArgs)e).Exception;
			}
			else if (e is UnhandledExceptionEventArgs)
			{
				ex = (Exception)((UnhandledExceptionEventArgs)e).ExceptionObject;
			}
			else if (e is FirstChanceExceptionEventArgs)
			{
				ex = ((FirstChanceExceptionEventArgs)e).Exception;
			}
			string text = $"Unhandled Exception ({e.GetType().Name}) : Exception: '{ex.ToString()}'";
			StaticFileLogger.Current.LogEvent("DPV client", "Unhandled exception", text, EventLogEntryType.Error);
			MessageBox.Show($"DEBUG info:\n{text}", "Exception occurred", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			DumpToDesktop(text);
		}

		private static NameValueCollection GetQueryStringParameters()
		{
			NameValueCollection result = new NameValueCollection();
			try
			{
				if (ApplicationDeployment.IsNetworkDeployed)
				{
					if (ApplicationDeployment.CurrentDeployment.ActivationUri == null || ApplicationDeployment.CurrentDeployment.ActivationUri.Query == null)
					{
						return result;
					}
					string query = ApplicationDeployment.CurrentDeployment.ActivationUri.Query;
					result = HttpUtility.ParseQueryString(query);
				}
			}
			catch
			{
			}
			return result;
		}

		private static void DumpToDesktop(string textToLog)
		{
			string path = $"DPV_debuglog_{Guid.NewGuid().ToString()}.txt";
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
			string path2 = Path.Combine(folderPath, path);
			File.WriteAllText(path2, textToLog);
		}
	}
}
