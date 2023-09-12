using Microsoft.UI.Xaml.Data;
using System;

namespace VCOM_WinUI.Converters
{
	public class Enum2IntConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return (int)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return value;
		}
	}
}
