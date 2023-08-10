using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Windows.AppLifecycle;
using VCOM_WinUI.Model;

namespace VCOM_WinUI.ViewModel
{
	public partial class SettingsVM : ObservableObject
	{
		[ObservableProperty]
		int _LanguageIndex = -1;
		readonly int currentAppLangIndex = ConfigMgr.ReadSetting("Language") switch
		{
			"en-US" => 0,
			"zh-CN" => 1,
			_ => 0
		};
		[ObservableProperty]
		string _RestartInfoString = string.Empty;
		[ObservableProperty]
		string _RestartButtonString = string.Empty;
		[ObservableProperty]
		bool _RestartInfoEnabled = false;

		[RelayCommand]
		public void Restart()
		{
			AppInstance.Restart("LangChange");
		}

		partial void OnLanguageIndexChanged(int value)
		{
			switch (value)
			{
				case 0: //English en-US
					ConfigMgr.AddUpdateAppSettings("Language", "en-US");
					RestartInfoString = "Language setting is saved and will take effect after restarting the app.";
					RestartButtonString = "Restart APP";
					break;
				case 1: //Chinese zh-CN
					ConfigMgr.AddUpdateAppSettings("Language", "zh-CN");
					RestartInfoString = "语言设置已保存, 将在下次启动应用时生效.";
					RestartButtonString = "重启应用";
					break;
			}
			if (value == currentAppLangIndex)
			{
				RestartInfoEnabled = false;
				return;
			}
			RestartInfoEnabled = true;
		}
		public SettingsVM()
		{
			LanguageIndex = currentAppLangIndex;
		}
	}
}