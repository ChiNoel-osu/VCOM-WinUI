using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
		[ObservableProperty]
		bool _IsCurrentPortOpen = false;

		public COMDeviceModel? ListSelectedCOM { get; set; } = null;

		Dictionary<string, string> portNumNameDict = new Dictionary<string, string>();

		[RelayCommand]
		public void RefreshCOMList()
		{
			Task.Run(() =>
			{
				string[] oldPortNums = portNumNameDict.Keys.ToArray();
				portNumNameDict.Clear();
				ListSelectedCOM = null;
				using ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'");
				string[] portNums = SerialPort.GetPortNames();
				string[] portNames = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(obj => obj["Caption"].ToString()).ToArray();
				List<string> dontAdd = new List<string>();
				List<string> removeThis = new List<string>();
				foreach (string portNum in portNums)    //Create dictionary based on port number and its name.
					portNumNameDict.Add(portNum, portNames.Where(name => name.Contains(portNum)).FirstOrDefault("Unknown Device"));
				foreach (string oldPortNum in oldPortNums)
					if (portNums.Contains(oldPortNum))  //Add existing port to the "Don't Add" list.
						dontAdd.Add(oldPortNum);
					else                                    //Add not existing port to the "Remove" list.
						removeThis.Add(oldPortNum);
				COMDeviceModel[] toBeRemoved = (from device in COMList where removeThis.Contains(device.COMNumStr) select device).ToArray();
				dispatcher.TryEnqueue(() =>
				{   //Use UI thread to update COMList.
					foreach (COMDeviceModel rm in toBeRemoved)
						COMList.Remove(rm);
					foreach (KeyValuePair<string, string> portPair in portNumNameDict)
						if (dontAdd.Contains(portPair.Key)) continue;   //Skip existing port.
						else
							COMList.Add(new COMDeviceModel { COMNumStr = portPair.Key, COMDeviceName = portPair.Value, IsOpen = false });
					//COMList.Add(new COMDeviceModel { COMNumStr = "uh whatever", COMDeviceName = "Ain't nobody got time for this.", IsOpen = true });   //Test
					NoCOM = COMList.Count == 0;    //Update visibility.
				});
			});
			IsCurrentPortOpen = ListSelectedCOM is not null;	//TODO: The ListSelectedCOM somehow changes to null after refreshing.
		}

		[RelayCommand]
		public void TogglePort(string portName)
		{
			if (ListSelectedCOM is not null)
			{
				ListSelectedCOM.IsOpen = !ListSelectedCOM.IsOpen;
				IsCurrentPortOpen = ListSelectedCOM.IsOpen;
			}
			else
				IsCurrentPortOpen = false;
		}

		public void SelectedCOMChanged()
		{
			if (ListSelectedCOM is not null)
				IsCurrentPortOpen = ListSelectedCOM.IsOpen;
		}

		public MainCOMVM()
		{
			RefreshCOMList();
		}
	}
}
