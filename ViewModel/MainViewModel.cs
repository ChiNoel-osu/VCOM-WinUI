namespace VCOM_WinUI.ViewModel
{
	public class MainViewModel
	{
		public static MainViewModel Instance { get; } = new MainViewModel();
		public SettingsVM Settings { get; } = new SettingsVM();
		public MainCOMVM MainCOM { get; } = new MainCOMVM();
		public MultiCOMVM MultiCOM { get; } = new MultiCOMVM();
	}
}