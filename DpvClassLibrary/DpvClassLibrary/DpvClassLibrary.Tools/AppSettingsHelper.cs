using System;
using System.Configuration;

namespace DpvClassLibrary.Tools
{
	public static class AppSettingsHelper
	{
		public static string GetValueFromAppSettings(string appSettingsKey)
		{
			string text = ConfigurationManager.AppSettings[appSettingsKey];
			if (string.IsNullOrEmpty(text))
			{
				throw new Exception($"Failure retrieving value! The key '{appSettingsKey}' was not found in appSettings of *.config file");
			}
			return text;
		}
	}
}
