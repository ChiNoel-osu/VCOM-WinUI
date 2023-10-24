using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
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
				MyInfoBar.IsOpen = true;
				MyInfoBar.Translation = new System.Numerics.Vector3(0, -60, 0);
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
				MyInfoBar.Translation = System.Numerics.Vector3.Zero;
				MyInfoBar.IsOpen = false;
			}
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			timer.Stop();
			DispatcherQueue.TryEnqueue(() => MyInfoBar.Translation = System.Numerics.Vector3.Zero);
			isInfoBarVisible = false;
		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{   //https://stackoverflow.com/questions/40114620/uwp-c-sharp-scroll-to-the-bottom-of-textbox
			if (((TextBox)sender).FocusState == Microsoft.UI.Xaml.FocusState.Unfocused)
			{   //Only scroll when unfocused.
				object scrViewer = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild((TextBox)sender, 0), 1);
				//Scroll to bottom unless user scrolled up manually.
				if (((ScrollViewer)scrViewer).ExtentHeight - ((ScrollViewer)scrViewer).VerticalOffset < ((TextBox)sender).ActualHeight + 40 || ((ScrollViewer)scrViewer).VerticalOffset == 0)
					((ScrollViewer)scrViewer).ChangeView(0.0f, ((ScrollViewer)scrViewer).ExtentHeight, 1.0f);
			}
		}
	}
}
