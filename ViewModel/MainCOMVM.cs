﻿using CommunityToolkit.Mvvm.ComponentModel;
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
		readonly DispatcherQueue dispatcher = DispatcherQueue.GetForCurrentThread(); //Gets the UI thread.

		#region Observable Properties
		[ObservableProperty]
		ObservableCollection<COMDeviceModel> _COMList = new ObservableCollection<COMDeviceModel>();
		[ObservableProperty]
		bool _NoCOM = true;
		[ObservableProperty]
		bool _IsCurrentPortOpen = false;
		[ObservableProperty]
		bool _IsNotRefreshing = true;
		[ObservableProperty]
		string _SettingPortString = Localization.Loc.Default;
		#region Port Settings
		[ObservableProperty]
		int _SettingBaudRate = 115200;
		[ObservableProperty]
		int _SettingDataBits = 8;
		[ObservableProperty]
		StopBits _SettingStopBitsOrdinal = StopBits.None;
		[ObservableProperty]
		Parity _SettingParityOrdinal = Parity.None;
		#endregion
		#endregion

		public COMDeviceModel? ListSelectedCOM { get; set; }

		List<SerialPort> activeSPs = new List<SerialPort>();

		Dictionary<string, string> portNumNameDict = new Dictionary<string, string>();

		[RelayCommand]
		public void RefreshCOMList()
		{
			if (!IsNotRefreshing) return;   //Still refreshing.
			IsNotRefreshing = false;        //Update RefreshBtn IsEnabled.	
			Task.Run(() =>
			{
				string[] oldPortNums = portNumNameDict.Keys.ToArray();
				portNumNameDict.Clear();
				//ListSelectedCOM = null;
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
				//Create an array of reference of COMDeviceModel that's going to be deleted.
				COMDeviceModel[] toBeRemoved = (from device in COMList where removeThis.Contains(device.COMNumStr) select device).ToArray();
				dispatcher.TryEnqueue(() =>
				{   //Use UI thread to update COMList.
					if (toBeRemoved.Contains(ListSelectedCOM))
						ListSelectedCOM = null;
					IsCurrentPortOpen = ListSelectedCOM is not null && ListSelectedCOM.IsOpen;   //Update Visuals.
					foreach (COMDeviceModel rm in toBeRemoved)
						COMList.Remove(rm);
					foreach (KeyValuePair<string, string> portPair in portNumNameDict)
						if (dontAdd.Contains(portPair.Key)) continue;   //Skip existing port.
						else COMList.Add(new COMDeviceModel { COMNumStr = portPair.Key, COMDeviceName = portPair.Value, IsOpen = false });
					//COMList.Add(new COMDeviceModel { COMNumStr = "uh whatever", COMDeviceName = "Ain't nobody got time for this.", IsOpen = true });   //Test
					NoCOM = COMList.Count == 0; //Update visibility.
					IsNotRefreshing = true;     //Update RefreshBtn IsEnabled.	
				});
			});
		}

		[RelayCommand]
		public void TogglePort(string portName)
		{
			if (ListSelectedCOM is not null)
			{
				SerialPort serialPort;
				if (ListSelectedCOM.IsOpen = !ListSelectedCOM.IsOpen)     //Toggle Port
				{
					if (activeSPs.Any(sp => sp.PortName == portName))
					{   //Port already exists (Opened before)
						serialPort = activeSPs.First(sp => sp.PortName == portName);
					}
					else
					{   //Port does not exist
						serialPort = NewSP(portName, 9600, 8, StopBits.One, Parity.None);
						activeSPs.Add(serialPort);
					}
					serialPort.Open();
				}
				else
				{   //TODO: Close Port, to be tested.
					serialPort = activeSPs.First(sp => sp.PortName == portName);
					if (serialPort.IsOpen)
						serialPort.Close();
					//TODO: Add logging.
				}
				IsCurrentPortOpen = ListSelectedCOM.IsOpen = serialPort.IsOpen; //Update Visuals
			}
			else
				IsCurrentPortOpen = false;
		}

		public void SelectedCOMChanged()
		{
			if (ListSelectedCOM is not null)
			{
				IsCurrentPortOpen = ListSelectedCOM.IsOpen;
				SettingPortString = ListSelectedCOM.COMNumStr;
				SerialPort serialPort;
				if (activeSPs.Any(sp => sp.PortName == ListSelectedCOM.COMNumStr))
				{   //Port exists.
					serialPort = activeSPs.First(sp => sp.PortName == ListSelectedCOM.COMNumStr);
				}
				else
				{   //Port doesn't exist.
					serialPort = serialPort = NewSP(ListSelectedCOM.COMNumStr, 9600, 8, StopBits.One, Parity.None);
					activeSPs.Add(serialPort);
				}
			}
		}

		public static SerialPort NewSP(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity) => new SerialPort()
		{
			PortName = portName,
			BaudRate = baudRate,
			DataBits = dataBits,
			StopBits = stopBits,
			Parity = parity,
			Handshake = Handshake.None,
		};

		public MainCOMVM()
		{
			RefreshCOMList();
		}
	}
}
