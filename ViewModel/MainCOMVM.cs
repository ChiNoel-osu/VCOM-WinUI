using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VCOM_WinUI.Model;

namespace VCOM_WinUI.ViewModel
{
	public partial class MainCOMVM : ObservableObject
	{
		readonly DispatcherQueue dispatcher = DispatcherQueue.GetForCurrentThread(); //Gets the UI thread.
		public static readonly string PortSettingDir = Path.Combine(Directory.GetCurrentDirectory(), "PortSettings");

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
		string _SettingPortString = Localization.Loc.Default;   //Default parameter for new SPs.
		[ObservableProperty]
		bool _UnableToOpenPort = false; //For InfoBar.IsOpen
		[ObservableProperty]
		string _ReceiveString = string.Empty;
		#region Port Settings
		[ObservableProperty]
		int _SettingBaudRate = 115200;
		[ObservableProperty]
		int _SettingDataBits = 8;
		[ObservableProperty]
		StopBits _SettingStopBitsOrdinal = StopBits.One;
		[ObservableProperty]
		Parity _SettingParityOrdinal = Parity.None;
		#endregion
		#endregion
		bool opProgrammaticallyChanging = false;

		public COMDeviceModel? ListSelectedCOM { get; set; }

		List<SerialPort> activeSPs = new List<SerialPort>();

		Dictionary<string, string> portNumNameDict = new Dictionary<string, string>();
		Dictionary<SerialPort, string> spMsgDict = new Dictionary<SerialPort, string>();


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
					{   //Port already exists in list (Opened before)
						serialPort = activeSPs.First(sp => sp.PortName == portName);
					}
					else
					{   //Port does not exist in list (shouldn't happen bc it's added after user selected a port)
						serialPort = NewSP(portName, 115200, 8, StopBits.One, Parity.None);
						activeSPs.Add(serialPort);
						spMsgDict.Add(serialPort, string.Empty);
					}
					try
					{   //Open the port.
						UnableToOpenPort = false;
						serialPort.Open();
						Task.Run(() => SerialPortRecv(serialPort));
					}
					catch (Exception)
					{   //Failed to open the port.
						UnableToOpenPort = true;
					}
				}
				else
				{
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
					if (File.Exists(Path.Combine(PortSettingDir, ListSelectedCOM.COMNumStr + ".json")))
					{   //Port setting file exists, load port setting from file.
						PortSettingStruct portSetting = JsonSerializer.Deserialize<PortSettingStruct>(File.ReadAllText(Path.Combine(PortSettingDir, ListSelectedCOM.COMNumStr + ".json")));
						if (portSetting.PortName != ListSelectedCOM.COMNumStr) { /*TODO: some shit might happen here.*/; }
						opProgrammaticallyChanging = true;
						serialPort = NewSP(ListSelectedCOM.COMNumStr, SettingBaudRate = portSetting.BaudRate, SettingDataBits = portSetting.DataBits, SettingStopBitsOrdinal = portSetting.StopBitsOrdinal, SettingParityOrdinal = portSetting.ParityOrdinal);
						opProgrammaticallyChanging = false;
					}
					else
					{   //Port setting file not exist, load defaults.
						//TODO: Add default setting logic here.
						serialPort = NewSP(ListSelectedCOM.COMNumStr, 115200, 8, StopBits.One, Parity.None);
					}
					activeSPs.Add(serialPort);
					spMsgDict.Add(serialPort, string.Empty);
				}
				//TODO: Add setting UI logic here!
				dispatcher.TryEnqueue(() => ReceiveString = spMsgDict[serialPort]); //Change UI textbox string ref to selected port's RX string.
			}
		}

		#region Port setting change handler
		partial void OnSettingBaudRateChanged(int value)
		{
			if (opProgrammaticallyChanging) return;
			PortSettingChanger(1, value);
		}
		partial void OnSettingDataBitsChanged(int value)
		{
			if (opProgrammaticallyChanging) return;
			PortSettingChanger(2, value);
		}
		partial void OnSettingStopBitsOrdinalChanged(StopBits value)
		{
			if (opProgrammaticallyChanging) return;
			PortSettingChanger(3, (int)value);
		}
		partial void OnSettingParityOrdinalChanged(Parity value)
		{
			if (opProgrammaticallyChanging) return;
			PortSettingChanger(4, (int)value);
		}
		void PortSettingChanger(byte type, int value)
		{
			if (SettingPortString == Localization.Loc.Default)
			{
				return;
			}
			SerialPort serialPort = activeSPs.First(sp => sp.PortName == ListSelectedCOM.COMNumStr);
			switch (type)
			{
				case 1:
					serialPort.BaudRate = value;
					break;
				case 2:
					serialPort.DataBits = value;    //TODO: Add restriction between 5 and 8
					break;
				case 3:
					serialPort.StopBits = (StopBits)value;
					break;
				case 4:
					serialPort.Parity = (Parity)value;
					break;
				default:
					break;
			}
			File.WriteAllText(Path.Combine(PortSettingDir, serialPort.PortName + ".json"),
				 JsonSerializer.Serialize(new PortSettingStruct
				 {
					 PortName = serialPort.PortName,
					 BaudRate = serialPort.BaudRate,
					 DataBits = serialPort.DataBits,
					 StopBitsOrdinal = serialPort.StopBits,
					 ParityOrdinal = serialPort.Parity
				 }));
		}
		#endregion

		public static SerialPort NewSP(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity) => new SerialPort()
		{
			PortName = portName,
			BaudRate = baudRate,
			DataBits = dataBits,
			StopBits = stopBits,
			Parity = parity,
			Handshake = Handshake.None,
		};

		/// <summary>
		/// Serial port reading routine.
		/// CALL THIS IN A TASK!! Contains endless looping that only stops by exceptions.
		/// </summary>
		/// <param name="sp">The one and only SerialPort object.</param>
		void SerialPortRecv(SerialPort sp)
		{
			StringBuilder stringBuilder = new StringBuilder(spMsgDict[sp]);
			byte[] rBuffer = new byte[1];
			char charBuffer;
			while (true)
			{
				sp.Read(rBuffer, 0, 1);
				charBuffer = Convert.ToChar(rBuffer[0]);
				if (charBuffer == '\0') continue;
				spMsgDict[sp] = stringBuilder.Append(charBuffer).ToString();
				if (ListSelectedCOM.COMNumStr == sp.PortName)
					dispatcher.TryEnqueue(() => ReceiveString = spMsgDict[sp]);
			}
		}

		public MainCOMVM()
		{
			if (!Directory.Exists(PortSettingDir))  //TODO: Log about it not existing.
				Directory.CreateDirectory(PortSettingDir);
			RefreshCOMList();
		}
	}
}
