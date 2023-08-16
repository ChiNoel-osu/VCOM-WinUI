using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using VCOM_WinUI.Model;

namespace VCOM_WinUI.ViewModel
{
	public partial class MainCOMVM : ObservableObject
	{
		DispatcherQueue dispatcher = DispatcherQueue.GetForCurrentThread(); //Gets the UI thread.

		[ObservableProperty]
		ObservableCollection<COMDeviceModel> _COMList = new ObservableCollection<COMDeviceModel>();
		[ObservableProperty]
		bool _NoCOM = true;

		public COMDeviceModel? ListSelectedCOM { get; set; } = null;

		Dictionary<string, string> portNumNameDict = new Dictionary<string, string>();

		[RelayCommand]
		public void RefreshCOMList()
		{
			Task.Run(() =>
			{
				portNumNameDict.Clear();
				ListSelectedCOM = null;
				using ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'");
				string[] portNums = SerialPort.GetPortNames();
				string[] portNames = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(obj => obj["Caption"].ToString()).ToArray();
				foreach (string portNum in portNums)    //Create dictionary based on port number and its name.
					portNumNameDict.Add(portNum, portNames.Where(name => name.Contains(portNum)).FirstOrDefault("Unknown Device"));
				dispatcher.TryEnqueue(() =>
				{
					COMList.Clear();
					foreach (KeyValuePair<string, string> portPair in portNumNameDict)
						COMList.Add(new COMDeviceModel { COMNumStr = portPair.Key, COMDeviceName = portPair.Value, IsOpen = false });
					COMList.Add(new COMDeviceModel { COMNumStr = "uh whatever", COMDeviceName = "Ain't nobody got time for this.", IsOpen = true });   //Test
					NoCOM = COMList.Count == 0;    //Update visibility.
				});
			});
		}

		public MainCOMVM()
		{
			RefreshCOMList();
		}
	}
}
