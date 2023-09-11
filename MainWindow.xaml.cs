using Microsoft.UI.Xaml;
using System.Threading.Tasks;
using VCOM_WinUI.View;
using WinUIEx.Messaging;

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
			ExtendsContentIntoTitleBar = true;  //Hide default title bar.
			SetTitleBar(AppTitleBar);   //Set custom title bar.
			MainNavView.DataContext = ViewModel.MainViewModel.Instance;
			ContentFrame.Navigate(typeof(MainCOMPage));
			ViewModel.MainViewModel.WndMsgMonitor = new WindowMessageMonitor(this); //Put it in VM and make it static so it won't be disposed??
			ViewModel.MainViewModel.WndMsgMonitor.WindowMessageReceived += Monitor_WindowMessageReceived; //Register window message event.
		}

		private async void Monitor_WindowMessageReceived(object sender, WindowMessageEventArgs e)
		{   //Does this affect performance? idk.
			if (e.Message.MessageId == 537 && e.Message.WParam == 0x7 && ViewModel.MainViewModel.Instance.MainCOM.IsNotRefreshing)  //WM_DEVICECHANGE->DBT_DEVNODES_CHANGED
			{
				await Task.Delay(500);      //Wait a bit so GetPortNames can actually get something.
				ViewModel.MainViewModel.Instance.MainCOM.RefreshCOMList();
			}
		}

		string currentPage;
		private void MainNavView_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
		{   //The Tag property contains the full type of the Page.
			if (args.InvokedItemContainer.Tag is not null && currentPage != args.InvokedItemContainer.Tag.ToString())
				ContentFrame.Navigate(System.Type.GetType(currentPage = args.InvokedItemContainer.Tag.ToString()));
		}

	}
}
