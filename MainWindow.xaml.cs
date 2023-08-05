using Microsoft.UI.Xaml;
using VCOM_WinUI.View;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VCOM_WinUI
{
	/// <summary>
	/// An empty window that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainWindow : Window
	{
		public MainWindow()
		{
			this.InitializeComponent();
			ExtendsContentIntoTitleBar = true;
			SetTitleBar(TTB);
			MainNavView.DataContext = ViewModel.MainViewModel.Instance;
			ContentFrame.Navigate(typeof(MainCOMPage));
		}

		private void MainNavView_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
		{   //The Tag property contains the full type of the Page.
			if (args.InvokedItemContainer.Tag is not null)
				ContentFrame.Navigate(System.Type.GetType(args.InvokedItemContainer.Tag.ToString()));
		}
	}
}
