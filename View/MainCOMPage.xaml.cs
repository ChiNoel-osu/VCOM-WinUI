using Microsoft.UI.Xaml.Controls;
using System.Timers;
using VCOM_WinUI.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VCOM_WinUI.View
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainCOMPage : Page
	{
		Timer timer = new Timer();
		bool isInfoBarVisible = false;

		public MainCOMPage()
		{
			this.InitializeComponent();
		}

		private void COMDeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			((MainViewModel)DataContext).MainCOM.SelectedCOMChanged();
		}
		private void COMDevice_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
		{
			((MainViewModel)DataContext).MainCOM.TogglePort(((UserControl)sender).Tag.ToString());
		}

		private void MyInfoBar_IsEnabledChanged(object sender, Microsoft.UI.Xaml.DependencyPropertyChangedEventArgs e)
		{   //Implicit Animation
			if ((bool)e.NewValue)
			{   //Unable to open port.
				((InfoBar)sender).IsOpen = true;
				((InfoBar)sender).Translation = new System.Numerics.Vector3(0, -60, 0);
				if (!isInfoBarVisible)
				{
					isInfoBarVisible = true;
					timer.Interval = 4000;
					timer.Start();
					timer.Elapsed += Timer_Elapsed;
				}
			}
			else
			{
				((InfoBar)sender).Translation = System.Numerics.Vector3.Zero;
				((InfoBar)sender).IsOpen = false;
			}
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			timer.Stop();
			DispatcherQueue.TryEnqueue(() => MyInfoBar.Translation = System.Numerics.Vector3.Zero);
			isInfoBarVisible = false;
		}
	}
}
