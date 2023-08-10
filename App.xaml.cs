// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using Microsoft.UI.Xaml;
using VCOM_WinUI.Model;

namespace VCOM_WinUI
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	public partial class App : Application
	{
		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
		{
			this.InitializeComponent();
			string lang;
			while ((lang = ConfigMgr.ReadSetting("Language")) == "Not Found")
				ConfigMgr.AddUpdateAppSettings("Language", "en-US");    //TODO: Log about this.
			System.Globalization.CultureInfo.CurrentUICulture = new System.Globalization.CultureInfo(lang);
		}

		/// <summary>
		/// Invoked when the application is launched.
		/// </summary>
		/// <param name="args">Details about the launch request and process.</param>
		protected override void OnLaunched(LaunchActivatedEventArgs args)
		{
			m_window = new MainWindow();
			m_window.AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 888, Height = 600 });
			m_window.Activate();
		}

		private Window m_window;
	}
}
