using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace EventSpaceBookingSystem.Converters
{
    public class ShowPayNowButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string status && status == "Confirmed";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

}
