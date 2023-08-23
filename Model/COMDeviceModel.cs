using CommunityToolkit.Mvvm.ComponentModel;

namespace VCOM_WinUI.Model
{
	public partial class COMDeviceModel : ObservableObject
	{
		public string COMNumStr { get; set; }
		public string COMDeviceName { get; set; }

		[ObservableProperty]
		bool _IsOpen = false;
	}
}
