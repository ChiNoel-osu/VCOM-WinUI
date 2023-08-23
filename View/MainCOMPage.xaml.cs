using Microsoft.UI.Xaml.Controls;
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
	}
}
