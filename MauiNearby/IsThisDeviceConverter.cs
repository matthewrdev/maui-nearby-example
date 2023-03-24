using System;
using System.Globalization;
using MauiNearby.Models;
using MauiNearby.Services;

namespace MauiNearby
{
	public class IsThisDeviceConverter : IValueConverter, IMarkupExtension
	{
		public IsThisDeviceConverter()
		{
		}

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as Ticket)?.CreatedByDeviceID == NearbySyncService.Instance.DeviceId;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

