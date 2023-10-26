using Microsoft.UI.Xaml.Controls;
using System;
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
		Timer infoBarTimer = new Timer() { Interval = 4000 };
		Timer selectionTimer = new Timer() { Interval = 888 };
		bool isInfoBarVisible = false;

		public MainCOMPage()
		{
			this.InitializeComponent();
			infoBarTimer.Elapsed += InfoBarTimer_Elapsed;
			selectionTimer.Elapsed += SelectionTimer_Elapsed;
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
				MyInfoBar.Translation = new System.Numerics.Vector3(0, -60, 0); //Move the info bar up.
				if (!isInfoBarVisible)
				{
					isInfoBarVisible = true;
					infoBarTimer.Start();
				}
			}
			else
			{
				MyInfoBar.Translation = System.Numerics.Vector3.Zero;   //Move the info bar down.
				MyInfoBar.IsOpen = false;
			}
		}
		private void InfoBarTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			infoBarTimer.Stop();
			DispatcherQueue.TryEnqueue(() => MyInfoBar.Translation = System.Numerics.Vector3.Zero);
			isInfoBarVisible = false;
		}

		private void TextBlock_SizeChanged(object sender, Microsoft.UI.Xaml.SizeChangedEventArgs e) => RecvScrViewer.ChangeView(RecvScrViewer.HorizontalOffset, RecvScrViewer.ExtentHeight, 1.0f);

		private void TextBlock_SelectionChanged(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
		{
			if (!String.IsNullOrEmpty(((TextBlock)sender).SelectedText))    //STOP UPDATING
			{
				selectionTimer.Stop();
				((MainViewModel)DataContext).MainCOM.RecvNoUpdate = true;
			}
			else
			{
				selectionTimer.Start();
			}
		}
		private void RecvTextBlock_LosingFocus(Microsoft.UI.Xaml.UIElement sender, Microsoft.UI.Xaml.Input.LosingFocusEventArgs args)
		{
			((MainViewModel)DataContext).MainCOM.RecvNoUpdate = false;
			((MainViewModel)DataContext).MainCOM.UpdateCurrentSPRecvString();
		}
		private void SelectionTimer_Elapsed(object sender, ElapsedEventArgs e)
		{   //If some time later no selection made, keep updating.
			selectionTimer.Stop();
			DispatcherQueue.TryEnqueue(() =>
			{
				((MainViewModel)DataContext).MainCOM.RecvNoUpdate = false;
				((MainViewModel)DataContext).MainCOM.UpdateCurrentSPRecvString();
			});
		}
	}
}
