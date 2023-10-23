namespace VCOM_WinUI.Model
{
	public struct PortSettingStruct
	{
		public string PortName { get; set; }
		public int BaudRate { get; set; }
		public int DataBits { get; set; }
		public System.IO.Ports.StopBits StopBitsOrdinal { get; set; }
		public System.IO.Ports.Parity ParityOrdinal { get; set; }
	}
}