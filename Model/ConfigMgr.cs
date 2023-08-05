using System.Configuration;
using System;

namespace VCOM_WinUI.Model
{
	public static class ConfigMgr
	{	//TODO: ERROR Handling!
		public static string ReadSetting(string key)
		{
			try
			{
				var appSettings = ConfigurationManager.AppSettings;
				return appSettings[key] ?? "Not Found";
			}
			catch (ConfigurationErrorsException)
			{
				Console.WriteLine("Error reading app settings");
				return "[ERROR]";
			}
		}
		public static void AddUpdateAppSettings(string key, string value)
		{
			try
			{
				var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				var settings = configFile.AppSettings.Settings;
				if (settings[key] == null)
				{
					settings.Add(key, value);
				}
				else
				{
					settings[key].Value = value;
				}
				configFile.Save(ConfigurationSaveMode.Modified);
				ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
			}
			catch (ConfigurationErrorsException)
			{
				Console.WriteLine("Error writing app settings");
			}
		}
	}
}
//https://learn.microsoft.com/en-us/dotnet/api/system.configuration.configurationmanager