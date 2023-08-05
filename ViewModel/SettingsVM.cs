using CommunityToolkit.Mvvm.ComponentModel;
using VCOM_WinUI.Model;
using Windows.UI.Popups;

namespace VCOM_WinUI.ViewModel
{
	public partial class SettingsVM : ObservableObject
	{
		[ObservableProperty]
		int _LanguageIndex = -1;
		readonly int currentLangIndex;

		partial void OnLanguageIndexChanged(int value)
		{
			if (value == currentLangIndex) return;
			switch (value)
			{
				case 0:
					
					break;
				case 1:

					break;
			}
		}
		public SettingsVM()
		{
			currentLangIndex = LanguageIndex = ConfigMgr.ReadSetting("Language") switch
			{
				"en-US" => 0,
				"zh-CN" => 1,
				_ => 0
			};
		}
	}
}