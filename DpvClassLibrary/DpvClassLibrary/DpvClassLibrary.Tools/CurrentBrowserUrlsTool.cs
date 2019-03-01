using DpvClassLibrary.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows.Automation;

namespace DpvClassLibrary.Tools
{
	public static class CurrentBrowserUrlsTool
	{
		public enum BrowserType
		{
			FIREFOX,
			INTERNET_EXPLORER,
			GOOGLE_CHROME,
			MICROSOFT_EDGE,
			Empty
		}

		private static Timer _timer = new Timer(20000.0);

		private static HashSet<string> _urlSet = new HashSet<string>();

		private static Dictionary<string, BrowserType> _processSearchStringsForBrowsertypes = new Dictionary<string, BrowserType>
		{
			{
				"chrome",
				BrowserType.GOOGLE_CHROME
			},
			{
				"iexplore",
				BrowserType.INTERNET_EXPLORER
			},
			{
				"firefox",
				BrowserType.FIREFOX
			},
			{
				"applicationframehost",
				BrowserType.MICROSOFT_EDGE
			}
		};

		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", SetLastError = true)]
		private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

		public static void Start()
		{
			_timer.Elapsed += _timer_Elapsed;
			_timer.Enabled = true;
		}

		public static void Stop()
		{
			if (_timer != null)
			{
				_timer.Enabled = false;
			}
		}

		private static void _timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			try
			{
				Process[] processes = Process.GetProcesses();
				foreach (KeyValuePair<string, BrowserType> browserPair in _processSearchStringsForBrowsertypes)
				{
					IEnumerable<Process> enumerable = from p in processes
					where p.ProcessName.ToLower().Contains(browserPair.Key)
					select p;
					foreach (Process item in enumerable)
					{
						if (!(item.MainWindowHandle == IntPtr.Zero))
						{
							string uRLFromProcess = GetURLFromProcess(item, browserPair.Value, item.MainWindowHandle);
							if (!string.IsNullOrEmpty(uRLFromProcess))
							{
								StaticFileLogger.Current.LogEvent("CurrentBrowserUrlsTool._timer_Elapsed()", "Url harvested", uRLFromProcess, EventLogEntryType.Information);
								_urlSet.Add(uRLFromProcess);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				StaticFileLogger.Current.LogEvent("CurrentBrowserUrlsTool._timer_Elapsed()", "Exception harvesting URL", ex.ToString(), EventLogEntryType.Error);
			}
		}

		public static IEnumerable<string> GetHarvestedUrlsFromRunningProcessesAndEmptyTheList()
		{
			List<string> result = _urlSet.ToList();
			_urlSet.Clear();
			return result;
		}

		private static BrowserType Parse(string processName)
		{
			processName = processName.ToLower();
			if (processName.Contains("chrome"))
			{
				return BrowserType.GOOGLE_CHROME;
			}
			if (processName.Contains("applicationframehost"))
			{
				return BrowserType.MICROSOFT_EDGE;
			}
			if (processName.Contains("iexplore"))
			{
				return BrowserType.INTERNET_EXPLORER;
			}
			if (processName.Contains("firefox"))
			{
				return BrowserType.FIREFOX;
			}
			return BrowserType.Empty;
		}

		private static string GetURLFromProcess(Process process, BrowserType browser, IntPtr handle)
		{
			if (process == null)
			{
				throw new ArgumentNullException("process");
			}
			if (process.MainWindowHandle == IntPtr.Zero)
			{
				return null;
			}
			AutomationElement automationElement = null;
			try
			{
				automationElement = AutomationElement.FromHandle(handle);
			}
			catch (Exception ex)
			{
				StaticFileLogger.Current.LogEvent("CurrentBrowswerUrlsTool.GetUrlFromProcess()", "Error getting AutomationElement.FromHandle()", ex.ToString(), EventLogEntryType.Error);
			}
			if (automationElement == null)
			{
				return null;
			}
			if (browser.Equals(BrowserType.GOOGLE_CHROME))
			{
				AutomationElement automationElement2 = null;
				try
				{
					AutomationElement automationElement3 = automationElement.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Google Chrome"));
					if (automationElement3 == null)
					{
						return null;
					}
					AutomationElement lastChild = TreeWalker.RawViewWalker.GetLastChild(automationElement3);
					AutomationElement automationElement4 = lastChild.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Pane));
					AutomationElement automationElement5 = automationElement4.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Pane));
					automationElement2 = automationElement5.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AccessKeyProperty, "Ctrl+L"));
					if (automationElement2 == null)
					{
						automationElement2 = automationElement3.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AccessKeyProperty, "Ctrl+L"));
					}
				}
				catch
				{
					return null;
				}
				if (automationElement2 != null)
				{
					if ((bool)automationElement2.GetCurrentPropertyValue(AutomationElement.HasKeyboardFocusProperty))
					{
						return null;
					}
					AutomationPattern[] supportedPatterns = automationElement2.GetSupportedPatterns();
					if (supportedPatterns.Length == 1)
					{
						string text = "";
						try
						{
							text = ((ValuePattern)automationElement2.GetCurrentPattern(supportedPatterns[0])).Current.Value;
						}
						catch
						{
						}
						if (text != "" && Regex.IsMatch(text, "^(https:\\/\\/)?[a-zA-Z0-9\\-\\.]+(\\.[a-zA-Z]{2,4}).*$"))
						{
							if (!text.StartsWith("http"))
							{
								text = "http://" + text;
							}
							return text;
						}
						return null;
					}
				}
				return null;
			}
			if (browser.Equals(BrowserType.FIREFOX))
			{
				try
				{
					AutomationElementCollection automationElementCollection = automationElement.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ToolBar));
					foreach (AutomationElement item in automationElementCollection)
					{
						AutomationElementCollection automationElementCollection2 = item.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
						foreach (AutomationElement item2 in automationElementCollection2)
						{
							if ((bool)item2.GetCurrentPropertyValue(AutomationElement.HasKeyboardFocusProperty))
							{
								return null;
							}
							string text2 = ((ValuePattern)item2.GetCurrentPattern(ValuePattern.Pattern)).Current.Value;
							if (text2 != "")
							{
								if (Regex.IsMatch(text2, "^(https:\\/\\/)?[a-zA-Z0-9\\-\\.]+(\\.[a-zA-Z]{2,4}).*$"))
								{
									if (!text2.StartsWith("http"))
									{
										text2 = "http://" + text2;
									}
									return text2;
								}
								return text2;
							}
						}
					}
				}
				catch
				{
				}
			}
			if (browser.Equals(BrowserType.INTERNET_EXPLORER))
			{
				AutomationElement automationElement8 = automationElement.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, "ReBarWindow32"));
				if (automationElement8 == null)
				{
					return null;
				}
				AutomationElement automationElement9 = automationElement8.FindFirst(TreeScope.Subtree, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
				if (automationElement9 != null)
				{
					string text3 = ((ValuePattern)automationElement9.GetCurrentPattern(ValuePattern.Pattern)).Current.Value;
					if (text3 != "" && Regex.IsMatch(text3, "^(https:\\/\\/)?[a-zA-Z0-9\\-\\.]+(\\.[a-zA-Z]{2,4}).*$"))
					{
						if (!text3.StartsWith("http"))
						{
							text3 = "http://" + text3;
						}
						return text3;
					}
				}
			}
			if (browser.Equals(BrowserType.MICROSOFT_EDGE))
			{
				AutomationElement automationElement10 = automationElement.FindFirst(TreeScope.Children, new AndCondition(new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window), new PropertyCondition(AutomationElement.NameProperty, "Microsoft Edge")));
				if (automationElement10 == null)
				{
					return null;
				}
				AutomationElement automationElement11 = automationElement10.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.AutomationIdProperty, "addressEditBox"));
				if (automationElement11 != null)
				{
					if ((bool)automationElement11.GetCurrentPropertyValue(AutomationElement.HasKeyboardFocusProperty))
					{
						return null;
					}
					string text4 = ((TextPattern)automationElement11.GetCurrentPattern(TextPattern.Pattern)).DocumentRange.GetText(int.MaxValue);
					if (text4 != "" && Regex.IsMatch(text4, "^(https:\\/\\/)?[a-zA-Z0-9\\-\\.]+(\\.[a-zA-Z]{2,4}).*$"))
					{
						if (!text4.StartsWith("http"))
						{
							text4 = "http://" + text4;
						}
						return text4;
					}
					return text4;
				}
			}
			return null;
		}
	}
}
